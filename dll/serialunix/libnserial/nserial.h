/*! \file nserial.h
 *  \brief The main header file describing the functionality of this library.
 */

#ifndef NSERIAL_H
#define NSERIAL_H

#include <stdlib.h>
#include <sys/types.h>

#ifdef __cplusplus
extern "C" {
#endif

/*! \def NSERIAL_EXPORT
 * \brief Mark the function as a library element that should be exported.
 *
 * The NSERIAL_EXPORT declares the function to be imported when using with
 * Windows targets. Internally, it is used for the library to mark methods that
 * should be exported. For Unix targets, this macro is empty.
 */

/*! \def WINAPI
 * \brief Mark the function as a library element conforming to Windows API
 * specifications.
 *
 * The WINAPI macro declares the method to be exported using standard windows
 * API calling convention. For 32-bit this is the "stdcall" calling
 * convention. For 64-bit this is the "fastcall" calling convention.
 */

#if defined(WIN32)
#define WINAPI __stdcall
#if defined(NSERIAL_EXPORTS)
#define NSERIAL_EXPORT __declspec(dllexport)
#else
#define NSERIAL_EXPORT __declspec(dllimport)
#endif
#else
#define NSERIAL_EXPORT
#define WINAPI
#endif

/*! \struct serialhandle
 * \brief An anonymous handle for internal serial port handling.
 *
 * The serial handle is provided by the serialopen method and contains internal
 * private data that should not be interpreted by your applications (as it may
 * change in the future).  Pass it as you would a void pointer for your
 * architecture to the methods in this C-module.
 */
struct serialhandle;

/*! \brief Get the version of this library.
 *
 * Get the version number of the library as a string. The result is a static
 * string, so the buffer, is allocated by the library and does not need to be
 * freed.
 *
 * The version is in the format MAJOR.MINOR.PATCH[.BUILD]. The version
 * represents the API version of the library, so that an increase in the MAJOR
 * indicates an incompatible change with the previous version. An increase in
 * the MINOR version indicates a new compatible change. The PATCH indicates a
 * compatible change that doesn't change the API, but usually introduces a
 * bugfix only.
 *
 * \return A static string that contains the version number of this library.
 */
NSERIAL_EXPORT const char *WINAPI serial_version();

/*! \brief Initialise the library and return a handle that can be used for all
 *  subsequent calls.
 *
 * Initialise internal datastructures and return a handle that can be used to
 * later open a connection to a serial port. Be sure to release the resources
 * with serial_terminate() to avoid leaking of resources.
 *
 * \return A structure handle (pointer) that is used for handling a specific
 *   connection. Be sure to free memory for this structure later by calling
 *   serial_terminate().
 */
NSERIAL_EXPORT struct serialhandle *WINAPI serial_init();

/*! \brief Close the device and release the resources.
 *
 * Release the resources allocated by the serial_init() function. This frees the
 * structure (you should not free this structure yourself!) and allows the
 * serial port to be opened by another application or process. If the serial
 * port was previously open, it is automatically closed.
 *
 * \param handle the handle as returned by the serial_init() function.
 */
NSERIAL_EXPORT void WINAPI serial_terminate(struct serialhandle *handle);

/*! \brief Set the name of the device when opening the serial port
 *
 * Set the string to use for the device name.
 *
 * \param handle The handle as returned by the serial_init() function.
 * \param device A pointer to the string containing the device name.
 * \returns 0 if the operation was successful.
 * \returns -1 if there was was an error.
 * \exception EINVAL Invalid parameter given.
 */
NSERIAL_EXPORT int WINAPI serial_setdevicename(struct serialhandle *handle, const char *device);

/*! \brief Get the name of the device that was used to initialise the handle.
 *
 * Get the string used for the device name. The string returned is const, and
 * should not be modified. Doing so will result in undefined behaviour.
 *
 * \param handle The handle as returned by the serial_init() function.
 * \returns A pointer to the device name, NULL if there is a problem.
 * \exception EINVAL invalid handle was provided.
 */
NSERIAL_EXPORT const char *WINAPI serial_getdevicename(struct serialhandle *handle);

/*! \brief Open the serial port device to be ready for reading and writing.
 *
 * Opens the serial port using the properties previously set, to allow for
 * reading and writing.
 *
 * In case of an error, you can check errno to get more details. You can also
 * use the serial_error() method to get a string that describes the type
 * of error in more detail (e.g. it could help to identify the particular
 * property that is not supported, or other diagnostic information more
 * specific than the errno itself).
 *
 * \param handle The handle as returned by serial_init().
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided.
 */
NSERIAL_EXPORT int WINAPI serial_open(struct serialhandle *handle);

/*! \brief Get the current properties of the serial port.
 *
 * After opening the serial port, get the settings of the serial port and set
 * the local properties. Not all properties are updated from this
 * method. Those that aren't updated remain as they were before this method
 * call. If a property is read and this module doesn't understand it, a
 * default property may be set. This library only guarantees that properties
 * set with this library will reliably result in the same setings.
 *
 * The serial port settings based on the underlying OS may be complex. Setting
 * the properties immediately after is likely to reconfigure the serial port.
 *
 * Properties that are updated are: data bits, stop bits, parity, hand shaking
 * and the baud rate.
 *
 * \param handle the handle as returned by the serial_init() function
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided.
 * \exception EIO there was a problem getting the properties. Use
 *   serial_error() to get more details.
 */
NSERIAL_EXPORT int WINAPI serial_getproperties(struct serialhandle *handle);

/*! \brief Set the properties of the serial port
 *
 * After opening the serial port, set the properties. By separating from
 * the open call, we can open the serial port directly without affecting
 * any settings for advanced usage.
 *
 * If there is a problem setting the properties (this method returns -1), then
 * the properties of the serial port are undefined. Some properties may be
 * set, some may not.
 *
 * \param handle the handle as returned by the serial_init() function
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided.
 * \exception EIO there was a problem setting a property. Use serial_error()
 *   to get more details.
 */
NSERIAL_EXPORT int WINAPI serial_setproperties(struct serialhandle *handle);

/*! \brief Release resources and close the device.
 *
 * Closes access to the serial port, stops any current operations for reading
 * and writing.  The port is closed, but can be reopened with the serial_open()
 * function again later with the same settings as before, without having to
 * reallocate resources.
 *
 * \param handle the handle as returned by the serial_init() function.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided.
 */
NSERIAL_EXPORT int WINAPI serial_close(struct serialhandle *handle);

/*! \brief Get the string error for why serial_open might have failed.
 *
 * Get the error string on why serial_open() failed, so that the user can
 * correct the error. There are many modes of failures. Setting the properties
 * only performs a very basic check, but may not necessarily fail, even though
 * the serial port cannot be opened.
 *
 * In case that there was no error, NULL is returned. If you provide a NULL
 * handle, NULL is also returned, but errno is not set (as it's not
 * differentiable to no error).
 *
 * \param handle the handle as returned by the serial_init() function.
 * \return NULL if there was no error or no error information is available.
 * \return a pointer to a string that describes the error.
 */
NSERIAL_EXPORT const char *WINAPI serial_error(struct serialhandle *handle);

/*! \brief Get the File Descriptor for the serial port.
 *
 * Get the file descriptor for the serial port that is open.
 *
 * \param handle The handle returned by serial_init().
 * \return The file descriptor.
 * \return If it is -1, the file descriptor is not valid  or there was an
 *   error. See errno for more details.
 * \exception EINVAL invalid handle was provided.
 * \exception EBADF serial port is not open.
 */
NSERIAL_EXPORT int WINAPI serial_getfd(struct serialhandle *handle);

/*! \brief Check if the serial port is currently in use by this library.
 *
 * Check if the serial port is currently in use by this library, by a previous
 * call the the serial_oen() function.
 *
 * \param handle The handle returned by the serial_init() function call.
 * \param isopen On successful return indicates if the serial port is open or
 *   not. Non-zero is open, Zero is closed.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided or isopen is NULL.
 */
NSERIAL_EXPORT int WINAPI serial_isopen(struct serialhandle *handle, int *isopen);

/*! \brief Get an array of supported baud rates from the library.
 *
 * You can get an array of all the baudrates supported by this library. This can
 * be used for giving the user a fixed option of baudrates to use. The last
 * element is zero, so you can detect the end of the list.
 *
 * \return An array of supported baud rates, with the last element being 0.
 */
NSERIAL_EXPORT int *WINAPI serial_getsupportedbaudrates();

/*! \brief Set the baud rate for the serial port.
 *
 * Set the baud rate of the serial port. The baud rate specified must be
 * supported by the Operating System, else -1 is returned. If the serial port is
 * currently opened, this will update the baud rate of the serial port.
 *
 * If custom baud rates are supported, this method will not return an
 * exception on setting the property, but first only when trying to set the
 * serial port properties with serial_setproperties() after opening the serial
 * port.
 *
 * If the serial port is already open when this property is set, the serial
 * port settings are set automatically. If the setting could not be applied
 * and results in an error, -1 is returned. The state of this property is then
 * undefined and you should attempt to set the property back to the original
 * value.
 *
 * \param handle The handle returned by serial_init().
 * \param baud The baud rate to set to. Must not be zero and must be a baud rate
 *   that is supported by the Operating System.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided, or an unknown baudrate was
 *   given.
 */
NSERIAL_EXPORT int WINAPI serial_setbaud(struct serialhandle *handle, int baud);

/*! \brief Get the currently set baud rate.
 *
 * Get the currently set baud rate.
 *
 * \param handle The handle returned by serial_init().
 * \param baud On success, the currently set baud rate.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided
 */
NSERIAL_EXPORT int WINAPI serial_getbaud(struct serialhandle *handle, int *baud);

/*! \brief Set the number of data bits for serial communication.
 *
 * Set the number of data bits used for serial communication.
 *
 * If the serial port is already open when this property is set, the serial
 * port settings are set automatically. If the setting could not be applied
 * and results in an error, -1 is returned. The state of this property is then
 * undefined and you should attempt to set the property back to the original
 * value.
 *
 * \param handle THe handle returned by serial_init().
 * \param databits Set the number of stop bits. This should be a value
 *   between 5 and 8.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided, or the databits is out of
 *   range.
 */
NSERIAL_EXPORT int WINAPI serial_setdatabits(struct serialhandle *handle, int databits);

/*! \brief Get the number of data bits set for serial communication.
 *
 * Get the number of databits set for serial communication.
 *
 * \param handle The handle returned by serial_init().
 * \param databits On successful return the number of databits between 5 and 8.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided, or the databits is NULL.
 */
NSERIAL_EXPORT int WINAPI serial_getdatabits(struct serialhandle *handle, int *databits);

/*! \brief Define the parity required when opening the serial port.
 *
 * The parity of the serial port. This enumeration is mapped internally to the
 * parity required by your Operating System.
 */
typedef enum serialparity {
  NOPARITY = 0,    /*!< Configure the serial port to use no parity. */
  ODD = 1,     /*!< Configure the serial port to use odd parity. */
  EVEN = 2,    /*!< Configure the serial port to use even parity. */
  MARK = 3,    /*!< Configure the serial port to use mark parity. */
  SPACE = 4,   /*!< Configure the serial port to use space parity. */
} serialparity_t;

/*! \brief Set the parity to use for communication.
 *
 * Set the parity to be used when opening the serial port. If the parity is
 * defined, but not suppored by the platform, an error will be returned by the
 * serial_setproperties() function call.
 *
 * If the serial port is already open when this property is set, the serial
 * port settings are set automatically. If the setting could not be applied
 * and results in an error, -1 is returned. The state of this property is then
 * undefined and you should attempt to set the property back to the original
 * value.
 *
 * \param handle The handle returned by serial_init().
 * \param parity The parity to set the serial port to.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided, or the parity is out of
 *   range.
 */
NSERIAL_EXPORT int WINAPI serial_setparity(struct serialhandle *handle, serialparity_t parity);

/*! \brief Get the parity used for communication.
 *
 * Get the parity used for communication.
 *
 * \param handle The handle returned by serial_init().
 * \param parity The parity to set the serial port to.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided, or the parity is out of
 *   range.
 */
NSERIAL_EXPORT int WINAPI serial_getparity(struct serialhandle *handle, serialparity_t *parity);

/*! \brief Define the number of stop bits when opening the serial port.
 *
 * Define the number of stop bits to use when opening the serial port.
 */
typedef enum serialstopbits {
  ONE = 0,     /*!< Configure to have 1 stop bit. */
  ONE5 = 1,    /*!< Configure to have 1.5 stop bits. */
  TWO = 2,     /*!< Configure to have 2 stop bits. */
} serialstopbits_t;

/*! \brief Set the number of stop bits to use for communication.
 *
 * Set the number of stop bits to be used when opening the serial port. If the
 * platform doesn't support the number of stop bits requested, the function
 * call serial_setproperties() will fail.
 *
 * If the serial port is already open when this property is set, the serial
 * port settings are set automatically. If the setting could not be applied
 * and results in an error, -1 is returned. The state of this property is then
 * undefined and you should attempt to set the property back to the original
 * value.
 *
 * \param handle The handle returned by serial_init().
 * \param stopbits The number of stop bits to set the serial port to.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided, or the stop bits is out of
 *   range.
 */
NSERIAL_EXPORT int WINAPI serial_setstopbits(struct serialhandle *handle, serialstopbits_t stopbits);

/*! \brief Get the number of stop bits used for communication.
 *
 * Get the number of stop bits used for communication.
 *
 * \param handle The handle returned by serial_init().
 * \param stopbits On success, the current setting for stop bits.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided, or stopbits is NULL.
 */
NSERIAL_EXPORT int WINAPI serial_getstopbits(struct serialhandle *handle, serialstopbits_t *stopbits);

/*! \brief Define the type of handshaking to use with other devices.
 *
 * The type of handshaking to use with other devices.
 */
typedef enum serialhandshake {
  NOHANDSHAKE = 0,            /*!< No handshaking */
  XON = 1,                    /*!< Software flow control handshaking */
  RTS = 2,                    /*!< Hardware RTS flow control */
  DTR = 4,                    /*!< Hardware DTR flow control */
  RTSXON = RTS | XON,         /*!< RTS and XON handshaking */
  DTRXON = DTR | XON,         /*!< DTR and XON handshaking */
  DTRRTS = DTR | RTS,         /*!< DTR and RTS handshaking */
  DTRRTSXON = DTR | RTS | XON /*!< DTR, RTS and XON handshaking */
} serialhandshake_t;

/*! \brief Set the handshaking mode for the serial port.
 *
 * Set the handshaking mode to use on the serial port. If the type of
 * handshaking is defined but not supported by the underlying platform, a call
 * to serial_setproperties() will return an error.
 *
 * If the serial port is already open when this property is set, the serial
 * port settings are set automatically. If the setting could not be applied
 * and results in an error, -1 is returned. The state of this property is then
 * undefined and you should attempt to set the property back to the original
 * value.
 *
 * \param handle The handle returned by serial_init().
 * \param handshake The handshaking mode to use.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided, or handshake is out of range.
 */
NSERIAL_EXPORT int WINAPI serial_sethandshake(struct serialhandle *handle, serialhandshake_t handshake);

/*! \brief Get the handshaking mode for the serial port.
 *
 * Get the handshaking mode set for the serial port.
 *
 * \param handle The handle returned by serial_init().
 * \param handshake On success, the current value of serial handshake.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided, or handshake is NULL.
 */
NSERIAL_EXPORT int WINAPI serial_gethandshake(struct serialhandle *handle, serialhandshake_t *handshake);

/*! \brief Set the property TxContinueOnXOff
 *
 * Set the property TxContinueOnXOff. If enabled, transmission continues after
 * the input buffer (data received from the remote terminal) has come within
 * XoffLim bytes of being full and the driver has transmitted the XoffChar
 * character to stop receiving bytes. If not enabled, transmission does not
 * continue until the input buffer is within XonLim bytes of being empty and
 * the driver has transmitted the XonChar character to resume reception.
 *
 * This property may not be supported on all platforms, and is provided for
 * compatibility with Windows. If it is not suported, the option is ignored.
 *
 * \param handle The handle returned by serial_init().
 * \param txcontinueonxoff The boolean value to set for TxContinueOnXOff
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided.
 */
NSERIAL_EXPORT int WINAPI serial_settxcontinueonxoff(struct serialhandle *handle, int txcontinueonxoff);

/*! \brief Get the property TxContinueOnXOff
 *
 * Get the property TxContinueOnXOff.
 *
 * \param handle The handle returned by serial_init().
 * \param txcontinueonxoff On success, contains the value of the property
 *   TxContinueOnXOff
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided, or txcontinueonxoff was NULL.
 */
NSERIAL_EXPORT int WINAPI serial_gettxcontinueonxoff(struct serialhandle *handle, int *txcontinueonxoff);

/*! \brief Set the property DiscardNull
 *
 * Set the property DiscardNull. This option is emulated in software.
 *
 * \param handle The handle returned by serial_init().
 * \param discardnull The boolean value to set for DiscardNull.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided.
 */
NSERIAL_EXPORT int WINAPI serial_setdiscardnull(struct serialhandle *handle, int discardnull);

/*! \brief Get the property DiscardNull
 *
 * Get the property DiscardNull.
 *
 * \param handle The handle returned by serial_init().
 * \param discardnull On success, contains the value of the property
 *    DiscardNull.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided, or discardnull was NULL.
 */
NSERIAL_EXPORT int WINAPI serial_getdiscardnull(struct serialhandle *handle, int *discardnull);

/*! \brief Set the property XOnLimit
 *
 * Set the property XOnLimit. This property should be used rarely, if
 * at all. An operating system may not support this property, in which
 * case this propery will have no effect.
 *
 * This property exists for compatibility with Windows. If the underlying
 * platform doesn't support setting the upper input buffer limit for sending
 * the XOn character, this property will be ignored. The default value is
 * 2048 bytes.
 *
 * \param handle The handle returned by serial_init().
 * \param xonlimit The number of bytes for the XOnLimit.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided.
 */
NSERIAL_EXPORT int WINAPI serial_setxonlimit(struct serialhandle *handle, int xonlimit);

/*! \brief Get the property XOnLimit
 *
 * Get the property XOnLimit. This property should be used rarely, if
 * at all. An operating system may not support this property, in which
 * case this propery will have no effect. Getting the property will return
 * the last set value, if it is used or not.
 *
 * \param handle The handle returned by serial_init().
 * \param xonlimit On success, the value of XOnLimit.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided, or xonlimit was NULL.
 */
NSERIAL_EXPORT int WINAPI serial_getxonlimit(struct serialhandle *handle, int *xonlimit);

/*! \brief Set the property XOffLimit
 *
 * Set the property XOffLimit. This property should be used rarely, if
 * at all. An operating system may not support this property, in which
 * case this propery will have no effect.
 *
 * This property exists for compatibility with Windows. If the underlying
 * platform doesn't support setting the lower input buffer limit for sending
 * the XOff character, this property will be ignored. The default value is
 * 512 bytes.
 *
 * \param handle The handle returned by serial_init().
 * \param xofflimit The number of bytes for the XOffLimit.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided.
 */
NSERIAL_EXPORT int WINAPI serial_setxofflimit(struct serialhandle *handle, int xofflimit);

/*! \brief Get the property XOffLimit
 *
 * Get the property XOffLimit. This property should be used rarely, if
 * at all. An operating system may not support this property, in which
 * case this propery will have no effect. Getting the property will return
 * the last set value, if it is used or not.
 *
 * \param handle The handle returned by serial_init().
 * \param xofflimit On success, the value of XOffLimit.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided, or xofflimit was NULL.
 */
NSERIAL_EXPORT int WINAPI serial_getxofflimit(struct serialhandle *handle, int *xofflimit);

/*! \brief Set the property ParityReplace.
 *
 * Set the property ParityReplace. If this property is set to zero, the parity
 * replace is ignored. Else parity errors will be replaced with the byte
 * specified. Note, this property describes a behaviour equivalent to Windows
 * and is different to how unix parity detection works.
 *
 * Unix parity errors provide the 0xFF character with zero characters on
 * errors. These are replaced with the byte specified here. If set to zero, no
 * parity detection is enabled.
 *
 * If the serial port is already open when this property is set, the serial
 * port settings are set automatically. If the setting could not be applied
 * and results in an error, -1 is returned. The state of this property is then
 * undefined and you should attempt to set the property back to the original
 * value.
 *
 * \param handle The handle returned by serial_init().
 * \param parityreplace The byte value to set for ParityReplace. A value of
 *   zero disables parity replace.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided.
 */
NSERIAL_EXPORT int WINAPI serial_setparityreplace(struct serialhandle *handle, int parityreplace);

/*! \brief Get the property ParityReplace
 *
 * Get the property ParityReplace.
 *
 * \param handle The handle returned by serial_init().
 * \param parityreplace On success, the value of ParityReplace.
 * \return 0 if the operation was successful.
 * \return -1 if something went wrong.
 * \exception EINVAL invalid handle was provided, or parityreplace was NULL.
 */
NSERIAL_EXPORT int WINAPI serial_getparityreplace(struct serialhandle *handle, int *parityreplace);

/*! \brief The kinds of events that we can wait for.
 *
 * The kinds of events to wait for when waiting, or the event that occurred.
 */
typedef enum serialevent {
  NOEVENT = 0,            /*!< No event occurred */
  READEVENT = 1,          /*!< Wait for, or got a read event */
  WRITEEVENT = 2,         /*!< Wait for, or got a write event */
  READWRITEEVENT = 3      /*!< Wait for either read/write */
} serialevent_t;

/*! \brief Clear the input and output buffers immediately
 *
 * Flush the driver buffers and clear any internal state and buffers.
 *
 * \param handle The handle returned by serial_init().
 * \return -1 if there was an error. Use errno to get the error code.
 */
NSERIAL_EXPORT int WINAPI serial_reset(struct serialhandle *handle);

/*! \brief wait for a serial event to happen on an opened serial port.
 *
 * Wait for an event on the serial port, based on the type of events we expect.
 *
 * \param handle The handle returned by serial_init().
 * \param event The events to wait for.
 * \param timeout The timeout before returning in milliseconds.
 * \return -1 if there was an error. Use errno to get the error code.
 * \return The event that occurred. Note, that events will be returned as a
 *   bitmask if multiple events occur.
 */
NSERIAL_EXPORT serialevent_t WINAPI serial_waitforevent(struct serialhandle *handle, serialevent_t event, int timeout);

/*! \brief Trigger abort of the serial_waitforevent() function
 *
 * Cause an existing invocation of serial_waitforevent() to be aborted. It
 * might be useful to do such a thing if new data arrives for input while
 * we're currently in the serial_waitforevent() waiting for new data. This
 * allows to have an implementation with fewer threads.
 *
 * The internal implementation uses anonymous pipes to avoid triggering signal
 * handlers that might have other undesirable effects in the process.
 *
 * Note, that if this function is called before the serial_waitforevent(),
 * then the next invocation of serial_waitforevent() will abort
 * immediately. This is by design to avoid race conditions. Consider the case
 * that a thread has collected its data and has determined that there is
 * nothing to write. So it calls serial_waitforevent() with the event of only
 * READEVENT. Immediately before it calls serial_waitforevent(), a separate
 * thread has determined that there is data to write, and calls
 * serial_abortwaitforevent(). That causes the function serial_waitforevent()
 * to exit immediately (and possible return data that is available) to then
 * process the next command that is also to write data. If it were to discard
 * this method before calling waitforevent, it would actually miss the fact
 * that there is data to write and may end up in a blocking loop forever.
 *
 * To see how this works, assume you have two threads. Thread 1 is responsible
 * for looping always ready to read data (assuming that buffer space is
 * available) and based on a flag, it might also want to send data. It has
 * pseudo code that looks like:
 *
 * \code{.c}
void readwriteeventloop(void) {
  serialevent_t event = NOEVENT;

  while (running) {
    event = IsDataAvailable() ? WRITEEVENT : NOEVENT;

    serialevent_t gotevent =
      serial_waitforevent(handle, READEVENT | event, -1);

    if (gotevent & READEVENT) {
      // Read from the serial port
    }

    if (gotevent & WRITEEVENT) {
      // Write to the serial port
    }
  }
}
 * \endcode
 *
 * In a separate thread, say Thread 2, there might be a function called when
 * write data is now placed in a buffer, ready for sending:
 *
 * \code{.c}
void onwritedataavailable(void)
{
  GetData();
  serial_abortwaitforevent(handle);
}
 * \endcode
 *
 * Thus, you can see it is important that when data arrives the current
 * serial_waitforevent() exits, so that it can process whatever data it has,
 * and reenter the loop with the flag modified to also write data.
 *
 * The serial_abortwaitforevent() uses mutexes internally to serialize data,
 * so it also behaves as a memory barrier, ensuring that variables (like event
 * in the above example) are correctly set. But you will still have to make
 * sure that you control race conditions in your own program. That is why we
 * don't modify the event flag in Thread 2. The methods GetData() and
 * IsDataAvailable() need to apply appropriate synchronisation.
 *
 * \param handle The handle returned by serial_init()
 * \return -1 if there was an error. Use errno to get the error code.
 * \return 0 on success.
 */
NSERIAL_EXPORT int WINAPI serial_abortwaitforevent(struct serialhandle *handle);

/*! \brief Read data from the serial port.
 *
 * Read data from the serial port and copy into the buffer. This read method
 * behaves a little differently to the way normal posix read() works. The read
 * is always non-blocking. If there is no data to read, the value 0 is
 * returned, which is not to be misinterpreted as end of file. If the serial
 * port has been closed for whatever reason, the errno EIO is returned and the
 * appropriate error message is set. The amount of bytes returned may be less
 * than the length of the buffer provided.
 *
 * The reason why the behaviour is changed is so that the programmer doesn't
 * need to rely or check the specific value of errno, simply getting an error
 * is enough to know there is a problem and no data is available. The second
 * reason is this library attempts to be compatible to .NET, which will
 * happily return 0 for no data (as well as end of file) in non-blocking
 * situations of which errno EWOULDBLOCK / EAGAIN is not a reason to raise an
 * exception. So get a value of -1 and we have an exception.
 *
 * \param handle The handle returned by serial_init().
 * \param buffer The buffer to read the data to.
 * \param length The length till the end of the buffer.
 * \return The number of bytes put into the buffer at the address given.
 * \return -1 if there was an error. Use errno to get the error code.
 * \exception EIO End of file has been reached, or the serial port is not
 *   open.
 * \exception EINVAL Invalid parameters, check that handle and buffer is not
 *   NULL.
 * \exception ENOMEM A previous call to serial_setproperties() also failed with
 *   this exception.
 * \exception - Other exceptions may be raised depending on the underlying
 *   system libc read() call.
 */
NSERIAL_EXPORT ssize_t WINAPI serial_read(struct serialhandle *handle, char *buffer, size_t length);

/*! \brief Write data to the serial port.
 *
 * Write the data to the serial port. The write method behaves a little
 * differently to the posix equivalent. The write is always non-blocking. If
 * the data can't be written, 0 is returned immediately. If writing would
 * indicate the operation is blocked, then instead of returning an error, this
 * function also returns zero. This allows the programmer to check the result
 * and raise an exception immediately without having to rely on errno, which
 * is useful for .NET implementations.
 *
 * \param handle The handle returned by serial_init().
 * \param buffer The buffer containing the data to write.
 * \param length The number of bytes to write.
 * \return The number of bytes sent into the serial port buffer.
 * \exception EIO The serial port is not open.
 * \exception EINVAL Invalid parameters, check that handle and buffer is not
 *   NULL.
 * \exception - Other exceptions may be raised depending on the underlying
 *   system libc read() call.
 */
NSERIAL_EXPORT ssize_t WINAPI serial_write(struct serialhandle *handle, const char *buffer, size_t length);

/*! \brief Get the state of the DCD line on the serial port.
 *
 * Read the state of the Data Carrier Detect signal from the serial port.
 *
 * \param handle The handle returned by serial_init().
 * \param dcd Pointer to the variable to get the state of the DCD line.
 * \return -1 if there was an error, 0 otherwise. Use errno to get the error code.
 * \exception EINVAL Invalid parameters, check that handle and dcd is not
 *   NULL.
 * \exception EBADF The serial port is not open.
 */
NSERIAL_EXPORT int WINAPI serial_getdcd(struct serialhandle *handle, int *dcd);

/*! \brief Get the state of the RI line on the serial port.
 *
 * Read the state of the Ring Indicator signal from the serial port.
 *
 * \param handle The handle returned by serial_init().
 * \param ri Pointer to the variable to get the state of the RI line.
 * \return -1 if there was an error, 0 otherwise. Use errno to get the error code.
 * \exception EINVAL Invalid parameters, check that handle and ri is not
 *   NULL.
 * \exception EBADF The serial port is not open.
 */
NSERIAL_EXPORT int WINAPI serial_getri(struct serialhandle *handle, int *ri);

/*! \brief Get the state of the DSR line on the serial port.
 *
 * Read the state of the Data Set Ready signal from the serial port.
 *
 * \param handle The handle returned by serial_init().
 * \param dsr Pointer to the variable to get the state of the DSR line.
 * \return -1 if there was an error, 0 otherwise. Use errno to get the error code.
 * \exception EINVAL Invalid parameters, check that handle and dsr is not
 *   NULL.
 * \exception EBADF The serial port is not open.
 */
NSERIAL_EXPORT int WINAPI serial_getdsr(struct serialhandle *handle, int *dsr);

/*! \brief Get the state of the CTS line on the serial port.
 *
 * Read the state of the Clear To Send signal from the serial port.
 *
 * \param handle The handle returned by serial_init().
 * \param cts Pointer to the variable to get the state of the CTS line.
 * \return -1 if there was an error, 0 otherwise. Use errno to get the error code.
 * \exception EINVAL Invalid parameters, check that handle and cts is not
 *   NULL.
 * \exception EBADF The serial port is not open.
 */
NSERIAL_EXPORT int WINAPI serial_getcts(struct serialhandle *handle, int *cts);

/*! \brief Set the state of the DTR line on the serial port.
 *
 * Write the state of the Data Terminal Ready signal to the serial port.
 *
 * \param handle The handle returned by serial_init().
 * \param dtr The state of the DTR line.
 * \return -1 if there was an error, 0 otherwise. Use errno to get the error code.
 * \exception EINVAL Invalid parameters, check that handle and dtr is not
 *   NULL.
 * \exception EBADF The serial port is not open.
 */
NSERIAL_EXPORT int WINAPI serial_setdtr(struct serialhandle *handle, int dtr);

/*! \brief Get the state of the DTR line on the serial port.
 *
 * Read the state of the Data Terminal Ready signal from the serial port.
 *
 * \param handle The handle returned by serial_init().
 * \param dtr Pointer to the variable to get the state of the DTR line.
 * \return -1 if there was an error, 0 otherwise. Use errno to get the error code.
 * \exception EINVAL Invalid parameters, check that handle and dtr is not
 *   NULL.
 * \exception EBADF The serial port is not open.
 */
NSERIAL_EXPORT int WINAPI serial_getdtr(struct serialhandle *handle, int *dtr);

/*! \brief Set the state of the RTS line on the serial port.
 *
 * Write the state of the Request To Send signal to the serial port.
 *
 * \param handle The handle returned by serial_init().
 * \param rts The state of the RTS line.
 * \return -1 if there was an error, 0 otherwise. Use errno to get the error code.
 * \exception EINVAL Invalid parameters, check that handle and rts is not
 *   NULL.
 * \exception EBADF The serial port is not open.
 */
NSERIAL_EXPORT int WINAPI serial_setrts(struct serialhandle *handle, int rts);

/*! \brief Get the state of the RTS line on the serial port.
 *
 * Read the state of the Request To Send signal from the serial port.
 *
 * \param handle The handle returned by serial_init().
 * \param rts Pointer to the variable to get the state of the RTS line.
 * \return -1 if there was an error, 0 otherwise. Use errno to get the error
 *    code.
 * \exception EINVAL Invalid parameters, check that handle and rts is not
 *   NULL.
 * \exception EBADF The serial port is not open.
 */
NSERIAL_EXPORT int WINAPI serial_getrts(struct serialhandle *handle, int *rts);

/*! \brief Wait for a modem event (change in the modem signals) to occur.
 *
 * The kinds of modem events to wait for.
 */
typedef enum serialmodemevent {
  MODEMEVENT_NONE = 0,   /*!< No modem event returned */
  MODEMEVENT_DCD = 1,    /*!< Wait for DCD event */
  MODEMEVENT_RI = 2,     /*!< Wait for RI event */
  MODEMEVENT_DSR = 4,    /*!< Wait for DSR event */
  MODEMEVENT_CTS = 8,    /*!< Wait for RTS event */
} serialmodemevent_t;

/*! \brief Wait for a modem event to occur.
 *
 * Block the current thread until a modem signal change occurs.
 *
 * The underlying operation TIOCMIWAIT blocks forever until a modem signal
 * changes. This method creates its own posix thread which it will run on and
 * your thread will block until a signal change is detected, or until you call
 * serial_abortwaitformodemevent().
 *
 * Note, only one invocation of this function is allowed per instance.
 *
 * The behaviour of this function depends on the support of the underlying
 * drivers. On Linux, there are two modes:
 * * Detect change through counters
 * * Detect change by comparing pin states
 *
 * The second case is less reliable than the first case and is only used if on
 * Linux the driver supports the TIOCGICOUNT ioctl() system call. Not all
 * drivers support this (and it is determined at run time every time this
 * function is invocated):
 *
 * +----------+-------+
 * |          | linux |
 * |          | kern  |
 * | Chipset  | 4.4.0 |
 * +----------+-------+
 * | PL2303H  | No    |
 * | PL2303RA | No    |
 * | 16550A   | YES   |
 * | FTDI     | YES   |
 * +----------+-------+
 *
 * If you have a chipset and you want to see what mode it is using, you can
 * recompile the sources (modem.c) with the define WAITDEBUG active and run
 * the component test cases:
 *
 * $ ./nserialcomptest --gtest_filter=SerialModemEventsTest.CtsEvent \
 *    /dev/ttyUSB0 /dev/ttyUSB1
 *
 * For example, the error code result of 25 (NOTTY) is common.
 *
 * \param handle The handle returned by serial_init().
 * \param event The events to wait for.
 * \return -1 if there was an error. Use errno to get the error code.
 * \return The event that occurred. Note, that events will be returned as a
 *   bitmask if multiple events occur.
 * \exception EBADFS The serial port is not open.
 */
NSERIAL_EXPORT serialmodemevent_t WINAPI serial_waitformodemevent(struct serialhandle *handle, serialmodemevent_t event);

/*! \brief Trigger abort of the serial_waitformodemevent
 *
 * Abort action of the function serial_waitformodemevent().
 *
 * \param handle The handle returned by serial_init()
 * \return -1 if there was an error. Use errno to get the error code.
 * \return 0 on success.
 */
NSERIAL_EXPORT int WINAPI serial_abortwaitformodemevent(struct serialhandle *handle);

/*! \brief Retrieve the number of bytes in the driver in queue.
 *
 * Get the number of bytes in the input queue.
 *
 * \param handle The handle returned by serial_init().
 * \param queue On return contains the number of bytes in the queue.
 * \return -1 if there was an error. Use errno to get the error code.
 * \return 0 Operation performed correctly.
 * \exception ENOSYS This operation is not supported on this platform.
 * \exception EBADFS The serial port is not open.
 */
NSERIAL_EXPORT int WINAPI serial_getreadbytes(struct serialhandle *handle, int *queue);

/*! \brief Retrieve the number of bytes in the driver out queue.
 *
 * Get the number of bytes out the input queue.
 *
 * \param handle The handle returned by serial_init().
 * \param queue On return contains the number of bytes in the queue.
 * \return -1 if there was an error. Use errno to get the error code.
 * \return 0 Operation performed correctly.
 * \exception ENOSYS This operation is not supported on this platform.
 * \exception EBADFS The serial port is not open.
 */
NSERIAL_EXPORT int WINAPI serial_getwritebytes(struct serialhandle *handle, int *queue);

/*! \brief Set the break state of the serial port (to the level, no pulse).
 *
 * Set the break state of the serial port.
 *
 * \param handle The handle returned by serial_init().
 * \param breakstate The state to set the break signal to.
 * \return -1 if there was an error. Use errno to get the error code.
 * \return 0 Operation performed correctly.
 * \exception ENOSYS This operation is not supported on this platform.
 * \exception EBADFS The serial port is not open.
 */
NSERIAL_EXPORT int WINAPI serial_setbreak(struct serialhandle *handle, int breakstate);

/*! \brief Get the break state of the serial port.
 *
 * Get the break state of the serial port. This parameter may be cached if the
 * underlying operating system doesn't have a method to get the state
 * directly. In this case, it is the last value given to serial_setbreak().
 *
 * \param handle The handle returned by serial_init().
 * \param breakstate On return contains state to set the break signal to.
 * \return -1 if there was an error. Use errno to get the error code.
 * \return 0 Operation performed correctly.
 * \exception ENOSYS This operation is not supported on this platform.
 * \exception EBADFS The serial port is not open.
 */
NSERIAL_EXPORT int WINAPI serial_getbreak(struct serialhandle *handle, int *breakstate);

/*! \brief Discard all buts in the input buffer of the driver
 *
 * Discard all the bytes in the input buffer
 *
 * \param handle The handle returned by serial_init().
 * \return -1 if there was an error. Use errno to get the error code.
 * \return 0 Operation performed correctly.
 */
NSERIAL_EXPORT int WINAPI serial_discardinbuffer(struct serialhandle *handle);

/*! \brief Discard all buts in the output buffer of the driver
 *
 * Discard all the bytes in the output buffer
 *
 * \param handle The handle returned by serial_init().
 * \return -1 if there was an error. Use errno to get the error code.
 * \return 0 Operation performed correctly.
 */
NSERIAL_EXPORT int WINAPI serial_discardoutbuffer(struct serialhandle *handle);

#ifdef __cplusplus
}
#endif
#endif
