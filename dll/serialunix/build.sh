#!/bin/sh
PROJECTBIN=`pwd`/bin
PROJECTBUILD=`pwd`/build

if [ -e $PROJECTBIN ]; then
  rm -rf $PROJECTBIN
fi
if [ -e $PROJECTBUILD ]; then
  rm -rf $PROJECTBUILD
fi

mkdir $PROJECTBUILD
cd $PROJECTBUILD

# In Ubuntu 14.04, installing 'libgtest-dev' installs the sources and the
# headers, but we have to build ourselves.
if [ x$GTEST_ROOT = x"" ]; then
  if [ -e /usr/src/gtest ]; then
    echo ======================================================================
    echo == Building GTest
    echo ======================================================================
    mkdir gtest
    cd gtest
    cmake /usr/src/gtest
    make
    cd ..
    export GTEST_ROOT=`pwd`/gtest
  fi
fi

echo ======================================================================
echo == Building Project
echo ======================================================================
CFLAGS="-O0 -g -Wall" cmake .. && \
make
if test $? = 0; then
  make test CTEST_OUTPUT_ON_FAILURE=1
  make install DESTDIR=$PROJECTBIN
fi

SYSTEM=`uname -s`
MACHINE=`uname -m`
if [ -e $PROJECTBIN/usr/local/lib/libnserial.so.1.0 ]; then
    cp $PROJECTBIN/usr/local/lib/libnserial.so.1.0 $PROJECTBIN/usr/local/lib/libnserial.$SYSTEM.$MACHINE.so.1.0
fi
