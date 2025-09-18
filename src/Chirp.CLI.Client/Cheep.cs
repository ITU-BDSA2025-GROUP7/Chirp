namespace Chirp.CLI.Client
{
    public record Cheep(string Author, string Message, long Timestamp)
    {
        public override string ToString()
        {
            
            var strTime = StringTime(Timestamp);
            
            return Author + " @ " + strTime + ": " + Message ;
        }

        public string StringTime(long Timestamp)
        {
            DateTime time = new DateTime(1970, 1, 1);
            time = time.AddSeconds(Timestamp);
            var strTime = time.ToLocalTime().ToString("dd-MM-yyyy HH':'mm':'ss");
            
            return strTime;
        }
    }
}

