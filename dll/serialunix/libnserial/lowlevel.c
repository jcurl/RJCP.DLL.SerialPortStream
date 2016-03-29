////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : lowlevel.c
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Perform low level driver operations
//
////////////////////////////////////////////////////////////////////////////////
#include "config.h"

#include <stdlib.h>
#include <termios.h>
#include <sys/ioctl.h>
#include <errno.h>

#define NSERIAL_EXPORTS
#include "nserial.h"
#include "serialhandle.h"
#include "errmsg.h"

NSERIAL_EXPORT int WINAPI serial_getreadbytes(struct serialhandle *handle, int *queue)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

#ifdef HAVE_TERMIOS_TIOCINQ
  if (handle->fd == -1) {
    serial_seterror(handle, ERRMSG_SERIALPORTNOTOPEN);
    errno = EBADF;
    return -1;
  }

  int queuesize;
  int result;
  result = ioctl(handle->fd, TIOCINQ, &queuesize);
  if (result < 0) {
    serial_seterror(handle, ERRMSG_IOCTL);
    return -1;
  }

  *queue = queuesize;
  return 0;
#else
  serial_seterror(handle, ERRMSG_NOSYS);
  errno = ENOSYS;
  return -1;
#endif
}

NSERIAL_EXPORT int WINAPI serial_getwritebytes(struct serialhandle *handle, int *queue)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

#ifdef HAVE_TERMIOS_TIOCOUTQ
  if (handle->fd == -1) {
    serial_seterror(handle, ERRMSG_SERIALPORTNOTOPEN);
    errno = EBADF;
    return -1;
  }

  int queuesize;
  int result;
  result = ioctl(handle->fd, TIOCOUTQ, &queuesize);
  if (result < 0) {
    serial_seterror(handle, ERRMSG_IOCTL);
    return -1;
  }

  *queue = queuesize;
  return 0;
#else
  serial_seterror(handle, ERRMSG_NOSYS);
  errno = ENOSYS;
  return -1;
#endif
}

static int serial_discardbuffer(struct serialhandle *handle, int inout)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  if (handle->fd == -1) {
    serial_seterror(handle, ERRMSG_SERIALPORTNOTOPEN);
    errno = EBADF;
    return -1;
  }

  if (tcflush(handle->fd, inout ? TCOFLUSH : TCIFLUSH)) {
    serial_seterror(handle, ERRMSG_SERIALTCFLUSH);
    return -1;
  }
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_discardinbuffer(struct serialhandle *handle)
{
  return serial_discardbuffer(handle, 0);
}

NSERIAL_EXPORT int WINAPI serial_discardoutbuffer(struct serialhandle *handle)
{
  return serial_discardbuffer(handle, 1);
}

