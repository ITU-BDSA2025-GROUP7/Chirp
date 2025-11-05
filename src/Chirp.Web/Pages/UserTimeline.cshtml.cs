using Chirp.Core.Domain_Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Razor.Pages;

public class UserTimelineModel : CheepTimelineModel {
    public Author? Author { get; set; }
    private UserManager<Author> userManager { get; set; }
    private IUserStore<Author> userStore { get; set; }

    public UserTimelineModel(ICheepService service,
                             UserManager<Author> userManager,
                             IUserStore<Author> userStore) : base(service) {
        this.userManager = userManager;
        this.userStore = userStore;
    }

    public async Task<IActionResult> OnGet([FromRoute] string author) {
        Author = await userManager.FindByNameAsync(author);
        if (Author != null) {
            int  pageNr = getPageNr(Request);

            Console.WriteLine("pageQuery: " + pageNr + " author: " + author);

            Cheeps = await _service.GetCheepsFromAuthor(author, pageNr);
        }
        return Page();
    }
}