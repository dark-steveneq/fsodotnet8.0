# FreeSO .NET 8.0 Port Server Container
This image contains a built copy of FreeSO .NET 8.0 Server. Both AMD64 and ARM64 are supported.

## Example Setup
### compose.yml
```yaml
services:
  freeso:
    build: .
    restart: unless-stopped
    ports:
      - 9000:9000/tcp
      - 33101:33101/tcp
      - 34101:34101/tcp
      - 35101:35101/tcp
    volumes:
      - ./config.json:/App/config.json:rw
      - ./Content:/App/Content:rw
      - ./version.txt:/App/version.txt:ro
      - ./simNFS:/simNFS:rw
      - ./game/TSOClient:/game:ro

  mariadb:
    image: mariadb:latest
    restart: unless-stopped
    environment:
      MARIADB_DATABASE: fso
      MARIADB_USER: fsoserver
      MARIADB_PASSWORD: password
      MARIADB_ROOT_PASSWORD: password
    ports:
      - 3306:3306/tcp # Do not port forward this or at least change the default database credentials 
    volumes:
      - ./mariadb:/var/lib/mysql:rw
```
### config.json
```json
{
    "gameLocation": "/game",
    "secret": "YOUR SECRET GOES HERE, IT SHOULD BE 32 CHARS LONG AND RANDOM",
    "simNFS": "/simNFS",
  
    "database": {
      "connectionString": "server=mariadb;uid=fsoserver;pwd=password;database=fso;"
    },
  
    "services": {
      "tasks":{
          "enabled": true,
          "call_sign": "callisto",
          "binding": "0.0.0.0:35100",
          "internal_host": "127.0.0.1:35",
          "public_host": "127.0.0.1:35",
  
          "schedule":[
              {
                  "cron":"0 3 * * *", 
                  "task":"prune_database", 
                  "timeout":3600,
                  "parameter":{}
              },
              {
                  "cron":"0 4 * * *", 
                  "task":"bonus", 
                  "timeout":3600,
                  "shard_id": 1,
                  "parameter":{}
              }
          ],
  
          "tuning":{
              "bonus":{
                  "property_bonus":{
                      "per_unit": 10,
                      "overrides":{
                          "1": 1500,
                          "2": 1250,
                          "3": 1000
                      }
                  },
                  "visitor_bonus":{
                      "per_unit": 8
                  }
              }
          }
      },
  
      "api": {
        "enabled": false,
        "bindings": [ "https://auth.east.ea.com:443/", "http://localhost:80/" ],
        "controllers": [ "auth", "citySelector" ]
      },
  
      "userApi": {
        "enabled": true,
        "bindings": [ "http://0.0.0.0:9000/" ],
        "siteName": "My Docker FreeSO Server", 
        "controllers": [ "auth", "citySelector" ],
        "updateUrl": "http://some-url"
      },
  
      "cities": [
        {
          "call_sign": "ganymede",
          "id": 1,
          "binding": "0.0.0.0:33100",
          "internal_host": "127.0.0.1:33",
          "public_host": "127.0.0.1:33",
    
          "maintenance":{
              "cron":"0 4 * * *",
              "timeout":3600,
              "visits_retention_period":7,
              "top100_average_period": 4
          }
        }
      ],
  
      "lots": [
          {
              "call_sign": "europa",
              "binding": "0.0.0.0:34100",
              "internal_host": "127.0.0.1:34",
              "public_host": "127.0.0.1:34",
  
              "max_lots": 100,
  
              "cities": [
                  {
                      "id": 1,
                      "host":"127.0.0.1:33100"
                  }
              ]
          }
      ]
    }
  }
  
```