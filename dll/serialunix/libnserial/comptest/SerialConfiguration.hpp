#ifndef SERIALCONFIGURATION_HPP
#define SERIALCONFIGURATION_HPP

class SerialConfiguration
{
public:
  SerialConfiguration(const char *writedevice, const char *readdevice);
  const char *GetReadDevice();
  const char *GetWriteDevice();

private:
  const char *m_readdevice;
  const char *m_writedevice;
};

#endif
