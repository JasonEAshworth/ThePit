# ThePit

[![.NET Core Desktop](https://github.com/JasonEAshworth/ThePit/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/JasonEAshworth/ThePit/actions/workflows/dotnet-desktop.yml)
[![SvelteKit Frontend](https://github.com/JasonEAshworth/ThePit/actions/workflows/sveltekit-frontend.yml/badge.svg)](https://github.com/JasonEAshworth/ThePit/actions/workflows/sveltekit-frontend.yml)

A .NET 8 REST API with a SvelteKit frontend for invoice and payment management.

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+ and npm

### Running the API

```bash
cd src/ThePitApi
dotnet run
```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`).

### Running the Frontend

```bash
cd src/thepit-ui
npm install
npm run dev
```

The frontend will be available at `http://localhost:5173`.

### Environment Configuration

Copy the example environment file and configure as needed:

```bash
cd src/thepit-ui
cp .env.example .env
```

**Environment Variables:**

| Variable | Description | Default |
|----------|-------------|---------|
| `PUBLIC_API_BASE_URL` | Base URL of the backend API | `http://localhost:5000/api` |

## Project Structure

```
ThePitApi/
├── src/
│   ├── ThePitApi/              # .NET Web API
│   │   ├── Controllers/        # API controllers
│   │   ├── Models/             # Request/response models
│   │   └── Program.cs          # Application entry point
│   ├── ThePit.Services/        # Business logic layer
│   │   ├── DTOs/               # Data transfer objects
│   │   ├── Interfaces/         # Service interfaces
│   │   └── Services/           # Service implementations
│   ├── ThePit.DataAccess/      # Data access layer
│   │   ├── Data/               # DbContext
│   │   ├── Entities/           # Entity models
│   │   ├── Interfaces/         # Repository interfaces
│   │   └── Repositories/       # Repository implementations
│   └── thepit-ui/              # SvelteKit frontend
│       ├── src/
│       │   ├── lib/
│       │   │   ├── api/        # API client and service modules
│       │   │   └── types/      # TypeScript type definitions
│       │   └── routes/         # SvelteKit pages
│       │       ├── invoices/   # Invoice management pages
│       │       └── payments/   # Payment management pages
│       └── package.json
└── ThePitApi.sln
```

## Frontend Features

The SvelteKit frontend provides a modern UI for managing invoices and payments:

### Dashboard (/)
- Overview of recent invoices and payments
- Quick stats and status summaries

### Invoices (/invoices)
- List all invoices with filtering and sorting
- Create new invoices
- View invoice details (/invoices/[id])
- Edit and update invoices
- Delete invoices

### Payments (/payments)
- List all payments with status and method filtering
- Process new payments
- View payment details (/payments/[id])
- Payment status tracking

## API Endpoints

### Swagger UI

When running in Development mode, Swagger UI is available at the root URL (`/`).
The OpenAPI specification is available at `/swagger/v1/swagger.json`.

### Invoices API (`/api/invoices`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/invoices` | List all invoices |
| GET | `/api/invoices/{id}` | Get invoice by ID |
| POST | `/api/invoices` | Create new invoice |
| PUT | `/api/invoices/{id}` | Update invoice |
| DELETE | `/api/invoices/{id}` | Delete invoice |

### Payments API (`/api/payments`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/payments` | List payments (supports `?status=` and `?method=` filters) |
| GET | `/api/payments/{id}` | Get payment by ID |
| GET | `/api/payments/by-invoice/{invoiceId}` | Get payments for an invoice |

## Technology Stack

**Backend:**
- .NET 8
- Entity Framework Core (InMemory for development)
- Swagger/OpenAPI

**Frontend:**
- SvelteKit 2
- Svelte 5
- TypeScript
- TailwindCSS

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
