#include <iostream>
#include <memory>
#include <stdlib.h>
#include <errno.h>
#include "gtest/gtest.h"
#include "main.hpp"
#include "SerialConfiguration.hpp"
#include "Buffer.hpp"
#include "BuffDump.hpp"
#include "PrintError.hpp"
#include "SerialReadWrite.hpp"
#include "nserial.h"

class SerialParityTest : public ::testing::Test
{
protected:
  SerialParityTest();
  virtual ~SerialParityTest();

  virtual void SetUp();
  virtual void TearDown();

protected:
  struct serialhandle *m_readhandle;
  struct serialhandle *m_writehandle;
};

SerialParityTest::SerialParityTest() : ::testing::Test()
{
}

SerialParityTest::~SerialParityTest()
{
}

void SerialParityTest::SetUp()
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

void SerialParityTest::TearDown()
{
  serial_terminate(m_writehandle);
  serial_terminate(m_readhandle);
}

static char parityeven[128] = {
  0x00, 0x81, 0x82, 0x03, 0x84, 0x05, 0x06, 0x87, 0x88, 0x09, 0x0A, 0x8B, 0x0C, 0x8D, 0x8E, 0x0F,
  0x90, 0x11, 0x12, 0x93, 0x14, 0x95, 0x96, 0x17, 0x18, 0x99, 0x9A, 0x1B, 0x9C, 0x1D, 0x1E, 0x9F,
  0xA0, 0x21, 0x22, 0xA3, 0x24, 0xA5, 0xA6, 0x27, 0x28, 0xA9, 0xAA, 0x2B, 0xAC, 0x2D, 0x2E, 0xAF,
  0x30, 0xB1, 0xB2, 0x33, 0xB4, 0x35, 0x36, 0xB7, 0xB8, 0x39, 0x3A, 0xBB, 0x3C, 0xBD, 0xBE, 0x3F,
  0xC0, 0x41, 0x42, 0xC3, 0x44, 0xC5, 0xC6, 0x47, 0x48, 0xC9, 0xCA, 0x4B, 0xCC, 0x4D, 0x4E, 0xCF,
  0x50, 0xD1, 0xD2, 0x53, 0xD4, 0x55, 0x56, 0xD7, 0xD8, 0x59, 0x5A, 0xDB, 0x5C, 0xDD, 0xDE, 0x5F,
  0x60, 0xE1, 0xE2, 0x63, 0xE4, 0x65, 0x66, 0xE7, 0xE8, 0x69, 0x6A, 0xEB, 0x6C, 0xED, 0xEE, 0x6F,
  0xF0, 0x71, 0x72, 0xF3, 0x74, 0xF5, 0xF6, 0x77, 0x78, 0xF9, 0xFA, 0x7B, 0xFC, 0x7D, 0x7E, 0xFF,
};

static char parityodd[128] = {
  0x80, 0x01, 0x02, 0x83, 0x04, 0x85, 0x86, 0x07, 0x08, 0x89, 0x8A, 0x0B, 0x8C, 0x0D, 0x0E, 0x8F,
  0x10, 0x91, 0x92, 0x13, 0x94, 0x15, 0x16, 0x97, 0x98, 0x19, 0x1A, 0x9B, 0x1C, 0x9D, 0x9E, 0x1F,
  0x20, 0xA1, 0xA2, 0x23, 0xA4, 0x25, 0x26, 0xA7, 0xA8, 0x29, 0x2A, 0xAB, 0x2C, 0xAD, 0xAE, 0x2F,
  0xB0, 0x31, 0x32, 0xB3, 0x34, 0xB5, 0xB6, 0x37, 0x38, 0xB9, 0xBA, 0x3B, 0xBC, 0x3D, 0x3E, 0xBF,
  0x40, 0xC1, 0xC2, 0x43, 0xC4, 0x45, 0x46, 0xC7, 0xC8, 0x49, 0x4A, 0xCB, 0x4C, 0xCD, 0xCE, 0x4F,
  0xD0, 0x51, 0x52, 0xD3, 0x54, 0xD5, 0xD6, 0x57, 0x58, 0xD9, 0xDA, 0x5B, 0xDC, 0x5D, 0x5E, 0xDF,
  0xE0, 0x61, 0x62, 0xE3, 0x64, 0xE5, 0xE6, 0x67, 0x68, 0xE9, 0xEA, 0x6B, 0xEC, 0x6D, 0x6E, 0xEF,
  0x70, 0xF1, 0xF2, 0x73, 0xF4, 0x75, 0x76, 0xF7, 0xF8, 0x79, 0x7A, 0xFB, 0x7C, 0xFD, 0xFE, 0x7F,
};

