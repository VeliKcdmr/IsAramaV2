import nodriver as uc
from bs4 import BeautifulSoup
import asyncio


async def get_page_html(url: str) -> str:
    browser = await uc.start()
    page = await browser.get(url)

    # CAPTCHA geçene kadar bekle (max 30 saniye)
    for _ in range(15):
        await asyncio.sleep(2)
        content = await page.get_content()
        if "px-captcha" not in content and "Access to this page has been denied" not in content:
            browser.stop()
            return content

    content = await page.get_content()
    browser.stop()
    return content


async def scrape_job_listings(page_number: int = 1) -> list[dict]:
    url = f"https://www.kariyer.net/is-ilanlari?sayfa={page_number}"
    html = await get_page_html(url)
    soup = BeautifulSoup(html, "lxml")

    jobs = []
    cards = soup.select("a[href^='/is-ilani/']")

    for card in cards:
        href = card.get("href", "")
        if not href:
            continue

        title_el = card.select_one(".k-ad-card-title")
        company_el = card.select_one("[data-test='subtitle']")
        location_el = card.select_one(".location")
        work_model_el = card.select_one(".work-model")
        work_type_el = card.select_one(".badge-item .text")
        date_el = card.select_one(".ad-date .date")
        logo_el = card.select_one("img[data-test='company-image']")

        raw_date = date_el.get_text(strip=True) if date_el else ""
        published_at = raw_date.replace("update", "").strip()

        raw_logo = logo_el.get("src", "") if logo_el else ""
        logo_url = "" if raw_logo.startswith("data:image/svg") else raw_logo

        job = {
            "url": f"https://www.kariyer.net{href}",
            "slug": href.replace("/is-ilani/", ""),
            "title": title_el.get_text(strip=True) if title_el else "",
            "company": company_el.get_text(strip=True) if company_el else "",
            "location": location_el.get_text(strip=True) if location_el else "",
            "work_model": work_model_el.get_text(strip=True) if work_model_el else "",
            "work_type": work_type_el.get_text(strip=True) if work_type_el else "",
            "published_at": published_at,
            "logo_url": logo_url,
        }

        if job["title"]:
            jobs.append(job)

    return jobs


async def scrape_job_detail(job_url: str) -> dict:
    html = await get_page_html(job_url)
    soup = BeautifulSoup(html, "lxml")

    title_el = soup.select_one("[data-test='job-title']")
    company_el = soup.select_one("[data-test='company-name']")
    location_el = soup.select_one("[data-test='company-location']")
    date_el = soup.select_one(".date-text")
    tags = [t.get_text(strip=True) for t in soup.select(".job-feature-item")]
    apply_count_el = soup.select_one(".job-application-count")
    description_el = soup.select_one(".job-detail-qualifications")
    header_img_el = soup.select_one("[data-test='kariyer-image']")

    # Şirket bölümü için
    company_logo_el = soup.select_one("[data-test='company-image'].company-card-logo")
    company_followers_el = soup.select_one("[data-test='job-detail-company-card-large-statistic-item']")
    company_profile_el = soup.select_one("[data-test='job-detail-company-card-large-name'] a")

    # Yayın tarihi: data attribute'dan al
    job_container = soup.select_one(".job-container[lastpublishdate]")
    publish_date = job_container.get("lastpublishdate", "") if job_container else ""
    closing_date = job_container.get("closingdate", "") if job_container else ""

    return {
        "url": job_url,
        "title": title_el.get_text(strip=True) if title_el else "",
        "company": company_el.get_text(strip=True) if company_el else "",
        "location": location_el.get_text(strip=True) if location_el else "",
        "published_at": date_el.get_text(strip=True) if date_el else "",
        "publish_date": publish_date,
        "closing_date": closing_date,
        "tags": tags,
        "apply_count": apply_count_el.get_text(" ", strip=True) if apply_count_el else "",
        "description_html": str(description_el) if description_el else "",
        "description_text": description_el.get_text(separator="\n", strip=True) if description_el else "",
        "header_image": header_img_el.get("src", "") if header_img_el else "",
        "company_logo": company_logo_el.get("src", "") if company_logo_el else "",
        "company_followers": company_followers_el.get_text(strip=True) if company_followers_el else "",
        "company_profile_url": company_profile_el.get("href", "") if company_profile_el else "",
    }
