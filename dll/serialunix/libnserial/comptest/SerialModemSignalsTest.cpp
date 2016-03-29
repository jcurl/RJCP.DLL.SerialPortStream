#include "gtest/gtest.h"
#include "main.hpp"
#include "SerialConfiguration.hpp"
#include "PrintError.hpp"
#include "nserial.h"

class SerialModemSignalsTest : public ::testing::Test
{
protected:
  SerialModemSignalsTest();
  virtual ~SerialModemSignalsTest();

  virtual void SetUp();
  virtual void TearDown();

protected:
  struct serialhandle *m_readhandle;
  struct serialhandle *m_writehandle;
};

SerialModemSignalsTest::SerialModemSignalsTest() : ::testing::Test()
{
}

SerialModemSignalsTest::~SerialModemSignalsTest()
{
}

void SerialModemSignalsTest::SetUp()
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

void SerialModemSignalsTest::TearDown()
{
  serial_terminate(m_writehandle);
  serial_terminate(m_readhandle);
}

TEST_F(SerialModemSignalsTest, GetSignals)
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

  int dcd, ri, dsr, cts, dtr, rts;
  EXPECT_NE(-1, serial_getdcd(m_writehandle, &dcd)) << "serial_getdcd" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_getri(m_writehandle, &ri)) << "serial_getri" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_getdsr(m_writehandle, &dsr)) << "serial_getdsr" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_getcts(m_writehandle, &cts)) << "serial_getcts" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_getdtr(m_writehandle, &dtr)) << "serial_getdtr" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_getrts(m_writehandle, &rts)) << "serial_getrts" << PrintError(m_writehandle);
  std::cout << "WriteHandle: DCD (in): " << dcd << "; RI (in): " << ri << "; DSR (in): " << dsr <<
    "; CTS (in): " << cts << "; DTR (out): " << dtr << "; RTS (out): " << rts << ".\n";

  EXPECT_NE(-1, serial_getdcd(m_readhandle, &dcd)) << "serial_getdcd" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_getri(m_readhandle, &ri)) << "serial_getri" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_getdsr(m_readhandle, &dsr)) << "serial_getdsr" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_getcts(m_readhandle, &cts)) << "serial_getcts" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_getdtr(m_readhandle, &dtr)) << "serial_getdtr" << PrintError(m_readhandle);
  EXPECT_NE(-1, serial_getrts(m_readhandle, &rts)) << "serial_getrts" << PrintError(m_readhandle);
  std::cout << " ReadHandle: DCD (in): " << dcd << "; RI (in): " << ri << "; DSR (in): " << dsr <<
    "; CTS (in): " << cts << "; DTR (out): " << dtr << "; RTS (out): " << rts << ".\n";
}

TEST_F(SerialModemSignalsTest, RtsCts)
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

  int cts, rts;
  EXPECT_NE(-1, serial_setrts(m_writehandle, 1)) << "serial_setrts" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_getrts(m_writehandle, &rts)) << "serial_getrts" << PrintError(m_writehandle);
  EXPECT_NE(0, rts);
  EXPECT_NE(-1, serial_getcts(m_readhandle, &cts)) << "serial_getcts" << PrintError(m_readhandle);
  EXPECT_NE(0, cts);

  EXPECT_NE(-1, serial_setrts(m_writehandle, 0)) << "serial_setrts" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_getrts(m_writehandle, &rts)) << "serial_getrts" << PrintError(m_writehandle);
  EXPECT_EQ(0, rts);
  EXPECT_NE(-1, serial_getcts(m_readhandle, &cts)) << "serial_getcts" << PrintError(m_readhandle);
  EXPECT_EQ(0, cts);

  EXPECT_NE(-1, serial_setrts(m_writehandle, 1)) << "serial_setrts" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_getrts(m_writehandle, &rts)) << "serial_getrts" << PrintError(m_writehandle);
  EXPECT_NE(0, rts);
  EXPECT_NE(-1, serial_getcts(m_readhandle, &cts)) << "serial_getcts" << PrintError(m_readhandle);
  EXPECT_NE(0, cts);
}

TEST_F(SerialModemSignalsTest, DtrDts)
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

  int dsr, dtr;
  EXPECT_NE(-1, serial_setdtr(m_writehandle, 1)) << "serial_setdtr" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_getdtr(m_writehandle, &dtr)) << "serial_getdtr" << PrintError(m_writehandle);
  EXPECT_NE(0, dtr);
  EXPECT_NE(-1, serial_getdsr(m_readhandle, &dsr)) << "serial_getdsr" << PrintError(m_readhandle);
  EXPECT_NE(0, dsr);

  EXPECT_NE(-1, serial_setdtr(m_writehandle, 0)) << "serial_setdtr" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_getdtr(m_writehandle, &dtr)) << "serial_getdtr" << PrintError(m_writehandle);
  EXPECT_EQ(0, dtr);
  EXPECT_NE(-1, serial_getdsr(m_readhandle, &dsr)) << "serial_getdsr" << PrintError(m_readhandle);
  EXPECT_EQ(0, dsr);

  EXPECT_NE(-1, serial_setdtr(m_writehandle, 1)) << "serial_setdtr" << PrintError(m_writehandle);
  EXPECT_NE(-1, serial_getdtr(m_writehandle, &dtr)) << "serial_getdtr" << PrintError(m_writehandle);
  EXPECT_NE(0, dtr);
  EXPECT_NE(-1, serial_getdsr(m_readhandle, &dsr)) << "serial_getdsr" << PrintError(m_readhandle);
  EXPECT_NE(0, dsr);
}
