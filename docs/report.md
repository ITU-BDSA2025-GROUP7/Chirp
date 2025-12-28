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

## User activities (Hassan)

## Sequence of functionality/calls trough _Chirp!_ (My)

The Chirp application contains a bunch of different calls, between various parts of the application.
The diagram below shows the calls made when an unauthorized user visits the page.

![img](docs/images/rootSequenceDiagram.jpg)
*Diagram ??. Sequence diagram of the calls made when the unauthorized user goes to the root.*

Starting when an unauthorized user goes to the root endpoint of the application. In this case the public timeline.

The fact that the user goes to the public timeline, sends a GET request to the public timeline which is handled on the ``PublicModel``.
The method ``GetCheeps()``is called on the ``CheepService``,
which calls the one in the ``CheepRepository``, this one fetches the cheeps from the database.
In the diagram that is denoted by SELECT cheeps, which is not the complete select statement, since
we take pagination into account for fetching cheeps, but on the diagram we care about the intent not the complete statement.


The ``GetCheeps`` methods are all asynchronous and therefore they return ``Task<List<CheepDTO>`` instead of just a list of cheep data transfer objects.
That the UI can render.
It all ends with the http message 200, meaning ok, this is the standard response when a request is successful.




# Process

## Build, test, release, and deployment (Nikki)

## Team work (Kris)

## How to make _Chirp!_ work locally (Louise)

## How to run test suite locally (Nikki)

# Ethics

## License (My)

To choose our license, we first looked at the dependencies of our project. And registered that we
are only using Microsoft libraries.

We chose the MIT license because it is simple and to the point. It is the one .Net uses. and we don’t
have anything else that shouldn't work with it, since no obscure libraries are used in this application.

There is no particular reason or fear that someone will copy the project and distribute it commercially,
and we are not a business trying to earn money from it.
Therefore, we have no need to gatekeep our code and want to take part in the sharing of code.



## LLMs, ChatGPT, CoPilot, and others (Kris)