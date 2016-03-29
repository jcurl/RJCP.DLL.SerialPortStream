#include "configuration.hpp"

SerialConfiguration::SerialConfiguration(const char *device)
{
  m_device = device;
}

const char *SerialConfiguration::GetDevice()
{
  return m_device;
}
