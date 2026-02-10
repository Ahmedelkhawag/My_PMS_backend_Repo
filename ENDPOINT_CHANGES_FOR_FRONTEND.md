# API Endpoint Changes – Frontend Update Guide

Summary of RESTful refactor. **All base paths are now kebab-case** (`/api/auth`, `/api/guests`, etc.). Replace old URLs with the new ones below.

---

## Quick reference: Old → New

### Auth (`/api/Auth` → `/api/auth`)

| Method | Old path | New path |
|--------|----------|----------|
| POST | `/api/Auth/register-employee` | `/api/auth/employees` |
| POST | `/api/Auth/login` | `/api/auth/login` |
| GET | `/api/Auth/roles` | `/api/auth/roles` |
| POST | `/api/Auth/change-password` | `/api/auth/password` |
| GET | `/api/Auth/profile` | `/api/auth/profile` |
| PUT | `/api/Auth/profile` | `/api/auth/profile` |
| GET | `/api/Auth/users` | `/api/auth/users` |
| GET | `/api/Auth/user/{id}` | `/api/auth/users/{id}` |
| DELETE | `/api/Auth/user/{id}` | `/api/auth/users/{id}` |
| POST | `/api/Auth/user/{id}/reset-password` | `/api/auth/users/{id}/reset-password` |
| PUT | `/api/Auth/update-employee` | `/api/auth/employees/{id}` |
| PUT | `/api/Auth/user/{id}/restore` | `/api/auth/users/{id}/restore` |
| GET | `/api/Auth/statuses` | `/api/auth/statuses` |
| POST | `/api/Auth/refresh-token` | `/api/auth/refresh-token` |
| POST | `/api/Auth/revoke-token` | `/api/auth/revoke-token` |

### Guests (`/api/Guests` → `/api/guests`)

| Method | Old path | New path |
|--------|----------|----------|
| POST | `/api/Guests` | `/api/guests` |
| GET | `/api/Guests` | `/api/guests` |
| PUT | `/api/Guests/{id}` | `/api/guests/{id}` |
| DELETE | `/api/Guests/{id}` | `/api/guests/{id}` |

### Lookups (`/api/LookupsConfiguration` → `/api/lookups`)

| Method | Old path | New path |
|--------|----------|----------|
| GET | `/api/LookupsConfiguration/sources` | `/api/lookups/sources` |
| GET | `/api/LookupsConfiguration/markets` | `/api/lookups/markets` |
| GET | `/api/LookupsConfiguration/meal-plans` | `/api/lookups/meal-plans` |
| GET | `/api/LookupsConfiguration/room-statuses` | `/api/lookups/room-statuses` |
| GET | `/api/LookupsConfiguration/room-types` | `/api/lookups/room-types` |
| GET | `/api/LookupsConfiguration/extra-services` | `/api/lookups/extra-services` |

### Reservations (`/api/Reservations` → `/api/reservations`)

| Method | Old path | New path |
|--------|----------|----------|
| POST | `/api/Reservations/creat-reservation` | `/api/reservations` |
| GET | `/api/Reservations/get-all-reservations` | `/api/reservations` |
| PUT | `/api/Reservations/change-reservation-status` | `/api/reservations/{id}/status` |
| GET | `/api/Reservations/get-reservation-details{id}` | `/api/reservations/{id}` |
| DELETE | `/api/Reservations/delete-reservation/{id}` | `/api/reservations/{id}` |
| PUT | `/api/Reservations/restore-reservation/{id}` | `/api/reservations/{id}/restore` |
| PUT | `/api/Reservations/update-reservation` | `/api/reservations/{id}` |

### Rooms (`/api/Rooms` → `/api/rooms`)

| Method | Old path | New path |
|--------|----------|----------|
| GET | `/api/Rooms` | `/api/rooms` |
| POST | `/api/Rooms` | `/api/rooms` |
| PUT | `/api/Rooms/Update-Room/{id}` | `/api/rooms/{id}` |
| DELETE | `/api/Rooms/delete-room/{id}` | `/api/rooms/{id}` |
| PUT | `/api/Rooms/change-status/{id}` | `/api/rooms/{id}/status` |
| **PUT** | `/api/Rooms/room-details/{id}` | **GET** `/api/rooms/{id}` |

---

## Breaking changes (request/URL)

1. **Auth – Update employee**  
   - **Before:** `PUT /api/Auth/update-employee` with employee `id` in body.  
   - **After:** `PUT /api/auth/employees/{id}` — send **id in the URL**, rest of payload unchanged (e.g. form/data as before).

2. **Reservations – Change status**  
   - **Before:** `PUT /api/Reservations/change-reservation-status` with body like `{ reservationId, newStatus, roomId?, note? }`.  
   - **After:** `PUT /api/reservations/{id}/status` — **id in URL**, body only: `{ newStatus, roomId?, note? }` (no `reservationId`).

3. **Reservations – Update reservation**  
   - **Before:** `PUT /api/Reservations/update-reservation` with body including `id`.  
   - **After:** `PUT /api/reservations/{id}` — **id in URL**; body still contains full update payload (including `id` for validation).

4. **Rooms – Get room by id**  
   - **Before:** `PUT /api/Rooms/room-details/{id}`.  
   - **After:** **GET** `/api/rooms/{id}` (same URL pattern, **method changed from PUT to GET**).

---

## Base URL pattern

Use a single base (e.g. `https://your-api.com` or env variable) and append the **new** paths above. All segments are **lowercase kebab-case** (e.g. `/api/guests`, `/api/reservations`, `/api/lookups`).

Example:

- Base: `https://api.pms.example.com`
- Login: `POST https://api.pms.example.com/api/auth/login`
- List reservations: `GET https://api.pms.example.com/api/reservations`
- One reservation: `GET https://api.pms.example.com/api/reservations/123`
- Change reservation status: `PUT https://api.pms.example.com/api/reservations/123/status` with body `{ "newStatus": "CheckedIn", "roomId": 1, "note": null }`

---

*Generated from PMS-backend RESTful refactor. If you need the same list in JSON or another format, ask the backend team.*
