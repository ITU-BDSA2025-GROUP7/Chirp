using Chirp.CLI.Client;
using System.IO;

namespace Chirp.CSVDB.Tests;

public class UnitTests
{
	[Fact]
    public void Read()
    {
		string current = Directory.GetCurrentDirectory(); // net9.0
		DirectoryInfo dir = new DirectoryInfo(current);
		// Must check that .Parent property is not null or you get warnings.
		Assert.NotNull(dir);
		Assert.NotNull(dir.Parent);
		Assert.NotNull(dir.Parent.Parent);
		Assert.NotNull(dir.Parent.Parent.Parent);
		dir = dir.Parent.Parent.Parent;
		string filename = "chirp_cli_db.csv";
		string dirString = dir.FullName;
		string path = Path.Combine(dirString, filename);
		Assert.True(File.Exists(path));

		CsvDataBase<Cheep> database = new CsvDataBase<Cheep>(path);
		Cheep results = database.Read().ToList()[0];
		Assert.Equal("ropf", results.Author);
    }
}
