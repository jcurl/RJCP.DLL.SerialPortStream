////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : flush.c
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Flushes the serial port
//
////////////////////////////////////////////////////////////////////////////////

#include "config.h"

#include <stdlib.h>
#include <errno.h>
#include <termios.h>
#include <unistd.h>

#define NSERIAL_EXPORTS
#include "nserial.h"
#include "types.h"
#include "serialhandle.h"
#include "errmsg.h"
#include "flush.h"

NSERIAL_EXPORT int WINAPI serial_reset(struct serialhandle *handle)
{
  if (handle == NULL) {
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

  tcflush(handle->fd, TCIOFLUSH);
  flushbuffer(handle);
  return 0;
}

void flushbuffer(struct serialhandle *handle)
{
  handle->tmpstart = 0;
  handle->tmplength = 0;
  handle->tmpread = FALSE;
}
