#include "config.h"

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <termios.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <sys/time.h>
#include <sys/ioctl.h>
#include <fcntl.h>

#ifdef HAVE_LINUX_SERIAL_ICOUNTER_STRUCT
#include <linux/serial.h>
#endif

int initserial(const char *device);

int main(int argc, char **argv)
{
#ifndef HAVE_LINUX_SERIAL_ICOUNTER_STRUCT
  fprintf(stderr, "Your system doesn't support TIOCGICOUNT headers.");
  return 255;
#else
  if (argc != 2) {
    fprintf(stderr, "Usage: %s <device>\n", argv[0]);
    return 1;
  }

  int handle;
  handle = initserial(argv[1]);
  if (handle == -1) {
    fprintf(stderr, "Exiting...\n");
    return 2;
  }

  struct serial_icounter_struct ocounter = {0, };

  if (ioctl(handle, TIOCGICOUNT, &ocounter) < 0) {
    int e = errno;
    fprintf(stderr, "Your driver doesn't support TIOCGICOUNT\n");
    fprintf(stderr, "  Error: %d (%s)\n", e, strerror(e));
  } else {
    printf("Your driver supports TIOCGICOUNT\n");
    printf("ocounter.cts=%d\n", ocounter.cts);
    printf("ocounter.dsr=%d\n", ocounter.dsr);
    printf("ocounter.rng=%d\n", ocounter.rng);
    printf("ocounter.dcd=%d\n", ocounter.dcd);
  }

  close(handle);
  return 0;
#endif
}

int initserial(const char *device)
{
  int fd;
  int lerrno;

  fd = open(device, O_RDWR | O_NOCTTY | O_NONBLOCK);
  if (fd == -1) {
    lerrno = errno;
    fprintf(stderr, "ERROR: opening %s: %s (%d)\n", device,
            strerror(lerrno), lerrno);
    return -1;
  }

  struct termios newtio;
  if (tcgetattr(fd, &newtio) == -1) {
    lerrno = errno;
    fprintf(stderr, "ERROR: getting attributes %s: %s (%d)\n", device,
            strerror(lerrno), lerrno);
    close(fd);
    return -1;
  }

  newtio.c_cflag |= (CLOCAL | CREAD | CS8);
  newtio.c_cflag &= ~(CSTOPB | PARENB | PARODD | CRTSCTS);
  newtio.c_lflag &= ~(ICANON | ECHO | ECHOE | ECHOK | ECHONL | ISIG | IEXTEN);
  newtio.c_oflag &= ~OPOST;
  newtio.c_iflag = IGNBRK;
  newtio.c_iflag &= ~(INPCK | ISTRIP | IGNPAR | PARMRK | IXON | IXOFF | IXANY);

  if (cfsetospeed(&newtio, B115200) < 0 ||
      cfsetispeed(&newtio, B115200) < 0) {
    lerrno = errno;
    fprintf(stderr, "ERROR: setting baudrate %s: %s (%d)\n", device,
            strerror(lerrno), lerrno);
    close(fd);
    return -1;
  }

  newtio.c_cc[VMIN] = 0;
  newtio.c_cc[VTIME] = 0;

  if (tcsetattr(fd, TCSADRAIN, &newtio) < 0) {
    lerrno = errno;
    fprintf(stderr, "ERROR: setting attributes %s: %s (%d)\n", device,
            strerror(lerrno), lerrno);
    close(fd);
    return -1;
  }

  return fd;
}
