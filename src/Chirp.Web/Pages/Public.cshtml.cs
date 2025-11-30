using Chirp.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Pages;

public class PublicModel : CheepTimelineModel {
    public PublicModel(ICheepService cheepService, IAuthorService authorService) : base(
        cheepService, authorService) { }

    public async Task<IActionResult> OnGet() {
        TotalPageCount = PageCount(_cheepService.TotalCheepCount);
        PageNr = ParsePageNr(Request);
        Cheeps = await _cheepService.GetCheeps(PageNr);
        GeneratePageLinks("");
        return Page();
    }
}