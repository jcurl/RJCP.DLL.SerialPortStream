#include "SerialConfiguration.hpp"

SerialConfiguration::SerialConfiguration(const char *writedevice, const char *readdevice)
{
  m_writedevice = writedevice;
  m_readdevice = readdevice;
}

const char *SerialConfiguration::GetReadDevice()
{
  return m_readdevice;
}

const char *SerialConfiguration::GetWriteDevice()
{
  return m_writedevice;
}
