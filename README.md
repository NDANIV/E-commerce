# E‑commerce .NET 8 — Clean Architecture

Backend .NET 8 + CQRS (MediatR) + EF Core (MySQL) + JWT + Docker.  
Funcionalidades: usuarios/roles, catálogo, carrito, checkout, pedidos, notificaciones.

## Requisitos
- Docker Desktop (Windows/Mac) o Docker + Compose (Linux)

## Arranque rápido
- Agrega .env (configura tu JWT y credenciales) 
```bash        
docker compose build
docker compose up -d
# Abrir Swagger:
# http://localhost:8080/swagger
```

Cuentas de prueba 
    Admin: admin@shop.local / Admin123!

Rutas principales
    POST /api/auth/login

    GET /api/products

    POST /api/cart/items, GET /api/cart

    POST /api/orders/checkout, GET /api/orders/my

Problemas comunes (y solución express)

    IDX10720 (HS256 key corta): cambia JWT_KEY por 64 chars (≥32 bytes).

    Puerto 8080 ocupado: cambia API_PORT en .env (p. ej. 8081) y repite docker compose up -d.

    MySQL tarda en levantar: el depends_on.healthcheck ya espera; si api falla la 1ª vez, docker compose up -d --force-recreate.