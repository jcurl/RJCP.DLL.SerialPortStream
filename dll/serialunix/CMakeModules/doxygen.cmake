#
# Check for Doxygen and add an output target
#

#
# This module looks for Doxygen and sets up targets in your build system to build documentation.
#
#  set (CMAKE_MODULE_PATH ${PROJECT_SOURCE_DIR}/CMakeModules)
#  include (${CMAKE_MODULE_PATH}/doxygen.cmake)
#
# The doxygen input file must be in the source directory where the CMakeLists.txt file is and be
# called 'doxyfile.in'. That will be converted to 'doxyfile'.
#
# If you try to build documentation with doxygen not present, a warning will be made present
# indicating the lack of supporting tools. If the option BUILD_DOCUMENTATION is not defined, the
# target 'doc' will not be made available.
#
include(GNUInstallDirs)

find_package(Doxygen)
option(BUILD_DOCUMENTATION "Create and install the HTML based API documentation (requires Doxygen)" ${DOXYGEN_FOUND})

if(BUILD_DOCUMENTATION)
  if(NOT DOXYGEN_FOUND)
    add_custom_target(doc
      COMMAND echo "You must have doxygen installed at time of configure"
      VERBATIM)
  else(NOT DOXYGEN_FOUND)
    set(doxyfile_in ${CMAKE_CURRENT_SOURCE_DIR}/doxyfile.in)
    set(doxyfile ${CMAKE_CURRENT_BINARY_DIR}/doxyfile)

    configure_file(${doxyfile_in} ${doxyfile} @ONLY)
    add_custom_target(doc ALL
      COMMAND ${DOXYGEN_EXECUTABLE} ${doxyfile}
      WORKING_DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}
      COMMENT "Generating API documentation with Doxygen"
      DEPENDS ${doxyfile_in}
      VERBATIM)

    install(DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}/html DESTINATION "${CMAKE_INSTALL_DOCDIR}")
  endif(NOT DOXYGEN_FOUND)
endif(BUILD_DOCUMENTATION)
