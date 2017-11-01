#!/bin/sh

pkg="com.browser.servofocus"

PATH=/Users/paul/Library/Android/sdk//platform-tools/:/Users/paul/Library/Android/sdk//build-tools/25.0.3/:$PATH
echo "stopping"
adb shell am force-stop $pkg

msbuild \
  /t:Clean,Build,PackageForAndroid,SignAndroidPackage,Install \
  /p:Configuration=Release /p:AdbTarget="-s CB5A2AS54G" \
  ./Servofocus.Android/Servofocus.Android.csproj

apk=./Servofocus.Android/bin/Release/com.browser.servofocus-Signed.apk

act=$(aapt dump badging $apk|awk -F" " '/launchable-activity/ {print $2}'|awk -F"'" '/name=/ {print $2}')

echo "installing"
adb install -r $apk
echo "starting"
adb shell am start -n $pkg/$act
