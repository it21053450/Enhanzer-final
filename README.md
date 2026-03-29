# 🛒 Mini Purchase Management Module

A full-stack **ERP-style Purchase Management System** built as part of the Enhanzer Full Stack Developer technical assessment.

---

## 📋 Overview

This application demonstrates a complete purchase management workflow including master data management, purchase bill creation with real-time calculations, PDF export, offline capability with auto-sync, and a full audit trail.

| Layer | Technology |
|---|---|
| Frontend | Angular 21 (Standalone Components, Reactive Forms, SCSS) |
| Backend | .NET 9 Web API (ASP.NET Core, EF Core 9) |
| Database | SQL Server LocalDB |
| PDF Export | jsPDF + jspdf-autotable |
| Offline Storage | LocalStorage with auto-sync |
| API Explorer | Scalar (available at `/scalar/v1`) |

---

## 🏗️ Project Structure

```
Enhanzer Final/
├── Backend/
│   └── PurchaseManagement.API/
│       ├── Controllers/         # API endpoints
│       ├── Data/                # EF Core DbContext + seed data
│       ├── DTOs/                # Data Transfer Objects
│       ├── Entities/            # Database entity models
│       ├── Migrations/          # EF Core migrations
│       ├── Repositories/        # Repository interfaces + implementations
│       ├── Services/            # Business logic layer
│       ├── Program.cs           # App bootstrap, DI, middleware
│       └── appsettings.json     # DB connection string
└── Frontend/
    └── purchase-management/
        ├── src/app/
        │   ├── models/          # TypeScript interfaces
        │   ├── services/        # ApiService, OfflineService, PdfService
        │   └── modules/purchase/
        │       ├── bill-list/   # Purchase bill list page
        │       ├── bill-form/   # Create / Edit bill form
        │       └── audit-log/   # Audit trail viewer
        ├── proxy.conf.json      # Dev-server proxy → backend
        └── angular.json
```

---

## ✅ Features Implemented

### Task 1 — Master Data (Seed Data)
- **3 Locations**: LOC001 Warehouse A, LOC002 Warehouse B, LOC003 Main Store
- **7 Items**: Mango, Apple, Banana, Orange, Grapes, Kiwi, Strawberry
- No authentication required (as per spec)

### Task 2 — Master Data APIs
- `GET /api/items` — returns all active items
- `GET /api/items/search?q=` — autocomplete search
- `GET /api/locations` — returns all active locations

### Task 3 — Purchase Bill Form
- Multi-row line item entry with **item autocomplete**
- **Batch / Location dropdown** per row
- Real-time per-row calculations: `Total Cost = (Cost × Qty) × (1 - Discount%)`, `Total Selling = Price × Qty`
- Live summary panel (Total Items, Total Quantity, Total Amount)

### Task 4 — Save Purchase Bill
- `POST /api/purchase-bill` — saves header + all line items atomically
- Auto-generated bill numbers (PB-YYYYMMDD-XXXX format)
- Full server-side validation

### Task 5.1 — PDF Export *(MANDATORY)*
- Landscape A4 PDF with company header, bill details, itemised table and totals
- Generated client-side using **jsPDF + jspdf-autotable** (no server round-trip)
- Available from the bill list via the **PDF** button

### Task 5.2 — Edit Purchase Bill *(MANDATORY)*
- `PUT /api/purchase-bill/{id}` — replaces all line items and recalculates
- Edit mode pre-populates the form with existing bill data
- Accessible via the **Edit** button on the bill list

### Task 5.3 — Offline Mode + Auto-Sync *(MANDATORY)*
- Bills can be created while **offline** — saved to **LocalStorage**
- Connection status badge shown in the UI (🟢 Online / 🔴 Offline)
- Auto-sync triggered on browser `online` event
- Manual **Sync Now** button with pending count indicator
- Sync statuses: `Pending` → `Synced` / `Failed`

### Task 5.4 — Audit Trail *(MANDATORY)*
- Every Create and Update operation on a Purchase Bill is logged to the `AuditLogs` table
- Stores: entity name, action type, old JSON value, new JSON value, timestamp
- `GET /api/auditlogs` — paginated list (default 50 entries)
- `GET /api/auditlogs/{entity}?entityId=` — filter by entity + optional record ID
- Expandable JSON diff view in the Audit Trail UI

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and [Angular CLI 21](https://angular.dev/tools/cli)
- SQL Server LocalDB (included with Visual Studio)

---

### 1. Start the Backend

```powershell
cd Backend/PurchaseManagement.API
dotnet run
```

The API will be available at **http://localhost:5015**  
API Explorer (Scalar): **http://localhost:5015/scalar/v1**

> The database `PurchaseManagementDB` is created and seeded automatically on first run via EF Core migrations.

**Connection string** (in `appsettings.json`):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PurchaseManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

---

### 2. Start the Frontend

```powershell
cd Frontend/purchase-management
npm install
ng serve --open
```

The app will open at **http://localhost:4200**

> The Angular dev-server proxy (`proxy.conf.json`) automatically forwards all `/api/*` requests to the backend — no CORS configuration needed in the browser.

---

## 🔗 API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/items` | List all items |
| GET | `/api/items/search?q=` | Search items (autocomplete) |
| GET | `/api/locations` | List all locations |
| GET | `/api/purchase-bill` | List all purchase bills (summary) |
| GET | `/api/purchase-bill/{id}` | Get full bill with line items |
| POST | `/api/purchase-bill` | Create new purchase bill |
| PUT | `/api/purchase-bill/{id}` | Update existing purchase bill |
| GET | `/api/auditlogs` | List audit log entries |
| GET | `/api/auditlogs/{entity}` | Filter audit logs by entity |

---

## 🗄️ Database Schema

```
Items           — id, name, isActive
Locations       — id, code, name, isActive
PurchaseBills   — id, billNumber, billDate, notes, status, totals, timestamps
PurchaseBillItems — id, billId, itemId, locationId, cost, price, qty, discountPercent, totalCost, totalSelling, sortOrder
AuditLogs       — id, entity, action, entityId, oldValue (JSON), newValue (JSON), createdAt
```

---

## 🧪 Running a Quick Test

```powershell
# Test all endpoints via PowerShell
Invoke-RestMethod http://localhost:5015/api/items        # 7 items
Invoke-RestMethod http://localhost:5015/api/locations    # 3 locations
Invoke-RestMethod http://localhost:5015/api/purchase-bill
Invoke-RestMethod http://localhost:5015/api/auditlogs
```

---

## 🏛️ Architecture

```
Browser (Angular 21)
    │  Reactive Forms + HttpClient
    ↓
Angular Dev Proxy (/api → :5015)
    ↓
ASP.NET Core 9 Web API
    │
    ├── Controllers  (HTTP layer)
    ├── Services     (business logic, calculations, audit logging)
    ├── Repositories (data access abstraction)
    └── EF Core 9 → SQL Server LocalDB
```

---

## 👤 Author

**Odith** — Enhanzer Full Stack Developer Technical Assessment  
Stack: Angular 21 · .NET 9 · SQL Server · EF Core 9
