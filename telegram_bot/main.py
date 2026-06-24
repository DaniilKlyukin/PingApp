import asyncio
import logging
import sys
from aiogram import Bot, Dispatcher, Router, F
from aiogram.filters import CommandStart, Command
from aiogram.types import Message, InlineKeyboardButton, InlineKeyboardMarkup, CallbackQuery
from aiogram.fsm.state import State, StatesGroup
from aiogram.fsm.context import FSMContext

from config import settings
from api_client import api_client
from rmq_consumer import start_rmq_consumer

logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(name)s - %(levelname)s - %(message)s")
logger = logging.getLogger(__name__)

dp = Dispatcher()
router = Router()

class SubscribeStates(StatesGroup):
    waiting_for_nickname = State()


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

    # Запрашиваем подписки пользователя для сопоставления с именами
    subs = await api_client.get_subscriptions(message.chat.id)
    sub_map = {s["address"]: s["nickname"] for s in subs}

    lines = ["📊 *Текущий статус устройств:*"]
    for s in statuses:
        emoji = "🟢" if s["atWork"] else "🔴"
        address = s["address"]
        nickname = sub_map.get(address)

        if nickname:
            device_label = f"*{nickname}* (`{address}`)"
        else:
            device_label = f"`{address}`"

        lines.append(f"{emoji} {device_label} — {s['statusString']}")

    await message.answer("\n".join(lines), parse_mode="Markdown")


@router.message(Command("subscribe"))
async def cmd_subscribe(message: Message):
    all_devices = await api_client.get_allowed_devices()
    active_subs = await api_client.get_subscriptions(message.chat.id)

    # Оставляем только те устройства, на которые пользователь еще НЕ подписан
    subscribed_addresses = {s["address"] for s in active_subs}
    available_devices = [addr for addr in all_devices if addr not in subscribed_addresses]

    if not available_devices:
        await message.answer("Нет новых доступных устройств для подписки.")
        return

    keyboard = InlineKeyboardMarkup(inline_keyboard=[
        [InlineKeyboardButton(text=addr, callback_data=f"sub_{addr}")] for addr in available_devices
    ])
    await message.answer("Выберите устройство для подписки на уведомления:", reply_markup=keyboard)


@router.message(Command("unsubscribe"))
async def cmd_unsubscribe(message: Message):
    active_subs = await api_client.get_subscriptions(message.chat.id)
    if not active_subs:
        await message.answer("У вас нет активных подписок.")
        return

    # Показываем только те устройства, на которые пользователь ПОДПИСАН
    keyboard_buttons = []
    for s in active_subs:
        addr = s["address"]
        nick = s["nickname"]
        label = f"{nick} ({addr})" if nick else addr
        keyboard_buttons.append([InlineKeyboardButton(text=label, callback_data=f"unsub_{addr}")])

    keyboard = InlineKeyboardMarkup(inline_keyboard=keyboard_buttons)
    await message.answer("Выберите устройство для отмены подписки:", reply_markup=keyboard)


@router.callback_query(F.data.startswith("sub_"))
async def process_sub_callback(callback: CallbackQuery, state: FSMContext):
    address = callback.data.split("sub_")[1]
    success = await api_client.subscribe(callback.message.chat.id, address)

    if success:
        await state.update_data(sub_address=address)
        await callback.message.edit_text(
            f"✅ Вы подписались на уведомления от `{address}`.\n\n"
            "📝 Хотите задать понятное имя для этого устройства?\n"
            "Напишите его в ответном сообщении или отправьте команду /skip, чтобы продолжить без имени.",
            parse_mode="Markdown"
        )
        await state.set_state(SubscribeStates.waiting_for_nickname)
    else:
        await callback.message.edit_text("❌ Не удалось оформить подписку. Обратитесь к администратору.")
    await callback.answer()


@router.message(SubscribeStates.waiting_for_nickname)
async def process_nickname(message: Message, state: FSMContext):
    state_data = await state.get_data()
    address = state_data.get("sub_address")

    if not address:
        await state.clear()
        return

    if message.text == "/skip":
        await message.answer("Подписка завершена без назначения имени.")
    else:
        nickname = message.text.strip()
        success = await api_client.subscribe(message.chat.id, address, nickname)
        if success:
            await message.answer(f"💾 Имя устройства успешно сохранено: *{nickname}* (`{address}`)", parse_mode="Markdown")
        else:
            await message.answer("❌ Не удалось сохранить имя. Подписка активна без имени.")

    await state.clear()


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

    await asyncio.gather(
        dp.start_polling(bot),
        start_rmq_consumer(settings.rabbitmq_uri, bot)
    )


if __name__ == "__main__":
    asyncio.run(main())