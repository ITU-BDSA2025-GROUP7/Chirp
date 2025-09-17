using Chirp.CLI.Client;
using Xunit.Sdk;

namespace Chirp.CSVDB.Tests;

/// Helps find the directory with the data files, and to copy these to temporary
/// files before using them.
public static class TestBase
{
	/// The "data" directory of this test project, where .csv files are stored for use in tests.
	/// The tests create temporary copies of these before doing anything else.
	private static DirectoryInfo Dir { get; }

	static TestBase()
	{
		string current = Directory.GetCurrentDirectory();
		var dir = new DirectoryInfo(current);

		/* Going up three directories, from <working directory>\bin\Debug\net9.0\
		to <working directory>\ */
		if (dir.Parent?.Parent?.Parent != null)
		{
			dir = dir.Parent.Parent.Parent;
		}
		Assert.True(dir.Exists);
		
		// Getting the "data" subdirectory.
		DirectoryInfo[] subDirs = dir.GetDirectories("data");
		Assert.Single(subDirs);
		DirectoryInfo dataDir = subDirs[0];
		Assert.NotNull(dataDir);
		Assert.True(dataDir.Exists);
		Dir = dataDir;
	}

	/** Copies the file located in the "data" directory of this test project into
	 * a new temporary file, asserts that it exists, then returns the path to that
	 * copy.<br/>
	 * <param name="filename">The filename, <i>not the full path</i>, of the .csv file to copy.</param>
	 */
	public static string CopyToTempFile(string filename) {
		string path = GetPathTo(filename);
		string tempPath = Path.GetTempFileName(); // This creates the file as well.
		File.Copy(path, tempPath, true);
		return tempPath;
	}

	/** Returns the path to the given filename located in the "data" directory of this
	 * test project. Asserts that the file exists first.<br/>
	 * Use this function for files that will be read but not written to.
	 * <param name="filename">The filename, <i>not the full path</i>, of the .csv file.</param>
	 * <seealso cref="CopyToTempFile(string)"/>
	 */
	public static string GetPathTo(string filename)
	{
		string path = Path.Combine(Dir.FullName, filename);
		Assert.True(File.Exists(path));
		return path;
	}

	/** Convenience function. Makes a copy of the given file (located in the
	 * "data" directory of this test project), then opens the database in that copy.<br/>
	 * Use this when opening the test .csv files to avoid accidentally overwriting the original.
	 * <param name="filename">The filename, <i>not the full path</i>,
	 * of the .csv file to copy and then open.</param> */
	public static CsvDataBase<Cheep> GetDatabaseCopy(string filename)
	{
		string pathOfCopy = CopyToTempFile(filename);
		return new CsvDataBase<Cheep>(pathOfCopy);
	}
}