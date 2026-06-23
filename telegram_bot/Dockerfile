FROM python:3.13-alpine

WORKDIR /app

COPY telegram_bot/requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY telegram_bot/ .

ENTRYPOINT ["python", "main.py"]