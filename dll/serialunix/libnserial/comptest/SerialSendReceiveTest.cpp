#include <iostream>
#include <memory>
#include <stdlib.h>
#include <errno.h>
#include <pthread.h>
#include <semaphore.h>
#include <time.h>
#include "gtest/gtest.h"
#include "main.hpp"
#include "SerialConfiguration.hpp"
#include "Buffer.hpp"
#include "BuffDump.hpp"
#include "PrintError.hpp"
#include "SerialReadWrite.hpp"
#include "nserial.h"

class SerialSendReceiveTest : public ::testing::Test
{
protected:
  SerialSendReceiveTest();
  virtual ~SerialSendReceiveTest();

  virtual void SetUp();
  virtual void TearDown();

public:
  struct serialhandle *m_readhandle;
  struct serialhandle *m_writehandle;
};


SerialSendReceiveTest::SerialSendReceiveTest() : ::testing::Test()
{
}

SerialSendReceiveTest::~SerialSendReceiveTest()
{
}

void SerialSendReceiveTest::SetUp()
{
  m_writehandle = serial_init();
  ASSERT_TRUE(m_writehandle != NULL)
    << "Error initialising writehandle: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_setdevicename(m_writehandle, serialconfig->GetWriteDevice()))
    << "Error setting write device: " << strerror(errno) << " (" << errno << ")";
  m_readhandle = serial_init();
  ASSERT_TRUE(m_readhandle != NULL)
    << "Error initialising readhandle: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_setdevicename(m_readhandle, serialconfig->GetReadDevice()))
    << "Error setting write device: " << strerror(errno) << " (" << errno << ")";
}

void SerialSendReceiveTest::TearDown()
{
  serial_terminate(m_writehandle);
  serial_terminate(m_readhandle);
}

TEST(SerialBasicTest, Version)
{
  const char *version;
  version = ::serial_version();
  EXPECT_EQ(0, strcmp("1.0.0", version));
}

// Open the two serial ports using 115200,8,n,1, send data from the first and
// receive data in the second. Check the content of the data matches.
//
// This is the first test case to run. If you run the test program twice, so
// that the test case SendReceiveFinalTestCase is the last one to run, this
// test case may fail. See the comments in SerialReadWrite.cpp for a potential
// workaround.
TEST_F(SerialSendReceiveTest, SendReceiveSmallBuffer)
{
  EXPECT_NE(-1, serial_setbaud(m_writehandle, 115200)) << "serial_setbaud" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdatabits(m_writehandle, 8)) << "serial_setdatabits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparity(m_writehandle, NOPARITY)) << "serial_setparity" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setstopbits(m_writehandle, ONE)) << "serial_setstopbits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_sethandshake(m_writehandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_writehandle, false)) << "serial_setdiscardnull" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparityreplace(m_writehandle, 0)) << "serial_setparityreplace" << PrintError(m_writehandle);
  ASSERT_NE(-1, serial_open(m_writehandle)) << "serial_open" << PrintError(m_writehandle);
  ASSERT_NE(-1, serial_setproperties(m_writehandle)) << "serial_setproperties" << PrintError(m_writehandle);

  EXPECT_NE(-1, serial_setbaud(m_readhandle, 115200)) << "serial_setbaud" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdatabits(m_readhandle, 8)) << "serial_setdatabits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparity(m_readhandle, NOPARITY)) << "serial_setparity" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setstopbits(m_readhandle, ONE)) << "serial_setstopbits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_sethandshake(m_readhandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_readhandle, false)) << "serial_setdiscardnull" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparityreplace(m_readhandle, 0)) << "serial_setparityreplace" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_open(m_readhandle)) << "serial_open" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_setproperties(m_readhandle)) << "serial_setproperties" << PrintError(m_readhandle);

  // Define the two buffers and the data set to send/receive
  std::auto_ptr<Buffer> sendbuff(new Buffer(1024));
  char *buffs = sendbuff->GetBuffer();
  for (int i = 0; i < sendbuff->GetCapacity(); i++) {
    buffs[i] = i % 256;
  }
  sendbuff->Produce(sendbuff->GetCapacity());

  std::auto_ptr<SerialReadWrite> readwrite(new SerialReadWrite(m_writehandle, m_readhandle, 1024));
  char *buffr = readwrite->GetReceiveBuffer()->GetBuffer();
  readwrite->DoTransfer(sendbuff.get());

  EXPECT_EQ(sendbuff->GetCapacity(), readwrite->GetReceiveBuffer()->GetLength())
    << "Didn't receive expected number of bytes";

  bool comparison = true;
  for (int i = 0; comparison && i < sendbuff->GetCapacity(); i++) {
    if (buffs[i] != buffr[i]) comparison = false;
  }
  EXPECT_TRUE(comparison)
    << "Send and Receive buffers don't match after transfer";

  if (!comparison) {
    DumpBuffer(buffs, sendbuff->GetCapacity(), "SendReceiveSmallBuffer.out.bin");
    DumpBuffer(buffr, readwrite->GetReceiveBuffer()->GetLength(), "SendReceiveSmallBuffer.in.bin");
  }
}

