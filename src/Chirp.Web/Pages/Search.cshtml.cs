using System.ComponentModel.DataAnnotations;
using Chirp.Core;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace Chirp.Web.Pages;

public class SearchPageModel : PageModel {
    private readonly IAuthorService _authorService;
    public List<AuthorDTO> Authors { get; set; } = [];

    [BindProperty]
    [MaxLength(256)]
    public string SearchQuery { get; set; } = "";

    public SearchPageModel(IAuthorService authorService) {
        _authorService = authorService;
    }

    public async Task<IActionResult> OnGet() {
        StringValues searchString = Request.Query["query"];
        if (!searchString.IsNullOrEmpty()) {
            Authors = await _authorService.Search(searchString!);
        } else {
            Authors = [];
        }

        return Page();
    }

    public IActionResult OnPost() {
        if (ModelState.IsValid) {
            return Redirect($"?query={SearchQuery}");
        }

        return Page();
    }
}