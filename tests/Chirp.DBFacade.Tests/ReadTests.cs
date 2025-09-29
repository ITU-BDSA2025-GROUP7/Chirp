using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using Chirp.CSVDB;
using Chirp.General;
using JetBrains.Annotations;
using Xunit;

namespace Chirp.DBFacade.Tests;

[TestSubject(typeof(DBFacade<>))]
public class ReadTests : IDisposable {
    private static IDataBaseRepository<Cheep> Implementation => 
        DBFacade<Cheep>.Instance;
    
    public ReadTests() {
        // The file has to be embedded in the project settings, see Chirp.DBFacade.Tests.csproj.
        Environment.SetEnvironmentVariable(DBEnv.envCHIRPDBPATH, Guid.NewGuid().ToString("N") + ".db");
        Environment.SetEnvironmentVariable(DBEnv.envSCHEMA, "data/schema.sql");
        Environment.SetEnvironmentVariable(DBEnv.envDATA, "data/dump.sql");
        DBFacade<Cheep>.Reset();
        Console.SetOut(new StringWriter());
    }

    /** Asserts that you can successfully read different amounts of records.
     * 657 is the normal amount in the database at time of writing.
     * Depending on parallelisation shenanigans, however, this may not be the case when this
     * test is executed. */
    [Theory]
    [InlineData(null, 657, 661)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(0, 0)]
    [InlineData(800, 657, 661)]
    public void Read(int? limit, int expectedCount, int? alternative = null) {
        IDataBaseRepository<Cheep> database = DBFacade<Cheep>.Instance;
        IEnumerable<Cheep> results = database.Read(limit);
        Assert.NotNull(results);
        Cheep[] arr = results as Cheep[] ?? results.ToArray();
        if (alternative != null) {
            Assert.True(
                expectedCount == arr.ToList().Count 
                || alternative == arr.ToList().Count);
        } else {
            Assert.Equal(expectedCount, arr.ToList().Count);
        }
    }

    /** Asserts that an ArgumentOutOfRangeException is not thrown when trying to read
     * a negative number of records. */
    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    public void ReadNegative(int limit) {
        IDataBaseRepository<Cheep> database = DBFacade<Cheep>.Instance;
        IEnumerable<Cheep> recordsBefore = database.Read(limit);
        Cheep[] before = recordsBefore as Cheep[] ?? recordsBefore.ToArray();
        Assert.Empty(before);
    }

    public void Dispose() {
        DBFacade<Cheep>.Reset();
    }
}