TEST_F(SerialSendReceiveTest, SendReceiveLargeBuffer)
{
  ASSERT_NE(-1, serial_open(m_writehandle)) << "serial_open" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setbaud(m_writehandle, 115200)) << "serial_setbaud" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdatabits(m_writehandle, 8)) << "serial_setdatabits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparity(m_writehandle, NOPARITY)) << "serial_setparity" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setstopbits(m_writehandle, ONE)) << "serial_setstopbits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_sethandshake(m_writehandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_writehandle, false)) << "serial_setdiscardnull" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparityreplace(m_writehandle, 0)) << "serial_setparityreplace" << PrintError(m_writehandle);
  ASSERT_NE(-1, serial_setproperties(m_writehandle)) << "serial_setproperties" << PrintError(m_writehandle);

  ASSERT_NE(-1, serial_open(m_readhandle)) << "serial_open" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setbaud(m_readhandle, 115200)) << "serial_setbaud" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdatabits(m_readhandle, 8)) << "serial_setdatabits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparity(m_readhandle, NOPARITY)) << "serial_setparity" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setstopbits(m_readhandle, ONE)) << "serial_setstopbits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_sethandshake(m_readhandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_readhandle, false)) << "serial_setdiscardnull" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparityreplace(m_readhandle, 0)) << "serial_setparityreplace" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_setproperties(m_readhandle)) << "serial_setproperties" << PrintError(m_readhandle);

  // Define the two buffers and the data set to send/receive
  std::auto_ptr<Buffer> sendbuff(new Buffer(256000));
  char *buffs = sendbuff->GetBuffer();
  for (int i = 0; i < sendbuff->GetCapacity(); i++) {
    buffs[i] = i % 256;
  }
  sendbuff->Produce(sendbuff->GetCapacity());

  std::auto_ptr<SerialReadWrite> readwrite(new SerialReadWrite(m_writehandle, m_readhandle, 256000));
  char *buffr = readwrite->GetReceiveBuffer()->GetBuffer();
  readwrite->DoTransfer(sendbuff.get());

  EXPECT_EQ(sendbuff->GetCapacity(), readwrite->GetReceiveBuffer()->GetLength())
    << "Didn't receive expected number of bytes";

  bool comparison = true;
  for (int i = 0; comparison && i < sendbuff->GetCapacity(); i++) {
    if (buffs[i] != buffr[i]) comparison = false;
  }
  EXPECT_TRUE(comparison)
    << "Send and Receive buffers don't match after transfer";

  if (!comparison) {
    DumpBuffer(buffs, sendbuff->GetCapacity(), "SendReceiveLargeBuffer.out.bin");
    DumpBuffer(buffr, readwrite->GetReceiveBuffer()->GetLength(), "SendReceiveLargeBuffer.in.bin");
  }
}

