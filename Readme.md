To run this API on a Windows machine you'll need to have Docker Desktop installed. (and WSL updated).

Run docker-compose:
```console
docker-compose up --build
```
It will build 2 docker images: one for the API and another for the SQL server.
after docker images are built open the app at http://localhost:8080/swagger/index.html

## Alternative option (without Docker):
 1. Provide a valid connection string to the SQL server instance into the launchSettings.json file (or directly add it to Environment variables)
 2. build the API using:
```console
dotnet run
```
after the app is built you can access it at http://localhost:5045/swagger/index.html
