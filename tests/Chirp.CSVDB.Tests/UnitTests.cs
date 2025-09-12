using Chirp.CLI.Client;

namespace Chirp.CSVDB.Tests;

/// Helps find the working directory
public static class TestBase
{
	/// The current working directory.
    public static DirectoryInfo Dir { get; }

    static TestBase()
    {
        var current = Directory.GetCurrentDirectory();
        var dir = new DirectoryInfo(current);

		/* Going up three directories, from <working directory>\bin\Debug\net9.0\
		to <working directory>\ */
		if (dir.Parent?.Parent?.Parent != null)
		{
			dir = dir.Parent.Parent.Parent;
		}

        Dir = dir;
        Assert.NotNull(Dir);
    }
}

public class UnitTests
{
	/** Testing reading valid data when <c>CsvDataBase&lt;T&gt;.Read(int?)</c>
	 * is not given a parameter, including data where there is a comma within a string. */
	[Theory]
	[InlineData(0, "ropf", "Hello, BDSA students!", 1690891760L)]
	[InlineData(1, "adho", "Welcome to the course!", 1690978778L)]
	[InlineData(7, "pines", "wow, so this is what it feels like to cheep!", 1757242423L)]
	public void Read(int cheepIndex, string author, string message, long timestamp)
	{
		string path = GetPathTo("chirp_cli_db.csv");

		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(path);
		Cheep result = database.Read().ToList()[cheepIndex];
		Assert.Equal(author, result.Author);
		Assert.Equal(message, result.Message);
		Assert.Equal(timestamp, result.Timestamp);
	}

