# Building

There are two ways to build:

* Locally; and
* Using Docker

## Building Locally

To build the libraries, you should have the following prerequisites installed:

* Google Test
* Doxygen

### Ubuntu

Run the following commands within Ubuntu 16.04 or later

```sh
sudo apt install libgtest-dev
sudo apt install doxygen
./build.sh
```

## Building using Docker (Ubuntu Images)

Docker containers can be used to build the library for a specific Ubuntu image.
It works by installing the necessary build packages for a docker container based
on a particular release of ubuntu, and then building inside the container, with
the sources and build results provided via mount points given to the `docker
run` command.

For instructions on building, see the file `README.md` in the `docker` folder
above.
