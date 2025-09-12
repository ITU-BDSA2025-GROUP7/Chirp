using Chirp.CLI.Client;
using static Chirp.CSVDB.Tests.TestBase;

namespace Chirp.CSVDB.Tests;

public class WriteTests
{
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
		var database = GetDatabaseCopy("chirp_cli_db.csv");

		Cheep cheep = new Cheep(author, writtenMessage, timestamp);
		var countBefore = database.Read().ToList().Count;

		database.Store(cheep);
		var results = database.Read().ToList();
		Assert.Equal(results.Count, countBefore + 1);

		Cheep result = results[countBefore];
		Assert.Equal(author, result.Author);
		Assert.Equal(readMessage, result.Message);
		Assert.Equal(timestamp, result.Timestamp);
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
		var database = GetDatabaseCopy("empty.csv");
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
		var database = GetDatabaseCopy("empty.csv");
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
}