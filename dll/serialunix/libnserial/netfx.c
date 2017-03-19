////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016-2017.
//
// FILE : netfx.c
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Routines specific for supporting managed libraries in interop.
//
////////////////////////////////////////////////////////////////////////////////

#include "config.h"

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <strings.h>
#include <errno.h>

#define NSERIAL_EXPORTS
#include "nserial.h"
#include "netfx.h"

NSERIAL_EXPORT int WINAPI netfx_errno(int lerrno)
{
  switch(lerrno) {
  case 0:           return NETFX_OK;
  case EINVAL:      return NETFX_EINVAL;
  case EACCES:      return NETFX_EACCES;
  case ENOMEM:      return NETFX_ENOMEM;
  case EBADF:       return NETFX_EBADF;
  case ENOSYS:      return NETFX_ENOSYS;
  case EIO:         return NETFX_EIO;
  case EINTR:       return NETFX_EINTR;
  default:
    // This needs to be here, as in some OSes, they're the same value
    // and would otherwise result in an error in the case statement.
    if (lerrno == EAGAIN)      return NETFX_EAGAIN;
    if (lerrno == EWOULDBLOCK) return NETFX_EWOULDBLOCK;
    return NETFX_UNKNOWN;
  }
}

NSERIAL_EXPORT const char *WINAPI netfx_errstring(int lerrno)
{
  return strerror(lerrno);
}
