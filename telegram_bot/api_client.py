import logging
from typing import List, Dict, Any, Optional
import httpx
from config import settings

logger = logging.getLogger(__name__)

class ApiClient:
    def __init__(self):
        self.base_url = settings.csharp_api_url
        self.headers = {
            "X-API-Key": settings.api_key,
            "Content-Type": "application/json"
        }

    async def get_allowed_devices(self) -> List[str]:
        """Получить список всех разрешенных адресов для подписки"""
        async with httpx.AsyncClient() as client:
            try:
                response = await client.get(f"{self.base_url}/devices", headers=self.headers)
                response.raise_for_status()
                return response.json()
            except Exception as e:
                logger.error(f"Ошибка при запросе разрешенных устройств: {e}")
                return []

    async def get_device_statuses(self) -> List[Dict[str, Any]]:
        """Получить текущие статусы всех разрешенных устройств"""
        async with httpx.AsyncClient() as client:
            try:
                response = await client.get(f"{self.base_url}/devices/statuses", headers=self.headers)
                response.raise_for_status()
                return response.json()
            except Exception as e:
                logger.error(f"Ошибка при запросе статусов устройств: {e}")
                return []

    async def subscribe(self, chat_id: int, address: str) -> bool:
        """Подписать пользователя на устройство в C# системе"""
        async with httpx.AsyncClient() as client:
            payload = {"chatId": chat_id, "deviceAddress": address}
            try:
                response = await client.post(
                    f"{self.base_url}/telegram/subscribe",
                    headers=self.headers,
                    json=payload
                )
                return response.status_code == 200
            except Exception as e:
                logger.error(f"Ошибка при отправке подписки на {address}: {e}")
                return False

    async def unsubscribe(self, chat_id: int, address: str) -> bool:
        """Отписать пользователя от устройства в C# системе"""
        async with httpx.AsyncClient() as client:
            payload = {"chatId": chat_id, "deviceAddress": address}
            try:
                response = await client.post(
                    f"{self.base_url}/telegram/unsubscribe",
                    headers=self.headers,
                    json=payload
                )
                return response.status_code == 200
            except Exception as e:
                logger.error(f"Ошибка при отправке отписки от {address}: {e}")
                return False

api_client = ApiClient()