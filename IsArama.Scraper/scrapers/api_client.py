import httpx

API_BASE_URL = "https://localhost:7119/api"


async def save_listings(jobs: list[dict]) -> dict:
    async with httpx.AsyncClient(timeout=30, verify=False) as client:
        response = await client.post(f"{API_BASE_URL}/jobs/listings", json=jobs)
        if not response.is_success:
            raise Exception(f"API error {response.status_code}: {response.text}")
        return response.json()


async def save_detail(detail: dict) -> dict:
    async with httpx.AsyncClient(timeout=30, verify=False) as client:
        response = await client.post(f"{API_BASE_URL}/jobs/detail", json=detail)
        response.raise_for_status()
        return response.json()
