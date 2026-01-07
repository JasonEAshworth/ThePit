# ThePit

A .NET REST API with Swagger/OpenAPI documentation.

## Getting Started

### Prerequisites
- .NET 8.0 SDK

### Running the API

```bash
cd src/ThePit.Api
dotnet run
```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`).

### Swagger UI

When running in Development mode, Swagger UI is available at the root URL (`/`).

The OpenAPI specification is available at `/swagger/v1/swagger.json`.

## API Endpoints

- `GET /api/weatherforecast` - Get weather forecast for the next 5 days

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
