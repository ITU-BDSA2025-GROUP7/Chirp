using Chirp.CLI.Client;
using static Chirp.CSVDB.Tests.TestBase;

namespace Chirp.CSVDB.Tests;

public class ReadTests
{
	/** Testing reading valid data when <c>CsvDataBase&lt;T&gt;.Read(int?)</c>
	 * is not given a parameter, including data where there is a comma within a string. */
	[Theory]
	[InlineData(0, "ropf", "Hello, BDSA students!", 1690891760L)]
	[InlineData(1, "adho", "Welcome to the course!", 1690978778L)]
	[InlineData(7, "pines", "wow, so this is what it feels like to cheep!", 1757242423L)]
	public void Read(int cheepIndex, string author, string message, long timestamp)
	{
		CsvDataBase<Cheep> database = GetDatabase("chirp_cli_db.csv");
		Cheep result = database.Read().ToList()[cheepIndex];
		Assert.Equal(author, result.Author);
		Assert.Equal(message, result.Message);
		Assert.Equal(timestamp, result.Timestamp);
	}

	/** Testing what happens when reading fewer entries than exist in the database. */
	[Fact]
	public void ReadLimitLessThan()
	{
		CsvDataBase<Cheep> database = GetDatabase("chirp_cli_db.csv");
		List<Cheep> results = database.Read(2).ToList();
		Assert.Equal(2, results.Count);
		Assert.Equal("ropf", results[0].Author);
		Assert.Equal("Hello, BDSA students!", results[0].Message);
		Assert.Equal(1690891760L, results[0].Timestamp);
		Assert.Equal("adho", results[1].Author);
		Assert.Equal("Welcome to the course!", results[1].Message);
		Assert.Equal(1690978778L, results[1].Timestamp);
	}

	/** Testing what happens when reading exactly the number of entries that exist in the database. */
	[Fact]
	public void ReadLimitEqualTo()
	{
		var database = GetDatabase("chirp_cli_db.csv");
		List<Cheep> results = database.Read(8).ToList();

		Assert.Equal(8, results.Count);
		Assert.Equal("ropf", results[0].Author);
		Assert.Equal("Hello, BDSA students!", results[0].Message);
		Assert.Equal(1690891760L, results[0].Timestamp);
		Assert.Equal("adho", results[1].Author);
		Assert.Equal("Welcome to the course!", results[1].Message);
		Assert.Equal(1690978778L, results[1].Timestamp);
		Assert.Equal("adho", results[2].Author);
		Assert.Equal("I hope you had a good summer.", results[2].Message);
		Assert.Equal(1690979858L, results[2].Timestamp);
		Assert.Equal("pines", results[7].Author);
		Assert.Equal("wow, so this is what it feels like to cheep!", results[7].Message);
		Assert.Equal(1757242423L, results[7].Timestamp);
	}

	/** Testing what happens when reading more entries than exist in the database.
	 * Asserts that we only retrieve all the ones possible without throwing an error.
	 */
	[Fact]
	public void ReadLimitHigher()
	{
		var database = GetDatabase("chirp_cli_db.csv");
		List<Cheep> results = database.Read(int.MaxValue).ToList();
		Assert.Equal(8, results.Count);
		Assert.Equal("ropf", results[0].Author);
		Assert.Equal("Hello, BDSA students!", results[0].Message);
		Assert.Equal(1690891760L, results[0].Timestamp);
		Assert.Equal("adho", results[1].Author);
		Assert.Equal("Welcome to the course!", results[1].Message);
		Assert.Equal(1690978778L, results[1].Timestamp);
		Assert.Equal("adho", results[2].Author);
		Assert.Equal("I hope you had a good summer.", results[2].Message);
		Assert.Equal(1690979858L, results[2].Timestamp);
		Assert.Equal("pines", results[7].Author);
		Assert.Equal("wow, so this is what it feels like to cheep!", results[7].Message);
		Assert.Equal(1757242423L, results[7].Timestamp);
	}

	/** Testing what happens when reading a CSV file that contains normal entries and a
	 * corrupted one (,,), but the limit was such that we only actually retrieve normal ones
	 * anyway.
	 */
	[Fact]
	public void ReadWhenNullRecordLater()
	{
		var database = GetDatabase("nullRecordInMiddle.csv");

		List<Cheep> results = database.Read(1).ToList();
		Assert.Single(results);
		Assert.Equal("ropf", results[0].Author);
		Assert.Equal("Hello, BDSA students!", results[0].Message);
		Assert.Equal(1690891760L, results[0].Timestamp);
	}

	/** Testing what happens when reading a CSV file that has both normal and a corrupted entry,
	 * and all the entries are read. */
	[Theory]
	[InlineData(null)]
	[InlineData(9)]
	public void ReadFromCSVFileWithANullEntryAmongNormalOnes(int? limit)
	{
		var database = GetDatabase("nullRecordInMiddle.csv");
		Assert.Equal(4, database.Read(limit).ToList().Count);
	}

	/** Testing what happens when reading all records from an empty CSV file. */
	[Theory]
	[InlineData(null)]
	[InlineData(1)]
	[InlineData(0)]
	[InlineData(2)]
	[InlineData(-1)]
	[InlineData(-2)]
	[InlineData(-1234)]
	public void ReadFromEmptyCSVFile(int? limit)
	{
		var database = GetDatabase("empty.csv");

		var results = database.Read(limit).ToList();
		Assert.NotNull(results);
		Assert.Empty(results);
	}

	/** Testing what happens when the CSV file only contains a single record, and it is
	 * unreadable (i.e. just two commas).
	 */
	[Fact]
	public void ReadFromCSVFileWithOneNullEntry()
	{
		var database = GetDatabase("oneNullRecord.csv");
		Assert.Empty(database.Read().ToList());
	}

	/** Testing the behaviour when there's a null entry at the very end of a file of
	 * otherwise normal entries. */
	[Theory]
	[InlineData(null)]
	[InlineData(8)]
	[InlineData(9)]
	public void ReadWhenNullRecordAtEnd(int? limit)
	{
		var database = GetDatabase("nullRecordAtEnd.csv");
		var records = database.Read(limit).ToList();
		Assert.Equal(8, records.Count);
	}

	/** Testing the behaviour when there's a null entry at the very beginning of a file of
	 * otherwise normal entries. Makes sure that Read() behaviours the same regardless
	 * of limit. */
	[Theory]
	[InlineData(null)]
	[InlineData(8)]
	[InlineData(9)]
	public void ReadWhenNullRecordAtStart(int? limit)
	{
		var database = GetDatabase("nullRecordAtStart.csv");
		var records = database.Read(limit).ToList();
		Assert.Empty(records);
	}

	/** Test what happens when reading a file where there are two entries on one line. */
	[Fact]
	public void ReadWhenTwoRecordsOnOneLine() {
		var database = GetDatabase("twoRecordsOnOneLine.csv");
		var records = database.Read().ToList();
		Assert.Single(records);
		var record = records[0];
		Assert.Equal("ropf", record.Author);
		Assert.Equal("Hello BDSA students!", record.Message);
		Assert.Equal(1690891760, record.Timestamp);
	}
}
