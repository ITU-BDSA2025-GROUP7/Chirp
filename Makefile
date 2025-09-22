# Can download a Windows-port of GNU Make here: https://gnuwin32.sourceforge.net/packages/make.htm

# Sets the environment name to "Test", which causes some changes in the
#  client and service programmes.
ENV := -e ASPNETCORE_ENVIRONMENT=Test

# Intentionally left empty so that by default an empty string is inserted in
#  its place in the commands below.
#  Override it by typing e.g. `make read n=3` or `make cheep n="Hello, World!"`
n := 

# Read from local server. Limit the amount by adding n=<limit>
read-local:
	dotnet run --project src/Chirp.CLI.Client ${ENV} read ${n}

# Cheep to local server. Set message with n=<message>
cheep-local:
	dotnet run --project src/Chirp.CLI.Client ${ENV} cheep ${n}

# Read from online server. Limit the amount by adding n=<limit>
read:
	dotnet run --project src/Chirp.CLI.Client read ${n}

# Cheep to online server. Set message with n=<message>
cheep:
	dotnet run --project src/Chirp.CLI.Client cheep ${n}

# Starts the service application. Override listening port by adding n=<port>
start:
	dotnet run --project src/Chirp.CSVDBService ${n}

# Runs all the tests in the solution
test:
	dotnet test ${ENV}

# Runs client tests
tclient:
	dotnet test $(ENV) tests/Chirp.CLI.Tests

# Runs database tests
tdatabase:
	dotnet test $(ENV) tests/Chirp.CSVDB.Tests

# Runs service tests
tservice:
	dotnet test $(ENV) tests/Chirp.ServicesTest

build:
	dotnet build

clean:
	dotnet clean