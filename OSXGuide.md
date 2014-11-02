## About
`Author: Markku Roponen`
Purpose of this guide is to help you to get KeePass 2 KeeChallenge plugin to work on Mac OS X Mavericks (10.9.x). This guide most likely helps also users with other OS X versions as well.

## Prerequisities
I'm asuming that you have successfully installed official KeePass 2 and you are running it using Mono, you also have official [KeeChallenge](http://sourceforge.net/projects/keechallenge/) plugin installed.
I have used unofficial build of KeePass 2 from [here](http://keepass2.openix.be/), because I was unable to get official portable version work.

## Problem
Using KeeChallenge plugin in Mac OS X 10.9.x has problem with [libykpers](https://developers.yubico.com/yubikey-personalization/Releases/) because there is no 32-bit build available for Mac OS X nor universal build. There is only 64-bit version and it is not working with Mono, because Mono is 32-bit, unless you have [build it yourself](http://www.mono-project.com/docs/about-mono/supported-platforms/osx/).

## Solution(s)
There are two solutions for this issue.

1. Use [libykpers](https://developers.yubico.com/yubikey-personalization/Releases/) provided by Yubico team and build 64-bit version of Mono. Help for that solution can be found [here](http://www.mono-project.com/docs/about-mono/supported-platforms/osx/).
1. Use 32-bit version of Mono and build universal version of the `libykpers`. I did choose this solution :)

# Preparations
First create file `KeeChallenge.dll.config` in to folder where your `KeeChallenge.dll` is located and paste the following contents in to it and save.
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
    <dllmap dll="libykpers-1-1.dll" target="libykpers-1.1.dylib" />
</configuration>
```

Add `DYLD_LIBRARY_PATH` environment variable to your `~/.bash_profile` and point it to `/usr/local/lib/`. Remember to restart your Terminal.
```bash
export DYLD_LIBRARY_PATH=$DYLD_LIBRARY_PATH:/usr/local/lib/
```

## Prerequisites for libykpers build
Before begining `libykpers` build process, you need to have [Homebrew](http://brew.sh/) installed and then install following dependencies.
```basb
brew install autoconf automake libtool pkg-config help2man asciidoc
```
To prevent errors from `a2x` (asciidoc) you need to do following (because `a2x` cannot access `XML_CATALOG_FILES` environment variable set for asciidoc, [read more](https://groups.google.com/forum/#!msg/asciidoc/FC-eOwU8rYg/ZEoO8UWuCvsJ))
```bash
sudo mkdir /etc/xml
sudo ln -s /usr/local/etc/xml/catalog /etc/xml/catalog
``` 

Create some folders to keep stuff organized.
```bash
cd ~/
mkdir workspace
cd workspace/
mkdir Yubico
cd Yubico/
```

### Building libyubikey (yubico-c)
`Libykpers` is depends on `libyubikey` and thereof we need to build it first, we are building universal build so that it will work in both 32-bit and 64-bit environments.
```bash
cd ~/worspace/Yubikey/
git clone https://github.com/Yubico/yubico-c.git
cd yubico-c/
autoreconf --install

./configure CC="gcc -arch i386 -arch x86_64" \
 	CXX="g++ -arch i386 -arch x86_64" \
 	CPP="gcc -E" CXXCPP="g++ -E" 

make check
make install

#Clean up
make clean
```

To check that `libyubikey` is installed correctly, type:
```bash
ls -l /usr/local/lib
``` 
...and you should see `libyubikey.dylib` in the file list.

### Building libykpers (yubikey-personalization)
If you got the `libyubikey` successfully build, you can start building the actually needed library `libykpers`. We are making universal build of this library as well.
```bash
cd ~/workspace/Yubikey/
git clone https://github.com/Yubico/yubikey-personalization.git
cd yubikey-personalization/
autoreconf --install

./configure CC="gcc -arch i386 -arch x86_64" \
 	CXX="g++ -arch i386 -arch x86_64" \
 	CPP="gcc -E" CXXCPP="g++ -E" \
 	--with-libyubikey-prefix=/usr/local

make check
make install

#Clean up
make clean
```

To check that `libykpers` is installed correctly, type:
```bash
ls -l /usr/local/lib
```
...and you should see `libykpers-1.1.dylib` in the file list. That is the library which KeeChallenge plugin is needing.
To make sure that is it universal, type:
```bash
lipo -info /usr/local/lib/libykpers-1.1.dylib
#And you should get this output
Architectures in the fat file: /usr/local/lib/libykpers-1.1.dylib are: i386 x86_64
``` 

## Testing
Now you should be able to run KeeChallenge plugin in Mac OS X 10.9.x. If you cannot get it working you can run KeePass from shell and use Mono loging mode to get more information.
```bash
cd /Applications/KeePass2.23.app/Contents/MacOS/
MONO_LOG_LEVEL=debug mono KeePass.exe
``` 
Now try to use KeeChallenge plugin and you get the log information in to console. Those should help you to resolve the possible issues.