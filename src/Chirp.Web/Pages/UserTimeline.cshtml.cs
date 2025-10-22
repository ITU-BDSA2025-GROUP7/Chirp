using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

public class UserTimelineModel : CheepTimelineModel
{
    public UserTimelineModel(ICheepService service) : base(service)
    {
    }
    
    public async Task<IActionResult> OnGet([FromRoute] string author)
    {
        int  pageNr = getPageNr(Request);
        
        Console.WriteLine("pageQuery: " + pageNr + " author: " + author);
        
        Cheeps = await _service.GetCheepsFromAuthor(author, pageNr);
        return Page();
    }
}