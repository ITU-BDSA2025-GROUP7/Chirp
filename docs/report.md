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

# Process

## Build, test, release, and deployment (Nikki)

## Team work (Kris)

## How to make _Chirp!_ work locally (Louise)

## How to run test suite locally (Nikki)
To run the test suite, the program does *not* need to be running locally in the background. Then running the test suite can be done in two ways, depending on if make is installed on you local computer.
If it is installed, running the command `make test` from the root directory will start the test suite.
Alternatively if make is not installed, the command `dotnet test` from the root directory will start the test suite.

The test suite comprises of (Insert number) tests. There are 3 types of tests in the suite:
1. Unit tests: Tests a singular function, class, or field.
2. Integration tests: Tests the interplay between classes and functions.
3. End to end tests: Tests the end product as interacted with by the user. This is done though playwright

Here is a breath list of what is being tested

##### Author Repository
1. Creating authors
2. Following behaves as expected
3. Retrieving Authors
4. Deleting Authors
5. AuthorDTO works as expected
6. CheepDTO works as expected

##### Cheep Repository
1. Retrieving Cheeps
2. Retrieving pages of cheeps
3. Cheep timestamps are correct
4. Creating cheeps
5. Cheep content
6. SQL-injection safe
7. Retrieving cheeps from user that follows another, also retrieves the followed user

##### Playwright tests
1. User can log in
2. User can register
3. Navigation bara changes
4. Users can Log out
5. Users can Follow & unfollow
6. About me has following page
7. My page shows displayName
8. Cheeps are shown
9. Deleting users
10. Users can Sending cheeps
11. Sending cheeps ae safe from xss attacks
12. Users can Delete cheeps
13. About me page exists
14. Page arrows works
15. Users can searching for user

# Ethics

## License (My)

## LLMs, ChatGPT, CoPilot, and others (Kris)