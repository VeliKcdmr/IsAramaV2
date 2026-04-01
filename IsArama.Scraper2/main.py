import asyncio
import sys
import traceback

if sys.platform == "win32":
    asyncio.set_event_loop_policy(asyncio.WindowsProactorEventLoopPolicy())

from fastapi import FastAPI, HTTPException, Query
from scrapers.kariyer_scraper import scrape_job_listings, scrape_job_detail

app = FastAPI(title="IsArama Scraper API", version="1.0.0")


@app.get("/")
def root():
    return {"status": "ok", "service": "IsArama Scraper"}


@app.get("/debug")
async def debug_html(url: str = Query(default="https://www.kariyer.net/is-ilani/gural-premier-belek-cocuk-kulubu-animatoru-492914")):
    """İlan detay sayfasının ham HTML'ini döner"""
    from scrapers.kariyer_scraper import get_page_html
    try:
        html = await get_page_html(url)
        from bs4 import BeautifulSoup
        soup = BeautifulSoup(html, "lxml")
        # Şirket bölümünü bul
        section = soup.select_one("[data-test='company-detail'], .company-detail, .firm-card, [class*='company-card'], [class*='firm-detail']")
        if not section:
            # Sağ kolon
            section = soup.select_one(".col-sm-5, .col-md-5, .col-lg-4")
        content = str(section)[:8000] if section else "not found"
        return {"length": len(html), "content": content}
    except Exception as e:
        raise HTTPException(status_code=500, detail=traceback.format_exc())


@app.get("/jobs")
async def get_jobs(page: int = Query(default=1, ge=1, description="Sayfa numarası")):
    """kariyer.net iş ilanlarını çeker"""
    try:
        jobs = await scrape_job_listings(page_number=page)
        return {"page": page, "count": len(jobs), "jobs": jobs}
    except Exception as e:
        raise HTTPException(status_code=500, detail=traceback.format_exc())


@app.get("/jobs/detail")
async def get_job_detail(url: str = Query(..., description="İlan URL'si")):
    """Bir ilanın detayını çeker"""
    try:
        detail = await scrape_job_detail(url)
        return detail
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
