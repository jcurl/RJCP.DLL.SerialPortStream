ARG CODE_VERSION=latest
FROM ubuntu:${CODE_VERSION}
LABEL MAINTAINER="Jason Curl <jcurl@arcor.de>"

# We need to make the install non-interactive, so that we don't get asked for
# tzdata. We don't need DEBIAN_FRONTEND after the build, so we don't make it an
# explicit variable.
ARG DEBIAN_FRONTEND=noninteractive

# And if we happen to build an unsupported release that doesn't receive updates
# anymore, just update the apt sources and continue.
RUN (apt update -y || \
      sed -i -re 's/([a-z]{2}\.)?archive.ubuntu.com|security.ubuntu.com/old-releases.ubuntu.com/g' /etc/apt/sources.list \
      && apt update -y) \
    && apt install -y --no-install-recommends \
      build-essential cmake doxygen libgtest-dev autoconf automake libtool m4 \
      lsb-release debhelper fakeroot devscripts \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

RUN mkdir /source
WORKDIR /source

COPY libnserial-build.sh /usr/local/bin/
RUN chmod 755 /usr/local/bin/libnserial-build.sh

ENTRYPOINT [ "libnserial-build.sh" ]
CMD [ "build" ]