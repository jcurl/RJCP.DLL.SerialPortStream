////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : types.h
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Generic types that are common to the project.
//
////////////////////////////////////////////////////////////////////////////////
#ifndef NSERIAL_TYPES_H
#define NSERIAL_TYPES_H

#include "config.h"

#include <sys/types.h>
#ifdef HAVE_STDINT_H
#include <stdint.h>
#endif
#ifdef HAVE_INTTYPES_H
#include <inttypes.h>
#endif

#define SIZEOF_ARRAY(array) (sizeof(array)/sizeof(array[0]))
#define min(a,b) ((a)>(b)?(a):(b))
#define max(a,b) ((a)<(b)?(a):(b))

// Somehow, somewhere Darwin defines TRUE. So we should check for it, and only
// define it if it is necessary. We should check for this in the configure
// script.
//
// In all cases, one should only assign the value of TRUE, never, ever compare
// if a value is TRUE. Always check if it is not FALSE if you need something
// to be TRUE.
//
// e.g.
//  if (x == TRUE)     // WRONG
//  if (x != TRUE)     // WRONG
//  if (x)             // correct
//  if (!x)            // correct
//  if (x != FALSE)    // correct
//  if (x == FALSE)    // correct

#ifndef TRUE
typedef enum _Boolean {
  TRUE = 1,
  FALSE = 0,
} Boolean;
#else
typedef int Boolean
#endif

#endif
