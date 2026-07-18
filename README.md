# TAWT-AI

Technical Assessment Web Tool Using Agentic AI Workflow

## Structure

```
.
├── frontend/   Angular application
├── backend/    .NET solution (Web API + tests)
└── docker-compose.yml
```

## Prerequisites

- [Node.js](https://nodejs.org/) 22+ and npm
- [.NET SDK](https://dotnet.microsoft.com/download) 10.0+
- [Docker](https://www.docker.com/) (optional, for containerized run)

## Frontend (Angular)

```bash
cd frontend
npm install
npm start          # serves on http://localhost:4200
npm test           # unit tests
npm run build       # production build -> dist/frontend/browser
```

## Backend (.NET)

```bash
cd backend
dotnet restore
dotnet run --project TawtAi.Api      # serves on http://localhost:5188
dotnet test                          # run test suite
```

The API exposes an OpenAPI document in development mode at `/openapi/v1.json`.

## Running both with Docker Compose

```bash
docker compose up --build
```

- Frontend: http://localhost:4200
- Backend: http://localhost:5188

## CI

GitHub Actions ([.github/workflows/ci.yml](.github/workflows/ci.yml)) builds and tests both the frontend and backend on every push and pull request to `main`.
