using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Modular.Web.Support;

namespace Modular.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Administrator")]
public sealed class ModuleManagementController(IModuleService service) : Controller
{
    public IActionResult Index()
    {
        return View(service.List());
    }

    public IActionResult Create()
    {
        return View(new ModuleEditModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create(ModuleEditModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        service.Create(model.Id, model.Name, model.Description, model.RequiredLicense);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(string id)
    {
        var m = service.Find(id);
        return m is null ? NotFound() : View(new ModuleEditModel
        {
            Id = m.Id,
            Name = m.Name,
            Description = m.Description,
            RequiredLicense = m.RequiredLicense
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(string id, ModuleEditModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        return service.Update(id, model.Name, model.Description, model.RequiredLicense) ?
            RedirectToAction(nameof(Index)) :
            NotFound();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Toggle(string id)
    {
        var m = service.Find(id);
        return m is null
            ? NotFound()
            : (service.SetEnabled(id, !m.IsEnabled), RedirectToAction(nameof(Index))).Item2;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Delete(string id)
    {
        service.Delete(id);
        return RedirectToAction(nameof(Index));
    }
}

public sealed class ModuleEditModel
{
    [System.ComponentModel.DataAnnotations.Required]
    public string Id { get; set; } = "";

    [System.ComponentModel.DataAnnotations.Required]
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string RequiredLicense { get; set; } = "";
}
