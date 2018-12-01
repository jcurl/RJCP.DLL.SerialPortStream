////////////////////////////////////////////////////////////////////////////////
// PROJECT : libnserial
//  (C) Jason Curl, 2016-2017.
//
// FILE : modem.c
//
// Published under the MIT license.
//
// AUTHOR : Jason Curl
//
// DESCRIPTION : Get and set modem signals
//
////////////////////////////////////////////////////////////////////////////////

#include "config.h"

#include <pthread.h>
#include <semaphore.h>
#include <termios.h>
#include <sys/ioctl.h>
#include <errno.h>
#ifdef HAVE_LINUX_SERIAL_ICOUNTER_STRUCT
#include <linux/serial.h>
#endif

#define NSERIAL_EXPORTS
#include "nserial.h"
#include "errmsg.h"
#include "log.h"
#include "serialhandle.h"
#include "modem.h"

static int getmodemsignal(int fd, int signal, int *outsignal)
{
  int serial;
  if (ioctl(fd, TIOCMGET, &serial) == -1) {
    return -1;
  }
  *outsignal = (serial & signal) ? 1 : 0;
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_getdcd(struct serialhandle *handle, int *dcd)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  serial_seterror(handle, ERRMSG_OK);
  if (dcd == NULL) {
    serial_seterror(handle, ERRMSG_INVALIDPARAMETER);
    errno = EINVAL;
    return -1;
  }

  if (handle->fd == -1) {
    serial_seterror(handle, ERRMSG_SERIALPORTNOTOPEN);
    errno = EBADF;
    return -1;
  }

  if (getmodemsignal(handle->fd, TIOCM_CAR, dcd) == -1) {
    serial_seterror(handle, ERRMSG_IOCTL);
    return -1;
  }
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_getri(struct serialhandle *handle, int *ri)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  serial_seterror(handle, ERRMSG_OK);
  if (ri == NULL) {
    serial_seterror(handle, ERRMSG_INVALIDPARAMETER);
    errno = EINVAL;
    return -1;
  }

  if (handle->fd == -1) {
    serial_seterror(handle, ERRMSG_SERIALPORTNOTOPEN);
    errno = EBADF;
    return -1;
  }

  if (getmodemsignal(handle->fd, TIOCM_RI, ri) == -1) {
    serial_seterror(handle, ERRMSG_IOCTL);
    return -1;
  }
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_getdsr(struct serialhandle *handle, int *dsr)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  serial_seterror(handle, ERRMSG_OK);
  if (dsr == NULL) {
    serial_seterror(handle, ERRMSG_INVALIDPARAMETER);
    errno = EINVAL;
    return -1;
  }

  if (handle->fd == -1) {
    serial_seterror(handle, ERRMSG_SERIALPORTNOTOPEN);
    errno = EBADF;
    return -1;
  }

  if (getmodemsignal(handle->fd, TIOCM_DSR, dsr) == -1) {
    serial_seterror(handle, ERRMSG_IOCTL);
    return -1;
  }
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_getcts(struct serialhandle *handle, int *cts)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  serial_seterror(handle, ERRMSG_OK);
  if (cts == NULL) {
    serial_seterror(handle, ERRMSG_INVALIDPARAMETER);
    errno = EINVAL;
    return -1;
  }

  if (handle->fd == -1) {
    serial_seterror(handle, ERRMSG_SERIALPORTNOTOPEN);
    errno = EBADF;
    return -1;
  }

  if (getmodemsignal(handle->fd, TIOCM_CTS, cts) == -1) {
    serial_seterror(handle, ERRMSG_IOCTL);
    return -1;
  }
  return 0;
}

NSERIAL_EXPORT int WINAPI serial_setdtr(struct serialhandle *handle, int dtr)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  handle->modembits.dtr = (dtr == 0) ? 0 : 1;
  if (handle->fd == -1) return 0;

  return serial_setdtrinternal(handle);
}

int serial_setdtrinternal(struct serialhandle *handle)
{
  int serial = TIOCM_DTR;
  int cmd = handle->modembits.dtr ? TIOCMBIS : TIOCMBIC;
  if (ioctl(handle->fd, cmd, &serial) == -1) {
    serial_seterror(handle, ERRMSG_IOCTL);
    return -1;
  }

  return 0;
}

NSERIAL_EXPORT int WINAPI serial_getdtr(struct serialhandle *handle, int *dtr)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  serial_seterror(handle, ERRMSG_OK);
  if (dtr == NULL) {
    serial_seterror(handle, ERRMSG_INVALIDPARAMETER);
    errno = EINVAL;
    return -1;
  }

  if (handle->fd == -1) {
    *dtr = handle->modembits.dtr;
    return 0;
  }

  int serial;
  if (ioctl(handle->fd, TIOCMGET, &serial) == -1) {
    serial_seterror(handle, ERRMSG_IOCTL);
    return -1;
  }
  *dtr = (serial & TIOCM_DTR) ? 1 : 0;

  return 0;
}

