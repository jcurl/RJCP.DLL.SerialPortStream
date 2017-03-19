/*! \file netfx.h
 *  \brief .NET compatibility layer
 */

#ifndef NSERIAL_NETFX_H
#define NSERIAL_NETFX_H

#include <stdlib.h>
#include <sys/types.h>
#include "nserial.h"

#ifdef __cplusplus
extern "C" {
#endif

typedef enum netfx_errno {
  NETFX_UNKNOWN = -1,       /*< All unknown errors mapped here */
  NETFX_OK = 0,             /*< No error */
  NETFX_EINVAL = 1,         /*< ArgumentException */
  NETFX_EACCES = 2,         /*< UnauthorizedAccessException */
  NETFX_ENOMEM = 3,         /*< OutOfMemoryException */
  NETFX_EBADF = 4,          /*< InvalidOperationException */
  NETFX_ENOSYS = 5,         /*< PlatformNotSupportedException */
  NETFX_EIO = 6,            /*< IOException */
  NETFX_EAGAIN = 7,         /*< No error */
  NETFX_EWOULDBLOCK = 8,    /*< No error */
  NETFX_EINTR = 9           /*< No error */
} netfx_errno_t;

/*! \brief Convert a C errno to a constant
 *
 * Take the given errno, and convert it to a constant that is the same value
 * for all operating systems. This is useful for managed code that doesn't
 * have access to a C-Compiler to determine what an error code is for the
 * Operating System this was compiled. One example is .NET Standard 1.5.
 *
 * While Mono has this code, we use this function instead for convenience
 * and consistency across frameworks.
 *
 * \param errno the error number as returned by the Operating System.
 * \return A constant defined by this library that can be used to map the
 *   error code to an exception type in managed code.
 */
NSERIAL_EXPORT int WINAPI netfx_errno(int lerrno);

/*! \brief Convert a C errno to a string
 *
 * Take the given errno, and convert it to a string that can provided
 * managed code more information.
 *
 * \param errno the error number as returned by the Operating System.
 * \return A string describing the error, as given by the C-library.
 */
NSERIAL_EXPORT const char *WINAPI netfx_errstring(int lerrno);

#ifdef __cplusplus
}
#endif
#endif
