# Comando para subir o SQL Server no docker
```
docker compose -f .\sql_server_compose.yml up -d
```

# Comando para subir as APIs
```
docker compose build
docker compose up -d
```

# Deploy na Azure
```
az container create --resource-group GoblinWarsRecursos --file deployPodsAz.yml
```

# Check Deploy
```
az container show --resource-group GoblinWarsRecursos --name goblin-wars --output table
```

# Registry Log
```
az acr login --name registrygw
```