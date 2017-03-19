#include <iostream>
#include <stdlib.h>
#include <errno.h>
#include "gtest/gtest.h"
#include "main.hpp"
#include "configuration.hpp"
#include "nserial.h"
#include "netfx.h"

class SerialErrorTest : public ::testing::Test
{
protected:
  SerialErrorTest();
  virtual ~SerialErrorTest();

  virtual void SetUp();
  virtual void TearDown();

protected:
  struct serialhandle *handle;
};

SerialErrorTest::SerialErrorTest() : ::testing::Test()
{
}

SerialErrorTest::~SerialErrorTest()
{
}

void SerialErrorTest::SetUp()
{
  handle = serial_init();
  ASSERT_TRUE(handle != NULL)
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_setdevicename(handle, serialconfig->GetDevice()))
    << "Error setting serial port: " << strerror(errno) << " (" << errno << ")";
}

void SerialErrorTest::TearDown()
{
  serial_terminate(handle);
}

TEST_F(SerialErrorTest, ErrorMapping)
{
  netfx_errno_t e;

  ASSERT_EQ(NETFX_OK,     netfx_errno(0));
  ASSERT_EQ(NETFX_EINVAL, netfx_errno(EINVAL));
  ASSERT_EQ(NETFX_EACCES, netfx_errno(EACCES));
  ASSERT_EQ(NETFX_ENOMEM, netfx_errno(ENOMEM));
  ASSERT_EQ(NETFX_EBADF,  netfx_errno(EBADF));
  ASSERT_EQ(NETFX_ENOSYS, netfx_errno(ENOSYS));
  ASSERT_EQ(NETFX_EIO,    netfx_errno(EIO));
  ASSERT_EQ(NETFX_EINTR,  netfx_errno(EINTR));

  e = (netfx_errno_t)netfx_errno(EAGAIN);
  ASSERT_TRUE(e == NETFX_EAGAIN || e == NETFX_EWOULDBLOCK);
  e = (netfx_errno_t)netfx_errno(EWOULDBLOCK);
  ASSERT_TRUE(e == NETFX_EAGAIN || e == NETFX_EWOULDBLOCK);

  ASSERT_EQ(NETFX_UNKNOWN, netfx_errno(-1));
}
