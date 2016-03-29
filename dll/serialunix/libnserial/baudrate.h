////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : baudrate.h
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Handles the baudrate properties.
//
////////////////////////////////////////////////////////////////////////////////
#ifndef NSERIAL_BAUDRATE_H
#define NSERIAL_BAUDRATE_H

#include "serialhandle.h"

struct validbaud {
  unsigned long baud;
  unsigned long cbaud;
};

extern struct validbaud baudrates[];

void serial_setdefaultbaud(struct serialhandle *handle);

#endif
