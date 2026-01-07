# ThePit

A .NET 8 REST API with Swagger/OpenAPI documentation.

## Getting Started

### Prerequisites
- .NET 8.0 SDK

### Running the API

```bash
cd src/ThePitApi
dotnet run
```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`).

### Swagger UI

When running in Development mode, Swagger UI is available at the root URL (`/`).

The OpenAPI specification is available at `/swagger/v1/swagger.json`.

## Project Structure

```
ThePitApi/
├── Controllers/
│   └── DefaultController.cs    # Main API controller
├── Models/
│   └── ContactModel.cs         # Contact request model
└── Program.cs                  # Application entry point
```

## API Endpoints

### DefaultController (`/default`)

| Method | Endpoint | Description | Request Body |
|--------|----------|-------------|--------------|
| GET | `/default` | Returns a simple greeting | - |
| PUT | `/default` | Echoes input with confirmation | `string` |
| POST | `/default` | Accepts contact information | `ContactModel` |

### Models

**ContactModel**
```json
{
  "name": "string",
  "email": "string"
}
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.