TEST_F(SerialParityTest, Parity7E1Receive)
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
  EXPECT_NE(-1, serial_setdatabits(m_readhandle, 7)) << "serial_setdatabits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparity(m_readhandle, EVEN)) << "serial_setparity" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setstopbits(m_readhandle, ONE)) << "serial_setstopbits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_sethandshake(m_readhandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_readhandle, false)) << "serial_setdiscardnull" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparityreplace(m_readhandle, 0)) << "serial_setparityreplace" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_open(m_readhandle)) << "serial_open" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_setproperties(m_readhandle)) << "serial_setproperties" << PrintError(m_readhandle);

  std::auto_ptr<Buffer> sendbuff(new Buffer(128));
  char *buffs = sendbuff->GetBuffer();
  for (int i = 0; i < sendbuff->GetCapacity(); i++) {
    buffs[i] = parityeven[i];
  }
  sendbuff->Produce(sendbuff->GetCapacity());

  std::auto_ptr<SerialReadWrite> readwrite(new SerialReadWrite(m_writehandle, m_readhandle, 128));
  char *buffr = readwrite->GetReceiveBuffer()->GetBuffer();
  readwrite->DoTransfer(sendbuff.get());

  EXPECT_EQ(sendbuff->GetCapacity(), readwrite->GetReceiveBuffer()->GetLength())
    << "Didn't receive expected number of bytes";

  bool comparison = true;
  for (int i = 0; comparison && i < sendbuff->GetCapacity(); i++) {
    if (buffr[i] != i) comparison = false;
  }
  EXPECT_TRUE(comparison)
    << "Unexpected byte received with Even Parity";

  if (!comparison) {
    DumpBuffer(buffs, sendbuff->GetCapacity(), "Parity7E1Receive.out.bin");
    DumpBuffer(buffr, readwrite->GetReceiveBuffer()->GetLength(), "Parity7E1Receive.in.bin");
  }
}

TEST_F(SerialParityTest, Parity7E1Send)
{
  EXPECT_NE(-1, serial_setbaud(m_writehandle, 115200)) << "serial_setbaud" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdatabits(m_writehandle, 7)) << "serial_setdatabits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparity(m_writehandle, EVEN)) << "serial_setparity" << PrintError(m_writehandle);
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

  std::auto_ptr<Buffer> sendbuff(new Buffer(128));
  char *buffs = sendbuff->GetBuffer();
  for (int i = 0; i < sendbuff->GetCapacity(); i++) {
    buffs[i] = i;
  }
  sendbuff->Produce(sendbuff->GetCapacity());

  std::auto_ptr<SerialReadWrite> readwrite(new SerialReadWrite(m_writehandle, m_readhandle, 128));
  char *buffr = readwrite->GetReceiveBuffer()->GetBuffer();
  readwrite->DoTransfer(sendbuff.get());

  EXPECT_EQ(sendbuff->GetCapacity(), readwrite->GetReceiveBuffer()->GetLength())
    << "Didn't receive expected number of bytes";

  bool comparison = true;
  for (int i = 0; comparison && i < sendbuff->GetCapacity(); i++) {
    if (buffr[i] != parityeven[i]) comparison = false;
  }
  EXPECT_TRUE(comparison)
    << "Unexpected byte received with Even Parity";

  if (!comparison) {
    DumpBuffer(buffs, sendbuff->GetCapacity(), "Parity7E1Send.out.bin");
    DumpBuffer(buffr, readwrite->GetReceiveBuffer()->GetLength(), "Parity7E1Send.in.bin");
  }
}

