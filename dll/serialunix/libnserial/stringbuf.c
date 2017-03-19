////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016-2017.
//
// FILE : stringbuf.c
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Utilities for string buffer handling.
//
////////////////////////////////////////////////////////////////////////////////

#include "config.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define NSERIAL_EXPORTS
#include "stringbuf.h"

// Append the string 'str' to the current position in 'buffer', of up to 'buflen'
// bytes. On return, give the pointer to the start of the copied string (or NULL
// if it couldn't be copied). Advance 'buffer' and 'buflen'.
char *strnappend(char *buffer, size_t *offset, size_t buflen, char *str)
{
  if (buffer == NULL || offset == NULL) return NULL;
  if (buflen == 0 || str == NULL) return NULL;

  int sl = strlen(str);
  if (buflen - *offset <= sl) return NULL;           // Not enough space

  char *dest = buffer + *offset;
  memcpy(dest, str, sl + 1);

  *offset += sl + 1;
  return dest;
}
