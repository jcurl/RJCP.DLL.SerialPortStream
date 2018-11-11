////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016-2018.
//
// FILE : portlinux.c
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Search for serial ports for Linux Operating Systems.
//
// We look in the path '/sys/class/tty/*' for a list of all TTY devices. Each
// TTY will be considered a real device if it contains a softlink
// 'device/driver' which is a symlink to the directory for the driver for the
// TTY. That then excludes a lot of the PTY (Pseudo TTYs).
//
// Then for each device, we look at the file 'dev' to get the major and minor
// device node. Once we have a list of all the device nodes, we then do a
// recursive search through '/dev' looking for any entries with those device
// nodes. If the device is of type 'platform:serial8250' which is readable
// by '/sys/class/tty/*/device/modalias', we will also open the device and
// check that it is a real serial port or not.
//
////////////////////////////////////////////////////////////////////////////////

#include "config.h"
#include <sys/types.h>
#include <sys/sysmacros.h>
#include <sys/stat.h>
#include <sys/ioctl.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <limits.h>
#include <dirent.h>
#include <time.h>
#include <fcntl.h>
#include <unistd.h>
#include <linux/serial.h>

#define NSERIAL_EXPORTS
#include "nserial.h"
#include "serialhandle.h"
#include "stringbuf.h"
#include "errmsg.h"
#include "log.h"
#include "types.h"

#define MAXINTPORTS 256
#define PORTBUFLEN (MAXPORTS * 256)

static const char *checkdevices[] = {
  "platform:serial8250"
};

static const char *sysclasstty = "/sys/class/tty";
static const char *devtree = "/dev";

struct portentry {
  int  major;        // Major node number
  int  minor;        // Minor node number
  int  check;        // If this should be checked
  int  found;        // Has already been logged
  const char *tty;   // String for the TTY device in /sys/class/tty
};

static int isreal(struct serialhandle *handle, const char *dir)
{
  char path[PATH_MAX];
  int len = snprintf(path, PATH_MAX, "%s/device/driver", dir);
  if (len >= PATH_MAX) {
    nslog(handle, NSLOG_WARNING, "getports: path truncated: %s", path);
    return FALSE;
  }

  struct stat sb;
  if (stat(path, &sb) < 0) {
    nslog(handle, NSLOG_DEBUG, "getports: Ignoring: %s", dir);
    return FALSE;
  }

  return TRUE;
}

static int getdevicenode(struct serialhandle *handle, const char *dir, struct portentry *entry)
{
  char path[PATH_MAX];
  int len = snprintf(path, PATH_MAX, "%s/dev", dir);
  if (len >= PATH_MAX) {
    nslog(handle, NSLOG_WARNING, "getports: path truncated: %s", path);
    return -1;
  }

  FILE *devfile = fopen(path, "r");
  if (!devfile) {
    nslog(handle, NSLOG_WARNING, "getports: path %s can't be opened: errno=%d", path, errno);
    return -1;
  }

  char *line = NULL;
  size_t linelen = 0;
  ssize_t read;
  read = getline(&line, &linelen, devfile);
  if (read < 0) {
    nslog(handle, NSLOG_WARNING, "getports: path %s can't be read: errno=%d", path, errno);
    fclose(devfile);
    return -1;
  }

  // Parse, getting the major number and minor number
  char *sep;
  entry->major = strtol(line, &sep, 10);
  if (*sep != ':') {
    nslog(handle, NSLOG_WARNING, "getports: path %s not in format major:minor", path);
    fclose(devfile);
    return -1;
  }

  entry->minor = strtol(sep+1, &sep, 10);
  if (*sep && *sep != '\n') {
    nslog(handle, NSLOG_WARNING, "getports: path %s not in format major:minor", path);
    fclose(devfile);
    return -1;
  }

  free(line);
  fclose(devfile);
  return 0;
}

static int mustcheck(const char *type)
{
  int i;
  int l = SIZEOF_ARRAY(checkdevices);
  for (i = 0; i < l; i++) {
    if (strcmp(type, checkdevices[i]) == 0) {
      return TRUE;
    }
  }
  return FALSE;
}

