# DarkMultiPlayer

DarkMultiPlayer is a multiplayer mod for Kerbal Space Program. It supports subspace style (and master controlled warp) warping and career mode, with an easy to edit server database.

The DarkMultiPlayer client and server are cross platform, see [Install](#install).

## Install
### Client
* Download the [DMPClient zip](https://spacedock.info/mod/10) and extract to `[KSP root folder]/GameData`
* Download [DMPUpdater](http://godarklight.privatedns.org/dmp/downloads/dmpupdater/), place the program on your KSP folder and run it.

### Server
The DarkMultiPlayer server is cross platform, meaning you can run it on any platform that supports .NET.
In Linux or macOS, you must have [Mono](http://mono-project.com) installed to be able to run the server.
* Download the [DMPServer zip](https://spacedock.info/mod/11/DarkMultiPlayer%20Server)
* Download [DMPUpdater](http://godarklight.privatedns.org/dmp/downloads/dmpupdater/), place the program on your server folder and run it.
  - NOTE: you must have a previous server version in the folder for DMPUpdater to work.

You can configure your server by editing `Config/Settings.txt`.  
If your server's game difficulty is set to `CUSTOM`, you can alter gameplay settings by editing `Config/GameplaySettings.txt`.

## Compiling
- Copy the assemblies from `[KSP root folder]/KSP_Data/Managed` to `External/KSPManaged`:
Run msbuild /p:Configuration=Release (or build with your preferred IDE) to build the plugin and the .NET Framework version of the server. This single build is completely cross platform.
Navigate to DotNet/Server/ and either run `dotnet publish -c release -f netcoreapp3.1` to build for only your OS, or `./compile.sh` to build for every OS

## Mod Control
Read `DMPModControl.txt`, it's commented. The file can be copied from a development KMPServer (The one where you can use SHA sums, not the one with the !md5 section) as the file format is the same.

If you are running a private server, it's safe enough to just add the missing parts.

You can get the DMP client to make a `DMPModControl.txt` file specific for your GameData directory by pressing `Options -> Advanced -> Mod Control -> Generate`. The whitelist option will only allow you to connect with the mods in your GameData directory. The blacklist option will allow you to connect with any mods.
