# OrderTracking

Веб-приложение для отслеживания статуса заказов. Позволяет отслеживать статусы заказов и получать уведомления о изменении статуса, а так же при создании заказов.

## Стек

- **Backend**: ASP.NET Core 8, CQRS/MediatR, Entity Framework Core, SignalR, Kafka
- **Frontend**: React, TypeScript, React Query (TanStack Query), SignalR, Tailwind CSS
- **Infrastructure**: PostgreSQL, Kafka, Docker

## Структура

```
backend/
  src/
    OrderTracking.API         - REST API + SignalR хабы
    OrderTracking.Application - CQRS команды, запросы, DTO
    OrderTracking.Domain      - Сущности, исключения, интерфейсы
    OrderTracking.Infrastructure - EF Core, Kafka, репозитории
frontend/
  src/
    api/        - HTTP клиент, SignalR сервис
    components/ - UI компоненты
    hooks/      - React хуки (запросы, SignalR)
    pages/      - Страницы приложения
    types/      - TypeScript типы
```

## Запуск

```bash
# Первый запуск
make init

# Остановка
make down

# Вся инфраструктура
make up

# Backend локально
make backend

# Frontend локально
make frontend

# Миграции БД
make migrate

# Тесты
make test

# Информация по остальным командам
make help
```

## API

- `GET    /api/orders`       — список заказов
- `GET    /api/orders/{id}`  — заказ по ID
- `POST   /api/orders`       — создать заказ
- `PATCH  /api/orders/{id}/status` — обновить статус

WebSocket-уведомления об изменении статусов и создании заказов — через SignalR хаб `/hubs/orders`.
