using Chirp.Core.Domain_Model;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Areas.Identity.Pages.Account.Manage;

public class MyCheepsModel : CheepTimelineModel {
    private readonly SignInManager<Author> _signInManager;

    public MyCheepsModel(UserManager<Author> userManager,
                         SignInManager<Author> signInManager,
                         ICheepService cheepService,
                         IAuthorService authorService)
        : base(cheepService, authorService, userManager) {
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnGet() {
        if (_signInManager.IsSignedIn(User)) {
            Author? user = _userManager.GetUserAsync(User).Result;
            if (user != null && user.UserName != null) {
                int pageNr = getPageNr(Request);
                Cheeps = await _cheepService.GetCheepsFromUserName(user.UserName, pageNr);
            }
        }

        return Page();
    }
}