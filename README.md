# MidroundJoin
![Build](https://github.com/ModioMori/MidroundJoin/actions/workflows/build.yml/badge.svg)

A [BepInEx](https://github.com/BepInEx/BepInEx) mod for the Steam game/demo [Gladio Mori](https://store.steampowered.com/app/2689120/Gladio_Mori/), allowing players to connect mid-round. They'll be spawned with the Bardiche by default on the next round restart.

**NOTE: Only the host of the match requires this mod! This will do nothing for clients.**

# Installation
1. [Download and install BepInEx to the game's directory.](https://docs.bepinex.dev/articles/user_guide/installation/index.html#installing-bepinex-1) You'll have to browse to the game's executable; on most systems, this will be at `C:\Program Files (x86)\Steam\steamapps\common\Gladio Mori Demo`. If you can't find it, use the lower instructions to open the game's install folder and find the path.
2. [Go to the Releases page and download the latest release.](https://github.com/ModioMori/MidroundJoin/releases)
3. Open the game's install folder. ![steamwebhelper_cULdZeOTQa](https://github.com/ModioMori/MorePlayers/assets/19525688/b07f69d6-7727-48b2-9810-6335479f66fb)
4. Drag the DLL you downloaded into the `BepInEx/plugins` folder. You may have to run the game once then close it if this folder doesn't exist. ![explorer_NyczDKF4uW](https://github.com/ModioMori/MorePlayers/assets/19525688/8a2ce78a-0caf-4a80-8fef-578378595896)
5. Launch the game and play!

# Compiling
The csproj uses reference paths based on the default Steam library installation folder of Gladio Mori. If you have the game installed on another drive, you will have to change these paths manually.
Otherwise, open the solution with Visual Studio 2022 or above (may work on older versions) and the .NET Desktop Development component installed in Visual Studio Installer.
Build > Build Solution should work perfectly fine, even without BepInEx installed (uses NuGet).

Alternatively, download and install [.NET 8](https://dotnet.microsoft.com/en-us/download) and use `dotnet build` in the project directory.
