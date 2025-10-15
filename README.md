# Game Of Life

## 1. Project Overview & Goals

This backend implements Conway’s Game of Life as a web + real-time API. Key features:

- Upload initial board state, compute next generation, advance N steps, detect stabilization cycles or failure.
- Support real-time updates via SignalR for continuous “Start” mode.
- Persistence so that server restarts/crashes do not lose last saved state.
- Health endpoints (live / ready) for monitoring.
- Clean architecture with separation of concerns, strong typing, and solid testability.
- This document captures the architectural decisions, trade-offs, and the operational setup.

## 2. High-level Architecture

The API is exposed over HTTP/HTTPS and supports both REST-like endpoints and SignalR hub endpoints. The client uses HTTP for control flows (e.g. “advance N steps”, “start”, “get next”) and subscribes to real-time updates for the “running” board via SignalR.

A ```compose.yaml``` is included to run the backend (and optionally frontend) in isolated containers, with proper port bindings, so you don’t need to install .NET locally to run the system.

Please see [Architeture Decisions](./backend/Architeture.md) for an in-depth explanation.

## 3. Running the Application & Tests

### 3.1 Prerequisites & .NET Version

- The backend targets .NET 7.0.

- You need the .NET 7 SDK installed (dotnet --version should show 7.*).

- The project uses EF Core, SignalR, background services, DI, etc., and ships with Docker support so local development is simplified.

### 3.2 Running Locally (dotnet run)

From root of backend:

```bash
cd backend/src/GameOfLife.Api
dotnet run
```
App can be accessed on ```http://localhost:5000/swagger```

### 3.3 Tests

Unit tests are in backend/tests/GameOfLife.Tests/GameOfLife.Tests.csproj. To run:

```bash
cd backend/tests/GameOfLife.Tests
dotnet test
```

### 3.4 Docker / Compose

```bash
docker-compose up
```

## 6. Frontend

To run it locally

```bash
npm install
npm run dev
```

