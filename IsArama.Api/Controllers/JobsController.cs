using IsArama.Data;
using IsArama.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IsArama.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly AppDbContext _db;

    public JobsController(AppDbContext db)
    {
        _db = db;
    }

    // POST api/jobs/listings
    [HttpPost("listings")]
    public async Task<IActionResult> SaveListings([FromBody] List<JobListingDto> dtos)
    {
        int added = 0;

        foreach (var dto in dtos)
        {
            if (string.IsNullOrWhiteSpace(dto.Slug) || string.IsNullOrWhiteSpace(dto.Title))
                continue;

            // Duplicate kontrolü
            if (await _db.JobListings.AnyAsync(j => j.Slug == dto.Slug))
                continue;

            // Şirket: bul veya oluştur
            var company = await _db.Companies.FirstOrDefaultAsync(c => c.Name == dto.Company);
            if (company == null)
            {
                company = new Company { Name = dto.Company, LogoUrl = dto.LogoUrl };
                _db.Companies.Add(company);
                await _db.SaveChangesAsync();
            }

            // İl eşleştir
            var cityName = ParseCity(dto.Location);
            var city = await _db.Cities.FirstOrDefaultAsync(c =>
                EF.Functions.Like(c.Name, cityName));

            if (city == null)
                continue; // Bilinmeyen il → kaydetme

            // İlçe eşleştir (varsa)
            var districtName = ParseDistrict(dto.Location);
            District? district = null;
            if (!string.IsNullOrEmpty(districtName))
            {
                district = await _db.Districts.FirstOrDefaultAsync(d =>
                    d.CityId == city.Id && EF.Functions.Like(d.Name, districtName));
            }

            // WorkModel eşleştir
            var workModel = await _db.WorkModels.FirstOrDefaultAsync(w =>
                EF.Functions.Like(w.Name, dto.WorkModel))
                ?? await _db.WorkModels.FirstAsync();

            // WorkType eşleştir
            var workType = await _db.WorkTypes.FirstOrDefaultAsync(w =>
                EF.Functions.Like(w.Name, dto.WorkType))
                ?? await _db.WorkTypes.FirstAsync();

            var listing = new JobListing
            {
                Url = dto.Url,
                Slug = dto.Slug,
                Title = dto.Title,
                PublishedAt = dto.PublishedAt,
                LogoUrl = dto.LogoUrl,
                CompanyId = company.Id,
                CityId = city.Id,
                DistrictId = district?.Id,
                WorkModelId = workModel.Id,
                WorkTypeId = workType.Id,
                SourceId = dto.SourceId ?? 1,
                ScrapedAt = DateTime.UtcNow
            };

            _db.JobListings.Add(listing);
            added++;
        }

        await _db.SaveChangesAsync();
        return Ok(new { added });
    }

    // POST api/jobs/detail
    [HttpPost("detail")]
    public async Task<IActionResult> SaveDetail([FromBody] JobDetailDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Url))
            return BadRequest("URL boş olamaz.");

        var slug = dto.Url.Split("/is-ilani/").LastOrDefault() ?? string.Empty;

        var listing = await _db.JobListings
            .Include(j => j.Detail)
            .FirstOrDefaultAsync(j => j.Slug == slug);

        if (listing == null)
            return NotFound("İlan bulunamadı.");

        if (listing.Detail != null)
            return Ok(new { message = "Detay zaten mevcut." });

        // Şirket bilgilerini güncelle
        var company = await _db.Companies.FindAsync(listing.CompanyId);
        if (company != null)
        {
            if (!string.IsNullOrEmpty(dto.CompanyLogo)) company.LogoUrl = dto.CompanyLogo;
            if (!string.IsNullOrEmpty(dto.CompanyFollowers)) company.Followers = dto.CompanyFollowers;
            if (!string.IsNullOrEmpty(dto.CompanyProfileUrl)) company.ProfileUrl = dto.CompanyProfileUrl;
            if (!string.IsNullOrEmpty(dto.CompanyLogo) && string.IsNullOrEmpty(company.LogoUrl)) company.LogoUrl = dto.CompanyLogo;
        }

        // ExperienceLevel eşleştir
        var expTag = dto.Tags.FirstOrDefault(t =>
            t.Contains("Başlayan") || t.Contains("Tecrübeli") || t.Contains("Uzman") || t.Contains("Orta"));
        var expLevel = expTag != null
            ? await _db.ExperienceLevels.FirstOrDefaultAsync(e => EF.Functions.Like(e.Name, $"%{expTag}%"))
            : null;

        // Sector eşleştir
        var sectorTag = dto.Tags.FirstOrDefault(t =>
            !t.Contains("Yerinde") && !t.Contains("Uzaktan") && !t.Contains("Hibrit") &&
            !t.Contains("Zamanlı") && !t.Contains("Dönemsel") && !t.Contains("Stajyer") &&
            !t.Contains("Başlayan") && !t.Contains("Tecrübeli") && !t.Contains("Uzman") &&
            !t.Contains("Orta") && !t.Contains("Yönetici"));

        Sector? sector = null;
        if (!string.IsNullOrEmpty(sectorTag))
        {
            sector = await _db.Sectors.FirstOrDefaultAsync(s => EF.Functions.Like(s.Name, sectorTag));
            if (sector == null)
            {
                sector = new Sector { Name = sectorTag };
                _db.Sectors.Add(sector);
                await _db.SaveChangesAsync();
            }
        }

        // PositionLevel eşleştir
        var posTag = dto.Tags.FirstOrDefault(t =>
            t.Contains("Yönetici") || t.Contains("Direktör") || t.Contains("Koordinatör") || t.Contains("Uzman"));
        var posLevel = posTag != null
            ? await _db.PositionLevels.FirstOrDefaultAsync(p => EF.Functions.Like(p.Name, $"%{posTag}%"))
            : null;

        var detail = new JobDetail
        {
            JobListingId = listing.Id,
            ApplyCount = dto.ApplyCount,
            DescriptionHtml = dto.DescriptionHtml,
            DescriptionText = dto.DescriptionText,
            HeaderImage = dto.HeaderImage,
            PublishDate = dto.PublishDate,
            ClosingDate = dto.ClosingDate,
            ExperienceLevelId = expLevel?.Id,
            SectorId = sector?.Id,
            PositionLevelId = posLevel?.Id,
            ScrapedAt = DateTime.UtcNow
        };

        _db.JobDetails.Add(detail);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Detay kaydedildi.", id = detail.Id });
    }

    // GET api/jobs
    [HttpGet]
    public async Task<IActionResult> GetJobs(
        [FromQuery] int? cityId,
        [FromQuery] int? workModelId,
        [FromQuery] int? workTypeId,
        [FromQuery] int? sectorId,
        [FromQuery] int? sourceId,
        [FromQuery] string? keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.JobListings
            .Include(j => j.Company)
            .Include(j => j.City)
            .Include(j => j.WorkModel)
            .Include(j => j.WorkType)
            .Include(j => j.Source)
            .Include(j => j.Detail)
            .AsQueryable();

        if (cityId.HasValue)
            query = query.Where(j => j.CityId == cityId);

        if (workModelId.HasValue)
            query = query.Where(j => j.WorkModelId == workModelId);

        if (workTypeId.HasValue)
            query = query.Where(j => j.WorkTypeId == workTypeId);

        if (sectorId.HasValue)
            query = query.Where(j => j.Detail != null && j.Detail.SectorId == sectorId);

        if (sourceId.HasValue)
            query = query.Where(j => j.SourceId == sourceId);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(j => j.Title.Contains(keyword) || j.Company.Name.Contains(keyword));

        var total = await query.CountAsync();

        var jobs = await query
            .OrderByDescending(j => j.ScrapedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new
            {
                j.Id,
                j.Slug,
                j.Title,
                j.Url,
                j.PublishedAt,
                LogoUrl = !string.IsNullOrEmpty(j.Company.LogoUrl) ? j.Company.LogoUrl : j.LogoUrl,
                Company = j.Company.Name,
                City = j.City.Name,
                WorkModel = j.WorkModel.Name,
                WorkType = j.WorkType.Name,
                SourceId = j.SourceId,
                SourceName = j.Source != null ? j.Source.Name : "kariyer.net",
                SourceLogoUrl = j.Source != null ? j.Source.LogoUrl : "https://www.kariyer.net/favicon.ico"
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, jobs });
    }

    // GET api/lookup
    [HttpGet("/api/lookup")]
    public async Task<IActionResult> GetLookup()
    {
        var cities = await _db.Cities.OrderBy(c => c.Name).Select(c => new { c.Id, c.Name }).ToListAsync();
        var workModels = await _db.WorkModels.Select(w => new { w.Id, w.Name }).ToListAsync();
        var workTypes = await _db.WorkTypes.Select(w => new { w.Id, w.Name }).ToListAsync();
        var sectors = await _db.Sectors.OrderBy(s => s.Name).Select(s => new { s.Id, s.Name }).ToListAsync();
        var sources = await _db.Sources.Select(s => new { s.Id, s.Name, s.LogoUrl, s.FaviconUrl }).ToListAsync();
        return Ok(new { cities, workModels, workTypes, sectors, sources });
    }

    // GET api/jobs/pending-details
    [HttpGet("pending-details")]
    public async Task<IActionResult> GetPendingDetails()
    {
        var pending = await _db.JobListings
            .Where(j => j.Detail == null)
            .Select(j => new { j.Slug, j.Url })
            .ToListAsync();

        return Ok(pending);
    }


    // GET api/jobs/{slug}
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetJobDetail(string slug)
    {
        var job = await _db.JobListings
            .Include(j => j.Company)
            .Include(j => j.City)
            .Include(j => j.District)
            .Include(j => j.WorkModel)
            .Include(j => j.WorkType)
            .Include(j => j.Source)
            .Include(j => j.Detail)
                .ThenInclude(d => d!.Sector)
            .Include(j => j.Detail)
                .ThenInclude(d => d!.ExperienceLevel)
            .Include(j => j.Detail)
                .ThenInclude(d => d!.PositionLevel)
            .FirstOrDefaultAsync(j => j.Slug == slug);

        if (job == null)
            return NotFound();

        return Ok(job);
    }

    // Yardımcı: "Antalya (Serik)" → "Antalya"
    private static string ParseCity(string location)
    {
        if (string.IsNullOrWhiteSpace(location)) return string.Empty;
        var city = location.Split('(')[0].Split('+')[0].Trim();
        // "İstanbul(Avr.)" gibi durumlar için
        city = city.Replace("(Avr.)", "").Replace("(Asya)", "").Trim();
        return city;
    }

    // Yardımcı: "Antalya (Serik)" → "Serik"
    private static string ParseDistrict(string location)
    {
        if (!location.Contains('(')) return string.Empty;
        var start = location.IndexOf('(') + 1;
        var end = location.IndexOf(')');
        if (end <= start) return string.Empty;
        var district = location.Substring(start, end - start).Trim();
        // "Avr." veya "Asya" gibi kariyer.net özel ifadelerini filtrele
        if (district is "Avr." or "Asya" or "Tümü") return string.Empty;
        return district;
    }
}

// DTOs
public record JobListingDto(
    string Url,
    string Slug,
    string Title,
    string Company,
    string Location,
    string WorkModel,
    string WorkType,
    string PublishedAt,
    string LogoUrl,
    int? SourceId
);

public record JobDetailDto(
    string Url,
    string Title,
    string Company,
    string Location,
    string PublishedAt,
    string PublishDate,
    string ClosingDate,
    List<string> Tags,
    string ApplyCount,
    string DescriptionHtml,
    string DescriptionText,
    string HeaderImage,
    string CompanyLogo,
    string CompanyFollowers,
    string CompanyProfileUrl
);