TEST_F(SerialParityTest, Parity7E1ReceiveError)
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
  EXPECT_NE(-1, serial_setdatabits(m_readhandle, 7)) << "serial_setdatabits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparity(m_readhandle, EVEN)) << "serial_setparity" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setstopbits(m_readhandle, ONE)) << "serial_setstopbits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_sethandshake(m_readhandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_readhandle, false)) << "serial_setdiscardnull" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparityreplace(m_readhandle, 0)) << "serial_setparityreplace" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_open(m_readhandle)) << "serial_open" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_setproperties(m_readhandle)) << "serial_setproperties" << PrintError(m_readhandle);

  std::auto_ptr<Buffer> sendbuff(new Buffer(128));
  char *buffs = sendbuff->GetBuffer();
  for (int i = 0; i < sendbuff->GetCapacity(); i++) {
    buffs[i] = parityeven[i];
    if (i == 0x45) buffs[i] ^= 0x80;
  }
  sendbuff->Produce(sendbuff->GetCapacity());

  std::auto_ptr<SerialReadWrite> readwrite(new SerialReadWrite(m_writehandle, m_readhandle, 256));
  char *buffr = readwrite->GetReceiveBuffer()->GetBuffer();
  readwrite->DoTransfer(sendbuff.get());

  EXPECT_EQ(sendbuff->GetCapacity(), readwrite->GetReceiveBuffer()->GetLength())
    << "Didn't receive expected number of bytes";

  bool comparison = true;
  for (int i = 0; comparison && i < sendbuff->GetCapacity(); i++) {
    if (i == 0x45) {
      if (buffr[i] != 0) comparison = false;
    } else {
      if (buffr[i] != i) comparison = false;
    }
  }
  EXPECT_TRUE(comparison)
    << "Unexpected byte received with Even Parity";

  if (!comparison) {
    DumpBuffer(buffs, sendbuff->GetCapacity(), "Parity7E1ReceiveError.out.bin");
    DumpBuffer(buffr, readwrite->GetReceiveBuffer()->GetLength(), "Parity7E1ReceiveError.in.bin");
  }
}

TEST_F(SerialParityTest, Parity7O1Receive)
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
  EXPECT_NE(-1, serial_setdatabits(m_readhandle, 7)) << "serial_setdatabits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparity(m_readhandle, ODD)) << "serial_setparity" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setstopbits(m_readhandle, ONE)) << "serial_setstopbits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_sethandshake(m_readhandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_readhandle, false)) << "serial_setdiscardnull" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparityreplace(m_readhandle, 0)) << "serial_setparityreplace" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_open(m_readhandle)) << "serial_open" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_setproperties(m_readhandle)) << "serial_setproperties" << PrintError(m_readhandle);

  std::auto_ptr<Buffer> sendbuff(new Buffer(128));
  char *buffs = sendbuff->GetBuffer();
  for (int i = 0; i < sendbuff->GetCapacity(); i++) {
    buffs[i] = parityodd[i];
  }
  sendbuff->Produce(sendbuff->GetCapacity());

  std::auto_ptr<SerialReadWrite> readwrite(new SerialReadWrite(m_writehandle, m_readhandle, 128));
  char *buffr = readwrite->GetReceiveBuffer()->GetBuffer();
  readwrite->DoTransfer(sendbuff.get());

  EXPECT_EQ(sendbuff->GetCapacity(), readwrite->GetReceiveBuffer()->GetLength())
    << "Didn't receive expected number of bytes";

  bool comparison = true;
  for (int i = 0; comparison && i < sendbuff->GetCapacity(); i++) {
    if (buffr[i] != i) comparison = false;
  }
  EXPECT_TRUE(comparison)
    << "Unexpected byte received with Even Parity";

  if (!comparison) {
    DumpBuffer(buffs, sendbuff->GetCapacity(), "Parity7O1Receive.out.bin");
    DumpBuffer(buffr, readwrite->GetReceiveBuffer()->GetLength(), "Parity7O1Receive.in.bin");
  }
}

TEST_F(SerialParityTest, Parity7O1Send)
{
  EXPECT_NE(-1, serial_setbaud(m_writehandle, 115200)) << "serial_setbaud" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setdatabits(m_writehandle, 7)) << "serial_setdatabits" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_setparity(m_writehandle, ODD)) << "serial_setparity" << PrintError(m_writehandle);
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

  std::auto_ptr<Buffer> sendbuff(new Buffer(128));
  char *buffs = sendbuff->GetBuffer();
  for (int i = 0; i < sendbuff->GetCapacity(); i++) {
    buffs[i] = i;
  }
  sendbuff->Produce(sendbuff->GetCapacity());

  std::auto_ptr<SerialReadWrite> readwrite(new SerialReadWrite(m_writehandle, m_readhandle, 128));
  char *buffr = readwrite->GetReceiveBuffer()->GetBuffer();
  readwrite->DoTransfer(sendbuff.get());

  EXPECT_EQ(sendbuff->GetCapacity(), readwrite->GetReceiveBuffer()->GetLength())
    << "Didn't receive expected number of bytes";

  bool comparison = true;
  for (int i = 0; comparison && i < sendbuff->GetCapacity(); i++) {
    if (buffr[i] != parityodd[i]) comparison = false;
  }
  EXPECT_TRUE(comparison)
    << "Unexpected byte received with Even Parity";

  if (!comparison) {
    DumpBuffer(buffs, sendbuff->GetCapacity(), "Parity7O1Send.out.bin");
    DumpBuffer(buffr, readwrite->GetReceiveBuffer()->GetLength(), "Parity7O1Send.in.bin");
  }
}

