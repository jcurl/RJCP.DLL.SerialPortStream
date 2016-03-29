#include <iostream>
#include <stdlib.h>
#include <errno.h>
#include "gtest/gtest.h"
#include "main.hpp"
#include "configuration.hpp"
#include "nserial.h"

class SerialInitTest : public ::testing::Test
{
protected:
  SerialInitTest();
  virtual ~SerialInitTest();
  
  virtual void SetUp();
  virtual void TearDown();

protected:
  struct serialhandle *handle;
};

SerialInitTest::SerialInitTest() : ::testing::Test()
{
}

SerialInitTest::~SerialInitTest()
{
}

void SerialInitTest::SetUp()
{
  handle = serial_init();
  ASSERT_TRUE(handle != NULL)
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_setdevicename(handle, serialconfig->GetDevice()))
    << "Error setting device name: " << strerror(errno) << " (" << errno << ")";
}

void SerialInitTest::TearDown()
{
  serial_terminate(handle);
}

TEST(SerialBasicTest, Version)
{
  const char *version;
  version = ::serial_version();
  EXPECT_EQ(0, strcmp("1.0.0", version));
}

TEST_F(SerialInitTest, Init)
{
  const char *devicename = serial_getdevicename(handle);
  EXPECT_STREQ(serialconfig->GetDevice(), devicename);

  int fd = serial_getfd(handle);
  EXPECT_EQ(-1, fd);

  int isopen;
  EXPECT_EQ(0, serial_isopen(handle, &isopen));
  EXPECT_EQ(0, isopen);

  int baud;
  EXPECT_EQ(0, serial_getbaud(handle, &baud))
    << "Error getting baud: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(115200, baud);
  
  int databits;
  EXPECT_EQ(0, serial_getdatabits(handle, &databits))
    << "Error getting data bits: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(8, databits);

  serialparity_t parity;
  EXPECT_EQ(0, serial_getparity(handle, &parity))
    << "Error getting parity: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(NOPARITY, parity);

  serialstopbits_t stopbits;
  EXPECT_EQ(0, serial_getstopbits(handle, &stopbits))
    << "Error getting stop bits: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(ONE, stopbits);

  serialhandshake_t handshake;
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake))
    << "Error getting handshake: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(NOHANDSHAKE, handshake);
  
  int txcontinueonxoff;
  EXPECT_EQ(0, serial_gettxcontinueonxoff(handle, &txcontinueonxoff))
    << "Error getting TxContinueOnXOff: "
    << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, txcontinueonxoff);
  
  int discardnull;
  EXPECT_EQ(0, serial_getdiscardnull(handle, &discardnull))
    << "Error getting DiscardNull: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, discardnull);

  int xonlimit;
  EXPECT_EQ(0, serial_getxonlimit(handle, &xonlimit))
    << "Error getting XOnLimit: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(2048, xonlimit);

  int xofflimit;
  EXPECT_EQ(0, serial_getxofflimit(handle, &xofflimit))
    << "Error getting XOffLimit: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(512, xofflimit);

  int parityreplace;
  EXPECT_EQ(0, serial_getparityreplace(handle, &parityreplace))
    << "Error getting ParityReplace: "
    << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, parityreplace);
}

TEST(SerialBasicTest, SupportedBaudRates)
{
  int *baudrates = serial_getsupportedbaudrates();
  ASSERT_TRUE(baudrates != NULL)
    << "Error getting supported baud rates: "
    << strerror(errno) << " (" << errno << ")";

  int bauditem = 0;
  while (baudrates[bauditem]) {
    if (bauditem > 0) {
      EXPECT_LT(baudrates[bauditem - 1], baudrates[bauditem]);
    }
    bauditem++;
  }

  EXPECT_NE(0, bauditem) << "Expected more than zero entries";
}

TEST_F(SerialInitTest, GetSetBaudWhenClosed)
{
  // We assume some very common baud rates are simply available, instead of
  // checking them.

  int baudrate;
  EXPECT_EQ(0, serial_setbaud(handle, 9600));
  EXPECT_EQ(0, serial_getbaud(handle, &baudrate));
  EXPECT_EQ(9600, baudrate);

  EXPECT_EQ(0, serial_setbaud(handle, 115200));
  EXPECT_EQ(0, serial_getbaud(handle, &baudrate));
  EXPECT_EQ(115200, baudrate);

  EXPECT_EQ(0, serial_setbaud(handle, 4800));
  EXPECT_EQ(0, serial_getbaud(handle, &baudrate));
  EXPECT_EQ(4800, baudrate);

  // Set some very weird baudrate that is more than 10% out of tolerance with
  // other baud rates.

  EXPECT_NE(0, serial_setbaud(handle, 103000));
  EXPECT_EQ(EINVAL, errno)
    << "Expected EINVAL; got " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(4800, baudrate);
}

