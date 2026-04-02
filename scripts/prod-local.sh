#!/bin/bash
set -e

COMPOSE="docker compose -f $(dirname "$0")/../docker-compose.prod.local.yml"

echo ">>> Building images..."
$COMPOSE build

echo ">>> Starting infrastructure..."
$COMPOSE up -d postgres-auth postgres-inventory postgres-orders postgres-payments rabbitmq

echo ">>> Waiting 20s for Postgres to be ready..."
sleep 20

echo ">>> Running migrations..."
$COMPOSE run --rm --entrypoint ./efbundle auth
$COMPOSE run --rm --entrypoint ./efbundle inventory
$COMPOSE run --rm --entrypoint ./efbundle orders
$COMPOSE run --rm --entrypoint ./efbundle payments

echo ">>> Starting services..."
$COMPOSE up -d

echo ">>> Done. Gateway at http://localhost:5054"
echo ">>> Logs: docker compose -f docker-compose.prod.local.yml logs -f"