NSERIAL_EXPORT int WINAPI serial_setrts(struct serialhandle *handle, int rts)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  handle->modembits.rts = (rts == 0) ? 0 : 1;
  if (handle->fd == -1) return 0;

  return serial_setrtsinternal(handle);
}

int serial_setrtsinternal(struct serialhandle *handle)
{
  int serial = TIOCM_RTS;
  int cmd = handle->modembits.rts ? TIOCMBIS : TIOCMBIC;
  if (ioctl(handle->fd, cmd, &serial) == -1) {
    serial_seterror(handle, ERRMSG_IOCTL);
    return -1;
  }

  return 0;
}

NSERIAL_EXPORT int WINAPI serial_getrts(struct serialhandle *handle, int *rts)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  serial_seterror(handle, ERRMSG_OK);
  if (rts == NULL) {
    serial_seterror(handle, ERRMSG_INVALIDPARAMETER);
    errno = EINVAL;
    return -1;
  }

  if (handle->fd == -1) {
    *rts = handle->modembits.rts;
    return 0;
  }

  int serial;
  if (ioctl(handle->fd, TIOCMGET, &serial) == -1) {
    serial_seterror(handle, ERRMSG_IOCTL);
    return -1;
  }
  *rts = (serial & TIOCM_RTS) ? 1 : 0;

  return 0;
}

#ifdef HAVE_LINUX_SERIAL_ICOUNTER_STRUCT
// Please note, that this method by be aborted at any time with
// "pthread_cancel" and the thread is set to be PTHREAD_CANCEL_ASYNCHRONOUS.
// This means, it must by async cancel safe. You cannot allocate heap, or
// call functions that are not async cancel safe.
//
// The only exception is "ioctl" as it appears that this is not implemented
// to be a cancellation point in Linux.
static serialmodemevent_t waitformodemevent(struct modemstate *mstate)
{
  int icount;
  int signals = 0;
  if (mstate->waitevent & MODEMEVENT_DCD) signals |= TIOCM_CAR;
  if (mstate->waitevent & MODEMEVENT_RI) signals |= TIOCM_RI;
  if (mstate->waitevent & MODEMEVENT_DSR) signals |= TIOCM_DSR;
  if (mstate->waitevent & MODEMEVENT_CTS) signals |= TIOCM_CTS;

  struct serial_icounter_struct icounter = {0, };
  if (ioctl(mstate->handle->fd, TIOCGICOUNT, &icounter) < 0) {
    // Not all serial port drivers support TIOCGICOUNT.
    //
    // If we get an error here, we assume that it's not supported and
    // we check the signals, which is a little less reliable.

    icount = FALSE;
  } else {
    icount = TRUE;
  }

  int ctssignal = -1;
  int dsrsignal = -1;
  int risignal = -1;
  int dcdsignal = -1;
  if ((mstate->waitevent & MODEMEVENT_CTS) &&
      getmodemsignal(mstate->handle->fd, TIOCM_CTS, &ctssignal) == -1)
    ctssignal = -1;
  if ((mstate->waitevent & MODEMEVENT_DSR) &&
      getmodemsignal(mstate->handle->fd, TIOCM_DSR, &dsrsignal) == -1)
    dsrsignal = -1;
  if ((mstate->waitevent & MODEMEVENT_DCD) &&
      getmodemsignal(mstate->handle->fd, TIOCM_CAR, &dcdsignal) == -1)
    dcdsignal = -1;
  if ((mstate->waitevent & MODEMEVENT_RI) &&
      getmodemsignal(mstate->handle->fd, TIOCM_RI, &risignal) == -1)
    risignal = -1;

  if (ioctl(mstate->handle->fd, TIOCMIWAIT, signals) < 0) {
    if (errno == EAGAIN || errno == EWOULDBLOCK || errno == EINTR) {
      return MODEMEVENT_NONE;
    }

    // Some USB drivers don't support modem signals.
    mstate->serialerror = ERRMSG_IOCTL;
    mstate->posixerrno = errno;
    return MODEMEVENT_ERROR;
  }

  struct serial_icounter_struct ocounter = {0, };
  if (icount && ioctl(mstate->handle->fd, TIOCGICOUNT, &ocounter) < 0) {
    icount = FALSE;
  }

  // TODO: Do we just raise an event in case on from zero to one?
  //  CTS 0->1 and 1->0.
  //  DSR 0->1 and 1->0.
  //  DCD 0->1 and 1->0.
  //  RI  0->1 only.
  int rsignals = MODEMEVENT_NONE;
  if (icount) {
    if (ocounter.cts != icounter.cts) rsignals |= MODEMEVENT_CTS;
    if (ocounter.dsr != icounter.dsr) rsignals |= MODEMEVENT_DSR;
    if (ocounter.rng != icounter.rng) rsignals |= MODEMEVENT_RI;
    if (ocounter.dcd != icounter.dcd) rsignals |= MODEMEVENT_DCD;
  } else {
    int rctssignal = -1;
    int rdsrsignal = -1;
    int rrisignal = -1;
    int rdcdsignal = -1;
    if (ctssignal != -1 &&
	getmodemsignal(mstate->handle->fd, TIOCM_CTS, &rctssignal) == -1)
      rctssignal = -1;
    if (dsrsignal != -1 &&
	getmodemsignal(mstate->handle->fd, TIOCM_DSR, &rdsrsignal) == -1)
      rdsrsignal = -1;
    if (dcdsignal != -1 &&
	getmodemsignal(mstate->handle->fd, TIOCM_CAR, &rdcdsignal) == -1)
      rdcdsignal = -1;
    if (risignal != -1 &&
	getmodemsignal(mstate->handle->fd, TIOCM_RI, &rrisignal) == -1)
      rrisignal = -1;

    if (rctssignal != -1 && ctssignal != -1)
      rsignals |= (rctssignal != ctssignal) ? MODEMEVENT_CTS : MODEMEVENT_NONE;
    if (rdsrsignal != -1 && dsrsignal != -1)
      rsignals |= (rdsrsignal != dsrsignal) ? MODEMEVENT_DSR : MODEMEVENT_NONE;
    if (rdcdsignal != -1 && dcdsignal != -1)
      rsignals |= (rdcdsignal != dcdsignal) ? MODEMEVENT_DCD : MODEMEVENT_NONE;
    if (rrisignal != -1 && risignal != -1)
      rsignals |= (rrisignal != risignal) ? MODEMEVENT_RI : MODEMEVENT_NONE;
  }

  return (serialmodemevent_t)(rsignals & mstate->waitevent);
}

