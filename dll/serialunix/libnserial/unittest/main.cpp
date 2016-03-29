#include "gtest/gtest.h"
#include "configuration.hpp"

SerialConfiguration *serialconfig;

int main(int argc, char **argv)
{
  ::testing::InitGoogleTest(&argc, argv);

  if (argc > 2) {
    std::cerr << "Usage " << argv[0] << " <device>\n";
    exit(0);
  }
  if (argc == 2) {
    serialconfig = new SerialConfiguration(argv[1]);
  } else {
    serialconfig = new SerialConfiguration("/dev/ttyS0");
  }

  return RUN_ALL_TESTS();
}
