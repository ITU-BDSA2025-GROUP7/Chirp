using Chirp.CLI.Client;

namespace Chirp.CSVDB.Tests;

/// Helps find the working directory
public static class TestBase
{
	/// The current working directory.
	private static DirectoryInfo Dir { get; }

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

	/** Copies the file located in the main directory of this test project into
	 * a new temporary file, asserts that it exists, then returns the path to that
	 * copy.<br/>
	 * If you don't need to write to the file, GetPathTo() might be preferable.<br/>
	 * The <b>filename</b> parameter refers to only the filename, not the full path.
	 */
	public static string CopyToTempFile(string filename)
	{
		string dirString = Dir.FullName;
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
	public static string GetPathTo(string filename)
	{
		string dirString = Dir.FullName;
		string path = Path.Combine(dirString, filename);
		Assert.True(File.Exists(path));
		return path;
	}

	/** Convenience function. Opens the database with the given filename in the
	 * main directory of this test project. Use <see cref="GetDatabaseCopy"/> instead if
	 * you need to write to the database.
	 */
	public static CsvDataBase<Cheep> GetDatabase(string filename)
	{
		var path = GetPathTo(filename);
		return new CsvDataBase<Cheep>(path);
	}

	/** Convenience function. Makes a copy of the given file (located in the
	 * main directory of this test project), then opens the database in that copy. */
	public static CsvDataBase<Cheep> GetDatabaseCopy(string filename)
	{
		var pathOfCopy = CopyToTempFile(filename);
		return new CsvDataBase<Cheep>(pathOfCopy);
	}
}