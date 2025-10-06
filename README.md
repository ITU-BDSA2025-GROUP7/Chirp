# Chirp
The **Chirp** project for the _Analysis, Design, and Software Architecture_ (BDSA) course at
the IT University of Copenhagen, Fall 2025.

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
dotnet run --project src/Chirp.Razor
make start-razor

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
dotnet run --project src/Chirp.Razor
```
```
make start-razor
```

Running the program will connect the client to the default database `src/Chirp.Razor/data/Chirp.db`.

To stop it listening, you will have to cancel the process (`Ctrl + C`), or close the
terminal window.

The environment variables `CHIRPDB_SCHEMA` and `CHIRPDB_DATA` can be set
to override the default .sql files that are executed when initialising
a new database.


## Online Setup and Usage

### Setup (Deploying to Azure)
The program will automaticly uploade and deploy to Azure on https://bdsagroup7chirprazor-buhcfwanakgyaabx.germanywestcentral-01.azurewebsites.net/. This happens whenever new changes are pushed into main.


### Running localy
The program can be run localy using the comand make start-razor

## Testing

To run all the tests in the solution:
```
dotnet test
```
```
make test
```

---

## Contributing

GitHub workflows enforce a few rules for the repository:
1. Pull requests cannot be accepted unless all tests pass.
2. All releases must have a tag in the format v\<major>.\<minor>.\<patch>, e.g. v1.0.4.
