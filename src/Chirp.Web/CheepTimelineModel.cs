using System.ComponentModel.DataAnnotations;
using Chirp.Core;
using Chirp.Core.Domain_Model;
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
    protected readonly ICheepService _service;
    private ICheepRepository _repository;
    public List<CheepDTO> Cheeps { get; set; } = new();

    [BindProperty]
    [StringLength(Core.Domain_Model.Cheep.MAX_TEXT_LENGTH, ErrorMessage =
            "The {0} must be at least {2} and at most {1} characters long.",
        MinimumLength = 1)]
    [Display(Name = "Message")]
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
        _ = _service.CreateCheep((await _service.GetAuthorByUserName(User.Identity!.Name!)).First(), Text);
        Response.Redirect(Request.GetDisplayUrl());
    }
    
    public async Task<bool> IsFollowing(Author authorA, Author authorB)
    {
        return await _service.IsFollowing(authorA, authorB);
    }

    public async Task<IActionResult> OnPostFollowAsync(string? authorA, string? authorB)
    {
        await _service.Follow(authorA!, authorB!);
        Console.WriteLine("This funtction does something");

        return RedirectToPage();
    }
}