TEST_F(SerialInitTest, GetSetDataBitsWhenClosed)
{
  int databits;
  EXPECT_EQ(0, serial_setdatabits(handle, 5));
  EXPECT_EQ(0, serial_getdatabits(handle, &databits));
  EXPECT_EQ(5, databits);

  EXPECT_EQ(0, serial_setdatabits(handle, 6));
  EXPECT_EQ(0, serial_getdatabits(handle, &databits));
  EXPECT_EQ(6, databits);

  EXPECT_EQ(0, serial_setdatabits(handle, 7));
  EXPECT_EQ(0, serial_getdatabits(handle, &databits));
  EXPECT_EQ(7, databits);

  EXPECT_EQ(0, serial_setdatabits(handle, 8));
  EXPECT_EQ(0, serial_getdatabits(handle, &databits));
  EXPECT_EQ(8, databits);

  EXPECT_NE(0, serial_setdatabits(handle, 9));
  EXPECT_EQ(EINVAL, errno)
    << "Expected EINVAL; got " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_getdatabits(handle, &databits));
  EXPECT_EQ(8, databits);

  EXPECT_NE(0, serial_setdatabits(handle, 0));
  EXPECT_EQ(EINVAL, errno)
    << "Expected EINVAL; got " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_getdatabits(handle, &databits));
  EXPECT_EQ(8, databits);

  EXPECT_NE(0, serial_setdatabits(handle, -1));
  EXPECT_EQ(EINVAL, errno)
    << "Expected EINVAL; got " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_getdatabits(handle, &databits));
  EXPECT_EQ(8, databits);
}

TEST_F(SerialInitTest, GetSetParityWhenClosed)
{
  serialparity_t parity;

  EXPECT_EQ(0, serial_setparity(handle, EVEN));
  EXPECT_EQ(0, serial_getparity(handle, &parity));
  EXPECT_EQ(EVEN, parity);

  EXPECT_EQ(0, serial_setparity(handle, ODD));
  EXPECT_EQ(0, serial_getparity(handle, &parity));
  EXPECT_EQ(ODD, parity);

  EXPECT_EQ(0, serial_setparity(handle, SPACE));
  EXPECT_EQ(0, serial_getparity(handle, &parity));
  EXPECT_EQ(SPACE, parity);

  EXPECT_EQ(0, serial_setparity(handle, MARK));
  EXPECT_EQ(0, serial_getparity(handle, &parity));
  EXPECT_EQ(MARK, parity);

  EXPECT_EQ(0, serial_setparity(handle, NOPARITY));
  EXPECT_EQ(0, serial_getparity(handle, &parity));
  EXPECT_EQ(NOPARITY, parity);

  EXPECT_NE(0, serial_setparity(handle, (serialparity_t)10));
  EXPECT_EQ(EINVAL, errno)
    << "Expected EINVAL; got " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_getparity(handle, &parity));
  EXPECT_EQ(NOPARITY, parity);
}

TEST_F(SerialInitTest, GetSetStopBitsWhenClosed)
{
  serialstopbits_t stopbits;

  EXPECT_EQ(0, serial_setstopbits(handle, TWO));
  EXPECT_EQ(0, serial_getstopbits(handle, &stopbits));
  EXPECT_EQ(TWO, stopbits);

  EXPECT_EQ(0, serial_setstopbits(handle, ONE5));
  EXPECT_EQ(0, serial_getstopbits(handle, &stopbits));
  EXPECT_EQ(ONE5, stopbits);

  EXPECT_EQ(0, serial_setstopbits(handle, ONE));
  EXPECT_EQ(0, serial_getstopbits(handle, &stopbits));
  EXPECT_EQ(ONE, stopbits);

  EXPECT_NE(0, serial_setstopbits(handle, (serialstopbits_t)10));
  EXPECT_EQ(EINVAL, errno)
    << "Expected EINVAL; got " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_getstopbits(handle, &stopbits));
  EXPECT_EQ(ONE, stopbits);

  EXPECT_NE(0, serial_setstopbits(handle, (serialstopbits_t)-1));
  EXPECT_EQ(EINVAL, errno)
    << "Expected EINVAL; got " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_getstopbits(handle, &stopbits));
  EXPECT_EQ(ONE, stopbits);
}