static void *modemeventthread(void *ptr)
{
  struct modemstate *mstate = (struct modemstate *)ptr;
  serialmodemevent_t result;

  // The IOCTL blocks until there is a change or a signal. There's no other
  // way to get out if you don't want a signal! So we have to ensure that we
  // can cancel the thread at any time and be careful about it.
  int oldstate, oldtype;
  pthread_setcancelstate(PTHREAD_CANCEL_ENABLE, &oldstate);
  pthread_setcanceltype(PTHREAD_CANCEL_ASYNCHRONOUS, &oldtype);

  int wait = TRUE;
  while (wait) {
    result = waitformodemevent(mstate);
    if (result == -1) {
      // We have a serious issue. We stop the thread. The error
      // was already set by waitformodemevent.
      mstate->eventresult = MODEMEVENT_NONE;
      wait = FALSE;
    }
    if (result != MODEMEVENT_NONE) {
      mstate->eventresult = result;
      wait = FALSE;
    }

    // else we just keep polling.
  }

  sem_post(&(mstate->modemevent));
  pthread_exit(NULL);
}
#endif

static int entercritsection(struct serialhandle *handle)
{
  int result;
  result = pthread_mutex_lock(&(handle->modemmutex));
  if (result) {
    nslog(handle, NSLOG_CRIT,
	  "modem: lock mutex failed: errno=%d", result);
    errno = result;
    serial_seterror(handle, ERRMSG_MUTEXLOCK);
    return -1;
  }
  return 0;
}

static int exitcritsection(struct serialhandle *handle)
{
  int result;
  result = pthread_mutex_unlock(&(handle->modemmutex));
  if (result) {
    nslog(handle, NSLOG_CRIT,
	  "modem: unlock mutex failed: errno=%d", result);
    errno = result;
    serial_seterror(handle, ERRMSG_MUTEXUNLOCK);
    return -1;
  }
  return 0;
}

