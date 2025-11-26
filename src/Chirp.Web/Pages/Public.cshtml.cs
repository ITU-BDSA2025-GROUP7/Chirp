using Chirp.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Pages;

public class PublicModel : CheepTimelineModel {
    public PublicModel(ICheepService cheepService, IAuthorService authorService) : base(
        cheepService, authorService) { }

    public async Task<IActionResult> OnGet() {
        int pageNr = getPageNr(Request);

        Cheeps = await _cheepService.GetCheeps(pageNr);
        return Page();
    }
}