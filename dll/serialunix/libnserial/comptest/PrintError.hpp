#ifndef PRINTERROR_HPP
#define PRINTERROR_HPP

#include <string.h>
#include <errno.h>
#include "nserial.h"

class PrintError
{
public:
  PrintError(struct serialhandle *handle) : m_handle(handle) { }

  friend std::ostream &operator << (std::ostream &os, const PrintError &oe)
  {
    os << "; Device: " << serial_getdevicename(oe.m_handle) <<
      "; Error: " << strerror(errno) << " (" << errno << ")\n" <<
      "Extended error: " << serial_error(oe.m_handle) << "\n";
    return os;
  }
private:
  struct serialhandle *m_handle;
};

#endif