TEST_F(SerialParityTest, Parity7O1ReceiveError)
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
  EXPECT_NE(-1, serial_setdatabits(m_readhandle, 7)) << "serial_setdatabits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparity(m_readhandle, ODD)) << "serial_setparity" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setstopbits(m_readhandle, ONE)) << "serial_setstopbits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_sethandshake(m_readhandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_readhandle, false)) << "serial_setdiscardnull" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparityreplace(m_readhandle, 0)) << "serial_setparityreplace" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_open(m_readhandle)) << "serial_open" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_setproperties(m_readhandle)) << "serial_setproperties" << PrintError(m_readhandle);

  std::auto_ptr<Buffer> sendbuff(new Buffer(128));
  char *buffs = sendbuff->GetBuffer();
  for (int i = 0; i < sendbuff->GetCapacity(); i++) {
    buffs[i] = parityodd[i];
    if (i == 0x45) buffs[i] ^= 0x80;
  }
  sendbuff->Produce(sendbuff->GetCapacity());

  std::auto_ptr<SerialReadWrite> readwrite(new SerialReadWrite(m_writehandle, m_readhandle, 256));
  char *buffr = readwrite->GetReceiveBuffer()->GetBuffer();
  readwrite->DoTransfer(sendbuff.get());

  EXPECT_EQ(sendbuff->GetCapacity(), readwrite->GetReceiveBuffer()->GetLength())
    << "Didn't receive expected number of bytes";

  bool comparison = true;
  for (int i = 0; comparison && i < sendbuff->GetCapacity(); i++) {
    if (i == 0x45) {
      if (buffr[i] != 0) comparison = false;
    } else {
      if (buffr[i] != i) comparison = false;
    }
  }
  EXPECT_TRUE(comparison)
    << "Unexpected byte received with Even Parity";

  if (!comparison) {
    DumpBuffer(buffs, sendbuff->GetCapacity(), "Parity7O1ReceiveError.out.bin");
    DumpBuffer(buffr, readwrite->GetReceiveBuffer()->GetLength(), "Parity7O1ReceiveError.in.bin");
  }
}

TEST_F(SerialParityTest, Parity7O1ReceiveErrorWithReplace)
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
  EXPECT_NE(-1, serial_setdatabits(m_readhandle, 7)) << "serial_setdatabits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparity(m_readhandle, ODD)) << "serial_setparity" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setstopbits(m_readhandle, ONE)) << "serial_setstopbits" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_sethandshake(m_readhandle, NOHANDSHAKE)) << "serial_sethandshake" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setdiscardnull(m_readhandle, false)) << "serial_setdiscardnull" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_setparityreplace(m_readhandle, 1)) << "serial_setparityreplace" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_open(m_readhandle)) << "serial_open" << PrintError(m_readhandle);
  ASSERT_NE(-1, serial_setproperties(m_readhandle)) << "serial_setproperties" << PrintError(m_readhandle);

  std::auto_ptr<Buffer> sendbuff(new Buffer(128));
  char *buffs = sendbuff->GetBuffer();
  for (int i = 0; i < sendbuff->GetCapacity(); i++) {
    buffs[i] = parityodd[i];
    if (i == 0x45) buffs[i] ^= 0x80;
  }
  sendbuff->Produce(sendbuff->GetCapacity());

  std::auto_ptr<SerialReadWrite> readwrite(new SerialReadWrite(m_writehandle, m_readhandle, 256));
  char *buffr = readwrite->GetReceiveBuffer()->GetBuffer();
  readwrite->DoTransfer(sendbuff.get());

  EXPECT_EQ(sendbuff->GetCapacity(), readwrite->GetReceiveBuffer()->GetLength())
    << "Didn't receive expected number of bytes";

  bool comparison = true;
  for (int i = 0; comparison && i < sendbuff->GetCapacity(); i++) {
    if (i == 0x45) {
      if (buffr[i] != 1) comparison = false;
    } else {
      if (buffr[i] != i) comparison = false;
    }
  }
  EXPECT_TRUE(comparison)
    << "Unexpected byte received with Even Parity";

  if (!comparison) {
    DumpBuffer(buffs, sendbuff->GetCapacity(), "Parity7O1ReceiveErrorWithReplace.out.bin");
    DumpBuffer(buffr, readwrite->GetReceiveBuffer()->GetLength(), "Parity7O1ReceiveErrorWithReplace.in.bin");
  }
}