static int getdevicemodalias(struct serialhandle *handle, const char *dir, struct portentry *entry)
{
  char path[PATH_MAX];
  int len = snprintf(path, PATH_MAX, "%s/device/modalias", dir);
  if (len >= PATH_MAX) {
    nslog(handle, NSLOG_WARNING, "getports: path truncated: %s", path);
    return -1;
  }

  FILE *devfile = fopen(path, "r");
  if (!devfile) {
    nslog(handle, NSLOG_WARNING, "getports: path %s can't be opened: errno=%d", path, errno);
    return -1;
  }

  char *line = NULL;
  size_t linelen = 0;
  ssize_t read;
  read = getline(&line, &linelen, devfile);
  if (read < 0) {
    nslog(handle, NSLOG_WARNING, "getports: path %s can't be read: errno=%d", path, errno);
    free(line);
    fclose(devfile);
    return -1;
  }

  int tl = strlen(line);
  if (tl > 0) {
    if (line[tl - 1] == '\n') line[tl - 1] = 0;
    entry->check = mustcheck(line);
  }

  free(line);
  fclose(devfile);
  return 0;
}

static char *getdescription(struct serialhandle *handle, const char *tty)
{
  char path[PATH_MAX];
  int len = snprintf(path, PATH_MAX, "%s/%s/device/uevent", sysclasstty, tty);
  if (len >= PATH_MAX) {
    nslog(handle, NSLOG_WARNING, "getports: path truncated: %s/%s/dev/uevent", sysclasstty, tty);
    return NULL;
  }

  FILE *ueventfile = fopen(path, "r");
  if (!ueventfile) {
    nslog(handle, NSLOG_WARNING, "getports: path %s can't be opened: errno=%d", path, errno);
    return NULL;
  }

  char *description = NULL;
  char *line = NULL;
  size_t linelen = 0;
  ssize_t read = 0;
  do {
    read = getline(&line, &linelen, ueventfile);
    if (read >= 0) {
      char *keyword = "DRIVER=";
      char *fline = strstr(line, keyword);
      if (fline == line) {
	if (line[read-1] == '\n') line[read-1] = 0;
	description = strnappend(handle->portbuffer, &handle->portbuffoffset, PORTBUFLEN, line + strlen(keyword));
      }
    }
  } while (read >= 0 && description == NULL);
  free(line);
  fclose(ueventfile);
  return description;
}

static int parsedevtree(struct serialhandle *handle, const char *basedir, struct portentry *entries, int nentries)
{
  int found = 0;
  DIR *devdir = opendir(basedir);
  if (devdir == NULL) {
    return 0;
  }

  struct dirent *entry;
  do {
    entry = readdir(devdir);
    if (entry) {
      char path[PATH_MAX];
      int len = snprintf(path, PATH_MAX, "%s/%s", basedir, entry->d_name);
      if (len >= PATH_MAX) {
        nslog(handle, NSLOG_WARNING, "getports: path truncated: %s/%s\n", basedir, entry->d_name);
        continue;
      }

      // Check this device is known by the kernel
      struct stat sb;
      if (lstat(path, &sb) < 0) {
        nslog(handle, NSLOG_WARNING, "getports: couldn't lstat: %s (errno=%d)", path, errno);
        continue;
      }

      if ((sb.st_mode & S_IFMT) == S_IFCHR) {
        int major = major(sb.st_rdev);
        int minor = minor(sb.st_rdev);

        int i;
        for (i = 0; i < nentries; i++) {
          if (entries[i].major == major && entries[i].minor == minor) {
            break;
          }
        }
        if (i == nentries) continue;
        if (entries[i].found) continue;

        // Check user has permissions before adding
        if (access(path, R_OK | W_OK)) {
          nslog(handle, NSLOG_WARNING, "getports: file not accessible: %s (errno=%d)", path, errno);
          continue;
        }

        // Check the serial port if not unknown for special cases
        if (entries[i].check) {
          int fd = open(path, O_RDONLY | O_NOCTTY);
          if (fd == -1) {
            nslog(handle, NSLOG_WARNING, "getports: couldn't open: %s (errno=%d)", path, errno);
            continue;
          }
          struct serial_struct serinfo;
          if (ioctl(fd, TIOCGSERIAL, &serinfo)) {
            close(fd);
            nslog(handle, NSLOG_WARNING, "getports: couldn't ioctl TIOCGSERIAL: %s (errno=%d)", path, errno);
            continue;
          }

          entries[i].found = TRUE;
          if (serinfo.type == PORT_UNKNOWN) {
            close(fd);
            nslog(handle, NSLOG_DEBUG, "getports: port unknown: %s", path);
            continue;
          }
          close(fd);
        }

        // Add to the list of known ports. it's OK!
	handle->ports[found].device = strnappend(handle->portbuffer, &handle->portbuffoffset, PORTBUFLEN, path);
	handle->ports[found].description = getdescription(handle, entries[i].tty);
	found++;
      } else if ((sb.st_mode & S_IFMT) == S_IFDIR) {
        if (strcmp(".", entry->d_name) == 0) continue;
        if (strcmp("..", entry->d_name) == 0) continue;
        found += parsedevtree(handle, path, entries, nentries);
      }
    }
  } while (entry);

  closedir(devdir);
  return found;
}

