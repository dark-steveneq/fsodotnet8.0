# Starting a Server
> [!WARNING]
> This guide will get updated soon enough


> [!NOTE]
> This guide only covers setting up the server natively. If you want to run the server in Docker, check [Starting a Server with Docker](https://github.com/dark-steveneq/fsodotnet8.0/blob/master/docs/server/Starting%20a%20Server%20with%20Docker.md) 

0. Install .NET 8.0 SDK  
You can find the downloads to it [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

1. Compile the Server  
Clone the repo if you haven't already. After that go into the `TSOClient/FSO.Server` folder, open a Terminal/Command Prompt and run `dotnet build`. The compiled server is located in `TSOClient/FSO.Server/bin/Debug/net8.0`. You can copy it somewhere else if you like.

2. Install and MySQL-compatible database (possibly outdated if I implement it)

3. Configure the Server  
Rename `config.sample.json` to `config.json` and edit it to your liking. You'll definitly need to change `gameLocation`, `secret`, `simNFS` and `connectionString` for the database.

4. Initialize the Database  
To initialize the database, you'll need to run the server executable with `--db-init`. The server will ask you if you want to proceed so press `y` (unless you don't want to actually make the server).

5. Start the server  
To start the server, simply run the executable without any arguments, it'll automatically run with `--run`.


After you have everything working, you can join the server with a user `admin` and `password`. You should delete that account from the database after you're sure the server runs. Also if you don't [create a neighborhood](https://github.com/dark-steveneq/fsodotnet8.0/blob/master/docs/server/Creating%20a%Neighborhood.md) the server will crash uppon lot purchase.