using System;
using System.Collections.Generic;
using System.Linq;
using Chirp.CSVDB;
using Chirp.General;
using JetBrains.Annotations;
using Xunit;

namespace Chirp.DBFacade.Tests;

[TestSubject(typeof(DBFacade<>))]
public class DBFacadeTest {
    [Fact]
    public void CanGetInstance() {
        IDataBaseRepository<Cheep> database = DBFacade<Cheep>.Instance;
        Assert.NotNull(database);
    }

    [Theory]
    [InlineData(null, 657)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(0, 0)]
    [InlineData(800, 657)]
    public void ReadAll(int? limit, int expectedCount) {
        IDataBaseRepository<Cheep> database = DBFacade<Cheep>.Instance;
        IEnumerable<Cheep> results = database.Read(limit);
        Assert.NotNull(results);
        Assert.Equal(expectedCount, results.ToList().Count);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-2)]
    public void ReadNegative(int limit) {
        Assert.Throws<ArgumentOutOfRangeException>(() => DBFacade<Cheep>.Instance.Read(limit));
    }
}