TEST_F(SerialSendReceiveTest, SendReceiveOpenCloseOpenClose)
{
  ASSERT_NE(-1, serial_open(m_writehandle)) << "serial_open" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setbaud(m_writehandle, 115200)) << "serial_setbaud" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdatabits(m_writehandle, 8)) << "serial_setdatabits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparity(m_writehandle, NOPARITY)) << "serial_setparity" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setstopbits(m_writehandle, ONE)) << "serial_setstopbits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_sethandshake(m_writehandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_writehandle, false)) << "serial_setdiscardnull" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparityreplace(m_writehandle, 0)) << "serial_setparityreplace" << PrintError(m_writehandle);
  ASSERT_NE(-1, serial_setproperties(m_writehandle)) << "serial_setproperties" << PrintError(m_writehandle);

  ASSERT_NE(-1, serial_open(m_readhandle)) << "serial_open" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setbaud(m_readhandle, 115200)) << "serial_setbaud" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdatabits(m_readhandle, 8)) << "serial_setdatabits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparity(m_readhandle, NOPARITY)) << "serial_setparity" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setstopbits(m_readhandle, ONE)) << "serial_setstopbits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_sethandshake(m_readhandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_readhandle, false)) << "serial_setdiscardnull" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparityreplace(m_readhandle, 0)) << "serial_setparityreplace" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_setproperties(m_readhandle)) << "serial_setproperties" << PrintError(m_readhandle);

  // Define the two buffers and the data set to send/receive
  std::auto_ptr<Buffer> sendbuff(new Buffer(8192));
  char *buffs = sendbuff->GetBuffer();
  for (int i = 0; i < sendbuff->GetCapacity(); i++) {
    buffs[i] = i % 256;
  }
  sendbuff->Produce(sendbuff->GetCapacity());

  std::auto_ptr<SerialReadWrite> readwrite(new SerialReadWrite(m_writehandle, m_readhandle, 1024));
  char *buffr = readwrite->GetReceiveBuffer()->GetBuffer();
  readwrite->DoTransfer(sendbuff.get());

  ASSERT_NE(-1, serial_close(m_writehandle));
  ASSERT_NE(-1, serial_close(m_readhandle));

  ASSERT_NE(-1, serial_open(m_writehandle)) << "serial_open" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setbaud(m_writehandle, 115200)) << "serial_setbaud" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdatabits(m_writehandle, 8)) << "serial_setdatabits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparity(m_writehandle, NOPARITY)) << "serial_setparity" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setstopbits(m_writehandle, ONE)) << "serial_setstopbits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_sethandshake(m_writehandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_writehandle, false)) << "serial_setdiscardnull" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparityreplace(m_writehandle, 0)) << "serial_setparityreplace" << PrintError(m_writehandle);
  ASSERT_NE(-1, serial_setproperties(m_writehandle)) << "serial_setproperties" << PrintError(m_writehandle);

  ASSERT_NE(-1, serial_open(m_readhandle)) << "serial_open" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setbaud(m_readhandle, 115200)) << "serial_setbaud" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdatabits(m_readhandle, 8)) << "serial_setdatabits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparity(m_readhandle, NOPARITY)) << "serial_setparity" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setstopbits(m_readhandle, ONE)) << "serial_setstopbits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_sethandshake(m_readhandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_readhandle, false)) << "serial_setdiscardnull" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparityreplace(m_readhandle, 0)) << "serial_setparityreplace" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_setproperties(m_readhandle)) << "serial_setproperties" << PrintError(m_readhandle);

  sendbuff->Reset();
  readwrite->GetReceiveBuffer()->Reset();
  for (int i = 0; i < 1024; i++) {
    buffs[i] = (i + 7) % 256;
  }
  sendbuff->Produce(1024);

  readwrite->DoTransfer(sendbuff.get());

  EXPECT_EQ(1024, readwrite->GetReceiveBuffer()->GetLength())
    << "Didn't receive expected number of bytes";

  bool comparison = true;
  for (int i = 0; comparison && i < 1024; i++) {
    if (buffs[i] != buffr[i]) comparison = false;
  }
  EXPECT_TRUE(comparison)
    << "Send and Receive buffers don't match after transfer";
}

