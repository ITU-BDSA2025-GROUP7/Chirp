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
                         IAuthorService authorService,
                         ILogger<CheepTimelineModel> logger)
        : base(cheepService, authorService, logger) {
        _userManager = userManager;
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