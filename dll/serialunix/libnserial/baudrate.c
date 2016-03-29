////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : baudrate.c
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Handles the baudrate properties.
//
////////////////////////////////////////////////////////////////////////////////

#include "config.h"

#include <stdlib.h>
#include <errno.h>
#include <termios.h>

#define NSERIAL_EXPORTS
#include "nserial.h"
#include "serialhandle.h"
#include "baudrate.h"
#include "errmsg.h"

struct validbaud baudrates[] = {
#ifdef HAVE_TERMIOS_B50
  {50, B50},
#endif
#ifdef HAVE_TERMIOS_B75
  {75, B75},
#endif
#ifdef HAVE_TERMIOS_B110
  {110, B110},
#endif
#ifdef HAVE_TERMIOS_B134
  {134, B134},
#endif
#ifdef HAVE_TERMIOS_B150
  {150, B150},
#endif
#ifdef HAVE_TERMIOS_B200
  {200, B200},
#endif
#ifdef HAVE_TERMIOS_B300
  {300, B300},
#endif
#ifdef HAVE_TERMIOS_B600
  {600, B600},
#endif
#ifdef HAVE_TERMIOS_B1200
  {1200, B1200},
#endif
#ifdef HAVE_TERMIOS_B1800
  {1800, B1800},
#endif
#ifdef HAVE_TERMIOS_B2400
  {2400, B2400},
#endif
#ifdef HAVE_TERMIOS_B4800
  {4800, B4800},
#endif
#ifdef HAVE_TERMIOS_B9600
  {9600, B9600},
#endif
#ifdef HAVE_TERMIOS_B14400
  {14400, B14400},
#endif
#ifdef HAVE_TERMIOS_B19200
  {19200, B19200},
#endif
#ifdef HAVE_TERMIOS_B33600
  {33600, B33600},
#endif
#ifdef HAVE_TERMIOS_B38400
  {38400, B38400},
#endif
#ifdef HAVE_TERMIOS_B57600
  {57600, B57600},
#endif
#ifdef HAVE_TERMIOS_B76800
  {76800, B76800},
#endif
#ifdef HAVE_TERMIOS_B115200
  {115200, B115200},
#endif
#ifdef HAVE_TERMIOS_B128000
  {128000, B128000},
#endif
#ifdef HAVE_TERMIOS_B153600
  {153600, B153600},
#endif
#ifdef HAVE_TERMIOS_B230400
  {230400, B230400},
#endif
#ifdef HAVE_TERMIOS_B256000
  {256000, B256000},
#endif
#ifdef HAVE_TERMIOS_B307200
  {307200, B307200},
#endif
#ifdef HAVE_TERMIOS_B460800
  {460800, B460800},
#endif
#ifdef HAVE_TERMIOS_B500000
  {500000, B500000},
#endif
#ifdef HAVE_TERMIOS_B576000
  {576000, B576000},
#endif
#ifdef HAVE_TERMIOS_B921600
  {921600, B921600},
#endif
#ifdef HAVE_TERMIOS_B1000000
  {1000000, B1000000},
#endif
#ifdef HAVE_TERMIOS_B1152000
  {1152000, B1152000},
#endif
#ifdef HAVE_TERMIOS_B1500000
  {1500000, B1500000},
#endif
#ifdef HAVE_TERMIOS_B2000000
  {2000000, B2000000},
#endif
#ifdef HAVE_TERMIOS_B2500000
  {2500000, B2500000},
#endif
#ifdef HAVE_TERMIOS_B3000000
  {3000000, B3000000},
#endif
#ifdef HAVE_TERMIOS_B3500000
  {3500000, B3500000},
#endif
#ifdef HAVE_TERMIOS_B4000000
  {4000000, B4000000},
#endif
  {0, }
};

////////////////////////////////////////////////////////////////////////////////
// Methods
////////////////////////////////////////////////////////////////////////////////
NSERIAL_EXPORT int *WINAPI serial_getsupportedbaudrates()
{
  // We're not going to free this memory. It's only allocated once and only when
  // needed.
  static int *supportedbaudrates = NULL;
  int i;

  if (supportedbaudrates == NULL) {
    supportedbaudrates = malloc(sizeof(int) * SIZEOF_ARRAY(baudrates));
    if (supportedbaudrates == NULL) {
      errno = ENOMEM;
      return NULL;
    }

    for (i = 0; i < SIZEOF_ARRAY(baudrates); i++) {
      supportedbaudrates[i] = baudrates[i].baud;
    }
  }

  return supportedbaudrates;
}

NSERIAL_EXPORT int WINAPI serial_setbaud(struct serialhandle *handle, int baud)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  // Check that the baudrate is valid
  int i = 0;
  while (baudrates[i].baud) {
    if (baud == baudrates[i].baud) {
      handle->baudrate = baudrates[i].baud;
      handle->cbaud = baudrates[i].cbaud;

      // TODO: Update the serial port baud rate if already opened.
      return 0;
    }
    i++;
  }

  serial_seterror(handle, ERRMSG_UNSUPPORTEDBAUDRATE);
  errno = EINVAL;
  return -1;
}

NSERIAL_EXPORT int WINAPI serial_getbaud(struct serialhandle *handle, int *baud)
{
  if (handle == NULL || baud == NULL) {
    errno = EINVAL;
    return -1;
  }
  *baud = handle->baudrate;
  return 0;
}

void serial_setdefaultbaud(struct serialhandle *handle)
{
  // Find a baudrate that is 115200 or lower and set that.
  int bauditem = 0;
  while (baudrates[bauditem].baud && baudrates[bauditem].baud <= 115200) {
    if (baudrates[bauditem].baud <= 115200) {
      handle->baudrate = baudrates[bauditem].baud;
      handle->cbaud = baudrates[bauditem].cbaud;
    }
    bauditem++;
  }
}
