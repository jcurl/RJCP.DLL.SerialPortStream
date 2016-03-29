////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : break.c
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Handle the break state.
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

NSERIAL_EXPORT int WINAPI serial_setbreak(struct serialhandle *handle, int breakstate)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

#ifdef HAVE_TERMIOS_BREAK
  if (handle->fd == -1) {
    serial_seterror(handle, ERRMSG_SERIALPORTNOTOPEN);
    errno = EBADF;
    return -1;
  }

  int result;
  if (breakstate) {
    result = ioctl(handle->fd, TIOCSBRK, NULL);
  } else {
    result = ioctl(handle->fd, TIOCCBRK, NULL);
  }

  if (result == -1) {
    serial_seterror(handle, ERRMSG_IOCTL);
    return -1;
  }

  handle->breakstate = breakstate;
  return 0;
#else
  serial_seterror(handle, ERRMSG_NOSYS);
  errno = ENOSYS;
  return -1;
#endif
}

NSERIAL_EXPORT int WINAPI serial_getbreak(struct serialhandle *handle, int *breakstate)
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

  *breakstate = handle->breakstate;
  return 0;
}
