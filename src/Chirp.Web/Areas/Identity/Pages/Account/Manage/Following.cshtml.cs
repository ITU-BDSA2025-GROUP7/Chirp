using Chirp.Core;
using Chirp.Core.Domain_Model;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Areas.Identity.Pages.Account.Manage;

public class Following : PageModel {
    private readonly UserManager<Author> _userManager;
    private readonly IAuthorService _authorService;

    public List<FollowRelation> FollowRelations { get; set; }

    public Following(UserManager<Author> userManager, IAuthorService authorService) {
        _userManager = userManager;
        _authorService = authorService;
        FollowRelations = new List<FollowRelation>();
    }

    public async Task<IActionResult> OnGetAsync() {
        Author? user = await _userManager.GetUserAsync(User);
        if (user?.UserName != null) {
            FollowRelations = await _authorService.GetFollowRelations(user.UserName);
        } else {
            FollowRelations = [];
        }

        return Page();
    }

    public async Task<IActionResult> OnPostUnfollowAsync(string? authorA, string? authorB) {
        if (authorA != null && authorB != null) {
            await _authorService.Unfollow(authorB, authorA);

            // reloading the relations
            Author? user = await _userManager.GetUserAsync(User);
            if (user?.UserName != null) {
                FollowRelations = await _authorService.GetFollowRelations(user.UserName);
            } else {
                FollowRelations = [];
            }
        }

        return RedirectToPage();
    }
}