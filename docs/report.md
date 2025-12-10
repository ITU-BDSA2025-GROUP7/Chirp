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

# Design and Architecture of _Chirp!_

## Domain model

Here comes a description of our domain model.

## Architecture — In the small

## Architecture of deployed application

## User activities

## Sequence of functionality/calls trough _Chirp!_

# Process

## Build, test, release, and deployment

## Team work

## How to make _Chirp!_ work locally

## How to run test suite locally
To run the test suite, the program does *not* need to be running locally in the background. Then running the test suite can be done in two ways, depending on if make installed on you local computer.
If it is installed, running the command `make test` from the root directory will start the test suite.
Alternatively if make is not installed, the command `dotnet test` from the root directory will start the test suite.

The test suite comprises of (Insert number) tests. There are 3 types of tests in the suite:
1. Unit tests: Tests a singular function, class or fealt.
2. Integration tests: Tests the interplay between classes and functions.
3. End to end tests: Tests the end produced as interacted with by the user. This is done though playwright

Here is a breaf list of what is being tested

##### Author Repository
1. Creating authors
2. Following behaves as expected
3. Retrieving Authors
4. Deleting Authors
5.

#### Cheep Repository
1. Retrieving Cheeps
2. Retrieving pages of cheeps
3. Cheep timestamps are correct
4. Creating cheeps
5. Cheep content
6. SQL-injection safe


1. Retrieving cheeps from user that follows another, also retrieves the followed user

# Ethics

## License

## LLMs, ChatGPT, CoPilot, and others