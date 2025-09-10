namespace Chirp.CLI.Client
{
    public record Cheep(string Author, string Message, long Timestamp)
    {
        public override string ToString()
        {
            DateTime time = new DateTime(1970, 1, 1);
            time = time.AddSeconds(Timestamp);
            time = time.ToLocalTime();
            
            return Author + " @ " + time + ": " + Message ;
        }
        
    }
}

