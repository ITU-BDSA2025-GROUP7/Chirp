namespace Chirp.CLI.Client
{
    public record Cheep(string Author, string Message, long Timestamp)
    {
        public override string ToString()
        {
            string strTime = StringTime();
            return Author + " @ " + strTime + ": " + Message ;
        }

        private string StringTime()
        {
            var time = new DateTime(1970, 1, 1);
            time = time.AddSeconds(Timestamp);
            var strTime = time.ToLocalTime().ToString("dd-MM-yyyy HH':'mm':'ss");
            
            return strTime;
        }

        public Cheep(string message) :
            this(Environment.UserName,
                "\"" + message + "\"",
                DateTimeOffset.UtcNow.ToUnixTimeSeconds()) {}
    }
}

