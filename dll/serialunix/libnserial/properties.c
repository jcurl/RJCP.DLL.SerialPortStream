////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : properties.c
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Handles some of the more advanced properties for the serial
// port.
//
////////////////////////////////////////////////////////////////////////////////

#include "config.h"

#include <stdlib.h>
#include <errno.h>

#define NSERIAL_EXPORTS
#include "nserial.h"
#include "serialhandle.h"

NSERIAL_EXPORT int WINAPI serial_settxcontinueonxoff(struct serialhandle *handle, int txcontinueonxoff)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  handle->txcontinueonxoff = txcontinueonxoff ? 1 : 0;

  // TODO: Update the serial port data if already opened
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_gettxcontinueonxoff(struct serialhandle *handle, int *txcontinueonxoff)
{
  if (handle == NULL || txcontinueonxoff == NULL) {
    errno = EINVAL;
    return -1;
  }

  *txcontinueonxoff = handle->txcontinueonxoff;
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_setdiscardnull(struct serialhandle *handle, int discardnull)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  handle->discardnull = discardnull ? 1 : 0;

  // TODO: Update the serial port data if already opened
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_getdiscardnull(struct serialhandle *handle, int *discardnull)
{
  if (handle == NULL || discardnull == NULL) {
    errno = EINVAL;
    return -1;
  }

  *discardnull = handle->discardnull;
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_setxonlimit(struct serialhandle *handle, int xonlimit)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  if (xonlimit < 0) {
    errno = EINVAL;
    return -1;
  }

  handle->xonlimit = xonlimit;

  // TODO: Update the serial port data if already opened
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_getxonlimit(struct serialhandle *handle, int *xonlimit)
{
  if (handle == NULL || xonlimit == NULL) {
    errno = EINVAL;
    return -1;
  }

  *xonlimit = handle->xonlimit;
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_setxofflimit(struct serialhandle *handle, int xofflimit)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  if (xofflimit < 0) {
    errno = EINVAL;
    return -1;
  }

  handle->xofflimit = xofflimit;

  // TODO: Update the serial port data if already opened
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_getxofflimit(struct serialhandle *handle, int *xofflimit)
{
  if (handle == NULL || xofflimit == NULL) {
    errno = EINVAL;
    return -1;
  }

  *xofflimit = handle->xofflimit;
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_setparityreplace(struct serialhandle *handle, int parityreplace)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  handle->parityreplace = parityreplace;
  if (handle->fd != -1) return serial_setproperties(handle);

  return 0;
}

NSERIAL_EXPORT int WINAPI serial_getparityreplace(struct serialhandle *handle, int *parityreplace)
{
  if (handle == NULL || parityreplace == NULL) {
    errno = EINVAL;
    return -1;
  }

  *parityreplace = handle->parityreplace;
  return 0;
}
