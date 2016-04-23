////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : openserial.c
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Opens and closes the serial port.
//
////////////////////////////////////////////////////////////////////////////////

#include "config.h"

#include <stdlib.h>
#include <errno.h>
#include <fcntl.h>
#include <termios.h>
#include <unistd.h>

#define NSERIAL_EXPORTS
#include "nserial.h"
#include "serialhandle.h"
#include "baudrate.h"
#include "errmsg.h"
#include "openserial.h"
#include "flush.h"

NSERIAL_EXPORT int WINAPI serial_open(struct serialhandle *handle)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  serial_seterror(handle, ERRMSG_OK);
  if (handle->fd != -1) {
    serial_seterror(handle, ERRMSG_SERIALPORTALREADYOPEN);
    errno = EINVAL;
    return -1;
  }

  handle->fd = open(handle->device, O_RDWR | O_NOCTTY | O_NONBLOCK);
  if (handle->fd == -1) {
    serial_seterror(handle, ERRMSG_CANTOPENSERIALPORT);
    return -1;
  }

  int pipefd[2];
  if (pipe(pipefd) == -1) {
    serial_seterror(handle, ERRMSG_CANTOPENANONPIPE);
    close(handle->fd);
    handle->fd = -1;
    return -1;
  }

  handle->prfd = pipefd[0];
  handle->pwfd = pipefd[1];
  if (fcntl(handle->prfd, F_SETFL, O_NONBLOCK) == -1 ||
      fcntl(handle->pwfd, F_SETFL, O_NONBLOCK) == -1) {
    serial_seterror(handle, ERRMSG_CANTCONFIGUREANONPIPE);
    close(handle->fd);
    close(handle->prfd);
    close(handle->pwfd);
    handle->fd = -1;
    handle->prfd = -1;
    handle->pwfd = -1;
    return -1;
  }

  handle->abortpending = FALSE;

  return 0;
}

NSERIAL_EXPORT int WINAPI serial_getproperties(struct serialhandle *handle)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  serial_seterror(handle, ERRMSG_OK);
  if (handle->fd == -1) {
    serial_seterror(handle, ERRMSG_SERIALPORTNOTOPEN);
    errno = EBADF;
    return -1;
  }

  struct termios tio;
  if (tcgetattr(handle->fd, &tio) == -1) {
    serial_seterror(handle, ERRMSG_SERIALTCGETATTR);
    return -1;
  }

  // Get the databits
  switch (tio.c_cflag & CSIZE) {
  case CS5:
    handle->databits = 5;
    break;
  case CS6:
    handle->databits = 6;
    break;
  case CS7:
    handle->databits = 7;
    break;
  case CS8:
    handle->databits = 8;
    break;
  default:
    // Unknown. Default to 8 databits.
    handle->databits = 8;
    break;
  }

  // Get the stop bits
  if (!(tio.c_cflag & CSTOPB)) {
    handle->stopbits = ONE;
  } else {
    handle->stopbits = TWO;
  }

  // Get the parity
  if (!(tio.c_cflag & PARENB)) {
    handle->parity = NOPARITY;
  } else {
#ifdef HAVE_TERMIOS_CMSPAR
    if (tio.c_cflag & CMSPAR) {
      if (!(tio.c_cflag & PARODD)) {
	handle->parity = SPACE;
      } else {
	handle->parity = MARK;
      }
    } else {
#endif
      if (!(tio.c_cflag & PARODD)) {
	handle->parity = EVEN;
      } else {
	handle->parity = ODD;
      }
#ifdef HAVE_TERMIOS_CMSPAR
    }
#endif
  }

  // Get the handshake
  handle->handshake = NOHANDSHAKE;
  if (tio.c_cflag & CRTSCTS) {
    handle->handshake |= RTS;
  }
  if (tio.c_iflag & (IXON | IXOFF | IXANY)) {
    handle->handshake |= XON;
  }

  // Get the baud rate
  handle->cbaud = cfgetispeed(&tio);
  handle->baudrate = 0;
  int i = 0;
  while (baudrates[i].baud && handle->baudrate == 0) {
    if (baudrates[i].cbaud == handle->cbaud) {
      handle->baudrate = baudrates[i].baud;
    }
    i++;
  }
  if (handle->baudrate == 0) {
    // This baudrate is not known, so we set the default
    serial_setdefaultbaud(handle);
  }

  return 0;
}

