using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.Packaging.Signing;

namespace Chirp.Razor.Pages;

public class PublicModel : CheepTimelineModel
{
    
    public PublicModel(ICheepService service) : base(service)
    {
    }

    public async Task<IActionResult> OnGet()
    {
        int pageNr = getPageNr(Request);
        Text = Text;

        Cheeps = await _service.GetCheeps(pageNr);
        return Page();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        _ = _service.CreateCheep((await _service.GetAuthorByUserName(User.Identity.Name)).First(), this.Text);
        return RedirectToPage("Public");
    }
    
}
