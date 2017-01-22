////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016-2017.
//
// FILE : log.h
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Handles logging
//
////////////////////////////////////////////////////////////////////////////////
#ifndef NSERIAL_LOG_H
#define NSERIAL_LOG_H

#include "serialhandle.h"

#define NSLOG_EMERG     0
#define NSLOG_ALERT     1
#define NSLOG_CRIT      2
#define NSLOG_ERR       3
#define NSLOG_WARNING   4
#define NSLOG_NOTICE    5
#define NSLOG_INFO      6
#define NSLOG_DEBUG     7

#ifdef NSLOG_ENABLED
void nslog(struct serialhandle *handle, int priority, const char *format, ...);
#else
#define nslog(...)
#endif
#endif
