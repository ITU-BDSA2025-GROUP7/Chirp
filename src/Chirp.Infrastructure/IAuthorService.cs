using Chirp.Core;
using Chirp.Core.Domain_Model;

namespace Chirp.Infrastructure;

public interface IAuthorService
{
    public Task<List<Author>> GetAuthorByUserName(string username);
}
