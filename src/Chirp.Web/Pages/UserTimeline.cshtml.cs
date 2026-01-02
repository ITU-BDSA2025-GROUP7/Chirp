using Chirp.Core;
using Chirp.Core.Domain_Model;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Pages;

public class UserTimelineModel : CheepTimelineModel {
    private const string NO_USER_HEADER = "User not found";
    private readonly SignInManager<Author> _signInManager;

    public AuthorDTO? Author { get; set; }
    public string Header { get; set; } = NO_USER_HEADER;

    public UserTimelineModel(ICheepService cheepService,
                             IAuthorService authorService,
                             UserManager<Author> userManager,
                             SignInManager<Author> signInManager, ILogger<UserTimelineModel> logger)
        : base(cheepService, authorService,logger, userManager) {
        _signInManager = signInManager;
    }

    /**
 * Gets the cheeps for the user timeline, meaning the ones by the user, and the people they follow.
 */
    public async Task<IActionResult> OnGet([FromRoute] string author) {
        Author? authorSource = await _userManager.FindByNameAsync(author);
        if (authorSource == null) {
            Header = NO_USER_HEADER;
            Cheeps = [];
            TotalPageCount = 1;
            PageNr = 1;
        } else {
            Author = new AuthorDTO(authorSource);
            Header = FormatPageHeader(Author);

            if (_signInManager.IsSignedIn(User) &&
                authorSource == await _userManager.GetUserAsync(User)) {
                await _authorService.Follow(Author.UserName, Author.UserName);
                TotalPageCount =
                    PageCount(await _cheepService.CheepCountFromFollowed(Author.UserName));
                PageNr = ParsePageNr(Request);
                Cheeps = await _cheepService.GetCheepsFromFollowed(Author.UserName, PageNr);
            } else {
                TotalPageCount =
                    PageCount(await _cheepService.CheepCountFromUserName(Author.UserName));
                PageNr = ParsePageNr(Request);
                Cheeps = await _cheepService.GetCheepsFromUserName(Author.UserName, PageNr);
            }

            GeneratePageLinks(author);
        }

        return Page();
    }
    /**
     * Formats the header with the username
     */
    private static string FormatPageHeader(AuthorDTO author) {
        if (author.DisplayName.EndsWith('s')) {
            return $"{author.DisplayName}' Timeline";
        }

        return $"{author.DisplayName}'s Timeline";
    }
}