TEST_F(SerialInitTest, GetSetHandshakeWhenClosed)
{
  serialhandshake_t handshake;

  EXPECT_EQ(0, serial_sethandshake(handle, DTRRTSXON));
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(DTRRTSXON, handshake);

  EXPECT_EQ(0, serial_sethandshake(handle, DTRXON));
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(DTRXON, handshake);

  EXPECT_EQ(0, serial_sethandshake(handle, DTRRTS));
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(DTRRTS, handshake);

  EXPECT_EQ(0, serial_sethandshake(handle, RTSXON));
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(RTSXON, handshake);

  EXPECT_EQ(0, serial_sethandshake(handle, DTR));
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(DTR, handshake);

  EXPECT_EQ(0, serial_sethandshake(handle, RTS));
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(RTS, handshake);

  EXPECT_EQ(0, serial_sethandshake(handle, XON));
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(XON, handshake);

  EXPECT_EQ(0, serial_sethandshake(handle, NOHANDSHAKE));
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(NOHANDSHAKE, handshake);

  EXPECT_NE(0, serial_sethandshake(handle, (serialhandshake_t)23));
  EXPECT_EQ(EINVAL, errno)
    << "Expected EINVAL; got " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(NOHANDSHAKE, handshake);

  EXPECT_NE(0, serial_sethandshake(handle, (serialhandshake_t)-1));
  EXPECT_EQ(EINVAL, errno)
    << "Expected EINVAL; got " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(NOHANDSHAKE, handshake);
}

TEST_F(SerialInitTest, GetSetTxContinueOnXOffWhenClosed)
{
  int txcontinue;

  EXPECT_EQ(0, serial_settxcontinueonxoff(handle, -1));
  EXPECT_EQ(0, serial_gettxcontinueonxoff(handle, &txcontinue));
  EXPECT_NE(0, txcontinue);

  EXPECT_EQ(0, serial_settxcontinueonxoff(handle, 1));
  EXPECT_EQ(0, serial_gettxcontinueonxoff(handle, &txcontinue));
  EXPECT_NE(0, txcontinue);
  
  EXPECT_EQ(0, serial_settxcontinueonxoff(handle, 0));
  EXPECT_EQ(0, serial_gettxcontinueonxoff(handle, &txcontinue));
  EXPECT_EQ(0, txcontinue);
}

TEST_F(SerialInitTest, GetSetDiscardNullWhenClosed)
{
  int discardnull;

  EXPECT_EQ(0, serial_setdiscardnull(handle, -1));
  EXPECT_EQ(0, serial_getdiscardnull(handle, &discardnull));
  EXPECT_NE(0, discardnull);

  EXPECT_EQ(0, serial_setdiscardnull(handle, 1));
  EXPECT_EQ(0, serial_getdiscardnull(handle, &discardnull));
  EXPECT_NE(0, discardnull);
  
  EXPECT_EQ(0, serial_setdiscardnull(handle, 0));
  EXPECT_EQ(0, serial_getdiscardnull(handle, &discardnull));
  EXPECT_EQ(0, discardnull);
}

