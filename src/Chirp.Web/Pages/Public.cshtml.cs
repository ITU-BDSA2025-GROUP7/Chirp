using Chirp.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Pages;

public class PublicModel : CheepTimelineModel {
    public PublicModel(ICheepService cheepService, IAuthorService authorService, ILogger<CheepTimelineModel> logger) : base(
        cheepService, authorService,  logger ) { }

    public async Task<IActionResult> OnGet() {
        int pageNr = getPageNr(Request);

        Cheeps = await _cheepService.GetCheeps(pageNr);
        return Page();
    }
}