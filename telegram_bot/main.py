import asyncio
import logging
import sys
from aiogram import Bot, Dispatcher, Router, F
from aiogram.filters import CommandStart, Command
from aiogram.types import Message, InlineKeyboardButton, InlineKeyboardMarkup, CallbackQuery

from config import settings
from api_client import api_client
from rmq_consumer import start_rmq_consumer

logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(name)s - %(levelname)s - %(message)s")
logger = logging.getLogger(__name__)

dp = Dispatcher()
router = Router()


@router.message(CommandStart())
async def cmd_start(message: Message):
    welcome_text = (
        "👋 *Приветствуем в системе сетевого мониторинга!*\n\n"
        "Я помогу вам отслеживать статус доступных компьютеров в сети.\n\n"
        "ℹ️ *Доступные команды:*\n"
        "📋 /status — Проверить статус всех устройств\n"
        "🔔 /subscribe — Подписаться на уведомления устройства\n"
        "🔕 /unsubscribe — Отписаться от уведомлений"
    )
    await message.answer(welcome_text, parse_mode="Markdown")


@router.message(Command("status"))
async def cmd_status(message: Message):
    statuses = await api_client.get_device_statuses()
    if not statuses:
        await message.answer("Связь с сервером мониторинга отсутствует или список пуст.")
        return

    lines = ["📊 *Текущий статус устройств:*"]
    for s in statuses:
        emoji = "🟢" if s["atWork"] else "🔴"
        lines.append(f"{emoji} `{s['address']}` — {s['statusString']}")

    await message.answer("\n".join(lines), parse_mode="Markdown")


@router.message(Command("subscribe"))
async def cmd_subscribe(message: Message):
    devices = await api_client.get_allowed_devices()
    if not devices:
        await message.answer("Нет доступных устройств для подписки.")
        return

    keyboard = InlineKeyboardMarkup(inline_keyboard=[
        [InlineKeyboardButton(text=addr, callback_data=f"sub_{addr}")] for addr in devices
    ])
    await message.answer("Выберите устройство для подписки на уведомления:", reply_markup=keyboard)


@router.message(Command("unsubscribe"))
async def cmd_unsubscribe(message: Message):
    devices = await api_client.get_allowed_devices()
    if not devices:
        await message.answer("Устройства для отписки не найдены.")
        return

    keyboard = InlineKeyboardMarkup(inline_keyboard=[
        [InlineKeyboardButton(text=addr, callback_data=f"unsub_{addr}")] for addr in devices
    ])
    await message.answer("Выберите устройство для отмены подписки:", reply_markup=keyboard)


@router.callback_query(F.data.startswith("sub_"))
async def process_sub_callback(callback: CallbackQuery):
    address = callback.data.split("sub_")[1]
    success = await api_client.subscribe(callback.message.chat.id, address)

    if success:
        await callback.message.edit_text(f"✅ Вы успешно подписались на уведомления от `{address}`.",
                                         parse_mode="Markdown")
    else:
        await callback.message.edit_text("❌ Не удалось оформить подписку. Обратитесь к администратору.")
    await callback.answer()


@router.callback_query(F.data.startswith("unsub_"))
async def process_unsub_callback(callback: CallbackQuery):
    address = callback.data.split("unsub_")[1]
    success = await api_client.unsubscribe(callback.message.chat.id, address)

    if success:
        await callback.message.edit_text(f"🔕 Подписка на уведомления от `{address}` отменена.", parse_mode="Markdown")
    else:
        await callback.message.edit_text("❌ Не удалось отменить подписку.")
    await callback.answer()


async def main():
    if not settings.bot_token:
        logger.critical("Переменная окружения BOT_TOKEN не задана!")
        sys.exit(1)

    bot = Bot(token=settings.bot_token)
    dp.include_router(router)

    # Запускаем поллинг бота и слушатель очереди сообщений одновременно
    await asyncio.gather(
        dp.start_polling(bot),
        start_rmq_consumer(settings.rabbitmq_uri, bot)
    )


if __name__ == "__main__":
    asyncio.run(main())