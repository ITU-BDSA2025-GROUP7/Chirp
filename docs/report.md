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
The application is deployed to an application server owned by the vendor Microsoft Azure
at https://bdsagroup7chirprazor-buhcfwanakgyaabx.germanywestcentral-01.azurewebsites.net/.

The following deployment diagram shows the most relevant (out of several hundred) artifacts which
are included as part of the composite `.net-app` artifact that is deployed to the application server.

![Deployment diagram](images\deployment.png)

## User activities (Hassan)

## Sequence of functionality/calls trough _Chirp!_ (My)

# Process

## Build, test, release, and deployment (Nikki)

## Team work (Kris)

## How to make _Chirp!_ work locally (Louis)

### Setup
#### Dependencies

The application requires you to have installed [.NET 9.0](https://dotnet.microsoft.com/en-us/download)
or later.
Any other missing dependencies should be automatically installed when building/running the programme.

#### User secrets
For the programme to work at all, a "user secret" related to GitHub authentication must be set.\
Two values, a _client ID_ and a _client secret_, need to be obtained directly from
[GitHub](https://www.github.com) through their interface for [registrering a new OAuth app](
https://docs.github.com/en/apps/oauth-apps/building-oauth-apps/authenticating-to-the-rest-api-with-an-oauth-app).\
When prompted, you can set the "Homepage URL" to `http://localhost:5273/`,
and the "Authorization call-back URL" to `http://localhost:5273/signin-github`.

Execute the following console commands (omitting the optional portion in brackets if
already standing in the `src/Chirp.Web` directory). Replace `<client ID>` and `<client secret>` with
the respective values.
```
dotnet user-secrets set "authenticationGitHubClientId" <client ID> [--project src/Chirp.Web]
```
```
dotnet user-secrets set "authenticationGitHubClientSecret" <client secret> [--project src/Chirp.Web]
```

If anything goes wrong, the user secret settings can be reset by executing the following commands in order:
```
dotnet user-secrets clear [--project src/Chirp.Web]
```

```
dotnet user-secrets init [--project src/Chirp.Web]
```

### Run
To build and start the programme on an available port, execute the following command in a terminal
emulator while standing in the project root:

```
dotnet run --project src/Chirp.Web
```

or, if you have an implementation of Make installed,

```
make start
```

A message in the console will inform you of the specific URL to navigate to in your browser in
order to interact with the web application. Depending on your terminal emulator, you may be able to simply
click the link to do so directly.

## How to run test suite locally (Nikki)

# Ethics

## License (My)

## LLMs, ChatGPT, CoPilot, and others (Kris)