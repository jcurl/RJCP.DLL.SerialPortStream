#ifndef SERIALCONFIGURATION_HPP
#define SERIALCONFIGURATION_HPP

class SerialConfiguration
{
public:
  SerialConfiguration(const char *device);
  const char *GetDevice();

private:
  const char *m_device;
};

#endif
