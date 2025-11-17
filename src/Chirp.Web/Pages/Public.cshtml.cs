using Chirp.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Pages;

public class PublicModel : CheepTimelineModel {
    public PublicModel(ICheepService service) : base(service) { }

    public async Task<IActionResult> OnGet() {
        int pageNr = getPageNr(Request);

        Cheeps = await _service.GetCheeps(pageNr);
        return Page();
    }
}