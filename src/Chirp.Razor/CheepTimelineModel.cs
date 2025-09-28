using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Chirp.General;
using Microsoft.Extensions.Primitives;

namespace Chirp.Razor;

// abstract class that PublicModel and UserTimelineModel extends. 
// this class should contain everything that these classes should share
public abstract class CheepTimelineModel : PageModel 
{
    protected readonly ICheepService _service;
    public List<CheepViewModel> Cheeps { get; set; } = new();

    public CheepTimelineModel(ICheepService service)
    {
        _service = service;
    }

    /***
     * returns the page nr of a given httpRequest
     * the pageNr is withing [1;infinity[
     * if a pageNr could not be found, return 1
     */ 
    protected int getPageNr(HttpRequest request)
    {
        StringValues pageQuery = Request.Query["page"];
        int pageNr;
        int.TryParse(pageQuery, out pageNr);
        if  (pageNr == 0) pageNr = 1; // if parsing failed, set page number to 1 as requested by session_05 1.b)
        return pageNr;
    }
}