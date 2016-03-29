#
# Check for serial specific features of the platform being compiles.
#

# Check #1: Check for the availability of particular baudrate constants:
#  B0       B50      B75      B110     B134     B150
#  B200     B300     B600     B1200    B1800    B2400
#  B4800    B9600    B19200   B38400   B57600   B76800
#  B115200  B128000  B153600  B230400  B256000  B307200
#  B460800  B500000  B576000  B921600  B1000000 B1152000
#  B1500000 B2000000 B2500000 B3000000 B3500000 B4000000
#
# For each of the constants found, the value HAVE_TERMIOS_BXXXXX is set. Thus
# you can put in your "config.h.in" file the following (as an example, including
# all others):
#
#  #cmakedefine HAVE_TERMIOS_B115200
#
include(CheckSymbolExists)
include(CheckTypeSize)
check_symbol_exists(B0       "termios.h" HAVE_TERMIOS_B0)
check_symbol_exists(B50      "termios.h" HAVE_TERMIOS_B50)
check_symbol_exists(B75      "termios.h" HAVE_TERMIOS_B75)
check_symbol_exists(B110     "termios.h" HAVE_TERMIOS_B110)
check_symbol_exists(B134     "termios.h" HAVE_TERMIOS_B134)
check_symbol_exists(B150     "termios.h" HAVE_TERMIOS_B150)
check_symbol_exists(B200     "termios.h" HAVE_TERMIOS_B200)
check_symbol_exists(B300     "termios.h" HAVE_TERMIOS_B300)
check_symbol_exists(B600     "termios.h" HAVE_TERMIOS_B600)
check_symbol_exists(B1200    "termios.h" HAVE_TERMIOS_B1200)
check_symbol_exists(B1800    "termios.h" HAVE_TERMIOS_B1800)
check_symbol_exists(B2400    "termios.h" HAVE_TERMIOS_B2400)
check_symbol_exists(B4800    "termios.h" HAVE_TERMIOS_B4800)
check_symbol_exists(B9600    "termios.h" HAVE_TERMIOS_B9600)
check_symbol_exists(B14400   "termios.h" HAVE_TERMIOS_B14400)
check_symbol_exists(B19200   "termios.h" HAVE_TERMIOS_B19200)
check_symbol_exists(B33600   "termios.h" HAVE_TERMIOS_B33600)
check_symbol_exists(B38400   "termios.h" HAVE_TERMIOS_B38400)
check_symbol_exists(B57600   "termios.h" HAVE_TERMIOS_B57600)
check_symbol_exists(B76800   "termios.h" HAVE_TERMIOS_B76800)
check_symbol_exists(B115200  "termios.h" HAVE_TERMIOS_B115200)
check_symbol_exists(B128000  "termios.h" HAVE_TERMIOS_B128000)
check_symbol_exists(B153600  "termios.h" HAVE_TERMIOS_B153600)
check_symbol_exists(B230400  "termios.h" HAVE_TERMIOS_B230400)
check_symbol_exists(B256000  "termios.h" HAVE_TERMIOS_B256000)
check_symbol_exists(B307200  "termios.h" HAVE_TERMIOS_B307200)
check_symbol_exists(B460800  "termios.h" HAVE_TERMIOS_B460800)
check_symbol_exists(B500000  "termios.h" HAVE_TERMIOS_B500000)
check_symbol_exists(B576000  "termios.h" HAVE_TERMIOS_B576000)
check_symbol_exists(B921600  "termios.h" HAVE_TERMIOS_B921600)
check_symbol_exists(B1000000 "termios.h" HAVE_TERMIOS_B1000000)
check_symbol_exists(B1152000 "termios.h" HAVE_TERMIOS_B1152000)
check_symbol_exists(B1500000 "termios.h" HAVE_TERMIOS_B1500000)
check_symbol_exists(B2000000 "termios.h" HAVE_TERMIOS_B2000000)
check_symbol_exists(B2500000 "termios.h" HAVE_TERMIOS_B2500000)
check_symbol_exists(B3000000 "termios.h" HAVE_TERMIOS_B3000000)
check_symbol_exists(B3500000 "termios.h" HAVE_TERMIOS_B3500000)
check_symbol_exists(B4000000 "termios.h" HAVE_TERMIOS_B4000000)

check_symbol_exists(CMSPAR   "termios.h" HAVE_TERMIOS_CMSPAR)

check_symbol_exists(TIOCSBRK "sys/ioctl.h" HAVE_TERMIOS_TIOCSBRK)
check_symbol_exists(TIOCCBRK "sys/ioctl.h" HAVE_TERMIOS_TIOCCBRK)
check_symbol_exists(TIOCINQ  "sys/ioctl.h" HAVE_TERMIOS_TIOCINQ)
check_symbol_exists(FIONREAD "sys/ioctl.h" HAVE_TERMIOS_FIONREAD)
check_symbol_exists(TIOCOUTQ "sys/ioctl.h" HAVE_TERMIOS_TIOCOUTQ)

check_symbol_exists(TIOCGICOUNT "sys/ioctl.h" HAVE_TERMIOS_TIOCGICOUNT)
check_symbol_exists(TIOCMIWAIT  "sys/ioctl.h" HAVE_TERMIOS_TIOCMIWAIT)
set(CMAKE_EXTRA_INCLUDE_FILES "linux/serial.h")
check_type_size("struct serial_icounter_struct" HAVE_LINUX_SERIAL_ICOUNTER_STRUCT)
set(CMAKE_EXTRA_INCLUDE_FILES)
