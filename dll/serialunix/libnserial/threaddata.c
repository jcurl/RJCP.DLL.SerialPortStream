////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016.
//
// FILE : threaddata.c
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Defines thread specific data such as error codes.
//
////////////////////////////////////////////////////////////////////////////////

#include <pthread.h>

#define NSERIAL_EXPORTS
#include "threaddata.h"

static pthread_key_t threaddatakey;
static pthread_once_t threaddatakeycreate = PTHREAD_ONCE_INIT;
static int pthreaderr = 0;

static void freethreadstate(void *value)
{
  free(value);
  pthread_setspecific(threaddatakey, NULL);
}

static void makethreadstate()
{
  pthreaderr = pthread_key_create(&threaddatakey, freethreadstate);
}

int threaddata_init()
{
  int result;

  result = pthread_once(&threaddatakeycreate, makethreadstate);
  if (result == -1 || pthreaderr == -1) {
    return -1;
  }
  return 0;
}

int threaddata_terminate()
{
  return 0;
}

struct threadstate *getthreaddata()
{
  struct threadstate *data;
  data = (struct threadstate *)pthread_getspecific(threaddatakey);
  if (data == NULL) {
    data = malloc(sizeof(struct threadstate));
    pthread_setspecific(threaddatakey, data);
  }
  return data;
}
