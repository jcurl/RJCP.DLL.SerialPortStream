# Introduction

This folder contains images to create docker files for the build environment.
Speed up the building of the SerialPortStream libnserial library by using
Docker, instead of creating virtual machines. This allows for faster deployment
over many different machines with lower machine overhead.

## Getting Started

The docker image is created to allow building the libraries for a specific
flavour of Ubuntu. The architecture that is being built matches the host
machine.

### Building the Docker Image

To create your docker image, which is based on the Ubuntu version, execute the
appropriate docker command.

```sh
export CODENAME=focal
docker build --build-arg CODE_VERSION=${CODENAME} -t libnserial:${CODENAME} .
```

### Compiling the Software within a Docker Container

To compile the software, go to the folder `dll/serialunix` and run

```sh
mkdir -p ../build
docker run -it --rm --read-only --cap-drop all \
   -v ${PWD}:/source:ro \
   -v ${PWD}/../build:/build:rw \
   --tmpfs /tmp \
   -u $(id -u ${USER}):$(id -g ${USER}) \
   libnserial:${CODENAME}
```

The docker container will look for the file `build.sh` in the source directory
(which is the current directory, as given by `${PWD}`). It will compile
everything in the output folder `./build` in a subdirectory given by the code
name of the docker container iteself (when it was built).

### Building the Debian Packages

Building Debian packages (everything except signing) is automated with this
docker container. It installs the debian builder packages, builds without
running any tests, and makes the debian files available, so they can be then
signed and provided to a package manager (e.g. such as reprepro).

For [https://www.debian.org/doc/manuals/debmake-doc/ch05.en.html](debmake) for
more information about Debian packages.

1. After building the docker image, go to the build directory, where the path
   `debian/changelog` exists. In this project, the correct branch needs to be
   checked out, as the debian scripts are under revision control.

2. Make the build directory

   ```sh
   mkdir -p ../build
   ```

3. Run the docker with the `deb` command. You'll notice it is the same just
   building the sources, but now with the command `deb` added at the end, which
   tells the script in the docker container to build using debian tools instead.

   ```sh
   mkdir -p ../build
   docker run -it --rm --read-only --cap-drop all \
      -v ${PWD}:/source:ro \
      -v ${PWD}/../build:/build:rw \
      --tmpfs /tmp \
      -u $(id -u ${USER}):$(id -g ${USER}) \
      libnserial:${CODENAME} \
      deb
   ```

When running the docker container, it performs the following checks:

* It checks that the file `debian/control` and `debian/changelog` exist
* The first line of `debian/changelog` which contains the code name matches the
  code name of the build docker container.

The output is in the `build/${CODENAME}` folder.

Finally, sign the packages with `debsign`.

### Accessing the Container using a Shell

If you want to access the docker containiner using a shall as root (so you can
check the contents, debug, etc.):

```sh
mkdir -p ../build
docker run -it --rm --read-only --cap-drop all \
   -v ${PWD}:/source:ro \
   -v ${PWD}/../build:/build:rw \
   --tmpfs /tmp \
   libnserial:${CODENAME} \
   sh
```

Note, that this command doesn't set the user name/group and runs as root.