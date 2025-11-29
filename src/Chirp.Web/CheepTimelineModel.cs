using System.ComponentModel.DataAnnotations;
using Chirp.Core;
using Chirp.Core.Domain_Model;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web;

// abstract class that PublicModel and UserTimelineModel extends.
// this class should contain everything that these classes should share
public abstract class CheepTimelineModel : PageModel {
    protected readonly ICheepService _cheepService;
    protected readonly IAuthorService _authorService;
    protected readonly UserManager<Author> _userManager;

    public List<CheepDTO> Cheeps { get; set; } = new();

    [BindProperty]
    [Required]
    [StringLength(Cheep.MAX_TEXT_LENGTH, ErrorMessage =
                      "The {0} must be at least {2} and at most {1} characters long.",
                  MinimumLength = 1)]
    [Display(Name = "Message")]
    public string Text { get; set; } = "";

    public CheepTimelineModel(ICheepService cheepService, IAuthorService authorService,
                              UserManager<Author> userManager) {
        this._cheepService = cheepService;
        this._authorService = authorService;
        this._userManager = userManager;
    }

    /**
     * returns the page nr of a given httpRequest
     * the pageNr is withing [1;infinity[
     * if a pageNr could not be found, return 1
     */
    protected int getPageNr(HttpRequest request) {
        StringValues pageQuery = Request.Query["page"];
        int pageNr;
        int.TryParse(pageQuery, out pageNr);
        if (pageNr == 0)
            pageNr = 1; // if parsing failed, set page number to 1 as requested by session_05 1.b
        return pageNr;
    }

    public async Task<IActionResult> OnPostAsync() {
        if (ModelState.IsValid && User.Identity?.Name != null) {
            Author? author = await _userManager.FindByNameAsync(User.Identity.Name);
            if (author != null) {
                await _cheepService.CreateCheep(author, Text);
            }
        }

        return RedirectToPage();
    }

    public async Task<bool> IsFollowing(string authorA, string authorB) {
        return await _authorService.IsFollowing(authorA, authorB);
    }

    public async Task<IActionResult> OnPostFollowAsync(string? authorA, string? authorB) {
        await _authorService.Follow(authorA!, authorB!);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnfollowAsync(string? authorA, string? authorB) {
        await _authorService.Unfollow(authorA!, authorB!);
        return RedirectToPage();
    }
}