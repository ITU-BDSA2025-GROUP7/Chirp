using System.ComponentModel.DataAnnotations;
using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web;

// abstract class that PublicModel and UserTimelineModel extends.
// this class should contain everything that these classes should share
public abstract class CheepTimelineModel : PageModel
{
    protected readonly ICheepService _cheepService;
    protected readonly IAuthorService _authorService;
    public List<CheepDTO> Cheeps { get; set; } = new();

    [BindProperty]
    [StringLength(Core.Domain_Model.Cheep.MAX_TEXT_LENGTH, ErrorMessage =
            "The {0} must be at least {2} and at most {1} characters long.",
        MinimumLength = 1)]
    [Display(Name = "Message")]
    public string Text { get; set; } = "";

    public CheepTimelineModel(ICheepService _cheepService, IAuthorService _authorService)
    {
        this._cheepService = _cheepService;
        this._authorService = _authorService;
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
        _ = _cheepService.CreateCheep((await _authorService.GetAuthorByUserName(User.Identity!.Name!)).First(), Text);
        Response.Redirect(Request.GetDisplayUrl());
    }
}