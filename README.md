# How to run

* ./jaeger-all-in-one.exe 

* ./consul.exe agent -dev -config-file consulConfig.json

* go to ```http://localhost:8500/ui/dc1/kv```

* add ```example/config```

```json
{
  "connectionString" : "Server=serverName; Database=databaseName; User ID=id;Password=password;"
}
```

* start services one by one

* consul will health check every service

* send request to ```http://adderess:port/api/GetValues```

* check tracing in jaeger
