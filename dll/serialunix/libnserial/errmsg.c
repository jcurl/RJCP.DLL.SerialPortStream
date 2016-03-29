////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : errmsg.c
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Contains common error messages
//
////////////////////////////////////////////////////////////////////////////////

#include "config.h"

#include <stdlib.h>
#include <errno.h>

#include "nserial.h"
#include "serialhandle.h"
#include "errmsg.h"
#include "threaddata.h"

int serial_seterror(struct serialhandle *handle, serialerrmsg_t error)
{
  struct threadstate *data = getthreaddata();
  if (data == NULL) {
    errno = ENOMEM;
    return -1;
  }
  data->serialerror = error;
  return 0;
}

int serial_geterror(struct serialhandle *handle, serialerrmsg_t *error)
{
  struct threadstate *data = getthreaddata();
  if (data == NULL) {
    errno = ENOMEM;
    return -1;
  }
  *error = data->serialerror;
  return 0;
}

const char *serial_geterrorstring(serialerrmsg_t errmsg)
{
  switch(errmsg) {
  case ERRMSG_OK:
    return "No error";
  case ERRMSG_INVALIDPARAMETER:
    return "Invalid parameter";
  case ERRMSG_UNSUPPORTEDBAUDRATE:
    return "Unsupported baud rate";
  case ERRMSG_CANTOPENSERIALPORT:
    return "Can't open serial port";
  case ERRMSG_CANTOPENANONPIPE:
    return "Can't open internal anonymous pipes";
  case ERRMSG_CANTCONFIGUREANONPIPE:
    return "Can't configure internal anonymous pipes";
  case ERRMSG_SERIALPORTALREADYOPEN:
    return "Serial port already open";
  case ERRMSG_SERIALPORTNOTOPEN:
    return "Serial port not open";
  case ERRMSG_SERIALTCGETATTR:
    return "Can't get serial port attributes from OS";
  case ERRMSG_SERIALTCSETATTR:
    return "Can't set serial port attributes to OS";
  case ERRMSG_SERIALTCFLUSH:
    return "Can't flush serial port input/output";
  case ERRMSG_INVALIDDATABITS:
    return "Unsupported setting for databits for this platform";
  case ERRMSG_INVALIDSTOPBITS:
    return "Unsuppored setting for stopbits for this platform";
  case ERRMSG_INVALIDPARITY:
    return "Unsupported setting for parity for this platform";
  case ERRMSG_INVALIDBAUD:
    return "Unsupported setting for baud rate for this platform";
  case ERRMSG_INVALIDHANDSHAKE:
    return "Unsupported setting for handshake for this platform";
  case ERRMSG_INVALIDHANDSHAKEDTR:
    return "DTR handhshaking not supported for this platform";
  case ERRMSG_UNEXPECTEDBAUDRATE:
    return "Unexpected baudrate returned after set";
  case ERRMSG_OUTOFMEMORY:
    return "Out of memory";
  case ERRMSG_SELECT:
    return "Select error";
  case ERRMSG_SERIALREAD:
    return "Read error";
  case ERRMSG_SERIALREADEOF:
    return "Read End-Of-File reached";
  case ERRMSG_SERIALWRITE:
    return "Write error";
  case ERRMSG_PIPEWRITE:
    return "Write error to internal anonymous pipe";
  case ERRMSG_IOCTL:
    return "ioctl error";
  case ERRMSG_IOCTL_ICOUNTER:
    return "ioctl(TIOCGICOUNT) error";
  case ERRMSG_NOSYS:
    return "Unsupported feature for this platform";

  default:
    return "Unknown error";
  }
}

NSERIAL_EXPORT const char *WINAPI serial_error(struct serialhandle *handle)
{
  if (handle == NULL) {
    return NULL;
  }

  serialerrmsg_t error;
  if (serial_geterror(handle, &error) == -1) {
    return NULL;
  }
  return serial_geterrorstring(error);
}
