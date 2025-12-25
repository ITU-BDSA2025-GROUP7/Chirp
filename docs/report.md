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

## Domain model

The database for the project is an SQLite database that's using Entity Framework Core as an object-relational mapper,
allowing the creation of a domain model containing classes that can be used as table withing the database while also allowing the use of these classes in the code.

The domain model of Chip consists of a couple of different classes. The two main classes are `Author` and `Cheep`.

`Author` represents a user in the program. It contains all relevant information about the user such as *UserName*, *DisplayName*, *Email*, *PasswordHash*, and a list of all cheeps that the author wrote.
Much of the functionality is inherited from `IdentityUser` as part of ASP.NET Identity.
This allows the use of functionality from ASP.NET identity such as support for registration, login page, as well as offloading some of the security concerns to Dotnet.
An author can also follow another author. This is done using the `FollowRelation` class. It contains the *Follower*, the *Followed* and a unique id representing the FollowRelation, allowing for a many-to-many relation between authors.
The reason for creating a separate class was to maximise the normal form of the database.
The course *Introduction to database systems* taught that it was best to achieve this by spitting the functionality of authors following each other into a separate table.
In hindsight, it would have been just as valid to add a field `public List<Author> Follows` to `Author` as EF Core separates the list into a separate table behind the screens.

`Cheep` represents a message from a user. It contains relevant information such as the author who wrote it, what time it was written, the text message itself and a unique Id.

Bellow is shown a UML diagram depicting the structure of the domain model. Note that only relevant fields of `IdentityUser` is shown.
![](.\images\DomainModel.png)

## Architecture — In the small (Kris)

## Architecture of deployed application (Louis)

## User activities (Hassan)

## Sequence of functionality/calls trough _Chirp!_ (My)

# Process

## Build, test, release, and deployment

The processes of Building, testing, releasing and deploying are relatively simple and always follows the same pattern making them simple to automate.
For this reason, there has been added a total of 3 GitBub actions to automate these processes. The actions are as follows: *BuildAndTest*, *Release*, and *Deploy*
Each GitHub has it's own triger that activate the action. These differ from each action.

### Build And Test
The purpose of the BuildAndTest action is, as the name implies, to insure the the project is always buildable and that each test passes.
The integration with GitHub makes this action highly useful as the action can be ran on a pull request,
and GitHub will point out any compiler waning as well as not allowing the pull request to be accepted if some of the tests does not pass.

The action is activated whenever a push or pull request is made.
After the action has sucessfuly build the project, then it will download playwright.
This is because the *ubuntu-latest* machine that the action is ran from doesn't already have playwright installed, and it's necessary to run some of the tests.
It then runes the tests.
If there was a problem with any of the steps involved, then the action wil *fail* and a potential pull request will be marked as not suitable for merging.
The action is illustrated in the UML activity diagram below.

![](.\images\BuildAndTest.png)

### Release
The purpose of the Release action is to publish the program and then make a GitHub release using the published program.

It's activated when a push is made with a tag that fits the regex expression `v[0-9]+.[0-9]+.[0-9]+`
It firsts runes what's equivalent to the *BuildAndTest* action, testing that all tests pass before preceding.
Once this is done, the action can publish the program. It is published 3 times, once for windows, mac, and linux.
The published program does not contain the database required to run the program. For this reason the database is also copied to each of the published programs.
After this is done the program can be ziped and released

The action is illustrated in the UML activity diagram below.

![](.\images\Publish.png)]

### Deploy
The purpose of the Deploy action is to deploy the program form GitHub onto Azure, making the program publicly assessable from the website.
The code for the action is based on the code provided by Azure when creating a new webapp.
It is activated whenever a push to main happens, insuring that the program live on azure is always up to date with the current state of main.

The action is illustrated in the UML activity diagram below.

![](.\images\Deploy.png)]

## Team work (Kris)

## How to make _Chirp!_ work locally (Louise)

## How to run test suite locally
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