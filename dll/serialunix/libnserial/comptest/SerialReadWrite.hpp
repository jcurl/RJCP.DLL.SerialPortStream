#ifndef SERIALREADWRITE_HPP
#define SERIALREADWRITE_HPP

#include "nserial.h"
#include "Buffer.hpp"

class SerialReadWrite
{
public:
  SerialReadWrite(struct serialhandle *writehandle, struct serialhandle *readhandle, size_t readbufsize);
  ~SerialReadWrite();

  Buffer *GetReceiveBuffer();
  int      DoTransfer(Buffer *sendBuffer);

private:
  struct serialhandle *m_readhandle;
  struct serialhandle *m_writehandle;
  Buffer              *m_readbuff;
  Buffer              *m_writebuff;
};

#endif
