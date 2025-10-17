# Design & Architecture Overview

## 1. Next-Generation Complexity & Optimization Opportunity

- The Game of Life rules are simple: each cell’s next state depends on its 8 neighbors. 

- A naive dense implementation for a grid of size r × c runs in O(r × c) per generation (you loop over each cell, check 8 neighbors — constant time).

- For fixed 20×20 design, that’s ~400 cells; the overhead is trivial. Even simulating thousands of steps is cheap at that scale.

- The larger cost is not the neighbor-check loop but cycle detection, serialization, memory overhead of storing seen states, etc.

- If in the future it allow variable or large grids, a sparse or hash-based representation may be beneficial: track only live cells, compute neighbor counts only for candidate cells, skip scanning full dead zones.

## 2. Real-Time Updates with SignalR & Background Services

### Why SignalR (instead of raw WebSocket)

- Abstracts transport negotiation (falls back if WebSocket unavailable)
- Integrates with ASP.NET Core DI and middleware
- Provides typed hub methods and group support
- Handles reconnection, lifetime, etc., alleviating boilerplate

### Why BackgroundService for “Start / Advance N” logic

- The simulation may involve many generations.
- Delegating this work to background services avoids blocking HTTP request threads.
- The background service can push updates via SignalR to clients as the board evolves.

## 3. Cache, Concurrency, Queue & Lock Design

### In-memory Cache of Running Boards

- When a board is running (via Start or Advance N), the logic load it into an in-memory cache so that the background service works from memory (fast) rather than querying DB repeatedly.

- The cache also tracks which boards are active, to know which to process and to broadcast updates efficiently.

### Concurrency: one board per critical area

- To avoid race conditions, it enforce that only one simulation per board ID runs at a time.

- The app use a lock mechanism (e.g. BoardLockService) to guard each board.

- The AdvanceNStepsQueue serializes Advance requests per board so that concurrent HTTP calls don’t interleave or corrupt state.

- The background service dequeues requests and processes one board at a time within the lock, ensuring safe, consistent updates.

## 4. Architectural Layers & Validation & Strongly Typed Controllers

### Layer structure

- **Models / Domain** — board, generation, etc.
- **Services** — game logic, orchestration.
- **Data / Persistence** — EF Core, DB context.
- **Cross-cutting / Infrastructure** — SignalR hubs, queue, cache, validation, Swagger filters.

### Validation

- I built a lightweight validation approach (inline rules) rather than heavy external dependencies, as domain is simple.

- Example rule: AddRule(dto => dto.Grid.All(row => row != null && row.All(cell => cell == 0 || cell == 1)), "Grid must contain only 0 or 1").

### Strongly typed controllers & Swagger filter

- Controllers use Results<Ok<Success<T>>, BadRequest<Fail<T>>> style return types, fully typed and expressive.

- This gives compile-time guarantees on what HTTP statuses and payloads are possible.

- To make Swagger documentation match those signatures (without littering each method with [ProducesResponseType]), we implemented a custom SwaggerOperationFilter that inspects Results<> return types and adds appropriate response schemas.

## 5. Testing Strategy: Unit & Integration

### Unit Tests

- Focus on isolated logic: GameOfLifeService, next-generation algorithm, cycle detection, validation, etc.
- Use test frameworks like xUnit, NUnit, or MSTest (e.g. xUnit is common in .NET).
- Mock dependencies (e.g. DB context, cache, clock) to isolate the unit under test.
- Example: test that a simple 3×3 input evolves to expected 3×3 output in one generation.
