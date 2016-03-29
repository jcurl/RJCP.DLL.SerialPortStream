#include <iostream>
#include <fstream>
#include "BuffDump.hpp"

void DumpBuffer(const char *buff, size_t len, const char *filename)
{
  std::ofstream file;
  file.open(filename,
            std::ofstream::out | std::ofstream::binary | std::ofstream::trunc);
  file.write(buff, len);
  file.close();
}
