# On Ubuntu 14.04:
#
#  sudo apt-get install libgtest-dev
find_package(GTest)

# Don't forget to enable_testing() in the root of your project
if(GTEST_FOUND)
  find_package(Threads REQUIRED)
  include_directories(${GTEST_INCLUDE_DIRS})

  set(SERIALUNIXCOMP_GTEST_SRCS
    SerialSendReceiveTest.cpp
    SerialParityTest.cpp
    SerialModemSignalsTest.cpp
    SerialModemEvents.cpp
    main.cpp
    SerialConfiguration.cpp
    Buffer.cpp
    BuffDump.cpp
    SerialReadWrite.cpp)
  add_executable(nserialcomptest ${SERIALUNIXCOMP_GTEST_SRCS})
  target_link_libraries(nserialcomptest nserial
    ${GTEST_MAIN_LIBRARIES} ${GTEST_LIBRARIES} ${CMAKE_THREAD_LIBS_INIT} rt)

  # Component tests should not run with "make test"
  #add_test(NAME nserialcomptests COMMAND nserialcomptest)
endif(GTEST_FOUND)

add_executable(kernelbug kernelbug.c)
add_executable(icount icount.c)
