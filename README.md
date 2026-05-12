---                                                                                                                                
  HOSDashboard — Project Description                                                                                                 
                                                                                                                                     
  What It Is                                                                                                                         
                                                                                                                                     
  A C# ASP.NET Core Web API that queries Geotab's internal BigQuery tables to retrieve duty status logs for a given device and date
  range. Built as a developer learning project to replace manual BigQuery queries done during support investigations.

  ---
  Why BigQuery Instead of the Geotab API

  - No overhead on customer database/server resources
  - Handles large date ranges without timeouts
  - Not subject to API rate limits
  - Faster for cross-database lookups

  ---
  Project Structure

  HOSDashboard.sln
  ├── HOSDashboard.Api      → ASP.NET Core Web API (HTTP layer)
  ├── HOSDashboard.Core     → Class library (all business logic)
  └── HOSDashboard.Tests    → Test project (in progress)
                                                                                                                                     
  ---
  Classes Built                                                                                                                      
                                                                                      
  HOSDashboard.Core

  ┌────────────────────────┬─────────────────────────────────────────────────────────────────────────────────────────────────┐
  │         Class          │                                             Purpose                                             │
  ├────────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ BigQueryService        │ Wraps BigQueryClient, authenticates using Application Default Credentials (ADC)                 │
  ├────────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ SchemaWithBigQueryType │ Fetches a table's schema from BigQuery and maps each field's type to BigQueryDbType             │
  ├────────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Dashboard              │ Orchestrates the full flow — resolves identifiers, builds queries, returns results              │
  ├────────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ DutyStatusLogQuery     │ Builds and executes the parameterized duty status logs SQL query                                │
  ├────────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ DatabaseGuidQuery      │ Resolves a database name to a company GUID via geotab-gateway.Gateway.ClientInfo                │
  ├────────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ DeviceStatusQuery      │ Resolves a serial number to HardwareId via DeviceCurrentStatusDaily, tries US → CA → EU regions │
  ├────────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ DutyStatusLogRecord    │ Maps a BigQuery row to a typed C# object                                                        │
  ├────────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ Parameter              │ Generic internal name/value pair                                                                │
  ├────────────────────────┼─────────────────────────────────────────────────────────────────────────────────────────────────┤
  │ RequestParameters      │ Wraps the parameter list for use across the domain                                              │
  └────────────────────────┴─────────────────────────────────────────────────────────────────────────────────────────────────┘

  HOSDashboard.Api

  ┌─────────────────┬──────────────────────────────────────────────────────────────────────────────┐
  │      Class      │                                   Purpose                                    │
  ├─────────────────┼──────────────────────────────────────────────────────────────────────────────┤
  │ HOSController   │ Receives HTTP requests, converts to domain types, returns results            │
  ├─────────────────┼──────────────────────────────────────────────────────────────────────────────┤
  │ ClientParameter │ HTTP deserialization DTO — separates the HTTP contract from the domain model │
  └─────────────────┴──────────────────────────────────────────────────────────────────────────────┘

  ---
  What Comes From the Google.Cloud.BigQuery.V2 Package

  ┌────────────────────────────────┬─────────────────────────────────────────────────────────────────────┐
  │              Type              │                            What It Does                             │
  ├────────────────────────────────┼─────────────────────────────────────────────────────────────────────┤
  │ BigQueryClient                 │ The actual client that authenticates and communicates with BigQuery │
  ├────────────────────────────────┼─────────────────────────────────────────────────────────────────────┤
  │ BigQueryParameter              │ A typed, named parameter for safe SQL parameterization              │
  ├────────────────────────────────┼─────────────────────────────────────────────────────────────────────┤
  │ BigQueryResults                │ The result set returned by a query                                  │
  ├────────────────────────────────┼─────────────────────────────────────────────────────────────────────┤
  │ BigQueryRow                    │ A single row from results, accessed by column name                  │
  ├────────────────────────────────┼─────────────────────────────────────────────────────────────────────┤
  │ BigQueryDbType                 │ Enum of BigQuery data types (Int64, String, Date, etc.)             │
  ├────────────────────────────────┼─────────────────────────────────────────────────────────────────────┤
  │ BigQueryTable                  │ Table metadata — used to fetch schema                               │
  ├────────────────────────────────┼─────────────────────────────────────────────────────────────────────┤
  │ TableSchema / TableFieldSchema │ Schema definition including field names and types                   │
  └────────────────────────────────┴─────────────────────────────────────────────────────────────────────┘

  ---
  Input Requirements

  The API accepts a JSON array of name/value pairs. Minimum required:

  ┌────────────────────┬───────────┬──────────────┐
  │     Parameter      │ Required? │ Alternative  │
  ├────────────────────┼───────────┼──────────────┤
  │ _FROMPARTITIONDATE │ Yes       │ —            │
  ├────────────────────┼───────────┼──────────────┤
  │ _TOPARTITIONDATE   │ Yes       │ —            │
  ├────────────────────┼───────────┼──────────────┤
  │ Guid               │ One of    │ DatabaseName │
  ├────────────────────┼───────────┼──────────────┤
  │ HardwareId         │ One of    │ SerialNo     │
  └────────────────────┴───────────┴──────────────┘

  If DatabaseName is provided instead of Guid, the API resolves it automatically. If SerialNo is provided instead of HardwareId, the
  API resolves it across US, CA, and EU regions.

  ---
  Request Flow

  POST /hos/logs
  [{"Name":"HardwareId","Value":"560154197"},
   {"Name":"Guid","Value":"9983e2f0-..."},
   {"Name":"_FROMPARTITIONDATE","Value":"2026-04-01"},
   {"Name":"_TOPARTITIONDATE","Value":"2026-04-02"}]
           │
           ▼
  HOSController
    Converts ClientParameter[] → Parameter[]
           │
           ▼
  RequestParameters
    Wraps parameters into a list
           │
           ▼
  Dashboard
    1. Fetches schemas for 3 BigQuery tables (SchemaWithBigQueryType)
    2. Resolves Guid
         ├── Guid provided directly → use as-is
         └── DatabaseName provided → DatabaseGuidQuery → ClientInfo table → returns Guid
    3. Resolves HardwareId
         ├── HardwareId provided directly → use as-is
         └── SerialNo provided → DeviceStatusQuery → DeviceCurrentStatusDaily (US/CA/EU) → returns HardwareId
    4. Builds BigQueryParameters
    5. Executes DutyStatusLogQuery → DutyStatusLogs table
    6. Maps each row to DutyStatusLogRecord
           │
           ▼
  HOSController
    Returns results as JSON

  ---
  Known Limitations / Next Steps

  - Schema is fetched from BigQuery on every request — should be cached
  - All queries are synchronous — async (ExecuteQueryAsync) to be implemented
  - No unit tests yet
