////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2018.
//
// FILE : modem.h
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Methods for setting modem flags in serial hardware.
//
////////////////////////////////////////////////////////////////////////////////
#ifndef NSERIAL_MODEML_H
#define NSERIAL_MODEM_H

#include "nserial.h"

int serial_setdtrinternal(struct serialhandle *handle);
int serial_setrtsinternal(struct serialhandle *handle);

#endif
