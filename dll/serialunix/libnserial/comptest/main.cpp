#include "gtest/gtest.h"
#include "SerialConfiguration.hpp"

SerialConfiguration *serialconfig;

int main(int argc, char **argv)
{
  ::testing::InitGoogleTest(&argc, argv);

  if (argc != 3) {
    std::cerr << "Usage " << argv[0] << " <writedevice> <readdevice>\n";
    exit(0);
  }
  serialconfig = new SerialConfiguration(argv[1], argv[2]);

  return RUN_ALL_TESTS();
}
