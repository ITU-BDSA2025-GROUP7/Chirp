using DocoptNet;
using Chirp.CSVDB;
using Chirp.General;

namespace Chirp.CLI.Client;

public class ProgramTest
{
	[Theory]
	[InlineData(true, null, false, "", 0)] // read, no message ( -- read
	[InlineData(false, null, true, "Hello World", 0)] // -- cheep "Hello World"
	[InlineData(false, null, false, "Hello World", 1)] // --  "Hello World"
	[InlineData(true, null, true, "", 1)] // -- cheep read
	[InlineData(false, null, false, null, 1)] // ** nothing ** 
	[InlineData(true, null, false, "Hello World", 0)] // -- read "Hello World"
	[InlineData(false, null, true, null, 1)] // -- cheep null
	[InlineData(true, "1", false, "Hello World", 0)] // -- read "Hello World"
	[InlineData(true, "notInt", false, "Hello World", 1)] // can't read "notInt" amount of cheeps
	[InlineData(true, "0", false, "Hello World", 1)] //Program will not bother reading 0 cheeps
	[InlineData(true, "-1", false, "Hello World", 1)] // no negative amount reading
    public void runTest(bool readFlag, string? amount, bool cheepFlag, string? message, int expected)
	{

		//arrange
		string tempFile = Path.GetTempFileName(); // creating a temporary file, to be able to run the program and not affect the actual database
		try
		{
			var args = new Dictionary<string, ArgValue>
			{
				["read"] = readFlag,
				["<amount>"] = amount != null ? amount : ArgValue.None,
				["cheep"] = cheepFlag,
				["<message>"] = message != null ? message : ArgValue.None,
				["post"] = false,
				["get"] = false
			};

			//act
			var fakeDb = CsvDataBase<Cheep>.Instance;
			fakeDb.SetPath(tempFile);
			int result = Program.Run(args, fakeDb);

			//assert
			//using the fact that run returns 0 if everything is good and 1 if something is wrong
			Assert.Equal(expected, result);
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Theory]
	[InlineData("--help", 0)]
	[InlineData("--h", 0)]
	[InlineData("--uifheriouhfos", 1)]
	[InlineData("", 1)]
	public void showHelpTest(string arg, int expected)
	{
    	string[] args = new[] { arg };

    	int result = Program.Main(args);

    	Assert.Equal(expected, result); // ShowHelp returns 0 if good, 1 if error
	}
}




