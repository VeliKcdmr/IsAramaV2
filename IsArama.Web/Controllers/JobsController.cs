using IsArama.Web.Models;
using IsArama.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace IsArama.Web.Controllers;

public class JobsController : Controller
{
    private readonly JobService _jobService;

    public JobsController(JobService jobService)
    {
        _jobService = jobService;
    }

    public async Task<IActionResult> Index(
        string? keyword, int? cityId, int? workModelId, int? workTypeId, int? sectorId, int? sourceId, int page = 1, int pageSize = 10)
    {
        var response = await _jobService.GetJobsAsync(page, keyword, cityId, workModelId, workTypeId, pageSize, sectorId, sourceId);
        var lookup = await _jobService.GetLookupAsync();
        var vm = new JobListViewModel
        {
            Response = response,
            Lookup = lookup,
            Keyword = keyword,
            CityId = cityId,
            WorkModelId = workModelId,
            WorkTypeId = workTypeId,
            SectorId = sectorId,
            SourceId = sourceId,
            Page = page,
            PageSize = pageSize
        };
        return View(vm);
    }

    public async Task<IActionResult> Detail(string slug)
    {
        var job = await _jobService.GetJobDetailAsync(slug);
        if (job == null) return NotFound();
        return View(job);
    }

    public async Task<IActionResult> DetailModal(string slug)
    {
        var job = await _jobService.GetJobDetailAsync(slug);
        if (job == null) return NotFound();

        if (job.Detail == null)
        {
            await _jobService.ScrapeDetailAsync(job.Url);
            job = await _jobService.GetJobDetailAsync(slug);
        }

        return PartialView("_DetailModalPartial", job);
    }
}