TEST_F(SerialSendReceiveTest, DiscardNullFirstBytes)
{
  EXPECT_NE(-1, serial_setbaud(m_writehandle, 115200)) << "serial_setbaud" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdatabits(m_writehandle, 8)) << "serial_setdatabits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparity(m_writehandle, NOPARITY)) << "serial_setparity" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setstopbits(m_writehandle, ONE)) << "serial_setstopbits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_sethandshake(m_writehandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_writehandle, false)) << "serial_setdiscardnull" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparityreplace(m_writehandle, 0)) << "serial_setparityreplace" << PrintError(m_writehandle);
  ASSERT_NE(-1, serial_open(m_writehandle)) << "serial_open" << PrintError(m_writehandle);
  ASSERT_NE(-1, serial_setproperties(m_writehandle)) << "serial_setproperties" << PrintError(m_writehandle);

  EXPECT_NE(-1, serial_setbaud(m_readhandle, 115200)) << "serial_setbaud" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdatabits(m_readhandle, 8)) << "serial_setdatabits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparity(m_readhandle, NOPARITY)) << "serial_setparity" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setstopbits(m_readhandle, ONE)) << "serial_setstopbits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_sethandshake(m_readhandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_readhandle, true)) << "serial_setdiscardnull" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparityreplace(m_readhandle, 0)) << "serial_setparityreplace" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_open(m_readhandle)) << "serial_open" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_setproperties(m_readhandle)) << "serial_setproperties" << PrintError(m_readhandle);

  std::auto_ptr<Buffer> sendbuff(new Buffer(256));
  char *buffs = sendbuff->GetBuffer();
  std::auto_ptr<SerialReadWrite> readwrite(new SerialReadWrite(m_writehandle, m_readhandle, 1024));
  char *buffr = readwrite->GetReceiveBuffer()->GetBuffer();

  // First byte contains NUL that should be discarded
  for (int i = 0; i < 256; i++) {
    buffs[i] = i % 256;
  }
  sendbuff->Produce(256);
  readwrite->DoTransfer(sendbuff.get());

  EXPECT_EQ(255, readwrite->GetReceiveBuffer()->GetLength())
    << "Didn't receive expected number of bytes";

  bool comparison = true;
  for (int i = 0; comparison && i < 254; i++) {
    if (static_cast<unsigned char>(buffr[i]) != i+1) comparison = false;
  }
  EXPECT_TRUE(comparison)
    << "Send and Receive buffers don't match after transfer";

  if (!comparison) {
    DumpBuffer(buffs, sendbuff->GetCapacity(), "DiscardNullFirstByte.out.bin");
    DumpBuffer(buffr, readwrite->GetReceiveBuffer()->GetLength(), "DiscardNullFirstByte.in.bin");
  }
}

