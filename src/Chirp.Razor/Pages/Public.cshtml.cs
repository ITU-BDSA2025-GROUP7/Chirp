using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly ICheepService _service;
    public List<CheepViewModel> Cheeps { get; set; } = new();

    public PublicModel(ICheepService service)
    {
        _service = service;
    }

    public async Task<IActionResult> OnGet()
    {
        Cheeps = await _service.GetCheeps();
        return Page();
    }
}
