using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Chirp.Core;
using Microsoft.AspNetCore.Http.Extensions;

namespace Chirp.Razor;

// abstract class that PublicModel and UserTimelineModel extends. 
// this class should contain everything that these classes should share
public abstract class CheepTimelineModel : PageModel 
{
    protected readonly ICheepService _service;
    public List<CheepDTO> Cheeps { get; set; } = new();
    [BindProperty]
    public string Text { get; set; } = "";

    public CheepTimelineModel(ICheepService service)
    {
        _service = service;
    }

    /**
     * returns the page nr of a given httpRequest
     * the pageNr is withing [1;infinity[
     * if a pageNr could not be found, return 1
     */
    protected int getPageNr(HttpRequest request)
    {
        StringValues pageQuery = Request.Query["page"];
        int pageNr;
        int.TryParse(pageQuery, out pageNr);
        if (pageNr == 0) pageNr = 1; // if parsing failed, set page number to 1 as requested by session_05 1.b)
        return pageNr;
    }
    public async Task OnPostAsync()
    {
        _ = _service.CreateCheep((await _service.GetAuthorByUserName(User.Identity.Name)).First(), this.Text);
        Response.Redirect(Request.GetDisplayUrl());
    }
}