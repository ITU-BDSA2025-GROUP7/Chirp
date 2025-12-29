using System.ComponentModel.DataAnnotations;
using Chirp.Core;
using Chirp.Core.Domain_Model;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace Chirp.Web;

// abstract class that PublicModel and UserTimelineModel extends.
// this class should contain everything that these classes should share
public abstract class CheepTimelineModel : PageModel {
    protected readonly ICheepService _cheepService;
    protected readonly IAuthorService _authorService;
    private ILogger<CheepTimelineModel> _logger;
    protected readonly UserManager<Author> _userManager;

    public List<CheepDTO> Cheeps { get; set; } = new();

    /// Text for link to go to the first page.
    public const string FirstPageSymbol = " |< ";

    /// Text for link to go to the previous page.
    public const string PrevPageSymbol = "  < ";

    /// Text for link to go to the next page.
    public const string NextPageSymbol = " >  ";

    /// Text for link to go to the last page.
    public const string LastPageSymbol = " >| ";

    /// Backing field for <see cref="PageNr"/>.
    private int _pageNr = 1;

    /** The current page number. Determines the 32-cheep chunk of the database to list,
     * and is shown in the middle of the page-arrow symbols.<br/>
     * When setting this value, it is clamped to a range of
     * [1, <see cref="TotalPageCount"/>].<br/>
     * As such, <see cref="TotalPageCount"/> needs to be set before this.<br/>
     * The current page number is parsed from the URL with <see cref="ParsePageNr"/>. */
    public int PageNr {
        get => _pageNr;
        set => _pageNr = ClampPageNr(value);
    }

    /// Link to the first page of cheeps.
    /// If null, the link is disabled.
    public string? FirstPageLink { get; set; }

    /// Link to the previous page of cheeps.
    /// If null, the link is disabled.
    public string? PrevPageLink { get; set; }

    /// Link to the next page of cheeps.
    /// If null, the link is disabled.
    public string? NextPageLink { get; set; }

    /// Link to the last page of cheeps.
    /// If null, the link is disabled.
    public string? LastPageLink { get; set; }

    private int _totalPageCount = 1;

    /** The number of pages needed to contain the current timeline's cheeps.
     This represents the (inclusive) upper bound of <see cref="PageNr"/>.<br/>
     Automatically clamped to be at least 1.
     <seealso cref="PageCount(int)"/> */
    public int TotalPageCount {
        get => _totalPageCount;
        set => _totalPageCount = Math.Max(value, 1);
    }

    [BindProperty]
    [Required]
    [StringLength(Cheep.MAX_TEXT_LENGTH, ErrorMessage =
                      "The {0} must be at least {2} and at most {1} characters long.",
                  MinimumLength = 1)]
    [Display(Name = "Message")]
    public string Text { get; set; } = "";

    public CheepTimelineModel(ICheepService cheepService, IAuthorService authorService,
                              ILogger<CheepTimelineModel> logger, UserManager<Author> userManager) {
        _cheepService = cheepService;
        _authorService = authorService;
        _logger = logger;
        _userManager = userManager;
    }

    /** Parses the page nr from the current URL.<br/>
     * The result is used to set the value of <see cref="PageNr"/>, which automatically
     * clamps the value to [1,<see cref="TotalPageCount"/>].<br/>
     * Because <see cref="TotalPageCount"/> is computed at run-time,
     * this method should be called <em>after</em> computing <see cref="TotalPageCount"/>.
     * <seealso cref="PageCount(int)"/>
     * This is a static function to make it clearer what is happening in the OnGet() functions
     * which call this.
     */
    protected static int ParsePageNr(HttpRequest request) {
        StringValues pageQuery = request.Query["page"];
        int pageNr;
        int.TryParse(pageQuery, out pageNr);
        if (pageNr == 0)
            pageNr = 1; // if parsing failed, set page number to 1 as requested by session_05 1.b
        return pageNr;
    }

    public async Task<IActionResult> OnPostAsync() {
        if (ModelState.IsValid && User.Identity?.Name != null) {
            Author? author = await _userManager.FindByNameAsync(User.Identity.Name);
            if (author != null) {
                await _cheepService.CreateCheep(author, Text);
            }
        }

        return RedirectToPage();
    }

    public async Task<bool> IsFollowing(string authorA, string authorB) {
        return await _authorService.IsFollowing(authorA, authorB);
    }

    public async Task<IActionResult> OnPostFollowAsync(string? authorA, string? authorB) {
        await _authorService.Follow(authorA!, authorB!);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnfollowAsync(string? authorA, string? authorB) {
        await _authorService.Unfollow(authorA!, authorB!);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteCheepAsync(string? username, string? text,
                                                            string? timestamp) {
        _logger.LogCritical("OnDeleteCheep");
        _logger.LogCritical(username + " " + text + " " + timestamp);

        if (username == null || text == null || timestamp == null) return RedirectToPage();

        CheepDTO cheep = new CheepDTO("", text, timestamp, username);

        _logger.LogCritical(cheep.ToString());
        await _cheepService.DeleteCheep(cheep);

        return RedirectToPage();
    }

    /** Computes the number of pages needed to display <c>totalCheepCount</c> cheeps. */
    protected static int PageCount(int totalCheepCount) {
        int totalPageCount = totalCheepCount / ICheepRepository.CHEEPS_PER_PAGE;
        if (totalCheepCount % ICheepRepository.CHEEPS_PER_PAGE != 0 || totalPageCount == 0) {
            totalPageCount++;
        }

        return totalPageCount;
    }

    /** Generates a link for an arrow symbol.
     * <param name="root">The relative base URL (excluding the page query portion), e.g. an
     * empty string or "/Adrian".</param>
     * <param name="targetPageNr">The page to target. This will be clamped to a valid page.</param>
     * <returns>Returns null if the link should be disabled.
     * Otherwise, returns a relative URL.
     * </returns> */
    protected string? GeneratePageLink(string root = "", int targetPageNr = 1) {
        targetPageNr = ClampPageNr(targetPageNr);
        if (PageNr == targetPageNr) {
            return null;
        }

        return $"{root}?page={targetPageNr}";
    }

    /** Generates the links behind the arrow symbols. */
    protected void GeneratePageLinks(string root) {
        FirstPageLink = GeneratePageLink(root, 1);
        PrevPageLink = GeneratePageLink(root, PageNr - 1);
        NextPageLink = GeneratePageLink(root, PageNr + 1);
        LastPageLink = GeneratePageLink(root, TotalPageCount);
    }

    /** Clamps the given <c>pageNr</c> to be at least 1 and at most <see cref="TotalPageCount"/>. */
    protected int ClampPageNr(int pageNr) {
        return Math.Clamp(pageNr, 1, TotalPageCount);
    }
}