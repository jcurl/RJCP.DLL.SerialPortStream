# On Ubuntu 14.04:
#
#  sudo apt-get install libgtest-dev
find_package(GTest)

# Don't forget to enable_testing() in the root of your project
if(GTEST_FOUND)
  find_package(Threads REQUIRED)
  include_directories(${GTEST_INCLUDE_DIRS})

  set(SERIALUNIX_GTEST_SRCS
    serialinit.cpp
    serialopen.cpp
    main.cpp
    configuration.cpp)
  add_executable(nserialtest ${SERIALUNIX_GTEST_SRCS})
  target_link_libraries(nserialtest nserial
    ${GTEST_MAIN_LIBRARIES} ${GTEST_LIBRARIES} ${CMAKE_THREAD_LIBS_INIT})
  add_test(NAME nserialtests COMMAND nserialtest)
endif(GTEST_FOUND)
