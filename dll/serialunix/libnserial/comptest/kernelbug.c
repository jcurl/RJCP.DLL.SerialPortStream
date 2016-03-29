#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <termios.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <sys/time.h>
#include <fcntl.h>

// This code recreates the situation described in the file
// NOTES.bug-serialportclose.txt. To workaround the bug, uncomment the line
// "write" in the function readwrite().
//
// Compile with the command:
//   gcc kernelbug.c -Wall
//
// It should provide the following output:
//
// $ sudo ./a.out /dev/ttyUSB0 /dev/ttyUSB1
// Offset: 4
// Flushing...
// Writing Complete...
// Reading complete...
// Comparison MATCH                    <---- PASS
// Flushing...
// Reading complete...
// Complete...
//
// In case of the bug:
//
// $ sudo ./a.out /dev/ttyUSB0 /dev/ttyUSB1
// Offset: 108
// Flushing...
// Flush 2 bytes
// Writing Complete...
// Reading complete...
// ERROR: Comparison mismatch          <---- ERROR
// Flushing...
// Flush 510 bytes
// Reading complete...
// Complete...
//
// The code was tested on:
//  Linux leon-ubuntu 3.19.0-49-generic #55~14.04.1-Ubuntu SMP
//   Fri Jan 22 11:23:34 UTC 2016 i686 i686 i686 GNU/Linux
//
// Chipsets tested:
//  * PL2303H -> FAIL
//  * FTDI -> FAIL
//  * 16550A -> PASS
//  * PL2303RA -> PASS


char writebuff[256000];
char readbuff[1024];

int initserial(const char *device);
int readwrite(int writefd, int readfd, int wsize, int rsize);
int waitforwrite(int writefd);
int waitforread(int readfd);

int main(int argc, char **argv)
{
  if (argc != 3) {
    fprintf(stderr, "Usage: %s <writedevice> <readdevice>\n", argv[0]);
    return 1;
  }

  struct timeval tv;
  struct timezone tz;
  gettimeofday(&tv, &tz);
  printf("Offset: %d\n", (int)(tv.tv_usec % 256));

  int i;
  for (i = 0; i < 256000; i++) {
    writebuff[i] = (tv.tv_usec + i * 7) % 256;
  }

  int readhandle, writehandle;
  writehandle = initserial(argv[1]);
  readhandle = initserial(argv[2]);
  if (writehandle == -1 || readhandle == -1) {
    fprintf(stderr, "Exiting...\n");
    return 2;
  }

  if (readwrite(writehandle, readhandle, 1024, 1024) == -1) {
    fprintf(stderr, "Exiting...\n");
    return 3;
  }

  // Compare the buffers
  int comparison = 1;
  for (i = 0; comparison && i < 1024; i++) {
    if (writebuff[i] != readbuff[i]) comparison = 0;
  }
  if (!comparison) {
    printf("ERROR: Comparison mismatch\n");
  } else {
    printf("Comparison MATCH\n");
  }

  // This is the code that causes the bug
  readwrite(writehandle, readhandle, 256000, 1024);
  printf("Complete...\n");

  close(readhandle);
  close(writehandle);
  return 0;
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

int readwrite(int writefd, int readfd, int wsize, int rsize)
{
  int woffset = 0, roffset = 0;
  int readfinished = 0, writefinished = 0;
  int revent, wevent;
  int lerrno;

  // First step, flush data
  printf("Flushing...\n");
  char tmpbuff[256];

  // UNCOMMENT this to workaround the bug....
  //write(writefd, tmpbuff, 1);

  int flushed = 0;
  while (!readfinished) {
    revent = waitforread(readfd);
    if (revent == -1) {
      lerrno = errno;
      fprintf(stderr, "ERROR: waiting for flushing read %s (%d)\n",
              strerror(lerrno), lerrno);
      return -1;
    } else if (revent) {
      ssize_t readbytes = read(readfd, tmpbuff, 256);
      flushed += readbytes;
    } else {
      readfinished = 1;
    }
  }
  printf("Flush %d bytes\n", flushed);

  // Send and Receive data
  readfinished = 0;
  while (!readfinished) {
    if (!writefinished) {
      wevent = waitforwrite(writefd);
      if (wevent == -1) {
        lerrno = errno;
        fprintf(stderr, "ERROR: waiting for write %s (%d)\n",
                strerror(lerrno), lerrno);
        return -1;
      } else if (wevent) {
        ssize_t writebytes = write(writefd, writebuff + woffset,
                                   wsize - woffset);
        //printf("%d=write(fd=%d, offset=%d, length=%d)\n",
        //       writebytes, writefd, woffset, wsize - woffset);
        if (writebytes < 0) {
          if (errno == EAGAIN || errno == EWOULDBLOCK) {
            fprintf(stderr, "WARNING: Write select error, would block...\n");
            writebytes = 0;
          } else {
            lerrno = errno;
            fprintf(stderr, "ERROR: writing %s (%d)\n",
                    strerror(lerrno), lerrno);
            return -1;
          }
        }
        if (writebytes == 0) {
          fprintf(stderr, "WARNING: writing zero bytes\n");
          writefinished = 1;
        } else {
          woffset += writebytes;
          if (woffset == wsize) {
            fprintf(stderr, "Writing Complete...\n");
            writefinished = 1;
          }
        }
      }
    }

    revent = waitforread(readfd);
    if (revent == -1) {
      lerrno = errno;
      fprintf(stderr, "ERROR: waiting for read %s (%d)\n",
              strerror(lerrno), lerrno);
      return -1;
    } else if (revent) {
      ssize_t readbytes = read(readfd, readbuff + roffset,
                               rsize - roffset);
      //printf("%d=read(fd=%d, offset=%d, length=%d)\n",
      //       readbytes, readfd, roffset, rsize - roffset);
      if (readbytes == 0) {
        fprintf(stderr, "ERROR: Read end of file reached...\n");
        return -1;
      } else if (readbytes < 0) {
        if (errno == EAGAIN || errno == EWOULDBLOCK) {
          fprintf(stderr, "WARNING: Read select error, would block...\n");
          readbytes = 0;
        } else {
          lerrno = errno;
          fprintf(stderr, "ERROR: reading %s (%d)\n",
                  strerror(lerrno), lerrno);
          return -1;
        }
      } else {
        roffset += readbytes;
        if (roffset == rsize) {
          fprintf(stderr, "Reading complete...\n");
          readfinished = 1;
        }
      }
    } else {
      fprintf(stderr, "No data pending for read...\n");
      readfinished = 1;
    }
  }

  return 0;
}

int waitforwrite(int writefd)
{
  fd_set writefds;
  FD_ZERO(&writefds);
  FD_SET(writefd, &writefds);

  int r = select(writefd + 1, NULL, &writefds, NULL, NULL);
  if (r < 0) {
    return -1;
  } else if (r > 0) {
    if (FD_ISSET(writefd, &writefds)) return 1;
  }
  return 0;
}

int waitforread(int readfd)
{
  fd_set readfds;
  FD_ZERO(&readfds);
  FD_SET(readfd, &readfds);

  struct timeval tval;
  tval.tv_sec = 0;
  tval.tv_usec = 500000;

  int r = select(readfd + 1, &readfds, NULL, NULL, &tval);
  if (r < 0) {
    return -1;
  } else if (r > 0) {
    if (FD_ISSET(readfd, &readfds)) return 1;
  }
  return 0;
}