// MAXPORTS entries of each TTY maximum 64 bytes.
#define TTYSIZE (MAXINTPORTS * (sizeof(char*)+64))

/*! \brief search for all ports
 *
 * Search through /sys/class/tty for all ports
 *
 * \param handle as given by serial_init(), used for logging
 * \param ports a preallocated array to store the ports
 * \param buffer a preallocated array for storing the device and description
 * \param buflen the size allocated to buffer
 * \returns the number of ports found. Returns -1 if there was an error.
 */
static int findports(struct serialhandle *handle)
{
  char *ttybuff;
  size_t ttyoffset = 0;
  struct portentry foundports[MAXINTPORTS+1];

  // As we iterate over /sys/class/tty/*, we remember the directory, so
  // we can query device information later.
  ttybuff = malloc(TTYSIZE);
  if (ttybuff == NULL) {
    nslog(handle, NSLOG_ERR, "getports: Out of memory: errno=%d", errno);
    return -1;
  }

  DIR *sysdir = opendir(sysclasstty);
  if (sysdir == NULL) {
    free(ttybuff);
    nslog(handle, NSLOG_ERR, "getports: Can't open %s: errno=%d", sysclasstty, errno);
    return -1;
  }

  // Iterate through all directories, looking for those with a subdirectory
  // 'device/driver'.
  struct dirent *entry;
  int entries = 0;
  do {
    errno = 0;
    entry = readdir(sysdir);
    if (entry) {
      char path[PATH_MAX];
      int len = snprintf(path, PATH_MAX, "%s/%s", sysclasstty, entry->d_name);
      if (len >= PATH_MAX) {
        nslog(handle, NSLOG_WARNING, "getports: path truncated: %s\n", path);
        continue;
      }

      foundports[entries].found = FALSE;
      foundports[entries].check = FALSE;
      if (!isreal(handle, path)) continue;
      if (getdevicenode(handle, path, foundports + entries)) continue;
      getdevicemodalias(handle, path, foundports + entries);

      // This is a valid port, remember the TTY.
      foundports[entries].tty = strnappend(ttybuff, &ttyoffset, TTYSIZE, entry->d_name);
      entries++;
    } else {
      if (errno) {
        nslog(handle, NSLOG_WARNING, "getports: readdir error: errno=%d", errno);
      }
    }
  } while (entry);
  closedir(sysdir);

  entries = parsedevtree(handle, devtree, foundports, entries);

  free(ttybuff);
  return entries;
}

static void sortports(struct serialhandle *handle, int ports)
{
  int i;
  int swapped;
  const void *tmp;
  if (ports <= 1) return;

  do {
    for (i = 1; i < ports; i++) {
      swapped = FALSE;
      if (strcmp(handle->ports[i-1].device, handle->ports[i].device) > 0) {
	tmp = handle->ports[i-1].device;
	handle->ports[i-1].device = handle->ports[i].device;
	handle->ports[i].device = tmp;

	tmp = handle->ports[i-1].description;
	handle->ports[i-1].description = handle->ports[i].description;
	handle->ports[i].description = tmp;
	swapped = TRUE;
      }
    }
  } while(swapped);
}

NSERIAL_EXPORT struct portdescription *WINAPI serial_getports(struct serialhandle *handle)
{
  int ports;

  if (handle == NULL) {
    errno = EINVAL;
    return NULL;
  }

  if (!handle->ports) {
    handle->ports = malloc((MAXPORTS+1) * sizeof(struct portdescription));
    if (!handle->ports) {
      serial_seterror(handle, ERRMSG_OUTOFMEMORY);
      return NULL;
    }
  }
  if (!handle->portbuffer) {
    handle->portbuffer = malloc(PORTBUFLEN);
    if (!handle->portbuffer) {
      free(handle->ports);
      serial_seterror(handle, ERRMSG_OUTOFMEMORY);
      return NULL;
    }
  }

  ports = findports(handle);
  handle->ports[ports].device = NULL;
  handle->ports[ports].description = NULL;
  sortports(handle, ports);
  return handle->ports;
}
