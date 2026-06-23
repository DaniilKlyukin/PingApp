import asyncio
import json
import logging
import aio_pika
from aiogram import Bot
from aiogram.exceptions import TelegramRetryAfter

logger = logging.getLogger(__name__)

MAX_CONCURRENT_SENDS = 25
semaphore = asyncio.Semaphore(MAX_CONCURRENT_SENDS)


async def send_notification_safe(bot: Bot, chat_id: int, text: str):
    """Безопасная отправка сообщения с учетом Rate Limit Telegram API"""
    async with semaphore:
        try:
            await bot.send_message(chat_id=chat_id, text=text, parse_mode="Markdown")
        except TelegramRetryAfter as e:
            logger.warning(f"Превышен лимит запросов Telegram. Ожидание {e.retry_after} сек.")
            await asyncio.sleep(e.retry_after)
            await send_notification_safe(bot, chat_id, text)
        except Exception as ex:
            logger.error(f"Не удалось отправить уведомление пользователю {chat_id}: {ex}")


async def process_message(message: aio_pika.IncomingMessage, bot: Bot):
    """Парсинг интеграционного события из C# системы"""
    async with message.process(requeue=True):
        try:
            payload = json.loads(message.body.decode())
            address = payload.get("Address")
            is_online = payload.get("IsOnline")
            target_chat_ids = payload.get("TargetChatIds", [])

            if not address or not target_chat_ids:
                return

            status_text = "В СЕТИ 🟢" if is_online else "ВНЕ СЕТИ 🔴"
            text = f"📢 *Изменение статуса!*\n\nУстройство `{address}` теперь *{status_text}*."

            tasks = [send_notification_safe(bot, chat_id, text) for chat_id in target_chat_ids]
            await asyncio.gather(*tasks)

        except Exception as e:
            logger.error(f"Ошибка обработки сообщения из очереди: {e}")


async def start_rmq_consumer(rabbit_url: str, bot: Bot):
    """Инициализация подключения и прослушивания очереди"""
    connection = await aio_pika.connect_robust(rabbit_url)
    channel = await connection.channel()

    await channel.set_qos(prefetch_count=50)

    exchange_name = "PingApp.Application.Contracts:DeviceStatusChangedIntegrationEvent"

    exchange = await channel.declare_exchange(
        name=exchange_name,
        type=aio_pika.ExchangeType.FANOUT,
        durable=True
    )

    queue = await channel.declare_queue("telegram.bot.notifications", durable=True)
    await queue.bind(exchange)

    logger.info("Слушатель RabbitMQ успешно запущен. Ожидание событий...")

    await queue.consume(lambda msg: process_message(msg, bot))