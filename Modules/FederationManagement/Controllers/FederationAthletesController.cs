using Microsoft.AspNetCore.Mvc;
using Modular.Modules.FederationManagement.Application;

namespace Modular.Modules.FederationManagement.Controllers;

[ApiController, Route("federation/athletes"), ServiceFilter(typeof(FederationLicenseGuardFilter))]
public sealed class FederationAthletesController(FederationManagementService s) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View("~/Views/Athletes/Index.cshtml", s.GetAthletes());
    }

    [HttpGet("{id:guid}")]
    public IActionResult Details(Guid id)
    {
        return s.GetAthlete(id) is { } a ? View("~/Views/Athletes/Details.cshtml", a) : NotFound();
    }

    [HttpGet("create")]

    public IActionResult Create() => View("~/Views/Athletes/Create.cshtml", s.GetClubs()); [HttpPost("create"), ValidateAntiForgeryToken]
    public IActionResult Create(RegisterAthleteRequest r)
    {
        try
        {
            return RedirectToAction(nameof(Details), new { id = s.RegisterAthlete(r).Id });
        }
        catch (Exception e) when (e is ArgumentException or InvalidOperationException or KeyNotFoundException)
        {
            ModelState.AddModelError("", e.Message); return View("~/Views/Athletes/Create.cshtml", s.GetClubs());
        }
    }

    [HttpGet("clubs")]
    public IActionResult Clubs()
    {
        return Ok(s.GetClubs());
    }

    [HttpPost("clubs"), ValidateAntiForgeryToken]
    public IActionResult CreateClub(CreateClubRequest r)
    {
        return Ok(s.CreateClub(r));
    }

    [HttpGet("rankings")]
    public IActionResult Rankings()
    {
        return Ok(s.GetRankings());
    }
}
