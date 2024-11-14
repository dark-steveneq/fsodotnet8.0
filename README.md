# FreeSO .NET 8.0 Port
Port of the original .NET Framework 4.5 codebase to .NET 8.0

This is not related in any way with [Riperiperi](https://github.com/riperiperi) or the FreeSO development team. This is a passion project that aims to make the engine workable for anyone who dares to do anything with it.

# TODO
## Client
- Possibly rewrite UI script parser so that it doesn't use any abandoned libraries
- Make sure runs on Linux

## Server
- Make `FSO.Server` actually use `CommandLineParser`
- Mina.NET: Remove `BinaryFormatter`
- Port `FSO.Server.API`
- Fix `FSO.Server.Watchdog`
- Remove `FSO.Server.Core`
- Make a Docker container
- Write documentation

## Other
- Rename project folders
- Remove unused projects
- Document each project

# RFC
- Isolate client from server

# Development on Linux
This project seems perfectly compilable (outside of compilation issues) on Linux using VSCode and `dotnet` CLI. If you're using an open source build of VSCode (like VSCodium) you should concider using the proprietary build since debugging is disabled on those because of the debugger's license

# License
> This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
> If a copy of the MPL was not distributed with this file, You can obtain one at
> http://mozilla.org/MPL/2.0/.
