using Chirp.CLI.Client;
using static Chirp.CSVDB.Tests.TestBase;
[assembly: CollectionBehavior(DisableTestParallelization = true)] // makes sure the tests are not parrallel to make sure the files are not used at the same time, (this became a problem after creating the singleton 
namespace Chirp.CSVDB.Tests;

/** Tests that validate the behaviour of <see cref="Chirp.CSVDB.CsvDataBase{T}.Read(int?)">
 * CsvDataBase.Read(int?)</see>. */
public class ReadTests : IDisposable
{
	/** Testing reading valid data when <c>CsvDataBase&lt;T&gt;.Read(int?)</c>
	 * is not given a parameter, including data where there is a comma within a string. */
	[Theory]
	[InlineData(0, "ropf", "Hello, BDSA students!", 1690891760L)]
	[InlineData(1, "adho", "Welcome to the course!", 1690978778L)]
	[InlineData(7, "pines", "wow, so this is what it feels like to cheep!", 1757242423L)]
	public void Read(int cheepIndex, string author, string message, long timestamp)
	{
		CsvDataBase<Cheep> database = GetDatabaseCopy("generalExample.csv");
		Cheep result = database.Read().ToList()[cheepIndex];
		Assert.Equal(author, result.Author);
		Assert.Equal(message, result.Message);
		Assert.Equal(timestamp, result.Timestamp);
	}

