using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Areas.Identity.Pages.Account.Manage;

public class Following : PageModel {



    public IActionResult OnGet() {
        return Page();
    }
}