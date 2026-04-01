import asyncio
import random
import sys
import traceback
import httpx

if sys.platform == "win32":
    asyncio.set_event_loop_policy(asyncio.WindowsProactorEventLoopPolicy())

from fastapi import FastAPI, HTTPException, Query
from scrapers.kariyer_scraper import scrape_job_listings as kariyer_scrape_listings, scrape_job_detail as kariyer_scrape_detail, reset_browser
from scrapers.isinolsun_scraper import scrape_job_listings as isinolsun_scrape_listings, scrape_job_detail as isinolsun_scrape_detail
from scrapers.api_client import save_listings, save_detail, API_BASE_URL


app = FastAPI(title="IsArama Scraper API", version="1.0.0")


@app.get("/", tags=["Genel"])
def root():
    return {"status": "ok", "service": "IsArama Scraper"}


@app.get("/debug", tags=["Genel"])
async def debug_html(url: str = Query(default="https://www.kariyer.net/is-ilani/gural-premier-belek-cocuk-kulubu-animatoru-492914")):
    """İlan detay sayfasının ham HTML'ini döner"""
    from scrapers.kariyer_scraper import get_page_html
    try:
        html = await get_page_html(url)
        from bs4 import BeautifulSoup
        soup = BeautifulSoup(html, "lxml")
        section = soup.select_one("[data-test='company-detail'], .company-detail, .firm-card, [class*='company-card'], [class*='firm-detail']")
        if not section:
            section = soup.select_one(".col-sm-5, .col-md-5, .col-lg-4")
        content = str(section)[:8000] if section else "not found"
        return {"length": len(html), "content": content}
    except Exception as e:
        raise HTTPException(status_code=500, detail=traceback.format_exc())


@app.post("/browser/reset", tags=["kariyer.net"])
async def browser_reset():
    """Browser'ı sıfırlar — takılı kalırsa kullan"""
    await reset_browser()
    return {"status": "reset"}


@app.get("/kariyer/jobs", tags=["kariyer.net"])
async def kariyer_get_jobs(page: int = Query(default=1, ge=1)):
    """kariyer.net iş ilanlarını çeker"""
    try:
        jobs = await kariyer_scrape_listings(page_number=page)
        return {"page": page, "count": len(jobs), "jobs": jobs}
    except Exception as e:
        raise HTTPException(status_code=500, detail=traceback.format_exc())


@app.post("/kariyer/scrape/listings", tags=["kariyer.net"])
async def kariyer_scrape_and_save(page: int = Query(default=1, ge=1)):
    """Kariyer.net'ten ilanları çekip kaydeder"""
    try:
        jobs = await kariyer_scrape_listings(page_number=page)
        result = await save_listings(jobs)
        return {"scraped": len(jobs), "saved": result}
    except Exception as e:
        raise HTTPException(status_code=500, detail=traceback.format_exc())


@app.post("/kariyer/scrape/listings/all", tags=["kariyer.net"])
async def kariyer_scrape_all(start_page: int = Query(default=1, ge=1), end_page: int = Query(default=100, ge=1)):
    """Kariyer.net tüm sayfaları çekip kaydeder"""
    results = {"total_pages": 0, "total_scraped": 0, "total_saved": 0, "errors": []}

    for page in range(start_page, end_page + 1):
        try:
            jobs = await kariyer_scrape_listings(page_number=page)
            if not jobs:
                break
            result = await save_listings(jobs)
            results["total_pages"] += 1
            results["total_scraped"] += len(jobs)
            results["total_saved"] += result.get("added", 0)
            delay = random.uniform(6, 14)
            print(f"[kariyer] Sayfa {page} tamam, {delay:.1f}s bekleniyor...", flush=True)
            await asyncio.sleep(delay)
        except Exception as e:
            results["errors"].append({"page": page, "error": str(e)})
            await asyncio.sleep(random.uniform(10, 20))

    return results


@app.get("/isinolsun/jobs", tags=["isinolsun.com"])
async def isinolsun_get_jobs(page: int = Query(default=1, ge=1)):
    """isinolsun.com iş ilanlarını çeker"""
    try:
        jobs = await isinolsun_scrape_listings(page_number=page)
        return {"page": page, "count": len(jobs), "jobs": jobs}
    except Exception as e:
        raise HTTPException(status_code=500, detail=traceback.format_exc())


@app.post("/isinolsun/scrape/listings", tags=["isinolsun.com"])
async def isinolsun_scrape_and_save(page: int = Query(default=1, ge=1)):
    """İsinolsun.com'dan ilanları çekip kaydeder"""
    try:
        jobs = await isinolsun_scrape_listings(page_number=page)
        result = await save_listings(jobs)
        return {"scraped": len(jobs), "saved": result}
    except Exception as e:
        raise HTTPException(status_code=500, detail=traceback.format_exc())


@app.post("/isinolsun/scrape/listings/all", tags=["isinolsun.com"])
async def isinolsun_scrape_all(start_page: int = Query(default=1, ge=1), end_page: int = Query(default=2026, ge=1)):
    """İsinolsun.com tüm sayfaları çekip kaydeder"""
    results = {"total_pages": 0, "total_scraped": 0, "total_saved": 0, "errors": []}

    for page in range(start_page, end_page + 1):
        try:
            jobs = await isinolsun_scrape_listings(page_number=page)
            if not jobs:
                break
            result = await save_listings(jobs)
            results["total_pages"] += 1
            results["total_scraped"] += len(jobs)
            results["total_saved"] += result.get("added", 0)
            delay = random.uniform(2, 5)
            print(f"[isinolsun] Sayfa {page} tamam ({len(jobs)} ilan), {delay:.1f}s bekleniyor...", flush=True)
            await asyncio.sleep(delay)
        except Exception as e:
            results["errors"].append({"page": page, "error": str(e)})
            await asyncio.sleep(5)

    return results


@app.post("/scrape/detail", tags=["Genel"])
async def scrape_and_save_detail(url: str = Query(...)):
    """Tek bir ilanın detayını çekip kaydeder"""
    try:
        if "isinolsun.com" in url:
            detail = await isinolsun_scrape_detail(url)
        else:
            detail = await kariyer_scrape_detail(url)
        result = await save_detail(detail)
        return {"url": url, "saved": result}
    except Exception as e:
        raise HTTPException(status_code=500, detail=traceback.format_exc())


@app.post("/scrape/details/bulk", tags=["Genel"])
async def scrape_and_save_all_details():
    """Detayı çekilmemiş tüm ilanların detayını çekip kaydeder"""
    try:
        async with httpx.AsyncClient(timeout=30, verify=False) as client:
            resp = await client.get(f"{API_BASE_URL}/jobs/pending-details")
            resp.raise_for_status()
            pending = resp.json()

        results = {"total": len(pending), "success": 0, "failed": 0, "errors": []}

        for item in pending:
            try:
                url = item["url"]
                if "isinolsun.com" in url:
                    detail = await isinolsun_scrape_detail(url)
                    await asyncio.sleep(random.uniform(1, 3))
                else:
                    detail = await kariyer_scrape_detail(url)
                    await asyncio.sleep(3)
                await save_detail(detail)
                results["success"] += 1
            except Exception as e:
                results["failed"] += 1
                results["errors"].append({"slug": item["slug"], "error": str(e)})

        return results
    except Exception as e:
        raise HTTPException(status_code=500, detail=traceback.format_exc())