	/** Testing what happens when reading fewer entries than exist in the database. */
	[Fact]
	public void ReadLimitLessThan()
	{
		CsvDataBase<Cheep> database = GetDatabaseCopy("generalExample.csv");
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
		CsvDataBase<Cheep> database = GetDatabaseCopy("generalExample.csv");
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
		CsvDataBase<Cheep> database = GetDatabaseCopy("generalExample.csv");
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

	/** Testing what happens when reading a CSV file that contains normal
	 * entries and a corrupted one (,,), but the limit was such that we only
	 * actually retrieve normal ones anyway.
	 */
	[Fact]
	public void ReadWhenNullRecordLater()
	{
		CsvDataBase<Cheep> database = GetDatabaseCopy("nullRecordInMiddle.csv");

		List<Cheep> results = database.Read(1).ToList();
		Assert.Single(results);
		Assert.Equal("ropf", results[0].Author);
		Assert.Equal("Hello, BDSA students!", results[0].Message);
		Assert.Equal(1690891760L, results[0].Timestamp);
	}

	/** Testing what happens when reading a CSV file that has both normal and
	 * a corrupted entry, and all the entries are read. */
	[Theory]
	[InlineData(null)]
	[InlineData(9)]
	[InlineData(int.MaxValue)] // If a list of this length is created, C# will run out of memory and crash.
	public void ReadFromCSVFileWithANullEntryAmongNormalOnes(int? limit)
	{
		CsvDataBase<Cheep> database = GetDatabaseCopy("nullRecordInMiddle.csv");
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
	[InlineData(int.MaxValue)] // If a list of this length is created, C# will run out of memory and crash.
	[InlineData(int.MinValue)]
	public void ReadFromEmptyCSVFile(int? limit)
	{
		CsvDataBase<Cheep> database = GetDatabaseCopy("empty.csv");

		List<Cheep> results = database.Read(limit).ToList();
		Assert.NotNull(results);
		Assert.Empty(results);
	}

	/** Testing what happens when the CSV file only contains a single record, and it is
	 * unreadable (i.e. just two commas).
	 */
	[Fact]
	public void ReadFromCSVFileWithOneNullEntry()
	{
		CsvDataBase<Cheep> database = GetDatabaseCopy("oneNullRecord.csv");
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
		CsvDataBase<Cheep> database = GetDatabaseCopy("nullRecordAtEnd.csv");
		List<Cheep> records = database.Read(limit).ToList();
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
		CsvDataBase<Cheep> database = GetDatabaseCopy("nullRecordAtStart.csv");
		List<Cheep> records = database.Read(limit).ToList();
		Assert.Empty(records);
	}

	/** Test what happens when reading a file where there are two entries on one line. */
	[Fact]
	public void ReadWhenTwoRecordsOnOneLine() {
		CsvDataBase<Cheep> database = GetDatabaseCopy("twoRecordsOnOneLine.csv");
		List<Cheep> records = database.Read().ToList();
		Assert.Single(records);
		Cheep record = records[0];
		Assert.Equal("ropf", record.Author);
		Assert.Equal("Hello BDSA students!", record.Message);
		Assert.Equal(1690891760, record.Timestamp);
	}

	/** Asserts that changing the <c>limit</c> parameter of
	 * <see cref="Chirp.CSVDB.CsvDataBase{T}.Read(int?)">CsvDataBase.Read(int?)</see>
	 * causes the expected number of records to be returned. */
	[Theory]
	[InlineData("generalExample.csv", 8, null)]
	[InlineData("generalExample.csv", 0, 0)]
	[InlineData("generalExample.csv", 1, 1)]
	[InlineData("generalExample.csv", 2, 2)]
	[InlineData("generalExample.csv", 8, int.MaxValue)]
	[InlineData("generalExample.csv", 0, -1)]
	[InlineData("generalExample.csv", 0, -2)]
	[InlineData("generalExample.csv", 0, int.MinValue)]
	[InlineData("empty.csv", 0, null)]
	[InlineData("empty.csv", 0, 0)]
	[InlineData("empty.csv", 0, 1)]
	[InlineData("empty.csv", 0, 2)]
	[InlineData("empty.csv", 0, int.MaxValue)]
	[InlineData("empty.csv", 0, -1)]
	[InlineData("empty.csv", 0, -2)]
	[InlineData("empty.csv", 0, int.MinValue)]
	public void ReadLimit(string filename, int expectedRecords, int? limit)
	{
		CsvDataBase<Cheep> database = GetDatabaseCopy(filename);
		List<Cheep> records = database.Read(limit).ToList();
		Assert.Equal(expectedRecords, records.Count);
	}

	/** Asserts that reading again from the same database returns the same result(s) as before.
	 * You don't continue where you left off, you read from the start each time.
	 */
	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(8)]
	public void SubsequentReads(int limit)
	{
		CsvDataBase<Cheep> database = GetDatabaseCopy("generalExample.csv");
		List<Cheep> records1 = database.Read(limit).ToList();
		Assert.Equal(limit, records1.Count);
		var cheep1 = new Cheep("ropf", "Hello, BDSA students!", 1690891760L);
		Assert.Equal(cheep1, records1[0]);
		
		List<Cheep> records2 = database.Read(limit).ToList();
		Assert.Equal(limit, records2.Count);
		var cheep2 = new Cheep("ropf", "Hello, BDSA students!", 1690891760L);
		Assert.Equal(cheep2, records2[0]);
	}

	/** Asserts that <see cref="Chirp.CSVDB.CsvDataBase{T}.Read(int?)">CsvDataBase.Read(int?)</see> does not mutate
	 * the input file in any way. */
	[Theory]
	[InlineData("generalExample.csv")]
	[InlineData("empty.csv")]
	[InlineData("noHeader.csv")]
	[InlineData("nullRecordAtEnd.csv")]
	[InlineData("nullRecordAtStart.csv")]
	[InlineData("nullRecordInMiddle.csv")]
	[InlineData("oneNullRecord.csv")]
	[InlineData("twoRecordsOnOneLine.csv")]
	public void EnsureReadDoesNotMutate(string filename)
	{
		string path = CopyToTempFile(filename);
		StreamReader f1 = File.OpenText(path);
		string before = f1.ReadToEnd();
		f1.Close();
		
		var database = CsvDataBase<Cheep>.Instance;
		database.SetPath(path);
		_ = database.Read().ToList();
		
		StreamReader f2 = File.OpenText(path);
		string after = f2.ReadToEnd();
		Assert.Equal(before, after);
		f2.Close();
	}

	/** Asserts that attempting to read from a .csv file without a header row
	 * will result in nothing being read.
	 */
	[Theory]
	[InlineData("noHeader.csv")]
	[InlineData("noHeaderAlt.csv")]
	public void ReadWhenNoHeader(string filename)
	{
		CsvDataBase<Cheep> database = GetDatabaseCopy(filename);
		List<Cheep> records = database.Read().ToList();
		Assert.Empty(records);
	}
		public void Dispose()
	{
		
		CsvDataBase<Cheep>.Reset();
	}
}
