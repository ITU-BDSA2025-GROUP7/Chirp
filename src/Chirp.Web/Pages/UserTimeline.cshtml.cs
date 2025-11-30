using Chirp.Core.Domain_Model;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Pages;

public class UserTimelineModel : CheepTimelineModel {
    private const string NO_USER_HEADER = "User not found";
    private readonly UserManager<Author> _userManager;
    private readonly SignInManager<Author> _signInManager;

    public Author? Author { get; set; }
    public string Header { get; set; } = NO_USER_HEADER;

    public UserTimelineModel(ICheepService cheepService,
                             IAuthorService authorService,
                             UserManager<Author> userManager,
                             SignInManager<Author> signInManager)
        : base(cheepService, authorService) {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnGet([FromRoute] string author) {
        Author = await _userManager.FindByNameAsync(author);
        if (Author == null) {
            Header = NO_USER_HEADER;
            Cheeps = [];
            TotalPageCount = 1;
            PageNr = 1;
        } else {
            Header = FormatPageHeader(Author);

            if (_signInManager.IsSignedIn(User)
             && Author == await _userManager.GetUserAsync(User)) {
                await _authorService.Follow(Author, Author);
                TotalPageCount =
                    PageCount(await _cheepService.CheepCountFromFollowed(author));
                PageNr = ParsePageNr(Request);
                Cheeps = await _cheepService.GetCheepsFromFollowed(Author, PageNr);
            } else {
                TotalPageCount =
                    PageCount(await _cheepService.CheepCountFromUserName(author));
                PageNr = ParsePageNr(Request);
                Cheeps = await _cheepService.GetCheepsFromUserName(author, PageNr);
            }

            GeneratePageLinks(author);
        }

        return Page();
    }

    private static string FormatPageHeader(Author author) {
        if (author.DisplayName.EndsWith('s')) {
            return $"{author.DisplayName}' Timeline";
        }

        return $"{author.DisplayName}'s Timeline";
    }
}