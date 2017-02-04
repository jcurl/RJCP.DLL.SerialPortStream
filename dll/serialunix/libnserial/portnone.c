////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016-2017.
//
// FILE : portnone.c
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : An empty implementation for getting ports. Used on operating
//   systems where we don't know how to detect serial ports yet.
//
////////////////////////////////////////////////////////////////////////////////

#include "config.h"
#include <stdlib.h>
#include <errno.h>

#define NSERIAL_EXPORTS
#include "nserial.h"
#include "serialhandle.h"
#include "errmsg.h"

NSERIAL_EXPORT struct portdescription *WINAPI serial_getports(struct serialhandle *handle)
{
  if (handle == NULL) {
    errno = EINVAL;
    return NULL;
  }

  errno = ENOSYS;
  serial_seterror(handle, ERRMSG_NOSYS);
  return NULL;
}
