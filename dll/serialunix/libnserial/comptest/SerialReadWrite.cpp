#include <stdexcept>
#include <iostream>
#include <unistd.h>
#include "SerialReadWrite.hpp"
#include "Buffer.hpp"
#include "PrintError.hpp"

SerialReadWrite::SerialReadWrite(struct serialhandle *writehandle, struct serialhandle *readhandle,
                                 size_t readbufsize) :
  m_readhandle(readhandle),
  m_writehandle(writehandle)
{
  if (readhandle == NULL || writehandle == NULL) {
    throw std::invalid_argument("readhandle or writehandle may not be null");
  }
  if (readbufsize <= 0) {
    throw std::invalid_argument("readbufsize must be more than 0 bytes");
  }

  m_readbuff = new Buffer(readbufsize);
}

SerialReadWrite::~SerialReadWrite()
{
  if (m_readbuff != NULL) delete m_readbuff;
}

Buffer *SerialReadWrite::GetReceiveBuffer()
{
  return m_readbuff;
}

int SerialReadWrite::DoTransfer(Buffer *sendBuffer)
{
  if (sendBuffer == NULL) {
    throw std::invalid_argument("sendBuffer may not be NULL");
  }

  m_writebuff = sendBuffer;

  char *rbuff = m_readbuff->GetBuffer();
  char *wbuff = m_writebuff->GetBuffer();

  int error = 0;

  bool writefinished;
  bool readfinished;
  serialevent_t event;

  // Read all data from the serial port.
  writefinished = false;
  readfinished = false;
  char tmpbuff[256];

  serial_reset(m_writehandle);
  serial_reset(m_readhandle);

  // Do the transfer. Normally, each serial port would run on it's own thread,
  // or you would use your own select routine for managing both serial ports
  // simultaneously. The libnserial implementation doesn't support waiting for
  // events from multiple serial ports or other FD's.
  writefinished = false;
  readfinished = false;
  while (!(readfinished)) {
    if (!writefinished) {
      event = serial_waitforevent(m_writehandle, WRITEEVENT, 0);

      if (event == WRITEEVENT) {
	int writelength = m_writebuff->GetReadLength();
	if (writelength > 1024) writelength = 1024;
        int writebytes = serial_write(m_writehandle,
                                      m_writebuff->GetStartBuffer(),
                                      writelength);
        //std::cout << writebytes << " = write(offset=" <<
        //  m_writebuff->GetStartOffset() <<
        //  ", length " << m_writebuff->GetReadLength() << ")\n";
        if (writebytes == 0 || writebytes < 0) {
          error = -1;
          std::cout << "Error writing bytes: " << writebytes <<
            PrintError(m_writehandle);
          writefinished = true;
        } else {
          m_writebuff->Consume(writebytes);
        }
      }
    }

    event = serial_waitforevent(m_readhandle, READEVENT, 500);
    if (event == READEVENT) {
      int readbytes = serial_read(m_readhandle,
                                  m_readbuff->GetEndBuffer(),
                                  m_readbuff->GetWriteLength());
      //std::cout << readbytes << " = read(offset=" <<
      //  m_readbuff->GetEndOffset() <<
      //  ", length=" << m_readbuff->GetWriteLength() << ")\n";
      if (readbytes == 0) {
        // Just try again later.
      } else if (readbytes < 0) {
        error = -1;
        std::cout << "Error reading bytes: " << readbytes <<
          PrintError(m_readhandle);
        readfinished = true;
      } else {
        m_readbuff->Produce(readbytes);
      }
    } else {
      readfinished = true;
    }

    if (!writefinished && m_writebuff->GetReadLength() == 0)
      writefinished = true;
    if (!readfinished && m_readbuff->GetWriteLength() == 0)
      readfinished = true;
  }

  return error;
}
