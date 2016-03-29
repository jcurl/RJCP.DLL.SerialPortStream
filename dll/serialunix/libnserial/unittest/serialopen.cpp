#include <iostream>
#include <stdlib.h>
#include <errno.h>
#include "gtest/gtest.h"
#include "main.hpp"
#include "configuration.hpp"
#include "nserial.h"

class SerialOpenTest : public ::testing::Test
{
protected:
  SerialOpenTest();
  virtual ~SerialOpenTest();

  virtual void SetUp();
  virtual void TearDown();

protected:
  struct serialhandle *handle;
};

SerialOpenTest::SerialOpenTest() : ::testing::Test()
{
}

SerialOpenTest::~SerialOpenTest()
{
}

void SerialOpenTest::SetUp()
{
  handle = serial_init();
  ASSERT_TRUE(handle != NULL)
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_setdevicename(handle, serialconfig->GetDevice()))
    << "Error setting serial port: " << strerror(errno) << " (" << errno << ")";
}

void SerialOpenTest::TearDown()
{
  serial_terminate(handle);
}

TEST_F(SerialOpenTest, OpenDirectClose)
{
  ASSERT_EQ(0, serial_open(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";
  EXPECT_NE(-1, serial_getfd(handle));
  ASSERT_EQ(0, serial_close(handle));
}

TEST_F(SerialOpenTest, NoSerialPortGiven)
{
  struct serialhandle *handle2 = serial_init();
  ASSERT_TRUE(handle2 != NULL);
  EXPECT_NE(0, serial_getproperties(handle2));
  if (handle2 != NULL) {
    serial_terminate(handle2);
  }
}

TEST_F(SerialOpenTest, SetPropertiesWithoutSerialPort)
{
  struct serialhandle *handle2 = serial_init();
  ASSERT_TRUE(handle2 != NULL);
  EXPECT_EQ(0, serial_setbaud(handle2, 115200));
  EXPECT_EQ(0, serial_setdatabits(handle2, 8));
  EXPECT_EQ(0, serial_setparity(handle2, NOPARITY));
  EXPECT_EQ(0, serial_setstopbits(handle2, ONE));
  EXPECT_EQ(0, serial_sethandshake(handle2, NOHANDSHAKE));
  if (handle2 != NULL) {
    serial_terminate(handle2);
  }
}

TEST_F(SerialOpenTest, Serial115200_8_N_1)
{
  EXPECT_EQ(0, serial_setbaud(handle, 115200));
  EXPECT_EQ(0, serial_setdatabits(handle, 8));
  EXPECT_EQ(0, serial_setparity(handle, NOPARITY));
  EXPECT_EQ(0, serial_setstopbits(handle, ONE));
  EXPECT_EQ(0, serial_sethandshake(handle, NOHANDSHAKE));
  ASSERT_EQ(0, serial_open(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_setproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error setting properties: " << strerror(errno) << " (" << errno << ")";

  EXPECT_EQ(0, serial_getproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error getting properties: " << strerror(errno) << " (" << errno << ")";
  int baud;
  EXPECT_EQ(0, serial_getbaud(handle, &baud));
  EXPECT_EQ(115200, baud);
  int databits;
  EXPECT_EQ(0, serial_getdatabits(handle, &databits));
  EXPECT_EQ(8, databits);
  serialparity_t parity;
  EXPECT_EQ(0, serial_getparity(handle, &parity));
  EXPECT_EQ(NOPARITY, parity);
  serialstopbits_t stopbits;
  EXPECT_EQ(0, serial_getstopbits(handle, &stopbits));
  EXPECT_EQ(ONE, stopbits);
  serialhandshake_t handshake;
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(NOHANDSHAKE, handshake);

  ASSERT_EQ(0, serial_close(handle));
}

TEST_F(SerialOpenTest, Serial4800_8_N_1)
{
  EXPECT_EQ(0, serial_setbaud(handle, 4800));
  EXPECT_EQ(0, serial_setdatabits(handle, 8));
  EXPECT_EQ(0, serial_setparity(handle, NOPARITY));
  EXPECT_EQ(0, serial_setstopbits(handle, ONE));
  EXPECT_EQ(0, serial_sethandshake(handle, NOHANDSHAKE));
  ASSERT_EQ(0, serial_open(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_setproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error setting properties: " << strerror(errno) << " (" << errno << ")";

  EXPECT_EQ(0, serial_getproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error getting properties: " << strerror(errno) << " (" << errno << ")";
  int baud;
  EXPECT_EQ(0, serial_getbaud(handle, &baud));
  EXPECT_EQ(4800, baud);
  int databits;
  EXPECT_EQ(0, serial_getdatabits(handle, &databits));
  EXPECT_EQ(8, databits);
  serialparity_t parity;
  EXPECT_EQ(0, serial_getparity(handle, &parity));
  EXPECT_EQ(NOPARITY, parity);
  serialstopbits_t stopbits;
  EXPECT_EQ(0, serial_getstopbits(handle, &stopbits));
  EXPECT_EQ(ONE, stopbits);
  serialhandshake_t handshake;
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(NOHANDSHAKE, handshake);

  ASSERT_EQ(0, serial_close(handle));
}

TEST_F(SerialOpenTest, Serial115200_7_E_1)
{
  EXPECT_EQ(0, serial_setbaud(handle, 115200));
  EXPECT_EQ(0, serial_setdatabits(handle, 7));
  EXPECT_EQ(0, serial_setparity(handle, EVEN));
  EXPECT_EQ(0, serial_setstopbits(handle, ONE));
  EXPECT_EQ(0, serial_sethandshake(handle, NOHANDSHAKE));
  ASSERT_EQ(0, serial_open(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_setproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error setting properties: " << strerror(errno) << " (" << errno << ")";

  EXPECT_EQ(0, serial_getproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error getting properties: " << strerror(errno) << " (" << errno << ")";
  int baud;
  EXPECT_EQ(0, serial_getbaud(handle, &baud));
  EXPECT_EQ(115200, baud);
  int databits;
  EXPECT_EQ(0, serial_getdatabits(handle, &databits));
  EXPECT_EQ(7, databits);
  serialparity_t parity;
  EXPECT_EQ(0, serial_getparity(handle, &parity));
  EXPECT_EQ(EVEN, parity);
  serialstopbits_t stopbits;
  EXPECT_EQ(0, serial_getstopbits(handle, &stopbits));
  EXPECT_EQ(ONE, stopbits);
  serialhandshake_t handshake;
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(NOHANDSHAKE, handshake);

  ASSERT_EQ(0, serial_close(handle));
}

TEST_F(SerialOpenTest, Serial115200_7_O_2)
{
  EXPECT_EQ(0, serial_setbaud(handle, 115200));
  EXPECT_EQ(0, serial_setdatabits(handle, 7));
  EXPECT_EQ(0, serial_setparity(handle, ODD));
  EXPECT_EQ(0, serial_setstopbits(handle, TWO));
  EXPECT_EQ(0, serial_sethandshake(handle, NOHANDSHAKE));
  ASSERT_EQ(0, serial_open(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_setproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error setting properties: " << strerror(errno) << " (" << errno << ")";

  EXPECT_EQ(0, serial_getproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error getting properties: " << strerror(errno) << " (" << errno << ")";
  int baud;
  EXPECT_EQ(0, serial_getbaud(handle, &baud));
  EXPECT_EQ(115200, baud);
  int databits;
  EXPECT_EQ(0, serial_getdatabits(handle, &databits));
  EXPECT_EQ(7, databits);
  serialparity_t parity;
  EXPECT_EQ(0, serial_getparity(handle, &parity));
  EXPECT_EQ(ODD, parity);
  serialstopbits_t stopbits;
  EXPECT_EQ(0, serial_getstopbits(handle, &stopbits));
  EXPECT_EQ(TWO, stopbits);
  serialhandshake_t handshake;
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(NOHANDSHAKE, handshake);

  ASSERT_EQ(0, serial_close(handle));
}

TEST_F(SerialOpenTest, SerialXonXoff)
{
  EXPECT_EQ(0, serial_setbaud(handle, 115200));
  EXPECT_EQ(0, serial_setdatabits(handle, 8));
  EXPECT_EQ(0, serial_setparity(handle, NOPARITY));
  EXPECT_EQ(0, serial_setstopbits(handle, ONE));
  EXPECT_EQ(0, serial_sethandshake(handle, XON));
  ASSERT_EQ(0, serial_open(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_setproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error setting properties: " << strerror(errno) << " (" << errno << ")";

  EXPECT_EQ(0, serial_getproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error getting properties: " << strerror(errno) << " (" << errno << ")";
  int baud;
  EXPECT_EQ(0, serial_getbaud(handle, &baud));
  EXPECT_EQ(115200, baud);
  int databits;
  EXPECT_EQ(0, serial_getdatabits(handle, &databits));
  EXPECT_EQ(8, databits);
  serialparity_t parity;
  EXPECT_EQ(0, serial_getparity(handle, &parity));
  EXPECT_EQ(NOPARITY, parity);
  serialstopbits_t stopbits;
  EXPECT_EQ(0, serial_getstopbits(handle, &stopbits));
  EXPECT_EQ(ONE, stopbits);
  serialhandshake_t handshake;
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(XON, handshake);

  ASSERT_EQ(0, serial_close(handle));
}

TEST_F(SerialOpenTest, SerialRtsCts)
{
  EXPECT_EQ(0, serial_setbaud(handle, 115200));
  EXPECT_EQ(0, serial_setdatabits(handle, 8));
  EXPECT_EQ(0, serial_setparity(handle, NOPARITY));
  EXPECT_EQ(0, serial_setstopbits(handle, ONE));
  EXPECT_EQ(0, serial_sethandshake(handle, RTS));
  ASSERT_EQ(0, serial_open(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_setproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error setting properties: " << strerror(errno) << " (" << errno << ")";

  EXPECT_EQ(0, serial_getproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error getting properties: " << strerror(errno) << " (" << errno << ")";
  int baud;
  EXPECT_EQ(0, serial_getbaud(handle, &baud));
  EXPECT_EQ(115200, baud);
  int databits;
  EXPECT_EQ(0, serial_getdatabits(handle, &databits));
  EXPECT_EQ(8, databits);
  serialparity_t parity;
  EXPECT_EQ(0, serial_getparity(handle, &parity));
  EXPECT_EQ(NOPARITY, parity);
  serialstopbits_t stopbits;
  EXPECT_EQ(0, serial_getstopbits(handle, &stopbits));
  EXPECT_EQ(ONE, stopbits);
  serialhandshake_t handshake;
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(RTS, handshake);

  ASSERT_EQ(0, serial_close(handle));
}

TEST_F(SerialOpenTest, SerialDtrDts)
{
  EXPECT_EQ(0, serial_setbaud(handle, 115200));
  EXPECT_EQ(0, serial_setdatabits(handle, 8));
  EXPECT_EQ(0, serial_setparity(handle, NOPARITY));
  EXPECT_EQ(0, serial_setstopbits(handle, ONE));
  EXPECT_EQ(0, serial_sethandshake(handle, DTR));
  ASSERT_EQ(0, serial_open(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_setproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error setting properties: " << strerror(errno) << " (" << errno << ")";

  EXPECT_EQ(0, serial_getproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error getting properties: " << strerror(errno) << " (" << errno << ")";
  int baud;
  EXPECT_EQ(0, serial_getbaud(handle, &baud));
  EXPECT_EQ(115200, baud);
  int databits;
  EXPECT_EQ(0, serial_getdatabits(handle, &databits));
  EXPECT_EQ(8, databits);
  serialparity_t parity;
  EXPECT_EQ(0, serial_getparity(handle, &parity));
  EXPECT_EQ(NOPARITY, parity);
  serialstopbits_t stopbits;
  EXPECT_EQ(0, serial_getstopbits(handle, &stopbits));
  EXPECT_EQ(ONE, stopbits);
  serialhandshake_t handshake;
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(DTR, handshake);

  ASSERT_EQ(0, serial_close(handle));
}

TEST_F(SerialOpenTest, Serial115200_7_S_1)
{
  EXPECT_EQ(0, serial_setbaud(handle, 115200));
  EXPECT_EQ(0, serial_setdatabits(handle, 7));
  EXPECT_EQ(0, serial_setparity(handle, SPACE));
  EXPECT_EQ(0, serial_setstopbits(handle, ONE));
  EXPECT_EQ(0, serial_sethandshake(handle, NOHANDSHAKE));
  ASSERT_EQ(0, serial_open(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_setproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error setting properties: " << strerror(errno) << " (" << errno << ")";

  EXPECT_EQ(0, serial_getproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error getting properties: " << strerror(errno) << " (" << errno << ")";
  int baud;
  EXPECT_EQ(0, serial_getbaud(handle, &baud));
  EXPECT_EQ(115200, baud);
  int databits;
  EXPECT_EQ(0, serial_getdatabits(handle, &databits));
  EXPECT_EQ(7, databits);
  serialparity_t parity;
  EXPECT_EQ(0, serial_getparity(handle, &parity));
  EXPECT_EQ(SPACE, parity);
  serialstopbits_t stopbits;
  EXPECT_EQ(0, serial_getstopbits(handle, &stopbits));
  EXPECT_EQ(ONE, stopbits);
  serialhandshake_t handshake;
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(NOHANDSHAKE, handshake);

  ASSERT_EQ(0, serial_close(handle));
}

TEST_F(SerialOpenTest, Serial115200_7_M_1)
{
  EXPECT_EQ(0, serial_setbaud(handle, 115200));
  EXPECT_EQ(0, serial_setdatabits(handle, 7));
  EXPECT_EQ(0, serial_setparity(handle, MARK));
  EXPECT_EQ(0, serial_setstopbits(handle, ONE));
  EXPECT_EQ(0, serial_sethandshake(handle, NOHANDSHAKE));
  ASSERT_EQ(0, serial_open(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_setproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error setting properties: " << strerror(errno) << " (" << errno << ")";

  EXPECT_EQ(0, serial_getproperties(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error getting properties: " << strerror(errno) << " (" << errno << ")";
  int baud;
  EXPECT_EQ(0, serial_getbaud(handle, &baud));
  EXPECT_EQ(115200, baud);
  int databits;
  EXPECT_EQ(0, serial_getdatabits(handle, &databits));
  EXPECT_EQ(7, databits);
  serialparity_t parity;
  EXPECT_EQ(0, serial_getparity(handle, &parity));
  EXPECT_EQ(MARK, parity);
  serialstopbits_t stopbits;
  EXPECT_EQ(0, serial_getstopbits(handle, &stopbits));
  EXPECT_EQ(ONE, stopbits);
  serialhandshake_t handshake;
  EXPECT_EQ(0, serial_gethandshake(handle, &handshake));
  EXPECT_EQ(NOHANDSHAKE, handshake);

  ASSERT_EQ(0, serial_close(handle));
}

TEST_F(SerialOpenTest, SerialDriverQueue)
{
  ASSERT_EQ(0, serial_open(handle));

  int driver;

  EXPECT_EQ(0, serial_getreadbytes(handle, &driver))
    << "Message: " << serial_error(handle) << "; "
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_getwritebytes(handle, &driver))
    << "Message: " << serial_error(handle) << "; "
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";
}

TEST_F(SerialOpenTest, SerialBreak)
{
  ASSERT_EQ(0, serial_open(handle));

  int breakstate;

  EXPECT_EQ(0, serial_setbreak(handle, 0));
  EXPECT_EQ(0, serial_getbreak(handle, &breakstate));
  EXPECT_EQ(0, breakstate);

  EXPECT_EQ(0, serial_setbreak(handle, 1));
  EXPECT_EQ(0, serial_getbreak(handle, &breakstate));
  EXPECT_NE(0, breakstate);

  EXPECT_EQ(0, serial_setbreak(handle, 0));
  EXPECT_EQ(0, serial_getbreak(handle, &breakstate));
  EXPECT_EQ(0, breakstate);
}
