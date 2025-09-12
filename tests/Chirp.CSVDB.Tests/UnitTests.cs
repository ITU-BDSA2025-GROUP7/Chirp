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
	public void Write()
	{
		var dir = TestBase.Dir;
		string filename = "chirp_cli_db.csv";
		string dirString = dir.FullName;
		string path = Path.Combine(dirString, filename);
		Assert.True(File.Exists(path));

		string tempPath = Path.GetTempFileName()

		if (File.Exists(tempPath))
		{
			File.Delete(tempPath);
		}
		File.Copy(path, tempPath);
		Assert.True(File.Exists(tempPath));

		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(tempPath);
		Cheep cheep = new Cheep("testauthor", "\"Test message, that's the way it is!!\"", 1757601000L);
		var countBefore = database.Read().ToList().Count;

		database.Store(cheep);
		var results = database.Read().ToList();
		Assert.Equal(results.Count, countBefore + 1);

		Cheep result = results[countBefore];
		Assert.Equal("testauthor", result.Author);
		Assert.Equal("Test message, that's the way it is!!", result.Message);
		Assert.Equal(1757601000L, result.Timestamp);
		if (File.Exists(tempPath))
		{
			File.Delete(tempPath);
		}
	}
}
