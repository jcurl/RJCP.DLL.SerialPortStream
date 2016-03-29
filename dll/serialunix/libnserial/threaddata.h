////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : threaddata.h
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Defines thread specific data
//
////////////////////////////////////////////////////////////////////////////////

#ifndef NSERIAL_THREADDATA_H
#define NSERIAL_THREADDATA_H

#include "errmsg.h"

// Data that is thread specific
struct threadstate {
  serialerrmsg_t     serialerror;
};

int threaddata_init();
int threaddata_terminate();
struct threadstate *getthreaddata();

#endif
