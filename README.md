Status: not working yet. WIP.

Experimenting with a very basic browser, trully multiplatform (Windows/MacOS/Linux/Android/iOS),
using [Xamarin.Forms](https://xamarin.com/forms) and [Servo](https://servo.org/).

Building libsimpleservo.so:
```shell
export ANDROID_NDK=…
export ANDROID_SDK=…
./rust/android/build.sh
```

This will create:
```shell
./xamarin/libsimpleservo.h
./xamarin/Servofocus.Android/lib/arm64/libsimpleservo.so
```
