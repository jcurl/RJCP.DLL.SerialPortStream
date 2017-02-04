////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : stringbuf.h
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : String utilities
//
////////////////////////////////////////////////////////////////////////////////
#ifndef NSERIAL_STRINGBUF_H
#define NSERIAL_STRINGBUF_H

char *strnappend(char *buffer, size_t *offset, size_t buflen, char *str);

#endif
