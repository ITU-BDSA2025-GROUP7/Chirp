using Chirp.Core.Domain_Model;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Pages;

public class PublicModel : CheepTimelineModel
{
    public PublicModel(ICheepService service) : base(service)
    {
    }

    public async Task<IActionResult> OnGet()
    {
        int pageNr = getPageNr(Request);

        Cheeps = await _service.GetCheeps(pageNr);
        return Page();
    }
    
    public async Task<bool> IsFollowing(Author authorA, Author authorB)
    {
        return await _service.IsFollowing(authorA, authorB);
    }

    public async Task<IActionResult> OnPostFollowAsync(Author authorA, Author authorB)
    {
        _service.Follow(authorA, authorB);
        Console.WriteLine("This funtction does something");

        return RedirectToPage();
    }
}
