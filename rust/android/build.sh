#!/bin/sh

BASEDIR=$(dirname "$0")
cd $BASEDIR/..

[ -z "$ANDROID_SDK" ] && echo "Need to set ANDROID_SDK" && exit 1;
[ -z "$ANDROID_NDK" ] && echo "Need to set ANDROID_NDK" && exit 1;

#### Feel free to change:

export RUST_TARGET="aarch64-linux-android"
export ANDROID_ARCH="arch-arm64"
export ANDROID_PLATFORM="android-21"
# used by toolchains.cmake
export CMAKE_ANDROID_ARCH_ABI="arm64-v8a"
export NDK_ANDROID_VERSION="21"
NDK_BIN_PATH="$ANDROID_NDK/toolchains/aarch64-linux-android-4.9/prebuilt/darwin-x86_64/bin"
export PATH="$NDK_BIN_PATH:$PATH"

#### You probably don't want to change that

rustup target add aarch64-linux-android

export CMAKE_TOOLCHAIN_FILE="$PWD/android/toolchain.cmake"
export CPPFLAGS="--sysroot $ANDROID_NDK/platforms/$ANDROID_PLATFORM/$ANDROID_ARCH"
export CXXFLAGS="--sysroot $ANDROID_NDK/platforms/$ANDROID_PLATFORM/$ANDROID_ARCH \
  -I$ANDROID_NDK/sources/android/support/include \
  -I$ANDROID_NDK/sources/cxx-stl/llvm-libc++/libcxx/include \
  -I$ANDROID_NDK/sources/cxx-stl/llvm-libc++abi/libcxxabi/include"
export CFLAGS="--sysroot $ANDROID_NDK/platforms/$ANDROID_PLATFORM/$ANDROID_ARCH \
  -I$ANDROID_NDK/sources/android/support/include"
export ANDROID_SYSROOT="$ANDROID_NDK/platforms/$ANDROID_PLATFORM/$ANDROID_ARCH"

echo "Compiling OpenSSL for Android"
# Build OpenSSL for android
export OPENSSL_DIR="$PWD/target/$RUST_TARGET/release/native/openssl/"
mkdir -p $OPENSSL_DIR
cp android/openssl.sh android/openssl.makefile $OPENSSL_DIR
export OPENSSL_VERSION="1.0.2k"
export ANDROID_NDK_ROOT="$ANDROID_NDK"
cd $OPENSSL_DIR
make -f openssl.makefile
cd -
export OPENSSL_LIB_DIR="$OPENSSL_DIR/openssl-$OPENSSL_VERSION/"
export OPENSSL_INCLUDE_DIR="$OPENSSL_LIB_DIR/include/"

export OPENSSL_STATIC="TRUE"

cargo build --release --target $RUST_TARGET

LIB_NAME="libsimpleservo.so"
HEADER_NAME="libsimpleservo.h"
TARGET="./target/$RUST_TARGET/release/$LIB_NAME"
echo "Stripping $TARGET"
$NDK_BIN_PATH/aarch64-linux-android-strip $TARGET
echo "Copying $TARGET to Android project"
cp $TARGET ../xamarin/Servofocus.Android/lib/arm64/$LIB_NAME
cp ./target/$HEADER_NAME ../xamarin/
