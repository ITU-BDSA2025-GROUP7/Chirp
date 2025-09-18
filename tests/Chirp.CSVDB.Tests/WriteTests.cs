using Chirp.CLI.Client;
using Chirp.General;
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
public class WriteTests : IDisposable
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
		CsvDataBase<Cheep> database = GetDatabaseCopy("generalExample.csv");
		var cheep = new Cheep(author, writtenMessage, timestamp);
		
		int countBefore = database.Read(null).ToList().Count;
		database.Store(cheep);
		List<Cheep> results = database.Read(null).ToList();
		Assert.Equal(results.Count, countBefore + 1);

		Cheep result = results[countBefore];
		Assert.Equal(author, result.Author);
		Assert.Equal(readMessage, result.Message);
		Assert.Equal(timestamp, result.Timestamp);
	}

	/** Missing <c>"</c> before and after the message causes a TypeConverterException if the
	 * message includes a comma, as the text after the comma will be understood as a long.
	 * We test that the programme handles this smoothly without crashing. */
	[Theory]
	[InlineData("testauthor", "Test message, that's the way it is!!", 1757601000L)]
	[InlineData("testauthor", "Test message, that's the way it is!!", -1757601000L)]
	[InlineData("testauthor", ",", 1757601000L)]
	[InlineData("testauthor", ",", -1757601000L)]
	public void WriteTypeConverterException(string author, string writtenMessage, long timestamp)
	{
		CsvDataBase<Cheep> database = GetDatabaseCopy("empty.csv");
		var cheep = new Cheep(author, writtenMessage, timestamp);
		database.Store(cheep);
		Assert.Empty(database.Read(null).ToList());
		
		database = GetDatabaseCopy("generalExample.csv");
		cheep = new Cheep(author, writtenMessage, timestamp);
		database.Store(cheep);
		Assert.Equal(8, database.Read(null).ToList().Count);
	}

	/** A message which includes only a new-line character and no bordering <c>"</c>
	 * causes a MissingFieldException when read back.
	 * We test that the programme handles this smoothly without crashing. */
	[Theory]
	[InlineData("testauthor", "\n", 1757601000L)]
	[InlineData("testauthor", "\n", -1757601000L)]
	public void WriteMissingFieldException(string author, string writtenMessage, long timestamp)
	{
		CsvDataBase<Cheep> database = GetDatabaseCopy("empty.csv");
		var cheep = new Cheep(author, writtenMessage, timestamp);
		database.Store(cheep);
		Assert.Empty(database.Read(null).ToList());
		
		database = GetDatabaseCopy("generalExample.csv");
		cheep = new Cheep(author, writtenMessage, timestamp);
		database.Store(cheep);
		Assert.Equal(8, database.Read(null).ToList().Count);
	}

	/** Tests that a Cheep whose message includes an odd number of extra <c>"</c>,
	 * although it might cause the CSV parsing library to throw a BadDataException,
	 * will still be recovered from and the contents before then returned. */
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
		CsvDataBase<Cheep> database = GetDatabaseCopy("generalExample.csv");
		List<Cheep> recordsBefore = database.Read(null).ToList();

		var cheep = new Cheep(author, writtenMessage, timestamp);
		database.Store(cheep);

		List<Cheep> records = database.Read(null).ToList();
		Assert.Equal(recordsBefore.Count, records.Count);
		for (var i = 0; i < recordsBefore.Count; i++)
		{
			Assert.Equal(recordsBefore[i], records[i]);
		}
	}

	/** Tests that when given a filename that does not exist already, the
	 * database can store and then read from it without problem by writing the
	 * header first. */
	[Fact]
	public void WriteNewFile()
	{
		string tempPath = Path.GetTempFileName();
		File.Delete(tempPath);
		Assert.False(File.Exists(tempPath));
		
		var database = CsvDataBase<Cheep>.Instance;
		database.SetPath(tempPath);
		var cheep = new Cheep("test author", "Test message!", 1757601000L);
		database.Store(cheep);
		List<Cheep> records = database.Read(null).ToList();
		Assert.Single(records);
		Assert.Equal(cheep, records[0]);
	}

	/** Tests that the functionality to make a temporary copy of a file works as expected,
	* asserting that their contents are identical and their paths unique. */
	[Fact]
	public void TestCopyToTempFile() {
		const string filename = "generalExample.csv";
		string path1 = GetPathTo(filename);
		string path2 = CopyToTempFile(filename);
		Assert.NotEqual(path1, path2); // filenames are different
		
		StreamReader f1 = File.OpenText(path1);
		StreamReader f2 = File.OpenText(path2);
		Assert.Equal(f1.ReadToEnd(), f2.ReadToEnd()); // contents are the same
	}
	public void Dispose()
	{
		
		CsvDataBase<Cheep>.Reset();
	}
}