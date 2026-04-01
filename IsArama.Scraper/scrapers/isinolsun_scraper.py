import httpx
import json
import re

BASE_URL = "https://isinolsun.com"
SOURCE_ID = 2

HEADERS = {
    "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
    "Accept-Language": "tr-TR,tr;q=0.9",
}

WORK_TYPE_MAP = {
    1: "Tam Zamanlı",
    2: "Yarı Zamanlı",
    3: "Dönemsel",
    4: "Stajyer",
    5: "Serbest",
}


async def get_next_data(url: str) -> dict:
    """Sayfadan __NEXT_DATA__ JSON'unu çeker"""
    async with httpx.AsyncClient(timeout=30, headers=HEADERS, follow_redirects=True) as client:
        resp = await client.get(url)
        resp.raise_for_status()
        match = re.search(
            r'<script id="__NEXT_DATA__" type="application/json">(.*?)</script>',
            resp.text,
            re.DOTALL
        )
        if not match:
            raise Exception(f"__NEXT_DATA__ bulunamadı: {url}")
        return json.loads(match.group(1))


async def scrape_job_listings(page_number: int = 1) -> list[dict]:
    """Belirtilen sayfadaki ilanları çeker"""
    url = f"{BASE_URL}/is-ilanlari?pn={page_number}"
    data = await get_next_data(url)
    jobs = data.get("props", {}).get("pageProps", {}).get("jobs", [])

    result = []
    for job in jobs:
        share_url = job.get("shareUrl", "")
        slug = share_url.split("/is-ilani/")[-1] if "/is-ilani/" in share_url else f"isinolsun-{job['jobId']}"

        city = job.get("cityName", "")
        town = job.get("townName", "")
        location = f"{city} ({town})" if town else city

        work_type = WORK_TYPE_MAP.get(job.get("workType"), "Tam Zamanlı")

        result.append({
            "url": share_url,
            "slug": slug,
            "title": job.get("positionName", ""),
            "company": job.get("companyName", ""),
            "location": location,
            "work_model": "İş Yerinde",
            "work_type": work_type,
            "published_at": job.get("durationDayText", ""),
            "logo_url": job.get("imageUrl", ""),
            "source_id": SOURCE_ID,
        })

    return result


async def scrape_job_detail(url: str) -> dict:
    """İlan detayını çeker"""
    data = await get_next_data(url)
    job = data.get("props", {}).get("pageProps", {}).get("job", {})

    city = job.get("cityName", "")
    town = job.get("townName", "")
    location = f"{city} ({town})" if town else city

    description_html = job.get("description", "")
    description_text = re.sub(r"<[^>]+>", " ", description_html).strip()

    tags = [b.get("fringeBenefitText", "") for b in job.get("fringeBenefitList", [])]

    company_profile_id = job.get("companyProfileUrl", "")
    company_profile_url = f"{BASE_URL}/firma-profil/{company_profile_id}" if company_profile_id else ""

    return {
        "url": url,
        "title": job.get("positionName", ""),
        "company": job.get("companyName", ""),
        "location": location,
        "published_at": job.get("durationDayText", ""),
        "publish_date": "",
        "closing_date": "",
        "tags": tags,
        "apply_count": str(job.get("totalApplicationCount", "")),
        "description_html": description_html,
        "description_text": description_text,
        "header_image": job.get("largeImageUrl", ""),
        "company_logo": job.get("imageUrl", ""),
        "company_followers": str(job.get("totalShowCount", "")),
        "company_profile_url": company_profile_url,
    }
