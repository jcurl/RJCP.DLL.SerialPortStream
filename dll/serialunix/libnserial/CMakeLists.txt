project(libnserial C CXX)
include(${CMAKE_MODULE_PATH}/doxygen.cmake)
include(${CMAKE_MODULE_PATH}/serial.cmake)
include(GNUInstallDirs)

configure_file(${CMAKE_CURRENT_SOURCE_DIR}/config.h.in
  ${CMAKE_CURRENT_BINARY_DIR}/config.h)

include_directories(${CMAKE_CURRENT_BINARY_DIR})
include_directories(${CMAKE_CURRENT_SOURCE_DIR})

set(NSERIAL_PUBLIC_HEADERS
  nserial.h)
set(NSERIAL_SRCS
  nserial.c
  baudrate.c
  basic.c
  openserial.c
  events.c
  properties.c
  flush.c
  modem.c
  lowlevel.c
  break.c
  errmsg.c
  threaddata.c)
add_library(nserial SHARED ${NSERIAL_SRCS})

# Version Details: This should specify the detail of the API. It is
# independent of the actual version of the project.
#
#  SOVERSION: This is the API version of the library. It should match the
#    major version for consistency.
#  MAJOR: Should be the same as the SOVERSION. Increment this by +1 when
#    there is an incompatible version with the previous release. The MINOR
#    version should be reset to 0.
#  MINOR: Increment by +1 if a new method is given and the API remains
#    backward compatible with the previous release.
#
# Unlike libtool, there is no "PATCH" version. CMake doesn't make more than
# one symbolic link
#  e.g.
#   libfoo.so -> libfoo.so.1
#   libfoo.so.1 -> libfoo.so.1.0
#   libfoo.so.1.0
#
# As the PATCH doesn't indicate a change in the API, there is no need for it
# in the file name (and a library can't link therefore to a specific patch
# version). This also means, that the SOVERSION and the MAJOR version must be
# the same to support incremental updates.
set_target_properties(nserial PROPERTIES
  VERSION 1.0
  SOVERSION 1
  FRAMEWORK ON
  PUBLIC_HEADER ${NSERIAL_PUBLIC_HEADERS})

# Define the headers that should be installed
install(TARGETS nserial
  RUNTIME DESTINATION "${CMAKE_INSTALL_BINDIR}"
  LIBRARY DESTINATION "${CMAKE_INSTALL_LIBDIR}"
  ARCHIVE DESTINATION "${CMAKE_INSTALL_LIBDIR}"
  PUBLIC_HEADER DESTINATION "${CMAKE_INSTALL_INCLUDEDIR}")

add_subdirectory(unittest)
add_subdirectory(comptest)

