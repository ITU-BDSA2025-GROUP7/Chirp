using Chirp.CLI.Client;
using Xunit.Abstractions;
using static Chirp.CSVDB.Tests.TestBase;

namespace Chirp.CSVDB.Tests;

/** <see cref="CsvDataBase"/> does not actually sanity check its output, and the CvsHelper
 * library doesn't seem to do so, either.<br/>
 * These tests therefore actually
 * look at what happens when the CsvDataBase reads back a record that it
 * previously stored. The intention is for the programme to never crash, instead
 * recovering from encountering unreadable entries by returning everything
 * found up until that point.
 */
public class WriteTests
{

	/** Asserts that valid (and essentially valid) entries are written and read
	 * as expected without throwing errors. This includes empty strings, messages
	 * that are not properly surrounded by quotation marks, and timestamps that
	 * are negative, as well as at the limit of what can be stored in a <see cref="long">long</see>.
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

	/** Missing " before and after the message causes a TypeConverterException if the
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

	/** A message which includes only a new-line character and no bordering "
	 * causes a MissingFieldException when read back.
	 * We test that the programme handles this smoothly without crashing.
	 */
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

	/** Tests that when given a filename that does not exist already, the
	 * database can store and then read from it without problem by writing the
	 * header first.
	 */
	[Fact]
	public void WriteNewFile()
	{
		var tempPath = Path.GetTempFileName();
		File.Delete(tempPath);
		Assert.False(File.Exists(tempPath));
		var database = new CsvDataBase<Cheep>(tempPath);
		var cheep = new Cheep("test author", "Test message!", 1757601000L);
		database.Store(cheep);
		var records = database.Read().ToList();
		Assert.Single(records);
		Assert.Equal(cheep, records[0]);
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