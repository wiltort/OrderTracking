.PHONY: help up down logs build clean test backend frontend migrate restore status db db-shell db-logs db-reset init

# Colors using tput
RED := $(shell tput setaf 1)
GREEN := $(shell tput setaf 2)
YELLOW := $(shell tput setaf 3)
BLUE := $(shell tput setaf 4)
NC := $(shell tput sgr0)

help:
	@echo "$(GREEN)Available commands:$(NC)"
	@echo ""
	@echo "$(YELLOW)Docker commands:$(NC)"
	@echo "  $(BLUE)make up$(NC)         - Start all services with Docker Compose"
	@echo "  $(BLUE)make down$(NC)       - Stop all services"
	@echo "  $(BLUE)make logs$(NC)       - Show logs of all services"
	@echo "  $(BLUE)make logs-backend$(NC) - Show logs of backend only"
	@echo "  $(BLUE)make build$(NC)      - Build all Docker images"
	@echo "  $(BLUE)make clean$(NC)      - Clean Docker containers and volumes"
	@echo ""
	@echo "$(YELLOW)Local development:$(NC)"
	@echo "  $(BLUE)make backend$(NC)    - Run backend locally"
	@echo "  $(BLUE)make migrate$(NC)    - Apply database migrations"
	@echo "  $(BLUE)make migration$(NC)  - Create new migration"
	@echo "  $(BLUE)make test$(NC)       - Run all tests"
	@echo "  $(BLUE)make restore$(NC)    - Restore NuGet packages"
	@echo ""
	@echo "$(YELLOW)Database utilities:$(NC)"
	@echo "  $(BLUE)make db$(NC)         - Connect to PostgreSQL"
	@echo "  $(BLUE)make db-shell$(NC)   - Enter PostgreSQL container shell"
	@echo "  $(BLUE)make db-logs$(NC)    - Show PostgreSQL logs"
	@echo "  $(BLUE)make db-reset$(NC)   - Reset database"
	@echo "  $(BLUE)make db-backup$(NC)  - Backup database"
	@echo ""
	@echo "$(YELLOW)Info:$(NC)"
	@echo "  $(BLUE)make status$(NC)     - Show status of all services"
	@echo "  $(BLUE)make init$(NC)       - Initialize project (first run)"

up:
	docker-compose up -d
	@echo "$(GREEN)All services started!$(NC)"
	@echo "$(BLUE)Access to services:$(NC)"
	@echo "  Frontend:     http://localhost:3000"
	@echo "  Backend API:  http://localhost:5000"
	@echo "  Swagger UI:   http://localhost:5000/swagger"
	@echo "  Adminer DB:   http://localhost:8080"
	@echo "  PostgreSQL:   localhost:5433"
	@echo "  Kafka:        localhost:9092"

down:
	docker-compose down
	@echo "$(GREEN)All services stopped$(NC)"

logs:
	docker-compose logs -f

logs-backend:
	docker-compose logs -f backend

build:
	docker-compose build

clean:
	docker-compose down -v
	@echo "$(GREEN)All containers and volumes removed$(NC)"

status:
	docker-compose ps

# Local development
backend:
	@echo "$(YELLOW)Running backend locally...$(NC)"
	cd backend && dotnet run --project src/OrderTracking.API/OrderTracking.API.csproj

migrate:
	@echo "$(YELLOW)Applying migrations...$(NC)"
	cd backend && dotnet ef database update --project src/OrderTracking.API/OrderTracking.API.csproj

migration:
	@echo "$(YELLOW)Creating new migration...$(NC)"
	@read -p "Enter migration name: " name; \
	cd backend && dotnet ef migrations add $$name --project src/OrderTracking.API/OrderTracking.API.csproj

test:
	@echo "$(YELLOW)Running tests...$(NC)"
	cd backend && dotnet test

restore:
	@echo "$(YELLOW)Restoring packages...$(NC)"
	cd backend && dotnet restore

# Database utilities
db:
	@echo "$(YELLOW)Connecting to PostgreSQL...$(NC)"
	psql -h localhost -p 5433 -U postgres -d orderdb

db-shell:
	@echo "$(YELLOW)Entering PostgreSQL container...$(NC)"
	docker exec -it order-tracking-postgres bash

db-logs:
	@echo "$(YELLOW)PostgreSQL logs...$(NC)"
	docker logs -f order-tracking-postgres

db-reset:
	@echo "$(YELLOW)Resetting database...$(NC)"
	docker exec -it order-tracking-postgres psql -U postgres -c "DROP DATABASE IF EXISTS orderdb;"
	docker exec -it order-tracking-postgres psql -U postgres -c "CREATE DATABASE orderdb;"
	@echo "$(GREEN)Database reset$(NC)"
	make migrate

db-backup:
	@echo "$(YELLOW)Creating database backup...$(NC)"
	docker exec -t order-tracking-postgres pg_dump -U postgres orderdb > backup_$(shell date +%Y%m%d_%H%M%S).sql
	@echo "$(GREEN)Backup created$(NC)"

# First run initialization
init:
	@echo "$(YELLOW)Initializing project...$(NC)"
	@echo "$(YELLOW)1. Restoring packages...$(NC)"
	make restore
	@echo "$(YELLOW)2. Building project...$(NC)"
	cd backend && dotnet build
	@echo "$(YELLOW)3. Starting Docker Compose...$(NC)"
	make up
	@echo "$(YELLOW)4. Waiting for database...$(NC)"
	sleep 10
	@echo "$(YELLOW)5. Applying migrations...$(NC)"
	make migrate
	@echo "$(GREEN)Project is ready!$(NC)"
	@echo "$(BLUE)Backend API: http://localhost:5000$(NC)"
	@echo "$(BLUE)Swagger UI: http://localhost:5000/swagger$(NC)"
	@echo "$(BLUE)Adminer DB: http://localhost:8080$(NC)"