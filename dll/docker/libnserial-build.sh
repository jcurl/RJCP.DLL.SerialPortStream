#!/bin/bash

CODENAME=`lsb_release -c | cut -f2`
if [ ! -e /build ]; then
  echo "The mount /build doesn't exist"
  echo " mkdir build"
  echo " docker run -it ... -v ${PWD}/build:/build:rw ..."
  exit 1
fi

export PROJECTSOURCE=/source
export PROJECTBIN=/build/${CODENAME}/bin
export PROJECTBUILD=/build/${CODENAME}/build

create_build_dir() {
  cd /build
  if [ -e /build/${CODENAME} ]; then
    rm -rf /build/${CODENAME}
  fi
  
  mkdir -p /build/${CODENAME} || {
    echo "Couldn't create build directory, probably didn't mount /build to be read/write with"
    echo " mkdir build"
    echo " docker run -it ... -v ${PWD}/build:/build:rw ..."
    exit 1
  }
}

create_home_dir() {
  # Use a temporary directory for when cmake needs to write stuff. We don't care
  # about this when we're done.
  export HOME="$(mktemp -d)"
  if [ -z ${HOME} ]; then
    # Couldn't create the home directory, probably because the user didn't mount
    # /tmpfs at /tmp.
    echo "Couldn't create a HOME directory, probably didn't mount /tmpfs. Using ${PROJECTBUILD} instead"
    echo " docker run -it ... --tmpfs /tmp ..."
    export HOME=${PROJECTBUILD}
  fi
}

build() {
  if [ ! -e /source/build.sh ]; then
    echo "Build script 'build.sh' not found in mount /source"
    echo " docker run -it ... -v ${PWD}:/source:ro ..."
    exit 1
  fi

  create_build_dir
  create_home_dir

  cd /source
  exec /source/build.sh
}

deb() {
  if [ ! -e /source/debian/control ]; then
    echo "File 'debian/control' not found"
    exit 1
  fi

  # TODO: Check that ${CODENAME} matches changelog
  if [ ! -e /source/debian/changelog ]; then
    echo "File 'debian/changelog' not found"
    exit 1
  fi

  CHANGELOG_CODENAME=`sed -n -E '1s/^\S+\s+\(\S+\)\s+(\S+);.*$/\1/p' /source/debian/changelog`
  if [ ${CODENAME} != ${CHANGELOG_CODENAME} ]; then
    echo "Changelog Codename ${CHANGELOG_CODENAME} is not ${CODENAME}"
    exit 1
  fi

  create_build_dir
  create_home_dir

  # We tar the sources to /tmp first, in case /build is a subdirectory of the
  # sources. That way, we might copy the build directory across, but then it's
  # ignored rather than have recursive problems with cp -R.
  tar -cf /tmp/source.tar /source
  cd /build/${CODENAME}/
  tar -xf /tmp/source.tar
  cd source

  export TEMP=/tmp
  export TMP=/tmp

  # For our project, we don't run tests as they're hardware specific
  DEB_BUILD_OPTIONS=nocheck dpkg-buildpackage -uc -us
  exit 0
}

shell() {
  create_home_dir
  exec /bin/bash
}

dpkgquery() {
  create_home_dir
  dpkg-query -W dpkg gcc binutils autoconf automake libtool m4 cmake doxygen debhelper
  exit 0
}

COMMANDARG="$1"
case ${COMMANDARG} in
  build)
    build
    ;;
  deb)
    deb
    ;;
  sh)
    shell
    ;;
  dpkg-query)
    dpkgquery
    ;;
  *)
    echo "Unknown command ${COMMANDARG}"
    exit 1
esac
