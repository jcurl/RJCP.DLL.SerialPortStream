////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : flush.h
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Get and set modem signals
//
////////////////////////////////////////////////////////////////////////////////
#ifndef NSERIAL_FLUSH_H
#define NSERIAL_FLUSH_H

#include "nserial.h"

void flushbuffer(struct serialhandle *handle);

#endif
