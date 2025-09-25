using Chirp.CSVDB;

namespace DBFacade;

public class DBFacade<T> : IDataBaseRepository<T> {
    private string _dbLocation = Environment.GetEnvironmentVariable("CHIRPDBPATH")!;
    
    public IEnumerable<T> Read(int? limit = null) {
        throw new NotImplementedException();
    }

    public void Store(T record) {
        throw new NotImplementedException();
    }
}