NSERIAL_EXPORT serialmodemevent_t WINAPI serial_waitformodemevent(struct serialhandle *handle, serialmodemevent_t event)
{
  if (handle == NULL) {
    errno = EINVAL;
    return MODEMEVENT_ERROR;
  }

#ifdef HAVE_TIOCMIWAIT
  if (handle->fd == -1) {
    serial_seterror(handle, ERRMSG_SERIALPORTNOTOPEN);
    errno = EBADF;
    return MODEMEVENT_ERROR;
  }

  if (entercritsection(handle)) return MODEMEVENT_ERROR;

  if (handle->modemstate) {
    exitcritsection(handle);
    nslog(handle, NSLOG_WARNING, "waitformodemevent: already running");
    serial_seterror(handle, ERRMSG_MODEMEVENT_RUNNING);
    errno = EINVAL;
    return MODEMEVENT_ERROR;
  }

  if ((event &
       (MODEMEVENT_DCD | MODEMEVENT_RI |
	MODEMEVENT_DSR | MODEMEVENT_CTS)) == 0) {
    if (exitcritsection(handle)) return MODEMEVENT_ERROR;
    return MODEMEVENT_NONE;
  }

  int result;

  struct modemstate mstate = {0, };
  mstate.handle = handle;
  mstate.waitevent = event;
  if (sem_init(&(mstate.modemevent), 0, 0) == -1) {
    nslog(handle, NSLOG_CRIT,
	  "waitformodemevent: seminit(modemevent): errno=%d", errno);
    exitcritsection(handle);
    serial_seterror(handle, ERRMSG_SEMINIT);
    return MODEMEVENT_ERROR;
  }
  handle->modemstate = &mstate;
  if (exitcritsection(handle)) {
    handle->modemstate = NULL;
    sem_destroy(&(mstate.modemevent));
    return MODEMEVENT_ERROR;
  }

  // We create the thread to wait on it afterwards, as we can
  // make that thread cancellable.
  result = pthread_create(&(handle->modemthread), NULL,
			  modemeventthread, &mstate);
  if (result) {
    nslog(handle, NSLOG_CRIT,
	  "waitformodemevent: pthread_create: errno=%d", result);
    errno = result;
  }

  // Wait for an abort, or that the thread is closed
  if (!result) {
    int wait = TRUE;
    int semresult;
    while (wait) {
      semresult = sem_wait(&(mstate.modemevent));
      if (semresult) {
	if (errno == EINTR) {
	  // Just an interrupt, we continue the loop
	  semresult = 0;
	} else {
	  nslog(handle, NSLOG_CRIT,
		"waitformodemevent: sem_wait: errno=%d", errno);
	  wait = FALSE;
	  result = -1;
	}
      } else {
	if (mstate.modemeventabort) {
	  result = pthread_cancel(handle->modemthread);
	}
	wait = FALSE;
      }
    }
  }

  if (!result) {
    result = pthread_join(handle->modemthread, NULL);
    if (result) {
      nslog(handle, NSLOG_CRIT,
	    "waitformodemevent: pthread_join: errno=%d", result);
      errno = result;
    }
  }

  if (!result) {
    if (mstate.serialerror != 0) {
      nslog(handle, NSLOG_CRIT,
	    "waitformodemevent: error in modemeventthread: errno=%d",
	    mstate.posixerrno);
      errno = mstate.posixerrno;
      serial_seterror(handle, mstate.serialerror);
      result = -1;
    }
  }

  if (entercritsection(handle)) result = -1;
  handle->modemstate = NULL;
  if (exitcritsection(handle)) result = -1;

  result = sem_destroy(&(mstate.modemevent));
  if (!result) {
    nslog(handle, NSLOG_CRIT,
	  "waitformodemevent: sem_destroy: errno=%d",
	  errno);
  }

  if (!result) {
    return mstate.eventresult & event;
  } else {
    return MODEMEVENT_ERROR;
  }
#else
  serial_seterror(handle, ERRMSG_NOSYS);
  errno = ENOSYS;
  return MODEMEVENT_ERROR;
#endif
}

NSERIAL_EXPORT int WINAPI serial_abortwaitformodemevent(struct serialhandle *handle)
{
  if (handle == NULL) {
    errno = EINVAL;
    return -1;
  }

  if (handle->fd == -1) {
    serial_seterror(handle, ERRMSG_SERIALPORTNOTOPEN);
    errno = EBADF;
    return -1;
  }

  int result = 0;
  if (entercritsection(handle)) return -1;
  if (handle->modemstate != NULL) {
    handle->modemstate->modemeventabort = TRUE;
    result = sem_post(&(handle->modemstate->modemevent));
    if (result == -1 && errno == EOVERFLOW) {
      // The thread has already been closed and posted its event.
      // Ignore it.
      result = 0;
      errno = 0;
    } else {
      nslog(handle, NSLOG_CRIT,
	    "abortwaitformodemevent: sem_post: errno=%d",
	    result);
    }
  }
  if (exitcritsection(handle)) result = -1;

  if (result) return -1;
  return 0;
}
