#include <stdlib.h>
#include "Buffer.hpp"

Buffer::Buffer(int size) :
  m_buffer(NULL),
  m_start(0),
  m_length(0)
{
  m_buffer = (char*)malloc(size);
  if (m_buffer != NULL) {
    m_capacity = size;
  }
}

Buffer::~Buffer()
{
  if (m_buffer != NULL) {
    free(m_buffer);
  }
}

int Buffer::GetCapacity()
{
  return m_capacity;
}

int Buffer::GetLength()
{
  return m_length;
}

int Buffer::GetReadLength()
{
  if (!m_buffer) return 0;
  return (m_start + m_length <= m_capacity) ?
    m_length :
    m_capacity - m_start;
}

int Buffer::GetWriteLength()
{
  if (!m_buffer) return 0;
  return (m_start + m_length < m_capacity) ?
    m_capacity - m_start - m_length :
    m_capacity - m_length;
}

int Buffer::GetStartOffset()
{
  if (!m_buffer) return 0;
  return m_start;
}

int Buffer::GetEndOffset()
{
  if (!m_buffer) return 0;
  return (m_start + m_length) % m_capacity;
}

char *Buffer::GetStartBuffer()
{
  if (!m_buffer) return NULL;
  return m_buffer + m_start;
}

char *Buffer::GetEndBuffer()
{
  if (!m_buffer) return NULL;
  return m_buffer + (m_start + m_length) % m_capacity;
}

void Buffer::Reset()
{
  m_start = 0;
  m_length = 0;
}

int Buffer::Produce(int length)
{
  if (!m_buffer) return 0;
  if (m_length + length > m_capacity) {
    length = m_capacity - m_length;
  }
  m_length += length;
  return length;
}

int Buffer::Consume(int length)
{
  if (!m_buffer) return 0;
  if (length > m_length) {
    length = m_length;
  }
  m_start = (m_start + length) % m_capacity;
  m_length -= length;
  return length;
}

char *Buffer::GetBuffer()
{
  return m_buffer;
}
