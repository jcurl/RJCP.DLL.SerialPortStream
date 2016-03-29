////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : basic.c
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Handles some of the most basic properties for a serial port.
//
////////////////////////////////////////////////////////////////////////////////

#include "config.h"

#include <stdlib.h>
#include <errno.h>

#define NSERIAL_EXPORTS
#include "nserial.h"
#include "serialhandle.h"

NSERIAL_EXPORT int WINAPI serial_setdatabits(struct serialhandle *handle, int databits)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  if (databits < 5 || databits > 8) {
    errno = EINVAL;
    return -1;
  }

  handle->databits = databits;
  if (handle->fd != -1) return serial_setproperties(handle);
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_getdatabits(struct serialhandle *handle, int *databits)
{
  if (handle == NULL || databits == NULL) {
    errno = EINVAL;
    return -1;
  }

  *databits = handle->databits;
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_setparity(struct serialhandle *handle, serialparity_t parity)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  if (parity < 0 || parity > 4) {
    errno = EINVAL;
    return -1;
  }

  handle->parity = parity;
  if (handle->fd != -1) return serial_setproperties(handle);
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_getparity(struct serialhandle *handle, serialparity_t *parity)
{
  if (handle == NULL || parity == NULL) {
    errno = EINVAL;
    return -1;
  }

  *parity = handle->parity;
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_setstopbits(struct serialhandle *handle, serialstopbits_t stopbits)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  // Note, this tests the range of the enum, not the number of stopbits as an integer.
  if (stopbits < 0 || stopbits > 2) {
    errno = EINVAL;
    return -1;
  }

  handle->stopbits = stopbits;
  if (handle->fd != -1) return serial_setproperties(handle);
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_getstopbits(struct serialhandle *handle, serialstopbits_t *stopbits)
{
  if (handle == NULL || stopbits == NULL) {
    errno = EINVAL;
    return -1;
  }

  *stopbits = handle->stopbits;
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_sethandshake(struct serialhandle *handle, serialhandshake_t handshake)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  if (handshake < 0 || handshake > 7) {
    errno = EINVAL;
    return -1;
  }

  handle->handshake = handshake;
  if (handle->fd != -1) return serial_setproperties(handle);
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_gethandshake(struct serialhandle *handle, serialhandshake_t *handshake)
{
  if (handle == NULL || handshake == NULL) {
    errno = EINVAL;
    return -1;
  }

  *handshake = handle->handshake;
  return 0;
}
