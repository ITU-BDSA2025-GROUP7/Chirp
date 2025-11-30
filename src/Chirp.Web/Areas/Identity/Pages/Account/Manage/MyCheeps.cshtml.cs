using Chirp.Core.Domain_Model;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Areas.Identity.Pages.Account.Manage;

public class MyCheepsModel : CheepTimelineModel {
    private readonly UserManager<Author> _userManager;
    private readonly SignInManager<Author> _signInManager;

    public MyCheepsModel(UserManager<Author> userManager,
                         SignInManager<Author> signInManager,
                         ICheepService cheepService,
                         IAuthorService authorService)
        : base(cheepService, authorService) {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnGet() {
        Author? author = await _userManager.GetUserAsync(User);
        string? username = author?.UserName;
        if (_signInManager.IsSignedIn(User) && username != null) {
            TotalPageCount = PageCount(await _cheepService.CheepCountFromUserName(username));
            PageNr = ParsePageNr(Request);
            Cheeps = await _cheepService.GetCheepsFromUserName(username, PageNr);
            GeneratePageLinks("");
        }

        return Page();
    }
}