namespace Chirp.CSVDB;

public interface IDataBaseRepository<T>
{ 
    /** Returns up to and including <c>limit</c> records from the database.<br/>
     * If <c>limit</c> is null, returns all records.<br/>
     * If a corrupted/unreadable record is encountered, everything
     * up until that point is returned, excluding the unreadable data. */
    public IEnumerable<T> Read(int? limit = null);
    
    /** Adds a new record to the database.<br/>
     * It is the caller's responsibility to sanity check the input before
     * passing it to this function. */
    public void Store(T record);
    
    public static IDataBaseRepository<T> Instance { get; }
}
