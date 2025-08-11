# E-commerce Backend — .NET 8 + Clean Architecture

Backend para un sistema de e-commerce desarrollado en **.NET 8**, siguiendo **Clean Architecture** y el patrón **CQRS con MediatR**.  
Incluye autenticación JWT, gestión de usuarios y roles, catálogo de productos, carrito de compras, checkout, pedidos y sistema de notificaciones.

---

##  Tecnologías principales
- **.NET 8**
- **Clean Architecture**
- **CQRS + MediatR**
- **Entity Framework Core** (MySQL en producción, SQLite in-memory en pruebas)
- **JWT Authentication**
- **Docker y Docker Compose**
- **xUnit**, **FluentAssertions** (pruebas)
- **Swagger**

---

## 📂 Estructura del proyecto

```text
ECommerce.Domain/          # Entidades de negocio y eventos de dominio
ECommerce.Application/     # Casos de uso, lógica de aplicación y contratos
ECommerce.Infrastructure/  # Implementaciones: EF Core, JWT, email, etc.
ECommerce.WebApi/          # API REST: controllers, configuración, middleware
ECommerce.UnitTests/       # Pruebas unitarias
ECommerce.IntegrationTests/# Pruebas de integración
```

---

## Arquitectura

```mermaid

flowchart TB
    subgraph Presentation [WebApi Layer]
        Controller[Controllers API REST]
    end

    subgraph Application [Application Layer]
        UseCases[Casos de Uso / Handlers]
        Contracts[Interfaces (IApplicationDbContext, ITransactionService...)]
    end

    subgraph Domain [Domain Layer]
        Entities[Entidades]
        Events[Eventos de Dominio]
    end

    subgraph Infrastructure [Infrastructure Layer]
        EFCore[EF Core MySQL/SQLite]
        Jwt[JWT Service]
        Email[Email Sender]
    end

    Controller --> UseCases
    UseCases --> Contracts
    Contracts --> EFCore
    Contracts --> Jwt
    Contracts --> Email
    UseCases --> Entities
    Entities --> Events
 
```
## Requisitos Previos 
- **Docker Desktop (Windows/Mac) o Docker + Compose (Linux)**
- **GIT**

## Instalacion y Ejecucion

1-Clonar el repositorio
```bash
git clone https://github.com/NDANIV/E-commerce.git
cd <tu-repo>
```
2-Configurar variables de entorno
- **Agrega .env (configura tu JWT y credenciales)**

3- **Ejecutar Docker**
```bash        
docker compose build
docker compose up -d
# Abrir Swagger:
# http://localhost:8080/swagger
```

##Cuentas de prueba 
    Admin: admin@shop.local / Admin123!

## Principales endpoints
**Autenticación**
-POST /api/auth/register — registro de usuario

-POST /api/auth/login — inicio de sesión

**Catálogo**
-GET /api/products — listar productos

-GET /api/products/{id} — detalle de producto

-POST /api/products (solo Admin) — crear producto

**Carrito**
-POST /api/cart/items — agregar producto al carrito

-PUT /api/cart/items/{productId} — actualizar cantidad

-GET /api/cart — ver carrito

**Checkout / Pedidos**
-POST /api/orders/checkout — procesar compra

-GET /api/orders/my — pedidos del usuario autenticado

##Pruebas

Pruebas unitarias e integración usando xUnit y FluentAssertions.

**Unitarias:** verifican handlers, validadores y lógica de dominio.

**Integración:** simulan flujo completo (auth, carrito, checkout) con SQLite in-memory y autenticación fake.

Para ejecutarlas:

```bash
dotnet test
```

## Problemas comunes 

    -IDX10720 (HS256 key corta): cambia JWT_KEY por 64 chars (≥32 bytes).

    -Puerto 8080 ocupado: cambia API_PORT en .env (p. ej. 8081) y repite docker compose up -d.

    -MySQL tarda en levantar: el depends_on.healthcheck ya espera; si api falla  la 1ª vez, docker compose up -d --force-recreate.