TEST_F(SerialInitTest, GetSetXOnLimitWhenClosed)
{
  int xonlimit;

  EXPECT_EQ(0, serial_setxonlimit(handle, 0));
  EXPECT_EQ(0, serial_getxonlimit(handle, &xonlimit));
  EXPECT_EQ(0, xonlimit);

  EXPECT_EQ(0, serial_setxonlimit(handle, 512));
  EXPECT_EQ(0, serial_getxonlimit(handle, &xonlimit));
  EXPECT_EQ(512, xonlimit);

  EXPECT_EQ(0, serial_setxonlimit(handle, 1024));
  EXPECT_EQ(0, serial_getxonlimit(handle, &xonlimit));
  EXPECT_EQ(1024, xonlimit);

  EXPECT_EQ(0, serial_setxonlimit(handle, 2048));
  EXPECT_EQ(0, serial_getxonlimit(handle, &xonlimit));
  EXPECT_EQ(2048, xonlimit);

  EXPECT_EQ(0, serial_setxonlimit(handle, 3072));
  EXPECT_EQ(0, serial_getxonlimit(handle, &xonlimit));
  EXPECT_EQ(3072, xonlimit);

  EXPECT_EQ(0, serial_setxonlimit(handle, 8192));
  EXPECT_EQ(0, serial_getxonlimit(handle, &xonlimit));
  EXPECT_EQ(8192, xonlimit);

  EXPECT_NE(0, serial_setxonlimit(handle, -1));
  EXPECT_EQ(EINVAL, errno)
    << "Expected EINVAL; got " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_getxonlimit(handle, &xonlimit));
  EXPECT_EQ(8192, xonlimit);

  EXPECT_NE(0, serial_setxonlimit(handle, -1024));
  EXPECT_EQ(EINVAL, errno)
    << "Expected EINVAL; got " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_getxonlimit(handle, &xonlimit));
  EXPECT_EQ(8192, xonlimit);
}

TEST_F(SerialInitTest, GetSetXOffLimitWhenClosed)
{
  int xofflimit;

  EXPECT_EQ(0, serial_setxofflimit(handle, 0));
  EXPECT_EQ(0, serial_getxofflimit(handle, &xofflimit));
  EXPECT_EQ(0, xofflimit);

  EXPECT_EQ(0, serial_setxofflimit(handle, 512));
  EXPECT_EQ(0, serial_getxofflimit(handle, &xofflimit));
  EXPECT_EQ(512, xofflimit);

  EXPECT_EQ(0, serial_setxofflimit(handle, 1024));
  EXPECT_EQ(0, serial_getxofflimit(handle, &xofflimit));
  EXPECT_EQ(1024, xofflimit);

  EXPECT_EQ(0, serial_setxofflimit(handle, 2048));
  EXPECT_EQ(0, serial_getxofflimit(handle, &xofflimit));
  EXPECT_EQ(2048, xofflimit);

  EXPECT_EQ(0, serial_setxofflimit(handle, 3072));
  EXPECT_EQ(0, serial_getxofflimit(handle, &xofflimit));
  EXPECT_EQ(3072, xofflimit);

  EXPECT_EQ(0, serial_setxofflimit(handle, 8192));
  EXPECT_EQ(0, serial_getxofflimit(handle, &xofflimit));
  EXPECT_EQ(8192, xofflimit);

  EXPECT_NE(0, serial_setxofflimit(handle, -1));
  EXPECT_EQ(EINVAL, errno)
    << "Expected EINVAL; got " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_getxofflimit(handle, &xofflimit));
  EXPECT_EQ(8192, xofflimit);

  EXPECT_NE(0, serial_setxofflimit(handle, -1024));
  EXPECT_EQ(EINVAL, errno)
    << "Expected EINVAL; got " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_getxofflimit(handle, &xofflimit));
  EXPECT_EQ(8192, xofflimit);
}

TEST_F(SerialInitTest, GetSetParityReplaceWhenClosed)
{
  int parityreplace;

  EXPECT_EQ(0, serial_setparityreplace(handle, -1));
  EXPECT_EQ(0, serial_getparityreplace(handle, &parityreplace));
  EXPECT_NE(0, parityreplace);

  EXPECT_EQ(0, serial_setparityreplace(handle, 1));
  EXPECT_EQ(0, serial_getparityreplace(handle, &parityreplace));
  EXPECT_EQ(1, parityreplace);
  
  EXPECT_EQ(0, serial_setparityreplace(handle, 128));
  EXPECT_EQ(0, serial_getparityreplace(handle, &parityreplace));
  EXPECT_EQ(128, parityreplace);
  
  EXPECT_EQ(0, serial_setparityreplace(handle, 127));
  EXPECT_EQ(0, serial_getparityreplace(handle, &parityreplace));
  EXPECT_EQ(127, parityreplace);
  
  EXPECT_EQ(0, serial_setparityreplace(handle, 255));
  EXPECT_EQ(0, serial_getparityreplace(handle, &parityreplace));
  EXPECT_EQ(255, parityreplace);
  
  EXPECT_EQ(0, serial_setparityreplace(handle, 0));
  EXPECT_EQ(0, serial_getparityreplace(handle, &parityreplace));
  EXPECT_EQ(0, parityreplace);
}
