To run this API on a Windows machine you should have Docker Desktop installed. (and WSL updated).

Run docker-compose:
```console
docker-compose up --build
```
It will build 2 docker images: one for the API and another for SQL server.
after docker images are built open the app at http://localhost:8080/swagger/index.html

Alternative option (without Docker):
To build and run the API without Docker, you'll need to provide a valid connection string to the SQL server instance into the launchSettings.json file (or directly add it to Environment variables)
build it using
```console
dotnet run
```
after the app is built you can access it at http://localhost:5045/swagger/index.html
