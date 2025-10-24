using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

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
    
}
