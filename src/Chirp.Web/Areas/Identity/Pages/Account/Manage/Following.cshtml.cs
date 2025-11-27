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
    private readonly ILogger<Following> _logger;

    public List<FollowRelation> FollowRelations{ get; set; }
    public Following (UserManager<Author> userManager, IAuthorService authorService,  ILogger<Following> logger)
    {
        _userManager = userManager;
        _authorService = authorService;
        _logger = logger;
        FollowRelations = new List<FollowRelation>();
    }





    public async Task<IActionResult> OnGetAsync() {
        Author user = ((await _userManager.GetUserAsync(User))!);
        FollowRelations = await _authorService.GetFollowRelations(user);

        return Page();
    }
    public async Task<IActionResult> OnPostUnfollowAsync(string? authorA, string? authorB) {

        await _authorService.Unfollow(authorB!, authorA!);

        // reloading the relations
        Author user = (await _userManager.GetUserAsync(User) ?? throw new InvalidOperationException());
        FollowRelations = await _authorService.GetFollowRelations(user);

        return RedirectToPage();
    }

}