using System.Diagnostics;

namespace PlaywrightTests;

public static class EndToEndUtil
{
    /*
     * starts the application as a localhost
     * wait's until the localhost responds on the pinged url
     * returns the process
     * This method was made in collaboration with Chatgpt
     */
    public static async Task<Process> StartServer(string urlToPing)
    {
        int timeoutMs = 5000;
        Process serverProcess = new Process();
        serverProcess.StartInfo = new ProcessStartInfo()
        {
            FileName = "dotnet",
            Arguments = "run --project ../../../../../src/Chirp.Web/Chirp.Web.csproj",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        serverProcess.Start();
        
        // wait for the server to be started
        using var client = new HttpClient();
        var start = DateTime.Now;
        
        while (DateTime.Now - start < TimeSpan.FromMilliseconds(timeoutMs))
        {
            try
            {
                Console.Write("cheking for live");
                var response = await client.GetAsync("http://localhost:5273");
                if (response.IsSuccessStatusCode) return serverProcess;
            }
            catch (Exception)
            {
                // ignore unit it starts responding
            }
            
            await Task.Delay(500);
        }
        throw new Exception($"Server did not start within {timeoutMs / 1000} seconds.");
    }
}