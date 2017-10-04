#!/bin/sh


# msbuild servofocus.sln /t:Clean,Build
msbuild  /t:Clean,Build,PackageForAndroid,SignAndroidPackage ./Servofocus.Android/Servofocus.Android.csproj

PATH=/Users/paul/Library/Android/sdk//platform-tools/:/Users/paul/Library/Android/sdk//build-tools/25.0.3/:$PATH

apk=./Servofocus.Android/bin/Debug/com.browser.servofocus-Signed.apk

pkg=$(aapt dump badging $apk|awk -F" " '/package/ {print $2}'|awk -F"'" '/name=/ {print $2}')
act=$(aapt dump badging $apk|awk -F" " '/launchable-activity/ {print $2}'|awk -F"'" '/name=/ {print $2}')

echo "stopping"
adb shell am force-stop $pkg
echo "installing"
adb install -r $apk
echo "starting"
adb shell am start -n $pkg/$act
adb logcat
