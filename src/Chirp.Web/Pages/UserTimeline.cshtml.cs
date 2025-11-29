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
                             SignInManager<Author> signInManager)
        : base(cheepService, authorService, userManager) {
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnGet([FromRoute] string author) {
        Author? authorSource = await _userManager.FindByNameAsync(author);
        if (authorSource != null) {
            Author = new AuthorDTO(authorSource);
            int pageNr = getPageNr(Request);
            Console.WriteLine("pageQuery: " + pageNr + " author: " + author);
            Header = FormatPageHeader(Author);

            if (_signInManager.IsSignedIn(User) &&
                authorSource == await _userManager.GetUserAsync(User)) {
                await _authorService.Follow(Author.UserName, Author.UserName);
                Cheeps = await _cheepService.GetOwnAndFollowedCheeps(Author.UserName, pageNr);
            } else {
                Cheeps = await _cheepService.GetCheepsFromUserName(author, pageNr);
            }
        } else {
            Header = NO_USER_HEADER;
            Cheeps = [];
        }

        return Page();
    }

    private static string FormatPageHeader(AuthorDTO author) {
        if (author.DisplayName.EndsWith('s')) {
            return $"{author.DisplayName}' Timeline";
        }

        return $"{author.DisplayName}'s Timeline";
    }
}