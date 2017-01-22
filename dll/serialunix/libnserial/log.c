////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016-2017.
//
// FILE : log.c
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Handles logging
//
////////////////////////////////////////////////////////////////////////////////

#include "config.h"

#include <stdlib.h>
#include <stdarg.h>
#include <stdio.h>
#include <errno.h>
#include <time.h>
#include <sys/time.h>

#define NSERIAL_EXPORTS
#include "log.h"

#ifdef NSLOG_ENABLED
static FILE *openlog(const char *filename)
{
  FILE *logfile;

  if (filename == NULL) return NULL;

  logfile = fopen(filename, "a");
  if (logfile == NULL) {
    fprintf(stderr, "Couldn't open log file for writing\n");
    return NULL;
  }
  return logfile;
}

void nslog(struct serialhandle *handle, int priority, const char *format, ...)
{
  FILE *logfile;
  int terrno;
  struct timeval tv;
  struct tm *ptm;
  char outstr[200];

  terrno = errno;
  va_list argp;
  va_start(argp, format);

  logfile = openlog("/tmp/nserial.log");
  if (logfile == NULL) logfile = stderr;
  if (logfile != NULL) {
    if (gettimeofday(&tv, NULL)) {
      tv.tv_sec = 0;
      tv.tv_usec = 0;
      snprintf(outstr, sizeof(outstr), "XXXXXXXX-XXXXXX");
    } else {
      ptm = localtime(&(tv.tv_sec));
      if (ptm != NULL) {
	if (strftime(outstr, sizeof(outstr), "%Y.%m.%d-%H:%M:%S", ptm) == 0) {
	  snprintf(outstr, sizeof(outstr), "strftime==%d", errno);
	}
      } else {
	snprintf(outstr, sizeof(outstr), "ptm==NULL");
      }
    }

    fprintf(logfile, "%s.%06ld\t%1d\t", outstr, tv.tv_usec, priority);

    if (handle != NULL && handle->device != NULL) {
      fprintf(logfile, "%s\t", handle->device);
    } else {
      fprintf(logfile, "(--)\t");
    }

    vfprintf(logfile, format, argp);
    fprintf(logfile, "\n");
    fflush(logfile);
    fclose(logfile);
  }
  va_end(argp);
  errno = terrno;
}
#endif