	/** Testing what happens when reading fewer entries than exist in the database. */
	[Fact]
	public void ReadLimitLessThan()
	{
		string path = GetPathTo("chirp_cli_db.csv");

		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(path);
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
		string path = GetPathTo("chirp_cli_db.csv");

		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(path);
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
		string path = GetPathTo("chirp_cli_db.csv");

		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(path);
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
		string path = GetPathTo("nullRecordInMiddle.csv");
		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(path);

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
		string path = GetPathTo("nullRecordInMiddle.csv");
		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(path);
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
		string path = GetPathTo("empty.csv");
		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(path);

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
		string path = GetPathTo("oneNullRecord.csv");
		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(path);
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
		string path = GetPathTo("nullRecordAtEnd.csv");
		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(path);
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
		string path = GetPathTo("nullRecordAtStart.csv");
		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(path);
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

	/** Tests that valid (and essentially valid) messages are written and read
	 * as expected without throwing errors.
	 */
	[Theory]
	[InlineData("testauthor", "\"Test message, that's the way it is!!\"",
		"Test message, that's the way it is!!", 1757601000L)]
	[InlineData("testauthor", "Test message that's the way it is!!",
		"Test message that's the way it is!!", 1757601000L)]
	[InlineData("", "\"Test message, that's the way it is!!\"",
		"Test message, that's the way it is!!", 1757601000L)]
	[InlineData("testauthor", "", "", 1757601000L)]
	[InlineData("testauthor", "\"\"", "", 1757601000L)]
	[InlineData("testauthor", "\"\n\"", "\n", 1757601000L)]
	[InlineData("testauthor", "\"Test message, that's the way it is!!\"",
		"Test message, that's the way it is!!", -1757601000L)]
	[InlineData("testauthor", "Test message that's the way it is!!",
		"Test message that's the way it is!!", -1757601000L)]
	[InlineData("", "\"Test message, that's the way it is!!\"",
		"Test message, that's the way it is!!", -1757601000L)]
	[InlineData("testauthor", "", "", -1757601000L)]
	[InlineData("testauthor", "\"\"", "", -1757601000L)]
	[InlineData("testauthor", "\"\n\"", "\n", -1757601000L)]
	[InlineData("testauthor", "\"Test message, that's the way it is!!\"",
		"Test message, that's the way it is!!", long.MaxValue)]
	[InlineData("testauthor", "\"Test message, that's the way it is!!\"",
		"Test message, that's the way it is!!", long.MinValue)]
	public void Write(string author, string writtenMessage, string readMessage, long timestamp)
	{
		string tempPath = CopyToTempFile("chirp_cli_db.csv");

		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(tempPath);
		Cheep cheep = new Cheep(author, writtenMessage, timestamp);
		var countBefore = database.Read().ToList().Count;

		database.Store(cheep);
		var results = database.Read().ToList();
		Assert.Equal(results.Count, countBefore + 1);

		Cheep result = results[countBefore];
		Assert.Equal(author, result.Author);
		Assert.Equal(readMessage, result.Message);
		Assert.Equal(timestamp, result.Timestamp);
		if (File.Exists(tempPath))
		{
			File.Delete(tempPath);
		}
	}

	/** Tests that missing " before and after message cause a type conversion exception if the
	 * message includes a comma, as the text after the comma will be understood as a long. */
	[Theory]
	[InlineData("testauthor", "Test message, that's the way it is!!", 1757601000L)]
	[InlineData("testauthor", "Test message, that's the way it is!!", -1757601000L)]
	[InlineData("testauthor", ",", 1757601000L)]
	[InlineData("testauthor", ",", -1757601000L)]
	public void WriteTypeConverterException(string author, string writtenMessage, long timestamp)
	{
		string tempPath = CopyToTempFile("empty.csv");

		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(tempPath);
		Cheep cheep = new Cheep(author, writtenMessage, timestamp);

		database.Store(cheep);
		Assert.Empty(database.Read().ToList());
	}

	/** Tests that a message which includes only a new-line character and no bordering " causes a MissingFieldException. */
	[Theory]
	[InlineData("testauthor", "\n", 1757601000L)]
	[InlineData("testauthor", "\n", -1757601000L)]
	public void WriteMissingFieldException(string author, string writtenMessage, long timestamp)
	{
		string tempPath = CopyToTempFile("empty.csv");

		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(tempPath);
		Cheep cheep = new Cheep(author, writtenMessage, timestamp);

		database.Store(cheep);
		Assert.Empty(database.Read().ToList());
	}

	/** Tests that a Cheep whose message includes an odd number of extra "
	 * causes the CSV parsing library to throw a BadDataException. */
	[Theory]
	[InlineData("testauthor", "\"Test message, that's the way it is!!", 1757601000L)]
	[InlineData("testauthor", "\"Test message, that's the way it is!!", -1757601000L)]
	[InlineData("testauthor", "Test message, that's the way it is!!\"", 1757601000L)]
	[InlineData("testauthor", "Test message, that's the way it is!!\"", -1757601000L)]
	[InlineData("testauthor", "Test message, that's\" the way it is!!", 1757601000L)]
	[InlineData("testauthor", "Test message, that's\" the way it is!!", -1757601000L)]
	[InlineData("testauthor", "\"Test message that's the way it is!!", 1757601000L)]
	[InlineData("testauthor", "\"Test message that's the way it is!!", -1757601000L)]
	[InlineData("testauthor", "Test message that's the way it is!!\"", 1757601000L)]
	[InlineData("testauthor", "Test message that's the way it is!!\"", -1757601000L)]
	[InlineData("testauthor", "Test message that's\" the way it is!!", 1757601000L)]
	[InlineData("testauthor", "Test message that's\" the way it is!!", -1757601000L)]
	[InlineData("testauthor", "Test message\", that's the\" way it is!!", 1757601000L)]
	[InlineData("testauthor", "Test message\", that's the\" way it is!!", -1757601000L)]
	[InlineData("testauthor", "\"\"\"", 1757601000L)]
	[InlineData("testauthor", "\"\"\"", -1757601000L)]
	[InlineData("testauthor", "\"", 1757601000L)]
	[InlineData("testauthor", "\"", -1757601000L)]
	public void WriteBadData(string author, string writtenMessage, long timestamp)
	{
		var database = GetDatabaseCopy("chirp_cli_db.csv");
		var recordsBefore = database.Read().ToList();

		Cheep cheep = new Cheep(author, writtenMessage, timestamp);
		database.Store(cheep);

		var records = database.Read().ToList();
		Assert.Equal(recordsBefore.Count, records.Count);

		for (var i = 0; i < recordsBefore.Count; i++)
		{
			Assert.Equal(recordsBefore[i], records[i]);
		}
	}

	/** Tests that the functionality to make a temporary copy of a file works as expected,
	 * asserting that their contents are identical and their paths unique. */
	[Fact]
	public void TestCopyToTempFile()
	{
		string path1 = GetPathTo("chirp_cli_db.csv");
		string path2 = CopyToTempFile("chirp_cli_db.csv");
		Assert.NotEqual(path1, path2);
		var f1 = File.OpenText(path1);
		var f2 = File.OpenText(path2);
		Assert.Equal(f1.ReadToEnd(), f2.ReadToEnd());
	}

	/** Copies the file located in the main directory of this test project into
	 * a new temporary file, asserts that it exists, then returns the path to that
	 * copy.<br/>
	 * If you don't need to write to the file, GetPathTo() might be preferable.<br/>
	 * The <b>filename</b> parameter refers to only the filename, not the full path.
	 */
	private static string CopyToTempFile(string filename)
	{
		var dir = TestBase.Dir;
		string dirString = dir.FullName;
		string path = Path.Combine(dirString, filename);
		Assert.True(File.Exists(path));

		string tempPath = Path.GetTempFileName();
		if (File.Exists(tempPath))
		{
			File.Delete(tempPath);
		}
		File.Copy(path, tempPath);
		Assert.True(File.Exists(tempPath));
		return tempPath;
	}

	/** Returns the path to the given filename located in the main directory of this
	 * test project. Asserts that the file exists first.<br/>
	 * Use this function for files that will be read but not written to.<br/>
	 * The <b>filename</b> parameter refers to only the filename, not the full path.<br/>
	 * Use CopyToTempFile(string) to get a copy of the file instead.
	 */
	private static string GetPathTo(string filename)
	{
		var dir = TestBase.Dir;
		string dirString = dir.FullName;
		string path = Path.Combine(dirString, filename);
		Assert.True(File.Exists(path));
		return path;
	}

	/** Convenience function. Opens the database with the given filename. */
	private static CsvDataBase<Cheep> GetDatabase(string filename)
	{
		return new CsvDataBase<Cheep>(GetPathTo(filename));
	}

	/** Convenience function. Makes a copy of the given file, then opens the
	 * database in that copy.
	 */
	private static CsvDataBase<Cheep> GetDatabaseCopy(string filename)
	{
		return new CsvDataBase<Cheep>(CopyToTempFile(filename));
	}
}
