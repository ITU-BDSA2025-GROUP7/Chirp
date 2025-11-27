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
    public Following (UserManager<Author> userManager, IAuthorService authorService,  ILogger<Following> logger)
    {
        _userManager = userManager;
        _authorService = authorService;
        _logger = logger;
    }

    [TempData]
    public string StatusMessage { get; set; }
    public List<FollowRelation> FollowRelations { get; set; }



    public async Task<IActionResult> OnGetAsync() {
        Author user = (await _userManager.GetUserAsync(User));
        if (user == null) {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }
        FollowRelations = await _authorService.GetFollowRelations(user);
        foreach (var followRelation in FollowRelations) {
            _logger.LogInformation(followRelation.Follower + " follows " + followRelation.Followed + "");
        }

        return Page();
    }
    public async Task<IActionResult> OnPostUnfollowAsync(string? authorA, string? authorB) {
        _logger.LogInformation($"'{authorB}'Unfollowing user '{authorA}'");

        await _authorService.Unfollow(authorB!, authorA!);


        // reloading the relations
        Author user = (await _userManager.GetUserAsync(User) ?? throw new InvalidOperationException());
        FollowRelations = await _authorService.GetFollowRelations(user);

        return RedirectToPage();
    }

}