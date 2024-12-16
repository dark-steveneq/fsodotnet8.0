# Starting a Server
> [!NOTE]
> This guide only covers setting up the server natively. If you want to run the server with Docker, check [Starting a Server with Docker](./Starting%20a%20Server%20with%20Docker.md) 

0. Install MariaDB  
MariaDB is required for the server to store info such as sims, cities, passwords and so on. You can download it [here](https://mariadb.org/download) and if you need help with setting it up, consult whatever tutorial you can find on Google.

1. Make a new folder  
You probably already knew that but I'm putting it here none the less.

2. Create a SimNFS folder  
SimNFS is a folder which contains lot and object data. You'll need to create it and then point the server to it in the next step

3. Rename `config.sample.json` to `config.json` and customize it  
Most importantly you'll need to
- Change `database` value
- Change `gameLocation` to point to TSO's `TSOClient` folder
- Change `secret` to a random string of data, preferably 32 characters long
- Change `simNFS` to the path of your folder  
After this you're pretty much done with `config.json`

4. Modify `version.txt`  
`version.txt` is a file that contains the version number the server "runs on". If you want your server to be compatible with the official FreeSO client, you should put `beta/update-91a` in there.
> [!NOTE]
> You can skip the updater by holding down Ctrl+Shift+C while clicking on `No`

5. Replace `Content/` with the one from the official FreeSO client  
Content included with the server doesn't contain everything FreeSO had to offer so copy the one from the client.

7. Initialize the database  
By default, the database is completely empty. To change that, open up a Terminal window in your server's folder and run `server.exe --db-init` or `./server --db-init` if you're on a *nix machine. The progam will ask you if you want to proceed, type in `y` and hit Enter.

8. Start the server  
After you've initialized the database, you can start the server by double-clicking `server.exe` or by launching the file from the Terminal without any arguments

## Ports used
- 9000/tcp
- 33101/tcp
- 34101/tcp
- 35101/tcp

After you have everything working, you can join the server with a user `admin` and `password`. You should delete that account from the database after you're sure the server runs. Also if you don't [create a neighborhood](./Creating%20a%20Neighborhood.md) the server will crash uppon lot purchase.