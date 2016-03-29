////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : nserial.c
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Supports initialising and terminating the libnserial
// datastructure. This is equivalent to the constructor and destructor for
// object oriented languages
//
////////////////////////////////////////////////////////////////////////////////

#include "config.h"

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <strings.h>
#include <errno.h>
#include <pthread.h>

#define NSERIAL_EXPORTS
#include "nserial.h"
#include "serialhandle.h"
#include "errmsg.h"
#include "threaddata.h"
#include "baudrate.h"

NSERIAL_EXPORT const char *WINAPI serial_version()
{
  static char version[32];
  snprintf(version, 32, "%d.%d.%d",
           NSERIAL_VERSION_MAJOR,
           NSERIAL_VERSION_MINOR,
           NSERIAL_VERSION_PATCH);
  return version;
}

static int initserialhandle(struct serialhandle *handle)
{
  bzero(handle, sizeof(struct serialhandle));

  handle->device = NULL;
  serial_setdefaultbaud(handle);
  handle->fd = -1;
  handle->prfd = -1;
  handle->pwfd = -1;
  handle->databits = 8;
  handle->parity = NOPARITY;
  handle->stopbits = ONE;
  handle->handshake = NOHANDSHAKE;
  handle->txcontinueonxoff = FALSE;
  handle->discardnull = FALSE;
  handle->xonlimit = 2048;
  handle->xofflimit = 512;
  handle->parityreplace = 0;
  pthread_mutex_init(&(handle->abortmutex), NULL);
  pthread_mutex_init(&(handle->modemmutex), NULL);
  handle->modemstate = NULL;

  threaddata_init();
  return 0;
}

NSERIAL_EXPORT struct serialhandle *WINAPI serial_init()
{
  struct serialhandle *handle;

  handle = malloc(sizeof(struct serialhandle));
  if (handle == NULL) {
    errno = ENOMEM;
    return NULL;
  }

  if (initserialhandle(handle)) {
    free(handle);
    return NULL;
  }

  return handle;
}

NSERIAL_EXPORT void WINAPI serial_terminate(struct serialhandle *handle)
{
  if (handle == NULL) return;

  serial_close(handle);

  if (handle->device) {
    free(handle->device);
  }

  if (handle->tmpbuffer) {
    free(handle->tmpbuffer);
  }

  pthread_mutex_destroy(&(handle->abortmutex));
  pthread_mutex_destroy(&(handle->modemmutex));
  free(handle);
}

NSERIAL_EXPORT int WINAPI serial_setdevicename(struct serialhandle *handle, const char *devicename)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  if (handle->fd != -1) {
    serial_seterror(handle, ERRMSG_SERIALPORTALREADYOPEN);
    errno = EIO;
    return -1;
  }

  if (devicename == NULL) {
    if (handle->device != NULL) {
      free(handle->device);
      handle->device = NULL;
      return 0;
    }
  } else {
    handle->device = strdup(devicename);
    if (handle->device == NULL) {
      serial_seterror(handle, ERRMSG_OUTOFMEMORY);
      return -1;
    }
  }
  return 0;
}

NSERIAL_EXPORT const char *WINAPI serial_getdevicename(struct serialhandle *handle)
{
  if (handle == NULL) {
    errno = EINVAL;
    return NULL;
  }

  return handle->device;
}
