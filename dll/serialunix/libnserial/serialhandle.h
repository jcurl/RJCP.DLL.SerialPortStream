////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : serialhandle.h
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Describes the internal datastructure for the serial port
// handle.
//
////////////////////////////////////////////////////////////////////////////////
#ifndef NSERIAL_SERIALHANDLE_H
#define NSERIAL_SERIALHANDLE_H

#include <pthread.h>

#define NSERIAL_EXPORTS
#include "nserial.h"
#include "types.h"

typedef enum parityrepmode {
  PARMODE_INACTIVE = 0,
  PARMODE_STRIP    = 1,
  PARMODE_NOSTRIP  = 2
} parityrepmode_t;

struct modemstate {
  struct serialhandle *handle;
  serialmodemevent_t   waitevent;
  serialmodemevent_t   eventresult;
  int                  serialerror;
};

struct serialhandle {
  char              *device;            // The device to open
  int                fd;                // File descriptor for the serial port
  int                baudrate;          // Integer value of the baudrate
  int                cbaud;             // Converted cbaud value
  int                databits;          // Databits: 5..8
  serialparity_t     parity;            // Parity: N, E, O, S, M
  serialstopbits_t   stopbits;          // Stopbits: 1, 1.5, 2
  serialhandshake_t  handshake;         // Handshake:
  int                txcontinueonxoff;  // TxContinueOnXOff boolean
  int                discardnull;       // DiscardNull boolean
  int                xonlimit;          // XOnLimit in bytes
  int                xofflimit;         // XOffLimit in bytes
  int                parityreplace;     // ParityReplace byte
  parityrepmode_t    parityrepactive;   // ParityReplace is active on open?
  int                breakstate;        // Current break state.

  char              *tmpbuffer;         // Temporary buffer
  int                tmpstart;          // Offset where last read starts
  int                tmplength;         // Length of data to read
  int                tmpread;           // If we should read into tmpbuffer

  // When handling the abort, we just can't rely on writing to the named
  // pipe, as if some stupid program happens to abort a million times, it
  // would eventually fill up the buffer and cause serial_abortwaitforevent()
  // to block. Even if we solved that by ignoring write errors, it's not
  // atomic and might still lead to some subtle race conditions. So we do
  // it very carefully with a mutex. See events.c for details.
  int                prfd;              // Pipe read file descriptor
  int                pwfd;              // Pipe write file descriptor
  int                abortpending;      // Flag if there is an abort pending
  pthread_mutex_t    abortmutex;        // For synchronisation of abortpending
                                        // and writing to the pipe
  pthread_mutex_t    modemmutex;        // Managing modem events
  struct modemstate *modemstate;        // Are we waiting on a modem event?
  pthread_t          modemthread;       // Waiting on a modem event
};

#endif
