# Docker Setup для Resort Booking API

## Описание
Этот файл описывает, как использовать Docker для развертывания приложения Resort Booking API.

## Требования
- Docker Desktop (версия 20.10+)
- Docker Compose (версия 1.29+)

## Структура файлов
- `Dockerfile` - Основной файл для сборки Docker образа
- `.dockerignore` - Файл для исключения ненужных файлов из контекста сборки
- `docker-compose.yml` - Конфигурация для локальной разработки
- `docker-compose.prod.yml` - Конфигурация для production окружения
- `.env.example` - Пример переменных окружения

## Быстрый старт (Разработка)

### 1. Клонирование репозитория
```bash
git clone https://github.com/SMAV3213/ResortBookingDiplomNew.git
cd ResortBookingDiplomNew
```

### 2. Создание .env файла
```bash
cp .env.example .env
# Отредактируйте .env файл при необходимости
```

### 3. Запуск приложения с Docker Compose
```bash
docker-compose up -d
```

### 4. Проверка статуса контейнеров
```bash
docker-compose ps
```

### 5. Просмотр логов
```bash
# Все логи
docker-compose logs -f

# Логи только API
docker-compose logs -f api

# Логи только базы данных
docker-compose logs -f sqlserver
```

### 6. Остановка приложения
```bash
docker-compose down

# С удалением volumes
docker-compose down -v
```

## Production Развертывание

### 1. Подготовка .env файла для production
```bash
cp .env.example .env.prod
# Отредактируйте .env.prod с безопасными значениями
```

### 2. Сборка Docker образа
```bash
docker build -t resort-booking-api:latest .
```

### 3. Запуск с production конфигурацией
```bash
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d
```

## Доступ к приложению

### Development
- **API**: http://localhost:8080
- **Swagger UI**: http://localhost:8080/swagger
- **Database**: localhost:1433

### Production
- **API**: http://localhost:8080
- **Database**: localhost:1433

## Подключение к базе данных

### SQL Server Management Studio (SSMS)
- **Server**: localhost,1433
- **Authentication**: SQL Server Authentication
- **Login**: sa
- **Password**: значение из .env файла (DB_PASSWORD)

### Azure Data Studio
- **Server**: localhost
- **Port**: 1433
- **Authentication**: SQL Server Authentication
- **User**: sa
- **Password**: значение из .env файла (DB_PASSWORD)

## Health Check

API включает встроенный health check endpoint. Проверить статус:
```bash
curl http://localhost:8080/health
```

## Полезные команды

### Просмотр образов
```bash
docker images | grep resort-booking
```

### Удаление образа
```bash
docker rmi resort-booking-api:latest
```

### Выполнение команды в контейнере
```bash
docker-compose exec api dotnet ef database update
```

### Просмотр использования ресурсов
```bash
docker stats
```

### Сохранение/загрузка образа
```bash
# Сохранение
docker save resort-booking-api:latest -o resort-booking-api.tar

# Загрузка
docker load -i resort-booking-api.tar
```

## Troubleshooting

### Порт уже в использовании
```bash
# Найти процесс на порту 8080
lsof -i :8080  # Linux/Mac
Get-Process -Id (Get-NetTCPConnection -LocalPort 8080).OwningProcess  # Windows
```

### Проблемы с подключением к БД
1. Проверьте, что контейнер sqlserver запущен: `docker-compose ps`
2. Проверьте логи БД: `docker-compose logs sqlserver`
3. Убедитесь, что строка подключения корректна в конфигурации

### Пересборка образа
```bash
docker-compose up -d --build
```

### Полная очистка
```bash
docker-compose down -v
docker system prune -a
```

## Переменные окружения

| Переменная | Описание | Пример |
|-----------|---------|--------|
| `DB_PASSWORD` | Пароль SQL Server SA пользователя | `YourStrongPassword123!` |
| `JWT_SECRET_KEY` | Секретный ключ для JWT | `your-secret-key-min-32-characters-long!` |
| `JWT_ISSUER` | Издатель JWT токена | `ResortBookingApp` |
| `JWT_AUDIENCE` | Аудитория JWT токена | `ResortBookingUsers` |
| `JWT_EXPIRATION_MINUTES` | Время жизни JWT токена | `60` |

## Security Notes для Production

1. **Измените пароль БД** на безопасный пароль, соответствующий требованиям SQL Server
2. **Измените JWT_SECRET_KEY** на длинную случайную строку (минимум 32 символа)
3. **Используйте HTTPS** в production - добавьте сертификаты
4. **Ограничьте доступ** к портам - не открывайте 1433 напрямую в интернет
5. **Используйте Docker secrets** или Azure Key Vault для чувствительных данных
6. **Включите логирование** для мониторинга и отладки

## Дополнительные ресурсы

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [MSSQL Docker Images](https://hub.docker.com/_/microsoft-mssql-server)
