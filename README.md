# FreeSO .NET 8.0 Port
Port of the original .NET Framework 4.5 codebase to .NET 8.0

This is not related in any way with [Riperiperi](https://github.com/riperiperi) or the FreeSO development team. This is a passion project that aims to make the engine easier to work with for anyone who dares to do anything with it.

# State
Server seems to work fine. The client on the other hand needs a little bit of work (broken PNG rendering, single interaction plays multiple different sounds over one another) but it's playable.

# TODO
## Client
- ~~Possibly rewrite UI script parser so that it doesn't use any abandoned libraries~~
- ~~Make sure runs on Linux~~
- Fix cursor loading
- Make sound and texture profiles
- Fix texture rendering
- Fix sound issue

## Server
- ~~Fix crashing (sometimes?) when buying a lot~~ (It's caused by missconfiguration)
- ~~Make `FSO.Server` actually use `CommandLineParser`~~
- Mina.NET: Remove `BinaryFormatter`
- ~~Port `FSO.Server.API`~~
- Fix `FSO.Server.Watchdog`
- ~~Remove `FSO.Server.Core`~~
- Improve logger
- ~~Implement SQLite3 database driver~~ Port to EFCore
  - Automatic database creation
- ~~Implement optional register page~~
- ~~Make admin avatars start off with 999999999 simoleons~~
- ~~Make a Docker container~~
- Write documentation

## All
- Remove all commented out things
- Make server content loading use interfaces 

## Other
- ~~Rename project folders~~
- Remove unused projects
- Document each project

# Development on Linux
This project seems perfectly compilable (outside of compilation issues) on Linux using VSCode and `dotnet` CLI. If you're using an open source build of VSCode (like VSCodium) you should concider using the proprietary build since debugging is disabled on those because of the debugger's license

# Credits
- [Riperiperi](https://github.com/riperiperi) - Making FreeSO in the first place
- [JDrocks450](https://github.com/JDrocks450) - Making [nio2so](https://github.com/JDrocks450/nio2so), a Pre-Alpha server emulator which I borrowed some code from and actually never ended up using it but his project is still worth to mention either way
- All the people you see on the right 
- You - Checking the repo out

# License
> This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
> If a copy of the MPL was not distributed with this file, You can obtain one at
> http://mozilla.org/MPL/2.0/.
