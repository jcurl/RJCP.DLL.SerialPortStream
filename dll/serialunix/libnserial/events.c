////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : events.c
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Handles waiting for events from the serial port. This allows
// the programmer to have an event loop thread and do things based on the
// current state of the application.
//
////////////////////////////////////////////////////////////////////////////////

#include "config.h"

#include <stdlib.h>
#include <sys/time.h>
#include <sys/types.h>
#include <sys/select.h>
#include <unistd.h>
#include <errno.h>

#define NSERIAL_EXPORTS
#include "nserial.h"
#include "serialhandle.h"
#include "errmsg.h"
#include "openserial.h"

static ssize_t internal_read(struct serialhandle *handle, char *buf, size_t count);

NSERIAL_EXPORT serialevent_t WINAPI serial_waitforevent(struct serialhandle *handle, serialevent_t event, int timeout)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  if (event == NOEVENT) return NOEVENT;

  serial_seterror(handle, ERRMSG_OK);

  int isopen;
  if (serial_isopen(handle, &isopen)) return -1;
  if (!isopen) {
    serial_seterror(handle, ERRMSG_SERIALPORTNOTOPEN);
    errno = EIO;
    return -1;
  }

  // Check if we have any data still cached.
  if (event & READEVENT) {
    if (handle->tmpbuffer && handle->tmplength) {
      return READEVENT;
    }
  }

  int maxfd = -1;
  fd_set serreadfds;
  fd_set serwritefds;
  FD_ZERO(&serreadfds);
  FD_ZERO(&serwritefds);
  if (event & READEVENT) {
    FD_SET(handle->fd, &serreadfds);
    if (maxfd < handle->fd) maxfd = handle->fd;
  }
  if (event & WRITEEVENT) {
    FD_SET(handle->fd, &serwritefds);
    if (maxfd < handle->fd) maxfd = handle->fd;
  }
  FD_SET(handle->prfd, &serreadfds);
  if (maxfd < handle->prfd) maxfd = handle->prfd;

  struct timeval *ptval;
  struct timeval  tval;
  if (timeout >= 0) {
    tval.tv_sec = timeout / 1000;
    tval.tv_usec = (timeout % 1000) * 1000;
    ptval = &tval;
  } else {
    ptval = NULL;
  }

  int r = select(maxfd + 1,
                 &serreadfds,
                 (event & WRITEEVENT) ? &serwritefds : NULL,
                 NULL, ptval);
  if (r < 0) {
    if (errno != EINTR) {
      serial_seterror(handle, ERRMSG_SELECT);
      return -1;
    }
  } else if (r > 0) {
    serialevent_t resultevent = NOEVENT;
    if ((event & READEVENT) &&
        FD_ISSET(handle->fd, &serreadfds)) resultevent |= READEVENT;
    if ((event & WRITEEVENT) &&
        FD_ISSET(handle->fd, &serwritefds)) resultevent |= WRITEEVENT;
    if (FD_ISSET(handle->prfd, &serreadfds)) {
      // We clear the buffer, while we're doing it, serialise access for
      // the next abort method.
      pthread_mutex_lock(&(handle->abortmutex));
      // Something wrote to the pipe to abort the select()
      char buffer[128];
      while (read(handle->prfd, buffer, SIZEOF_ARRAY(buffer)) > 0) { }
      errno = 0;
      handle->abortpending = FALSE;
      pthread_mutex_unlock(&(handle->abortmutex));
    }
    return resultevent;
  }
  return NOEVENT;
}

NSERIAL_EXPORT int WINAPI serial_abortwaitforevent(struct serialhandle *handle)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  pthread_mutex_lock(&(handle->abortmutex));
  if (handle->abortpending) {
    // We've already signalled an abort.
    pthread_mutex_unlock(&(handle->abortmutex));
    return 0;
  }
  char pabort = 'X';
  if (write(handle->pwfd, &pabort, 1) == -1) {
    serial_seterror(handle, ERRMSG_PIPEWRITE);
    pthread_mutex_unlock(&(handle->abortmutex));
    return -1;
  }
  handle->abortpending = TRUE;
  pthread_mutex_unlock(&(handle->abortmutex));
  return 0;
}