TEST_F(SerialSendReceiveTest, DiscardNullEverySecondBytes)
{
  EXPECT_NE(-1, serial_setbaud(m_writehandle, 115200)) << "serial_setbaud" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdatabits(m_writehandle, 8)) << "serial_setdatabits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparity(m_writehandle, NOPARITY)) << "serial_setparity" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setstopbits(m_writehandle, ONE)) << "serial_setstopbits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_sethandshake(m_writehandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_writehandle, false)) << "serial_setdiscardnull" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparityreplace(m_writehandle, 0)) << "serial_setparityreplace" << PrintError(m_writehandle);
  ASSERT_NE(-1, serial_open(m_writehandle)) << "serial_open" << PrintError(m_writehandle);
  ASSERT_NE(-1, serial_setproperties(m_writehandle)) << "serial_setproperties" << PrintError(m_writehandle);

  EXPECT_NE(-1, serial_setbaud(m_readhandle, 115200)) << "serial_setbaud" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdatabits(m_readhandle, 8)) << "serial_setdatabits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparity(m_readhandle, NOPARITY)) << "serial_setparity" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setstopbits(m_readhandle, ONE)) << "serial_setstopbits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_sethandshake(m_readhandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_readhandle, true)) << "serial_setdiscardnull" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparityreplace(m_readhandle, 0)) << "serial_setparityreplace" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_open(m_readhandle)) << "serial_open" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_setproperties(m_readhandle)) << "serial_setproperties" << PrintError(m_readhandle);

  std::auto_ptr<Buffer> sendbuff(new Buffer(256));
  char *buffs = sendbuff->GetBuffer();
  std::auto_ptr<SerialReadWrite> readwrite(new SerialReadWrite(m_writehandle, m_readhandle, 1024));
  char *buffr = readwrite->GetReceiveBuffer()->GetBuffer();

  // First byte contains NUL that should be discarded
  for (int i = 0; i < 128; i++) {
    buffs[i * 2] = i;
    buffs[i * 2 + 1] = 0;
  }
  sendbuff->Produce(256);
  readwrite->DoTransfer(sendbuff.get());

  EXPECT_EQ(127, readwrite->GetReceiveBuffer()->GetLength())
    << "Didn't receive expected number of bytes";

  bool comparison = true;
  for (int i = 0; comparison && i < 127; i++) {
    if (static_cast<unsigned char>(buffr[i]) != i + 1) comparison = false;
  }
  EXPECT_TRUE(comparison)
    << "Send and Receive buffers don't match after transfer";

  if (!comparison) {
    DumpBuffer(buffs, sendbuff->GetCapacity(), "DiscardNullEverySecondByte.out.bin");
    DumpBuffer(buffr, readwrite->GetReceiveBuffer()->GetLength(), "DiscardNullEverySecondByte.in.bin");
  }
}

TEST_F(SerialSendReceiveTest, FlushInputOutput)
{
  EXPECT_NE(-1, serial_setbaud(m_writehandle, 115200)) << "serial_setbaud" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdatabits(m_writehandle, 8)) << "serial_setdatabits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparity(m_writehandle, NOPARITY)) << "serial_setparity" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setstopbits(m_writehandle, ONE)) << "serial_setstopbits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_sethandshake(m_writehandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_writehandle, false)) << "serial_setdiscardnull" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparityreplace(m_writehandle, 0)) << "serial_setparityreplace" << PrintError(m_writehandle);
  ASSERT_NE(-1, serial_open(m_writehandle)) << "serial_open" << PrintError(m_writehandle);
  ASSERT_NE(-1, serial_setproperties(m_writehandle)) << "serial_setproperties" << PrintError(m_writehandle);

  EXPECT_EQ(0, serial_discardinbuffer(m_writehandle));
  EXPECT_EQ(0, serial_discardoutbuffer(m_writehandle));
}

struct waitforabortstruct {
  struct serialhandle *readhandle;
  sem_t exitsema;
  int result;
};

void *waitforaborttest(void *ptr)
{
  struct waitforabortstruct *test = (struct waitforabortstruct *)ptr;
  int result = serial_waitforevent(test->readhandle, READEVENT, -1);
  test->result = result;
  sem_post(&(test->exitsema));
  pthread_exit(NULL);
}

