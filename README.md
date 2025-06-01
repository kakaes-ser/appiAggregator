# Api Aggregator

# API Aggregator Service

This service fetches and aggregates data from multiple external sources (news, weather, library), supporting filtering, retry logic, performance tracking, and caching.

## Features
- Configurable API list via `appsettings.json`
- Filtering via users input
- Retry policy with error handling
- Aggregated statistics endpoint

## Project Structure

- `Services/`
  - `ApiAggregationService.cs`: Core logic for aggregating API responses.
  - `RequestService.cs`: Executes HTTP calls with retry and performance logging.
  - `DataRetrieverService.cs`: Gets Data from memory or asks refreshed data.
  - `PerformanceStatisticsService.cs`: Gets statisitcs or requests elapsed time.
  - `PerformanceLogService.cs`: store info about requests elapsed time in memory.
- `Contract/`
  - Models for the project taska
- `Interfaces/`
  - Interfaces for services
- `Controllers/`
  - `AggregationController.cs`: Endpoint to retieve aggregated data.
  - `StatsController.cs`: Endpoint to retrieve performance stats.
  
  ### `GET /aggregate`
Retrieve aggregated data.

#### Optional Filter Parameters:
- `Refresh=true` — forces bypassing cache.

### `GET /stats`
Returns performance stats:
- Fast: < 100ms
- Average: 100-200ms
- Slow: > 200ms
