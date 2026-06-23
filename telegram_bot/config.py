from pydantic_settings import BaseSettings, SettingsConfigDict

class Settings(BaseSettings):
    bot_token: str
    rabbitmq_uri: str = "amqp://guest:guest@localhost:5672/"
    csharp_api_url: str = "http://localhost:8080/api"
    api_key: str = "default_secure_key_here"

    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        extra="ignore"
    )

settings = Settings()