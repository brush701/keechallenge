[KeeChallenge v1.5](http://brush701.github.io/keechallenge/ "KeeChallenge Documentation")
=================
Copyright 2014 Ben Rush

## Changes
v1.5
* Thanks to Robert Claypool for his numerous contributions to clean up and improve Keechallenge
* Migrated to Github from Sourceforge.
* Changed recovery mode to better support variable length challenges. 
* MD5 Checksum: `80A7EADA1C86332B3F91B75E4E8317F0`
* SHA1 Checksum: `06C3B96ED674E5617F0DAFF5101E23EF95AFF71C`

v1.4
* Added support for variable length challenges. To use it, a new composite master key must be created.
* MD5 Checksum: `7C2F5D8CCBE9549767CA15DE0FBF7383`
* SHA1 Checksum: `BE00768E0279B9206A5A73A143B54AB77F29093A`

v1.3
* MD5 Checksum: `21112DB3FE7AD688FD0DEA4B3738F90A`
* SHA1 Checksum: `D61CCAC053EFAF112D60D0C0DA4683638B8FE1F9`
* Added OSX support. Thanks to Markku Roponen for figuring this out!
* Updated Yubico libraries to v1.16.2 to support Yubikey Neo firmware 3.3.0

v1.2
* Bug fixes for dynamic 32/64 bit support
* Added button for recovery mode and fixed a bug

v1.1
* Changed release numbering scheme to major.minor
* Added support for OpenURL function
* Persisted slot choice
* Provide support for 32 bit systems
* Windows installs require XP SP1 or higher
* Fixed null reference error on cancellation

v1.0.2
* Added support for choosing Yubikey slot via Tools->KeeChallenge Settings. Default is slot 2
* Added plugin update checking
* Don't start the 15 second countdown until the Yubikey is inserted

v1.0.1
* Updated KeeEntry.cs and YubiWrapper.cs to properly initialize and clean up the native Yubico libraries

## Supported Platforms
As of v1.0.1 both Windows and Linux (Ubuntu) have been tested successfully. To run under Linux using mono, you must modify `KeeChallenge.dll.config` and add a dllmap entry to let Mono know where to find the native libraries. On my system this looks like `<dllmap dll="libykpers-1-1.dll" target="libykpers-1.so>`. For this to work, you must also obtain the appropriate versions of the Yubico libraries. Make sure all of the Yubico libraries are installed where mono can find them (for example, /usr/lib). Put both `KeeChallenge.dll` and `KeeChallenge.dll.config` in the KeePass2 folder (on Ubuntu this is `/usr/lib/keepass2`). The same technique will work on OSX, but getting the 32bit Yubico libraries requires building from source. See the OSX Guide by Markku for detailed instructions on how to do this.  

## Dependencies
KeeChallenge requires KeePass2, available from http://keepass.info/download.html. It also requires the Yubico open source library yubico-personalization (which in turn depends on yubico-c). Prebuilt bundled binaries are available from http://opensource.yubico.com/yubikey-personalization/releases.html. 

## Building
Open the top level solution and adjust the references to point at your installed `KeePass.exe`. It should (hopefully) build without problems once this is done. You should check that the DllImport statements in Yubiwrapper.cs match the file names of the binaries you have obtained. 

## Running
Copy `KeeChallenge.dll` and the "32bit" and "64bit" subdirectories containing all the Yubico libraries and dependencies into the directory containing `KeePass.exe`. The plugin should be loaded as a key provider when creating/changing your database password.

## Using
KeeChallenge works using the HMAC-SHA1 challenge response functionality built into the Yubikey. First, configure your Yubikey to use HMAC-SHA1 in slot 2. Ensure that the challenge is set to fixed 64 byte (the Yubikey does some odd formatting games when a variable length is used, so that's unsupported at the moment). I recommend requiring a button press to issue the response, but it should work either way. Copy the secret and keep it somewhere safe since you'll need it to recover your database if you lose your Yubikey. 

When you set the password on your database, you should select Yubikey challenge-response under key providers and click ok. In the window that comes up, copy and paste the secret from your Yubikey. You will be prompted to insert your Yubikey and press the button to verify that you entered the correct secret. 

Your secret is used as the key to encrypt the database. In order to avoid storing the secret in plain text, we generate a challenge-response pair ahead of time. The challenge is stored to be issued on the next login and the response is used as an AES256 key to encrypt the secret. All relevant data is stored in a xml file in the same directory as your database. 

If the xml file gets corrupted or deleted (or if you lose your Yubikey) a recovery mode is provided to allow you to enter your secret (you did save it, didn't you?) and decrypt the database. 

KeeChallenge is not intended to be used as the sole means of authenticating yourself to KeePass. It's entirely vulnerable to physical attacks: if you are only using your Yubikey to login and somebody steals it, your database will be compromised. You should always use KeeChallenge in conjunction with a strong master password to mitigate this risk. This also allows us to take advantage of KeePass' built in protections against brute forcing.

## Common Errors
Users occasionally report that KeeChallenge does not work with a new version of KeePass. This occurs immediately after an update to KeePass and is easily identified by the warning message: "The following plugin is incompatible with the current KeePass version...". This error is caused by an out of date version of the file `KeePass.exe.config`, which lives in the KeePass install directory. The problem can be most reliably resolved by doing a complete uninstall/reinstall of KeePass. Alternatively, you can download the portable version of KeePass and copy the config file from there into your KeePass install directory.
