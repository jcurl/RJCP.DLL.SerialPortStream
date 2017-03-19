#include <iostream>
#include <memory>
#include <stdlib.h>
#include <errno.h>
#include "gtest/gtest.h"
#include "main.hpp"
#include "PrintError.hpp"
#include "nserial.h"

class ListPortTest : public ::testing::Test
{
protected:
  ListPortTest();
  virtual ~ListPortTest();

  virtual void SetUp();
  virtual void TearDown();

protected:
  struct serialhandle *m_handle;
};

ListPortTest::ListPortTest() : ::testing::Test()
{
}

ListPortTest::~ListPortTest()
{
}

void ListPortTest::SetUp()
{
  m_handle = serial_init();
  ASSERT_TRUE(m_handle != NULL)
    << "Error initialising handle: " << strerror(errno) << " (" << errno << ")";
}

void ListPortTest::TearDown()
{
  serial_terminate(m_handle);
}

TEST_F(ListPortTest, ListPorts)
{
  struct portdescription *ports;

  ports = serial_getports(m_handle);
  if (ports == NULL) {
    // This is a valid error for systems that don't support getting the port
    // list.
    ASSERT_EQ(ENOSYS, errno)
      << "Error getting port list: " << strerror(errno) << " (" << errno << ")";
    return;
  }

  int p = 0;
  while (ports[p].device) {
    std::cout << "Port: " << p << "; Device: " << ports[p].device;
    if (ports[p].description) {
      std::cout << "; Description: " << ports[p].description;
    }
    std::cout << std::endl;
    p++;
  }
}
