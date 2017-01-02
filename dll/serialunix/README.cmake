These small set of instructions provide information on how you can find
the 'libnserial.so' library using CMake.

Your `CMakeLists.txt` file should look like:

```
cmake_minimum_required(VERSION 2.8)
project(helloworld)
add_executable(helloworld hello.c)
find_package(Threads REQUIRED)
find_package(nserial CONFIG REQUIRED)
include_directories(${nserial_INCLUDE_DIRS})
target_link_libraries(helloworld ${nserial_LIBRARIES} ${CMAKE_THREAD_LIBS_INIT})
```

The file `hello.c` should look like:

```
#include <stdio.h>
#include <stdlib.h>
#include <nserial.h>
 
void main(void)
{
    printf("Version: %s\n", serial_version());
}
```

If you've installed the debian packages `libnserial` and `libnserial-dev`, you
can then run CMake and it will find the package (courtesy of the file
`nserialConfig.cmake` installed in the libs directory).

