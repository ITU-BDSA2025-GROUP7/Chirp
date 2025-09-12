using Chirp.CLI.Client;

namespace Chirp.CSVDB.Tests;

// Helps finding the working directory
public class TestBase
{
	/* The current working directory. */
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
	[Theory]
	[InlineData(0, "ropf", "Hello, BDSA students!", 1690891760L)]
	[InlineData(1, "adho", "Welcome to the course!", 1690978778L)]
	[InlineData(7, "pines", "wow, so this is what it feels like to cheep!", 1757242423L)]
	public void Read(int cheepIndex, string author, string message, long timestamp)
	{
		var dir = TestBase.Dir;
		string filename = "chirp_cli_db.csv";
		string dirString = dir.FullName;
		string path = Path.Combine(dirString, filename);
		Assert.True(File.Exists(path));

		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(path);
		Cheep result = database.Read().ToList()[cheepIndex];
		Assert.Equal(author, result.Author);
		Assert.Equal(message, result.Message);
		Assert.Equal(timestamp, result.Timestamp);
	}

	[Fact]
	public void ReadLimitLessThan()
	{
		var dir = TestBase.Dir;
		string filename = "chirp_cli_db.csv";
		string dirString = dir.FullName;
		string path = Path.Combine(dirString, filename);
		Assert.True(File.Exists(path));

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

	[Fact]
	public void ReadLimitEqualTo()
	{
		var dir = TestBase.Dir;
		string filename = "chirp_cli_db.csv";
		string dirString = dir.FullName;
		string path = Path.Combine(dirString, filename);
		Assert.True(File.Exists(path));

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

	[Fact]
	public void ReadLimitHigher()
	{
		var dir = TestBase.Dir;
		string filename = "chirp_cli_db.csv";
		string dirString = dir.FullName;
		string path = Path.Combine(dirString, filename);
		Assert.True(File.Exists(path));

		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(path);
		List<Cheep> results = database.Read(10).ToList();
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

	[Fact]
	public void ReadFromEmptyCSVFile()
	{
		var dir = TestBase.Dir;
		string filename = "empty.csv";
		string dirString = dir.FullName;
		string path = Path.Combine(dirString, filename);
		Assert.True(File.Exists(path));

		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(path);
		var results = database.Read().ToList();
		Assert.NotNull(results);
		Assert.Empty(results);
	}

	[Fact]
	public void ReadFromCSVFileWithOneNullEntry()
	{
		var dir = TestBase.Dir;
		string filename = "oneNullRecord.csv";
		string dirString = dir.FullName;
		string path = Path.Combine(dirString, filename);
		Assert.True(File.Exists(path));

		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(path);
		Assert.Throws<CsvHelper.TypeConversion.TypeConverterException>(() => database.Read());
	}

	[Fact]
	public void ReadFromCSVFileWithANullEntryAmongNormalOnes()
	{
		var dir = TestBase.Dir;
		string filename = "nullRecordInMiddle.csv";
		string dirString = dir.FullName;
		string path = Path.Combine(dirString, filename);
		Assert.True(File.Exists(path));

		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(path);
		Assert.Throws<CsvHelper.TypeConversion.TypeConverterException>(() => database.Read());
	}

	/** Tests that messages are written and read as expected without throwing errors. */
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
	public void Write(string author, string writtenMessage, string readMessage, long timestamp)
	{
		var dir = TestBase.Dir;
		string filename = "chirp_cli_db.csv";
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
	 message includes a comma, as the text after the comma will be understood as a long. */
	[Theory]
	[InlineData("testauthor", "Test message, that's the way it is!!", 1757601000L)]
	[InlineData("testauthor", "Test message, that's the way it is!!", -1757601000L)]
	[InlineData("testauthor", ",", 1757601000L)]
	[InlineData("testauthor", ",", -1757601000L)]
	public void WriteTypeConverterException(string author, string writtenMessage, long timestamp)
	{
		var dir = TestBase.Dir;
		string filename = "chirp_cli_db.csv";
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

		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(tempPath);
		Cheep cheep = new Cheep(author, writtenMessage, timestamp);
		var countBefore = database.Read().ToList().Count;

		database.Store(cheep);
		Assert.Throws<CsvHelper.TypeConversion.TypeConverterException>(() => database.Read());
	}

	/** Tests that a message which includes a new-line character and no bordering " causes a MissingFieldException. */
	[Theory]
	[InlineData("testauthor", "\n", 1757601000L)]
	[InlineData("testauthor", "\n", -1757601000L)]
	public void WriteMissingFieldException(string author, string writtenMessage, long timestamp)
	{
		var dir = TestBase.Dir;
		string filename = "chirp_cli_db.csv";
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

		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(tempPath);
		Cheep cheep = new Cheep(author, writtenMessage, timestamp);
		var countBefore = database.Read().ToList().Count;

		database.Store(cheep);
		Assert.Throws<CsvHelper.MissingFieldException>(() => database.Read());
	}

	/* Tests that a Cheep whose message includes an odd number of extra "
	 causes the CSV parsing library to throw a BadDataException. */
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
		var dir = TestBase.Dir;
		string filename = "chirp_cli_db.csv";
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

		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(tempPath);
		Cheep cheep = new Cheep(author, writtenMessage, timestamp);
		var countBefore = database.Read().ToList().Count;

		database.Store(cheep);
		Assert.Throws<CsvHelper.BadDataException>(() => database.Read());
	}
}
