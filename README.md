# Chirp
The **Chirp.CLI** project for the _Analysis, Design, and Software Architecture_ (BDSA) course at
the IT University of Copenhagen, Fall 2025.

This project has two sub-programmes; a **client** and a **service**.

The **service** is a daemon: once started, it keeps running until someone stops it, and
while it runs, it is continuously listening for incoming HTTP requests and responding accordingly.

The **client** programme is very simple; it can display a list of records stored on the server,
or store a new record on the server. "Server" is here understood to be the computer
running the service programme.\
To perform either of its tasks, the client must interact with the service, which in turn interacts with
a database of records. Therefore, the service must be running (and listening on the same
address and port the client is speaking to) for the client to do anything.\
In contrast to the service programme, once the client has finished its task, it exits.

---

## Requirements

The application targets .NET 9.0, and so requires the .NET 9.0 Framework.\
You will be prompted to download any such missing dependencies when running the application.

---

## Command overview

All commands here and below expect you to be in the main solution directory (i.e. the same as this file).\
When a `dotnet` and `make` command are listed together, they are equivalent.\
Executing `make` commands requires you to have GNU Make installed.
```
dotnet run --project src/Chirp.CLI.Client read [<limit>]
make read [n=<limit>]

dotnet run --project src/Chirp.CLI.Client cheep <message>
make cheep n=<message>

dotnet run --project src/Chirp.CLI.Client -e ASPNETCORE_ENVIRONMENT=Test read [<limit>]
make read-local [n=<limit>]

dotnet run --project src/Chirp.CLI.Client -e ASPNETCORE_ENVIRONMENT=Test cheep <message>
make cheep-local n=<message>

dotnet run --project src/Chirp.CSVDBService [<port>]
make start [n=<port>]

dotnet test
make test

dotnet build
make build
```

---

## Local Setup and Usage

### Starting the Service Locally

To build and start the daemon on the current computer, execute either:
```
dotnet run --project src/Chirp.CSVDBService [<port>] [-e CHIRPDB_PATH=<DB path>]
```
```
make start [<port>]
```

If a `<port>` is provided, it will start listening on `http://localhost:<port>`.\
Otherwise, it will default to something like `http://localhost:5000`.

If the environment variable `CHIRPDB_PATH` is set, i.e. `DB path` as seen above,
the system it will attempt to open the specified file. If it has not been set,
the default database `src/ChirpCSVDBService/data/Chirp.db` is used instead.
If the database it looks for cannot be found, it will create a new one of the same name in
the user's temporary directory.

To stop it listening, you will have to cancel the process (`Ctrl + C`), or close the
terminal window.

The environment variables `CHIRPDB_SCHEMA` and `CHIRPDB_DATA` can be set
to override the default .sql files that are executed when initialising
a new database.

### Using the Client Locally

**To read cheep(s):**
```
dotnet run --project src/Chirp.CLI.Client -e ASPNETCORE_ENVIRONMENT=Test read [<limit>]
```
```
make read-local [n=<limit>]
```
where `<limit>` is the maximum number of records to retrieve.

**To store a cheep:**
```
dotnet run --project src/Chirp.CLI.Client -e ASPNETCORE_ENVIRONMENT=Test cheep <message>
```
```
make cheep-local n=<message>
```
where `<message>` is the message to store.

---

## Online Setup and Usage

### Setup (Deploying to Azure)

It will likely be easiest to use the [Azure CLI application](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest).

First, login to Azure:
```
az login
```
Then, upload and deploy the application:
```
az webapp up --sku F1 --name <name> --os-type Linux --location <location> --runtime DOTNETCORE:9.0
```
It will be accessible on https://\<name>.azurewebsites.net.\
Only a subset of Azure's \<location>s are valid to all users.
Check [this page](https://portal.azure.com/#view/Microsoft_Azure_Policy/PolicyMenuBlade/~/Assignments) for details.

You may encounter an error relating to a subscription registration. In that case,
go to the Azure web portal, and then
`Subscriptions -> Settings -> Resource Providers` and select `Microsoft.Web` (or whatever was mentioned in the error message).

Note that you only have a limited amount of uptime allotted to you.\
To pause the app:
```
az webapp stop
```
To (re)start the app:
```
az webapp start
```
To take the app down entirely: 
```
az webapp delete
```

See more options with
```
az webapp --help
```

### Using the Client over the Network

To use the client programme with the server already running on the Azure cloud platform:

**Print records to the standard output:**
```
dotnet run --project src/Chirp.CLI.Client read [<limit>]
```
```
make read [n=<limit>]
```
where `<limit>` is the maximum number of records to retrieve.

**Store a new record (a "cheep"):**
```
dotnet run --project src/Chirp.CLI.Client cheep <message>
```
```
make cheep n=<message>
```
where `<message>` is the message to store.

---

## Testing

To run all the tests in the solution:
```
dotnet test
```
```
make test
```

Overview of commands to run individual projects' tests:
```
dotnet test tests/Chirp.CLI.Tests -e ASPNETCORE_ENVIRONMENT=Test
make tclient

dotnet test tests/Chirp.CSVDB.Tests -e ASPNETCORE_ENVIRONMENT=Test
make tdatabase

dotnet test tests/Chirp.ServicesTest -e ASPNETCORE_ENVIRONMENT=Test
make tservice
```

---

## Contributing

GitHub workflows enforce a few rules for the repository:
1. Pull requests cannot be accepted unless all tests pass.
2. All releases must have a tag in the format \<major>.\<minor>.\<patch>, e.g. 1.0.4.
