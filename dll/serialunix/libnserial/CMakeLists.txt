project(libnserial C CXX)
include(${CMAKE_MODULE_PATH}/doxygen.cmake)
include(${CMAKE_MODULE_PATH}/serial.cmake)
include(GNUInstallDirs)

configure_file(${CMAKE_CURRENT_SOURCE_DIR}/config.h.in
  ${CMAKE_CURRENT_BINARY_DIR}/config.h)

option(NSLOG_ENABLED "Build the project to enable logging" OFF)
if(NSLOG_ENABLED)
  message(STATUS "Logging enabled")
  add_definitions(-DNSLOG_ENABLED)
endif(NSLOG_ENABLED)

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
  threaddata.c
  log.c
  stringbuf.c
  netfx.c)

if(${CMAKE_SYSTEM_NAME} MATCHES "Linux")
  set(NSERIAL_SRCS ${NSERIAL_SRCS}
    portlinux.c)
else()
  set(NSERIAL_SRCS ${NSERIAL_SRCS}
    portnone.c)
endif()


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
  VERSION ${VERSION_MAJOR}.${VERSION_MINOR}
  SOVERSION ${VERSION_MAJOR}
  FRAMEWORK ON
  PUBLIC_HEADER ${NSERIAL_PUBLIC_HEADERS})

# Add all targets to the build-tree export set
export(TARGETS nserial
  FILE "${CMAKE_CURRENT_BINARY_DIR}/nserialTargets.cmake")
 
# Export the package for use from the build-tree
# (this registers the build-tree with a global CMake-registry)
export(PACKAGE nserial)

# Define the headers that should be installed
install(TARGETS nserial
  EXPORT nserialTargets
  RUNTIME DESTINATION "${CMAKE_INSTALL_BINDIR}"
  LIBRARY DESTINATION "${CMAKE_INSTALL_LIBDIR}"
  ARCHIVE DESTINATION "${CMAKE_INSTALL_LIBDIR}"
  PUBLIC_HEADER DESTINATION "${CMAKE_INSTALL_INCLUDEDIR}")

# Calculate relative paths for installation
function(relative relpath path1 path2)
  if(NOT IS_ABSOLUTE ${path1})
    set(path1 "${CMAKE_INSTALL_PREFIX}/${path1}")
  endif()
  if(NOT IS_ABSOLUTE ${path2})
    set(path2 "${CMAKE_INSTALL_PREFIX}/${path2}")
  endif()
  file(RELATIVE_PATH CALCRELPATH ${path1} ${path2})
  set(${relpath} ${CALCRELPATH} PARENT_SCOPE)
endfunction(relative)

set(CMAKE_INSTALL_CMAKEDIR "${CMAKE_INSTALL_LIBDIR}/nserial-${PACKAGE_VERSION}" CACHE PATH "Installation directory for CMake files")

# Create the nserialConfig.cmake and nserialConfigVersion.cmake
relative(REL_INCLUDE_DIR "${CMAKE_INSTALL_CMAKEDIR}" "${CMAKE_INSTALL_INCLUDEDIR}")
# ... for the build tree
set(CONF_INCLUDE_DIRS "${CMAKE_CURRENT_SOURCE_DIR}" "${CMAKE_CURRENT_BINARY_DIR}")
configure_file(nserialConfig.cmake.in "${CMAKE_CURRENT_BINARY_DIR}/nserialConfig.cmake" @ONLY)
# ... for the install tree (note, the file nserialConfig.cmake defines NSERIAL_CMAKE_DIR there)
set(CONF_INCLUDE_DIRS "\${NSERIAL_CMAKE_DIR}/${REL_INCLUDE_DIR}")
configure_file(nserialConfig.cmake.in "${CMAKE_CURRENT_BINARY_DIR}${CMAKE_FILES_DIRECTORY}/nserialConfig.cmake" @ONLY)
# ... for both
configure_file(nserialConfigVersion.cmake.in "${CMAKE_CURRENT_BINARY_DIR}/nserialConfigVersion.cmake" @ONLY)

# Install nserialConfig.cmake and nserialConfigVersion.cmake
install(FILES
  "${CMAKE_CURRENT_BINARY_DIR}${CMAKE_FILES_DIRECTORY}/nserialConfig.cmake"
  "${CMAKE_CURRENT_BINARY_DIR}/nserialConfigVersion.cmake"
  DESTINATION "${CMAKE_INSTALL_CMAKEDIR}" COMPONENT dev)
install(EXPORT nserialTargets DESTINATION
  "${CMAKE_INSTALL_CMAKEDIR}" COMPONENT dev)

add_subdirectory(unittest)
add_subdirectory(comptest)

