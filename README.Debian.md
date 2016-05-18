# Introduction

This branch is Debian specific. It contains extra software specific for
creating packages that suit a specific variant of Debian.

You do not normally need to check out this branch, unless you're about
to prepare a binary package release for the libnserial.so component.

# Changes

The changes made are in the path `dll/serialunix/debian`.

# Building

## Reference Documentation

The Debian based packages are created based on instructions from various
sources on the Internet:
* https://www.debian.org/doc/manuals/maint-guide/index.en.html
* http://packaging.ubuntu.com/html/packaging-new-software.html
* https://lintian.debian.org/manual/index.html
* https://www.debian.org/doc/debian-policy/policy.pdf

## Generating the Package

* Check out this branch
* Ensure that the build system is clean, so that source tarballs don't
  contain extra files that aren't in the repository:
  ```
  $ git clean -xfd
  ```
* Build the debian packages
  ```
  $ cd serialportstream/dll/serialunix
  $ dpkg-buildpackage -us -uc
  $ lintian -i
  ```

After the build, you'll have four packages:
* libnserial-XXX.deb: The binary library
* libnserial-doc-XXX.deb: Doxygen API
* libnserial-dbg-XXX.deb: Debug symbols
* libnserial-dev-XXX.deb: Header files

# Preparing a New Release

When there's an update to the relevant branch (this is branched from v2.x),
merge to this branch, update the change logs and rebuild.

# Open Issues

## Lintian Warnings

* There are four warnings that are overridden. Please refer to the
  `lintian-overrides` files in the `debian`directory.

## Shared Libraries

* There are a few warnings during package generation about the usage of posix
  threading libraries.
* We don't use the shared library feature of packaging. This needs more
  investigation.

## Multi-arch Libraries

* We use GNU Install DIRs in the CMakeLists.txt. Need to check Multiarch
  packaging for Debian. Some references:
  * https://wiki.ubuntu.com/MultiarchSpec
  * https://wiki.debian.org/Multiarch/Implementation
