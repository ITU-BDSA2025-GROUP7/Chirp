# Can download a Windows-port of GNU Make here: https://gnuwin32.sourceforge.net/packages/make.htm
#  There are probably other places to get it, too. Might be available on Winget.
# If you're on GNU/Linux then you probably have it already.

# Usage:
#   `make <target> [n=<arg>]`
#  where <target> is the text before a colon in the list of commands below.
#  The optional `n=<arg>` will add an argument to the command, in the sense of
#   `dotnet run ... read <arg>` or `dotnet run ... cheep <arg>` 
#  So you'll have to do that if you want to cheep with this system.

# ==============================================================================
# Variables

# Sets the environment name to "Test", which causes some changes in the
#  client and service programmes.
ENV := -e ASPNETCORE_ENVIRONMENT=Test

# Intentionally left empty so that by default an empty string is inserted in
#  its place in the commands below.
#  Override it by typing e.g. `make read n=3` or `make cheep n="Hello, World!"`
n := 

# ==============================================================================

# starts the razer application on local host
start-razor:
	dotnet run --project src/Chirp.Web

# Runs all the tests in the solution
test:
	dotnet test ${ENV}

#
test-linux:
	ASPNETCORE_ENVIRONMENT=Test dotnet test

build:
	dotnet build

clean:
	dotnet clean
	
# make a new migration for the database. Remember to give the migration a name by also typing 'n=<name>' as shown in this example 'make newMigration n=MyMygration'
newMigration:
	dotnet ef migrations add ${n} --project src/Chirp.Infastructure  --startup-project src/Chirp.Web