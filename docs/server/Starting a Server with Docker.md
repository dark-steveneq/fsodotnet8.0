# Starting a Server with Docker
> [!NOTE]
> This guide only covers setting up the server in a Docker container. If you want to run the server natively, check [Starting a Server](./Starting%20a%20Server.md) 


0. Make a new folder  
You probably already knew that but I'm putting it here none the less.

1. Copy and customize `compose.yml`  
Copy or download `compose.yml` from [here](https://github.com/dark-steveneq/fsodotnet8.0/blob/master/docker/compose.yml). You can obviously customize it however you like.

2. Copy and customize `config.json`  
Do a simiar thing to but to `config.json` instead, you can find it [here](https://github.com/dark-steveneq/fsodotnet8.0/blob/master/docker/compose.yml). At least you'll have to change the secret.

3. Create `version.txt`  
`version.txt` is a file that contains the version number the server "runs on". If you want your server to be compatible with the official FreeSO client, you should put `beta/update-91a` in there.
> [!NOTE]
> You can skip the updater by holding down Ctrl+Shift+C while clicking on `No`

4. Copy `Content/` from the official FreeSO client  
FreeSO's content is technically not required but it's advised to have it.

5. Copy TSO files to `game/`  
Actually the server only cares about the `TSOClient/` folder.

6. Make `simNFS/` folder  
Docker should make one by itself but eh

7. Start the database and initialize it  
Start the database alone with `docker compose up -d mariadb`, then run `docker compose run -i freeso --db-init`. The program will prompt you if you want to continue, type in `y` and hit Enter.

8. Start the server  
After you've initialized the database, you can start the server with `docker compose up -d --remove-orphans`

## Updating the server
To update the server you can run `docker compose pull`, followed by `docker compose restart`.



After you have everything working, you can join the server with a user `admin` and `password`. You should delete that account from the database after you're sure the server runs. Also if you don't [create a neighborhood](./Creating%20a%20Neighborhood.md) the server will crash uppon lot purchase.