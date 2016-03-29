#ifndef BUFFER_HPP
#define BUFFER_HPP

class Buffer
{
public:
  Buffer(int size);
  ~Buffer();

  int   GetCapacity();
  int   GetLength();
  int   GetReadLength();
  int   GetWriteLength();
  int   GetStartOffset();
  int   GetEndOffset();
  char *GetStartBuffer();
  char *GetEndBuffer();
  char *GetBuffer();
  void  Reset();
  int   Produce(int length);
  int   Consume(int length);

private:
  char *m_buffer;
  int   m_capacity;
  int   m_start;
  int   m_length;
};

#endif