NSERIAL_EXPORT int WINAPI serial_setproperties(struct serialhandle *handle)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  serial_seterror(handle, ERRMSG_OK);
  if (handle->fd == -1) {
    serial_seterror(handle, ERRMSG_SERIALPORTNOTOPEN);
    errno = EBADF;
    return -1;
  }

  struct termios newtio;
  if (tcgetattr(handle->fd, &newtio) == -1) {
    serial_seterror(handle, ERRMSG_SERIALTCGETATTR);
    return -1;
  }

  // To know what the flags are, see:
  //  http://pubs.opengroup.org/onlinepubs/009695399/basedefs/termios.h.html

  // Enable receiver
  newtio.c_cflag |= (CLOCAL | CREAD);
  // Set input to be raw
  newtio.c_lflag &= ~(ICANON | ECHO | ECHOE | ECHOK | ECHONL | ISIG | IEXTEN);
  // Set output to be raw
  newtio.c_oflag &= ~OPOST;
  // Turn off translations that might occur. Also kills IUCLC and IMAXBEL
  newtio.c_iflag = IGNBRK;

  // Set databits
  newtio.c_cflag &= ~CSIZE;
  switch (handle->databits) {
  case 5:
    newtio.c_cflag |= CS5;
    break;
  case 6:
    newtio.c_cflag |= CS6;
    break;
  case 7:
    newtio.c_cflag |= CS7;
    break;
  case 8:
    newtio.c_cflag |= CS8;
    break;
  default:
    serial_seterror(handle, ERRMSG_INVALIDDATABITS);
    errno = EINVAL;
    return -1;
  }

  // Set stopbits
  switch (handle->stopbits) {
  case ONE:
    newtio.c_cflag &= ~CSTOPB;
    break;
  case TWO:
    newtio.c_cflag |= CSTOPB;
    break;
  default:
    // Unix doesn't seem to have a way to set 1.5 stop bits, unless you set it
    // to two and databits is CS5 (see comments in linux kernel code).
    serial_seterror(handle, ERRMSG_INVALIDSTOPBITS);
    errno = EINVAL;
    return -1;
  }

  // Set parity. Framing errors result in zero (~IGNPAR)
  newtio.c_iflag &= ~(INPCK | ISTRIP | IGNPAR | PARMRK);
  switch (handle->parity) {
  case NOPARITY:
    newtio.c_cflag &= ~(PARENB | PARODD);
    break;
  case EVEN:
    newtio.c_cflag |= PARENB;
    newtio.c_cflag &= ~PARODD;
#ifdef HAVE_TERMIOS_CMSPAR
    newtio.c_cflag &= ~CMSPAR;
#endif
    newtio.c_iflag |= INPCK;
    // If we have less than 8 bits, we should strip it, else the 8th bit
    // contains the parity bit. That may confuse applications.
    if (handle->databits != 8) newtio.c_iflag |= ISTRIP;
    // If we have parity replace, then we need to know when there's a parity
    // error. The read routine then also needs to check this and ensure that
    // post processing occurs.
    if (handle->parityreplace) {
      newtio.c_iflag |= PARMRK;
      if (handle->databits != 8) {
        handle->parityrepactive = PARMODE_STRIP;
      } else {
        handle->parityrepactive = PARMODE_NOSTRIP;
      }
    } else {
      handle->parityrepactive = PARMODE_INACTIVE;
    }
    break;
  case ODD:
    newtio.c_cflag |= PARENB | PARODD;
#ifdef HAVE_TERMIOS_CMSPAR
    newtio.c_cflag &= ~CMSPAR;
#endif
    newtio.c_iflag |= INPCK;
    if (handle->databits != 8) newtio.c_iflag |= ISTRIP;
    if (handle->parityreplace) {
      newtio.c_iflag |= PARMRK;
      if (handle->databits != 8) {
        handle->parityrepactive = PARMODE_STRIP;
      } else {
        handle->parityrepactive = PARMODE_NOSTRIP;
      }
    } else {
      handle->parityrepactive = PARMODE_INACTIVE;
    }
    break;
#ifdef HAVE_TERMIOS_CMSPAR
  case SPACE:
    // CMSPAR is not Posix standard, but works for Linux.
    //  See: http://man7.org/linux/man-pages/man3/termios.3.html
    newtio.c_cflag |= PARENB | CMSPAR;
    newtio.c_cflag &= ~PARODD;
    if (handle->databits != 8) newtio.c_iflag |= ISTRIP;
    break;
#endif
#ifdef HAVE_TERMIOS_CMSPAR
  case MARK:
    // CMSPAR is not Posix standard, but works for Linux.
    //  See: http://man7.org/linux/man-pages/man3/termios.3.html
    newtio.c_cflag |= PARENB | CMSPAR | PARODD;
    if (handle->databits != 8) newtio.c_iflag |= ISTRIP;
    break;
#endif
  default:
    serial_seterror(handle, ERRMSG_INVALIDPARITY);
    errno = EINVAL;
    return -1;
  }

  // Set handshake
  newtio.c_cflag &= ~CRTSCTS;
  newtio.c_iflag &= ~(IXON | IXOFF | IXANY);

  if (handle->handshake & !DTRRTSXON) {
    serial_seterror(handle, ERRMSG_INVALIDHANDSHAKE);
    errno = EINVAL;
    return -1;
  }
  if (handle->handshake & RTS) {
    newtio.c_cflag |= CRTSCTS;
  }
  if (handle->handshake & XON) {
    newtio.c_iflag |= IXOFF | IXON;
  }
  if (handle->handshake & DTR) {
    serial_seterror(handle, ERRMSG_INVALIDHANDSHAKEDTR);
    errno = EINVAL;
    return -1;
  }

  // Set baudrate. Here we assume first no custom baudrate
  if (cfsetospeed(&newtio, handle->cbaud) < 0 ||
      cfsetispeed(&newtio, handle->cbaud) < 0) {
    serial_seterror(handle, ERRMSG_INVALIDBAUD);
    errno = EINVAL;
    return -1;
  }

  // TODO: Custom baudrates are not currently supported

  // Turn off delays in the system. Read only the data in the serial port.
  newtio.c_cc[VMIN] = 0;
  newtio.c_cc[VTIME] = 0;

  // Wait until all output written to fd has been transmitted. In case that
  // we've just initialised, there is no output, so is equivalent to TCSANOW.
  if (tcsetattr(handle->fd, TCSADRAIN, &newtio) < 0) {
    serial_seterror(handle, ERRMSG_SERIALTCSETATTR);
    return -1;
  }

  flushbuffer(handle);

  // Get the baudrate and compare with what we set
  tcgetattr(handle->fd, &newtio);
  if (cfgetispeed(&newtio) != handle->cbaud ||
      cfgetospeed(&newtio) != handle->cbaud) {
    // For some reason the baudrate was not set to what we had asked.
    serial_seterror(handle, ERRMSG_UNEXPECTEDBAUDRATE);
    errno = EIO;
    return -1;
  }

  // Allocate temporary buffer if needed
  if (handle->tmpbuffer == NULL) {
    if (handle->parityrepactive || handle->discardnull) {
      // In these two modes, we need to read data first, then parse it into
      // the user supplied buffer.
      handle->tmpbuffer = malloc(SERIALBUFFERSIZE);
      if (handle->tmpbuffer == NULL) {
	serial_seterror(handle, ERRMSG_OUTOFMEMORY);
	errno = ENOMEM;
	return -1;
      }
    }
  }

  return 0;
}

NSERIAL_EXPORT int WINAPI serial_close(struct serialhandle *handle)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  if (handle->fd == -1) return 0;
  flushbuffer(handle);

  if (handle->prfd != -1) {
    close(handle->prfd);
    handle->prfd = -1;
  }
  if (handle->pwfd != -1) {
    close(handle->pwfd);
    handle->pwfd = -1;
  }

  tcflush(handle->fd, TCIOFLUSH);
  close(handle->fd);
  handle->fd = -1;
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_isopen(struct serialhandle *handle, int *isopen)
{
  if (handle == NULL || isopen == NULL) {
    if (handle != NULL) {
      serial_seterror(handle, ERRMSG_INVALIDPARAMETER);
    }
    errno = EINVAL;
    return -1;
  }

  serial_seterror(handle, ERRMSG_OK);
  *isopen = handle->fd == -1 ? 0 : 1;
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_getfd(struct serialhandle *handle)
{
  int isopen;

  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  serial_seterror(handle, ERRMSG_OK);
  if (serial_isopen(handle, &isopen)) return -1;
  if (!isopen) {
    serial_seterror(handle, ERRMSG_SERIALPORTNOTOPEN);
    errno = EBADF;
    return -1;
  }

  return handle->fd;
}
