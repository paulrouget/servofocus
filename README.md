Building libservobridge.so:
```shell
export ANDROID_NDK=…
export ANDROID_SDK=…
./rust/android/build.sh
```

This will create:
```shell
./xamarin/libservobridge.h
./xamarin/Servofocus.Android/lib/armeabi-v7a/libsimpleservo.so
```
