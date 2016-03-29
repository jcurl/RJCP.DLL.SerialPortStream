#include <pthread.h>
#include <semaphore.h>
#include "gtest/gtest.h"
#include "main.hpp"
#include "SerialConfiguration.hpp"
#include "PrintError.hpp"
#include "nserial.h"

class SerialModemEventsTest : public ::testing::Test
{
protected:
  SerialModemEventsTest();
  virtual ~SerialModemEventsTest();

  virtual void SetUp();
  virtual void TearDown();

protected:
  struct serialhandle *m_readhandle;
  struct serialhandle *m_writehandle;
};

SerialModemEventsTest::SerialModemEventsTest() : ::testing::Test()
{
}

SerialModemEventsTest::~SerialModemEventsTest()
{
}

void SerialModemEventsTest::SetUp()
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

void SerialModemEventsTest::TearDown()
{
  serial_terminate(m_writehandle);
  serial_terminate(m_readhandle);
}

struct waitforeventstruct {
  struct serialhandle *handle;
  sem_t eventsema;
  int result;
};

void *waitforeventtest(void *ptr)
{
  struct waitforeventstruct *etest = (struct waitforeventstruct *)ptr;
  serialmodemevent_t result =
    serial_waitformodemevent(etest->handle,
			     (serialmodemevent_t)
			     (MODEMEVENT_DCD | MODEMEVENT_RI |
			      MODEMEVENT_DSR | MODEMEVENT_CTS));
  etest->result = result;
  sem_post(&(etest->eventsema));
  pthread_exit(NULL);
}

TEST_F(SerialModemEventsTest, CtsEvent)
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

  EXPECT_NE(-1, serial_setrts(m_writehandle, false));

  struct waitforeventstruct test = {0, };
  test.handle = m_readhandle;
  test.result = 255;
  ASSERT_NE(-1, sem_init(&(test.eventsema), 0, 0));

  pthread_t waitforeventthread;
  ASSERT_NE(-1, pthread_create(&waitforeventthread, NULL, waitforeventtest, (void *)&test));

  struct timespec timeout;
  ASSERT_NE(-1, clock_gettime(CLOCK_REALTIME, &timeout));
  timeout.tv_sec += 4;

  // TODO: After creating the thread, give it some time to start. Should
  // probably use some better synchronisation here.
  sleep(1);
  EXPECT_NE(-1, serial_setrts(m_writehandle, true));
  ASSERT_EQ(0, sem_timedwait(&(test.eventsema), &timeout));
  ASSERT_EQ(MODEMEVENT_CTS, test.result);
  pthread_cancel(waitforeventthread);
}

TEST_F(SerialModemEventsTest, AbortCtsEvent)
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

  EXPECT_NE(-1, serial_setrts(m_writehandle, false));

  struct waitforeventstruct test = {0, };
  test.handle = m_readhandle;
  test.result = 255;
  ASSERT_NE(-1, sem_init(&(test.eventsema), 0, 0));

  pthread_t waitforeventthread;
  ASSERT_NE(-1, pthread_create(&waitforeventthread, NULL, waitforeventtest, (void *)&test));

  struct timespec timeout;
  ASSERT_NE(-1, clock_gettime(CLOCK_REALTIME, &timeout));
  timeout.tv_sec += 4;

  // TODO: After creating the thread, give it some time to start. Should
  // probably use some better synchronisation here.
  sleep(1);
  EXPECT_NE(-1, serial_abortwaitformodemevent(m_readhandle));
  ASSERT_EQ(0, sem_timedwait(&(test.eventsema), &timeout));
  ASSERT_EQ(MODEMEVENT_NONE, test.result);
  pthread_cancel(waitforeventthread);
}