NSERIAL_EXPORT ssize_t WINAPI serial_read(struct serialhandle *handle, char *buffer, size_t length)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  serial_seterror(handle, ERRMSG_OK);

  if (buffer == NULL) {
    serial_seterror(handle, ERRMSG_INVALIDPARAMETER);
    errno = EINVAL;
    return -1;
  }

  int isopen;
  if (serial_isopen(handle, &isopen)) return -1;
  if (!isopen) {
    serial_seterror(handle, ERRMSG_SERIALPORTNOTOPEN);
    errno = EIO;
    return -1;
  }

  if (length == 0) return 0;

  if (!(handle->parityrepactive || handle->discardnull)) {
    return internal_read(handle, buffer, length);
  }

  // Read into a temporary buffer and then Post process the data for one of
  // the options:
  // * parityreplace
  // * discardnull

  if (handle->tmpbuffer == NULL) {
    serial_seterror(handle, ERRMSG_OUTOFMEMORY);
    errno = ENOMEM;
    return -1;
  }

  ssize_t readbytes;
  char   *buff;

  if (handle->tmpread) {
    // Just simulate the read, because we read more data than the user
    // wanted last time.
    buff = handle->tmpbuffer + handle->tmpstart;
    readbytes = handle->tmplength;
  } else {
    // Last call to this function required that we need to read more data to
    // process further. The previous call made sure that relevant data is at
    // the beginning of the buffer, so we don't have to implement a circular
    // queue.
    buff = handle->tmpbuffer;
    readbytes = internal_read(handle,
                              handle->tmpbuffer + handle->tmplength,
                              SERIALBUFFERSIZE - handle->tmplength);
    if (readbytes <= 0) return readbytes;
    readbytes += handle->tmplength;
  }

  int i = 0, j = 0;
  while (i < readbytes && j < length) {
    // Handle parity replacement. The spec has in simplest form:
    // * 0xFF 0x00 0xNN => handle->parityreplace
    // * 0xFF 0xFF      => 0xFF
    //
    // We don't actually have to check if ISTRIP is active or not, because if
    // it's active, we know we can never get 0xFF 0xFF in the stream (unless
    // there's a bug in the kernel driver).
    if (handle->parityrepactive != PARMODE_INACTIVE) {
      if (buff[i] == (char)0xFF) {
        if ((i + 1) >= readbytes) {
          // Not enough data in the buffer. Store it for when new data
          // arrives.
          handle->tmpread = FALSE;
          handle->tmpbuffer[0] = 0xFF;
          handle->tmplength = 1;
          i = readbytes;
          continue;
        }

        // Bytes 0xFF 0xFF indicate a single byte 0xFF in the output buffer.
        if (buff[i + 1] == (char)0xFF) {
          buff[j++] = 0xFF;
          i += 2;
          continue;
        }

        if ((i + 2) >= readbytes) {
          // Not enough data in the buffer. Store it for when new data
          // arrives.
          handle->tmpread = FALSE;
          handle->tmpbuffer[0] = 0xFF;
          handle->tmpbuffer[1] = 0x00;
          handle->tmplength = 2;
          i = readbytes;
          continue;
        }

        buffer[j++] = handle->parityreplace;
        i += 3;
        continue;
      }
    }

    // Handle discardnull. Parity errors will be removed due to this flag.
    if (!handle->discardnull || buff[i]) {
      buffer[j++] = buff[i];
    }
    i++;
  }

  if (i < readbytes) {
    // If we didn't copy all of the internal data into the user supplied
    // buffer, remember where we got to and resume on the next read call.
    handle->tmpread = TRUE;
    handle->tmpstart += i;
    handle->tmplength -= i;
  } else {
    handle->tmpread = FALSE;
    handle->tmpstart = 0;
    handle->tmplength = 0;
  }

  readbytes = j;
  return readbytes;
}

static ssize_t internal_read(struct serialhandle *handle, char *buf, size_t count)
{
  ssize_t readbytes;

  readbytes = read(handle->fd, buf, count);
  if (readbytes == 0) {
    serial_seterror(handle, ERRMSG_SERIALREADEOF);
    errno = EIO;
    return -1;
  } else if (readbytes < 0) {
    if (errno == EAGAIN || errno == EWOULDBLOCK) {
      return 0;
    }
    serial_seterror(handle, ERRMSG_SERIALREAD);
    return -1;
  }

  return readbytes;
}

NSERIAL_EXPORT ssize_t WINAPI serial_write(struct serialhandle *handle, const char *buffer, size_t length)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  serial_seterror(handle, ERRMSG_OK);

  if (buffer == NULL) {
    serial_seterror(handle, ERRMSG_INVALIDPARAMETER);
    errno = EINVAL;
    return -1;
  }

  int isopen;
  if (serial_isopen(handle, &isopen)) return -1;
  if (!isopen) {
    serial_seterror(handle, ERRMSG_SERIALPORTNOTOPEN);
    errno = EIO;
    return -1;
  }

  if (length == 0) return 0;

  ssize_t writebytes;
  writebytes = write(handle->fd, buffer, length);
  if (writebytes < 0) {
    if (errno == EAGAIN || errno == EWOULDBLOCK) {
      return 0;
    }
    serial_seterror(handle, ERRMSG_SERIALWRITE);
    return -1;
  }

  return writebytes;
}
