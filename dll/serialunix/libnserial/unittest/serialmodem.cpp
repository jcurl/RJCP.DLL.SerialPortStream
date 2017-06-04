#include <iostream>
#include <stdlib.h>
#include <errno.h>
#include <pthread.h>
#include <semaphore.h>
#include "gtest/gtest.h"
#include "main.hpp"
#include "configuration.hpp"
#include "nserial.h"

class SerialModemTest : public ::testing::Test
{
protected:
  SerialModemTest();
  virtual ~SerialModemTest();

  virtual void SetUp();
  virtual void TearDown();

protected:
  struct serialhandle *handle;
};

SerialModemTest::SerialModemTest() : ::testing::Test()
{
}

SerialModemTest::~SerialModemTest()
{
}

void SerialModemTest::SetUp()
{
  handle = serial_init();
  ASSERT_TRUE(handle != NULL)
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";
  EXPECT_EQ(0, serial_setdevicename(handle, serialconfig->GetDevice()))
    << "Error setting serial port: " << strerror(errno) << " (" << errno << ")";
}

void SerialModemTest::TearDown()
{
  serial_terminate(handle);
}

struct waitforeventstruct {
  struct serialhandle *handle;
  sem_t                eventsema;
  serialmodemevent_t   result;
};

void *waitforeventtest(void *ptr)
{
  struct waitforeventstruct *etest = (struct waitforeventstruct *)ptr;

  int oldpthreadstate;
  int pscs = pthread_setcancelstate(PTHREAD_CANCEL_DISABLE, &oldpthreadstate);
  if (pscs) {
    std::cout <<
      "pthread_setcancelstate(PTHREAD_CANCEL_DISABLE, ...) failed." <<
      "Error = " << pscs << " (" << strerror(pscs) << ")" << std::endl;
    etest->result = (serialmodemevent_t)255;
    pthread_exit(NULL);
  }

  // We wait for an event. If an event occurs, we keep trying, as when it's
  // cancelled, it should return MODEMEVENT_NONE.
  serialmodemevent_t result;
  do {
    result = serial_waitformodemevent(etest->handle,
				      (serialmodemevent_t)
				      (MODEMEVENT_DCD | MODEMEVENT_RI |
				       MODEMEVENT_DSR | MODEMEVENT_CTS));
    if (result == MODEMEVENT_ERROR) {
      std::cout << "Error = " << errno << " (" << strerror(errno) << ")" << std::endl;
    }
  } while (result != MODEMEVENT_ERROR && result != MODEMEVENT_NONE);
  etest->result = result;
  sem_post(&(etest->eventsema));
  int dummypthreadstate;
  pthread_setcancelstate(oldpthreadstate, &dummypthreadstate);
  pthread_exit(NULL);
}

TEST_F(SerialModemTest, EventAbort)
{
  ASSERT_EQ(0, serial_open(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";

  struct waitforeventstruct test = {0, };
  test.handle = handle;
  test.result = (serialmodemevent_t)255;
  ASSERT_NE(-1, sem_init(&(test.eventsema), 0, 0));

  pthread_t waitforeventthread;
  ASSERT_NE(-1, pthread_create(&waitforeventthread, NULL,
			       waitforeventtest, (void *)&test));

  // Sleep 100ms, then ensure that the thread is still running (it
  // hasn't been aborted yet!)
  usleep(100000);
  ASSERT_EQ(-1, sem_trywait(&(test.eventsema)));
  ASSERT_EQ(EAGAIN, errno);
  serial_abortwaitformodemevent(handle);

  // Wait for the thread to end and provide the results.
  struct timespec timeout;
  ASSERT_NE(-1, clock_gettime(CLOCK_REALTIME, &timeout));
  timeout.tv_sec += 4;
  ASSERT_EQ(0, sem_timedwait(&(test.eventsema), &timeout));

  ASSERT_EQ(0, test.result);
  ASSERT_EQ(0, serial_close(handle));
}

TEST_F(SerialModemTest, EventAbortNotWaiting)
{
  ASSERT_EQ(0, serial_open(handle))
    << "Message: " << serial_error(handle) << "; "
    << "Error initialising: " << strerror(errno) << " (" << errno << ")";

  ASSERT_EQ(0, serial_abortwaitformodemevent(handle));

  ASSERT_EQ(0, serial_close(handle));
}