TEST_F(SerialSendReceiveTest, WaitForEventsAbort)
{
  EXPECT_NE(-1, serial_setbaud(m_writehandle, 115200)) << "serial_setbaud" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdatabits(m_writehandle, 8)) << "serial_setdatabits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparity(m_writehandle, NOPARITY)) << "serial_setparity" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setstopbits(m_writehandle, ONE)) << "serial_setstopbits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_sethandshake(m_writehandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_writehandle, false)) << "serial_setdiscardnull" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparityreplace(m_writehandle, 0)) << "serial_setparityreplace" << PrintError(m_writehandle);
  ASSERT_NE(-1, serial_open(m_writehandle)) << "serial_open" << PrintError(m_writehandle);
  ASSERT_NE(-1, serial_setproperties(m_writehandle)) << "serial_setproperties" << PrintError(m_writehandle);

  EXPECT_NE(-1, serial_setbaud(m_readhandle, 115200)) << "serial_setbaud" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdatabits(m_readhandle, 8)) << "serial_setdatabits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparity(m_readhandle, NOPARITY)) << "serial_setparity" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setstopbits(m_readhandle, ONE)) << "serial_setstopbits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_sethandshake(m_readhandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_readhandle, true)) << "serial_setdiscardnull" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparityreplace(m_readhandle, 0)) << "serial_setparityreplace" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_open(m_readhandle)) << "serial_open" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_setproperties(m_readhandle)) << "serial_setproperties" << PrintError(m_readhandle);

  struct waitforabortstruct test = {0, };
  test.readhandle = m_readhandle;
  test.result = 255;
  ASSERT_NE(-1, sem_init(&(test.exitsema), 0, 0));

  pthread_t waitforabortthread;
  ASSERT_NE(-1, pthread_create(&waitforabortthread, NULL, waitforaborttest, (void*)&test));

  struct timespec timeout;
  ASSERT_NE(-1, clock_gettime(CLOCK_REALTIME, &timeout));
  timeout.tv_sec += 4;

  ASSERT_EQ(-1, sem_timedwait(&(test.exitsema), &timeout));
  ASSERT_EQ(ETIMEDOUT, errno);

  // Now signal to abort the wait event
  ASSERT_NE(-1, serial_abortwaitforevent(m_readhandle));

  ASSERT_NE(-1, clock_gettime(CLOCK_REALTIME, &timeout));
  timeout.tv_sec += 4;
  ASSERT_EQ(0, sem_timedwait(&(test.exitsema), &timeout));

  EXPECT_NE(-1, pthread_join(waitforabortthread, NULL));
  ASSERT_EQ(0, test.result);
}

TEST_F(SerialSendReceiveTest, WaitForEventsAbortCleared)
{
  EXPECT_NE(-1, serial_setbaud(m_writehandle, 115200)) << "serial_setbaud" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdatabits(m_writehandle, 8)) << "serial_setdatabits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparity(m_writehandle, NOPARITY)) << "serial_setparity" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setstopbits(m_writehandle, ONE)) << "serial_setstopbits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_sethandshake(m_writehandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_writehandle, false)) << "serial_setdiscardnull" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparityreplace(m_writehandle, 0)) << "serial_setparityreplace" << PrintError(m_writehandle);
  ASSERT_NE(-1, serial_open(m_writehandle)) << "serial_open" << PrintError(m_writehandle);
  ASSERT_NE(-1, serial_setproperties(m_writehandle)) << "serial_setproperties" << PrintError(m_writehandle);

  EXPECT_NE(-1, serial_setbaud(m_readhandle, 115200)) << "serial_setbaud" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdatabits(m_readhandle, 8)) << "serial_setdatabits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparity(m_readhandle, NOPARITY)) << "serial_setparity" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setstopbits(m_readhandle, ONE)) << "serial_setstopbits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_sethandshake(m_readhandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_readhandle, true)) << "serial_setdiscardnull" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparityreplace(m_readhandle, 0)) << "serial_setparityreplace" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_open(m_readhandle)) << "serial_open" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_setproperties(m_readhandle)) << "serial_setproperties" << PrintError(m_readhandle);

  struct waitforabortstruct test = {0, };
  test.readhandle = m_readhandle;
  test.result = 255;
  ASSERT_NE(-1, sem_init(&(test.exitsema), 0, 0));

  // We send an abort, before it actually starts. It should abort immediately.
  ASSERT_NE(-1, serial_abortwaitforevent(m_readhandle));

  pthread_t waitforabortthread;
  ASSERT_NE(-1, pthread_create(&waitforabortthread, NULL, waitforaborttest, (void*)&test));

  struct timespec timeout;
  ASSERT_NE(-1, clock_gettime(CLOCK_REALTIME, &timeout));
  timeout.tv_sec += 4;

  // The semaphore should be triggered as when the thread started, it was immediately
  // aborted.
  ASSERT_EQ(0, sem_timedwait(&(test.exitsema), &timeout));

  EXPECT_NE(-1, pthread_join(waitforabortthread, NULL));
  ASSERT_EQ(0, test.result);
}
