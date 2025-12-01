using Chirp.Core.Domain_Model;

using Chirp.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Pages;

public class PublicModel : CheepTimelineModel {
    public PublicModel(ICheepService cheepService, IAuthorService authorService,
                       UserManager<Author> userManager, ILogger<PublicModel> logger)
        : base(cheepService, authorService, logger, userManager) { }

    public async Task<IActionResult> OnGet() {
        int pageNr = getPageNr(Request);

        Cheeps = await _cheepService.GetCheeps(pageNr);
        return Page();
    }
}