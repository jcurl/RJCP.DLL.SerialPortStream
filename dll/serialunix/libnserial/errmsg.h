////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : errmsg.h
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Contains common error messages
//
////////////////////////////////////////////////////////////////////////////////
#include "nserial.h"

#ifndef NSERIAL_ERRMSG_H
#define NSERIAL_ERRMSG_H

typedef enum serialerrmsg {
  ERRMSG_OK = 0,
  ERRMSG_INVALIDPARAMETER,
  ERRMSG_UNSUPPORTEDBAUDRATE,
  ERRMSG_CANTOPENSERIALPORT,
  ERRMSG_CANTOPENANONPIPE,
  ERRMSG_CANTCONFIGUREANONPIPE,
  ERRMSG_SERIALPORTALREADYOPEN,
  ERRMSG_SERIALPORTNOTOPEN,
  ERRMSG_SERIALTCGETATTR,
  ERRMSG_SERIALTCSETATTR,
  ERRMSG_SERIALTCFLUSH,
  ERRMSG_INVALIDDATABITS,
  ERRMSG_INVALIDSTOPBITS,
  ERRMSG_INVALIDPARITY,
  ERRMSG_INVALIDBAUD,
  ERRMSG_INVALIDHANDSHAKE,
  ERRMSG_INVALIDHANDSHAKEDTR,
  ERRMSG_UNEXPECTEDBAUDRATE,
  ERRMSG_OUTOFMEMORY,
  ERRMSG_SERIALREAD,
  ERRMSG_SERIALREADEOF,
  ERRMSG_SERIALWRITE,
  ERRMSG_PIPEWRITE,
  ERRMSG_SELECT,
  ERRMSG_IOCTL,
  ERRMSG_IOCTL_ICOUNTER,
  ERRMSG_NOSYS,
} serialerrmsg_t;

int serial_seterror(struct serialhandle *handle, serialerrmsg_t error);
int serial_geterror(struct serialhandle *handle, serialerrmsg_t *error);
const char *serial_geterrorstring(serialerrmsg_t errmsg);
#endif
