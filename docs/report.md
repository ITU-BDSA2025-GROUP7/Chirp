---
title: _Chirp!_ Project Report
subtitle: ITU BDSA 2025 Group 7
author:
- "Nikki Skarsholm Risager <nris@itu.dk>"
- "Louis Falk Knudsen <lofk@itu.dk>"
- "Hassan Hamoud Al Wakiel <halw@itu.dk>"
- "Kristoffer Mejborn Eliasson <krme@itu.dk>"
- "Mette My Gabelgaard <mmga@itu.dk>"
numbersections: true
---
# Introduction (Hassan)

# Design and Architecture of _Chirp!_

## Domain model (Nikki)

Here comes a description of our domain model.

## Architecture — In the small (Kris)

## Architecture of deployed application (Louis)
The application is deployed on Microsoft Azure
[here](https://bdsagroup7chirprazor-buhcfwanakgyaabx.germanywestcentral-01.azurewebsites.net/),
but it can be run locally as well, in which case the executing terminal process is the application
server.

![Deployment diagram](images\deployment.png)

## User activities (Hassan)

## Sequence of functionality/calls trough _Chirp!_ (My)

# Process

## Build, test, release, and deployment (Nikki)

## Team work (Kris)

## How to make _Chirp!_ work locally (Louis)

### Dependencies

The application requires you to have installed [.NET 9.0](https://dotnet.microsoft.com/en-us/download)
or later.
You will be prompted to install any other missing dependencies.

### Startup

To build and start the programme on an available port, execute the following command:

```
dotnet run --project src/Chirp.Web
```

or, if you have an implementation of Make installed,

```
make start
```

A message in the console will inform you of the specific URL and port to navigate to in order to
interact with the web application.

## How to run test suite locally (Nikki)

# Ethics

## License (My)

## LLMs, ChatGPT, CoPilot, and others (Kris)