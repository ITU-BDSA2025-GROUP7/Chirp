namespace Chirp.Core;

public class CheepDTO : IComparable<CheepDTO> {
    public string AuthorDisplayName { get; set; }
    public string Message { get; set; }
    public string TimeStamp { get; set; }
    public string AuthorUserName { get; set; }

    public CheepDTO(string authorDisplayName, string message, string timeStamp,
                    string? authorUserName) {
        AuthorDisplayName = authorDisplayName;
        Message = message;
        TimeStamp = timeStamp;
        AuthorUserName = authorUserName ?? authorDisplayName;
    }

    public int CompareTo(CheepDTO? other) {
        if (other == null) return 1;
        return DateTime.Compare(DateTime.Parse(TimeStamp),
                                DateTime.Parse(other.TimeStamp));
    }
}