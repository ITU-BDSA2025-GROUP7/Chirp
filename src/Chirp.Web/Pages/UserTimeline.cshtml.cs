using Chirp.Core.Domain_Model;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web.Pages;

public class UserTimelineModel : CheepTimelineModel
{
    private const string NO_USER_HEADER = "User not found";
    private readonly UserManager<Author> _userManager;
    public Author? Author { get; set; }
    public string Header { get; set; } = NO_USER_HEADER;

    public UserTimelineModel(ICheepService service, UserManager<Author> userManager)
        : base(service)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGet([FromRoute] string author)
    {
        Author = await _userManager.FindByNameAsync(author);
        if (Author == null) {
            Header = NO_USER_HEADER;
            Cheeps = [];
        } else {
            int pageNr = getPageNr(Request);
            Console.WriteLine("pageQuery: " + pageNr + " author: " + author);
            Header = FormatPageHeader(Author);
            Cheeps = await _service.GetCheepsFromUserName(author, pageNr);
        }
        return Page();
    }

    private static string FormatPageHeader(Author author)
    {
        if (author.DisplayName.EndsWith('s')) {
            return $"{author.DisplayName}' Timeline";
        }
        return $"{author.DisplayName}'s Timeline";
    }
}