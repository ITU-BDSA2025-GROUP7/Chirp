using Chirp.Core.Domain_Model;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Pages;

public class PublicModel : CheepTimelineModel {
    public PublicModel(ICheepService cheepService, IAuthorService authorService,
                       UserManager<Author> userManager)
        : base(cheepService, authorService, userManager) { }

    public async Task<IActionResult> OnGet() {
        TotalPageCount = PageCount(_cheepService.TotalCheepCount);
        PageNr = ParsePageNr(Request);
        Cheeps = await _cheepService.GetCheeps(PageNr);
        GeneratePageLinks("");
        return Page();
    }
}