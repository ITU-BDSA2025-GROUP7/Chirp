using static Chirp.Core.ICheepRepository;

namespace Chirp.Infrastructure;

public static class QueryExtensions {
    /** Pick out the contents of a particular page of this queryable. */
    public static IQueryable<T> Pick<T>(this IQueryable<T> self, int pageNr = 1) {
        return self.Skip((pageNr - 1) * CHEEPS_PER_PAGE)
                   .Take(CHEEPS_PER_PAGE);
    }
}