# Global Logistics Management System (GLMS)

Enterprise logistics management platform built using a decoupled **Service-Oriented Architecture (SOA)**, containerized for **Cloud-Native deployment**, and tested with **automated integration tests**.

---

## Project Overview

TechMove Logistics relies on the **Global Logistics Management System (GLMS)** to manage freight contracts and service requests. The system has been refactored from a monolithic application into a decoupled, modern architecture:

1. **GLMS.Shared (Common Layer)**: A shared class library containing core entity models and enums, ensuring model consistency between frontend and backend.
2. **GLMS.API (Backend Service Layer)**: An ASP.NET Core Web API that handles all business logic, database access (via Entity Framework Core), PDF file storage, and exposes secured REST endpoints.
3. **GLMS.Web (Frontend Presentation Layer)**: An ASP.NET Core MVC client that communicates with the Web API via `HttpClient` using JWT Bearer authentication.
4. **GLMS.Tests (Testing Suite)**: Expanded to include unit tests and automated integration tests using `WebApplicationFactory` to spin up and assert endpoints.

---

## Decoupled Architecture

```mermaid
graph TD
    subgraph Client Layer (Presentation)
        Web[GLMS.Web MVC App]
    end

    subgraph Service Layer (API Service)
        API[GLMS.API Web API]
        DB[(SQL Server Database)]
        API --> DB
    end

    subgraph Common Layer
        Shared[GLMS.Shared Models Library]
    end

    Web -->|HTTP / JWT Bearer| API
    API -.->|Reference| Shared
    Web -.->|Reference| Shared
```

---

## Technologies Used

| Component            | Technology                                 |
| -------------------- | ------------------------------------------ |
| Frontend Client      | ASP.NET Core MVC + Bootstrap               |
| Backend Service API  | ASP.NET Core Web API + Swashbuckle Swagger |
| Shared Library       | C# Class Library (.NET 8.0)                |
| Database             | SQL Server Express / Docker SQL Server     |
| ORM                  | Entity Framework Core                      |
| API Authentication   | JWT Bearer Token Authentication            |
| Containerization     | Docker + Docker Compose                    |
| Testing              | xUnit + Microsoft.AspNetCore.Mvc.Testing   |

---

## Key Features

- **Decoupled Architecture**: Separates the presentation layer from the service/database layer, enabling them to scale independently.
- **JWT Authentication**: Secure API endpoints decorated with `[Authorize]`. The MVC frontend logs in via the API, retrieves a JWT, and attaches it to all outgoing `HttpClient` headers.
- **RESTful API Endpoints**: Exposes endpoints for client management, contract creation, contract filtering (`startDate`, `endDate`, `status`), status patching (approve/decline), and service requests.
- **Automated Integration Testing**: Verifies API endpoints in memory using `WebApplicationFactory`, performing full data integrity validation (Create then Read).
- **Zero-Config Database Seeding**: Automatic database schema creation and data seeding via `context.Database.EnsureCreated()` and `DbSeeder.Seed()` on startup.
- **Docker Compose Orchestration**: Containerizes the database, Web API, and Web App into a cohesive environment using Docker networks.

---

## Project Structure

```text
GLMS_AaliyahAllie_EAPD_ST10212542
├── GLMS.sln                          # Core Solution File
├── GLMS.Shared                       # Shared Models Library
│   └── Models                        # Client, Contract, ServiceRequest entities
├── GLMS.API                          # Backend Web API (DB Access & Logic)
│   ├── Controllers                   # Auth, Clients, Contracts, ServiceRequests API Controllers
│   ├── Data                          # ApplicationDbContext, DbSeeder
│   └── Services                      # Currency conversion, PDF validation, Workflows
├── GLMS.Web                          # Frontend MVC Client (HttpClient Integrations)
│   ├── Controllers                   # Proxy MVC Controllers invoking Web API
│   ├── Views                         # Responsive Razor Admin Dashboard and Public Pages
│   └── wwwroot                       # Static CSS, JS, and uploads
├── GLMS.Tests                        # Unit & Integration Testing Suite
│   ├── IntegrationTests.cs           # Automated endpoint integration tests
│   └── UnitTests                     # Workflow, Currency, and File validations
├── docker-compose.yml                # Docker Compose orchestration
└── TECHNICAL_REFLECTION_REPORT.md    # Reflection report on DevOps & Containerization
```

---

## Setup & Running Instructions

### Option A: Run via Docker Compose (Recommended)

1. Ensure **Docker Desktop** is running on your machine.
2. Open a terminal in the root solution directory.
3. Run the following command to build and start the entire stack:
   ```bash
   docker-compose up --build
   ```
4. Access the applications:
   - **Frontend Web Portal**: [http://localhost:5000](http://localhost:5000)
   - **Backend Web API Swagger UI**: [http://localhost:5001/swagger](http://localhost:5001/swagger)

---

### Option B: Run Locally (Without Docker)

#### 1. Setup the Database
Ensure your local SQL Server instance is running. Update the connection string in `GLMS.API/appsettings.json` if needed:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=LINKIEZ\\SQLEXPRESS;Database=GLMS_DB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

#### 2. Run the Web API Backend
```bash
cd GLMS.API
dotnet run
```
The API will start on [http://localhost:5001](http://localhost:5001) (check your Swagger UI at [http://localhost:5001/swagger](http://localhost:5001/swagger)).

#### 3. Run the Web App Frontend
Ensure the API is running, then start the frontend:
```bash
cd GLMS.Web
dotnet run
```
Access the MVC client portal at [http://localhost:5000](http://localhost:5000).

---

## Running the Tests

To run the automated unit and integration tests, run the following command in the root folder or test folder:
```bash
dotnet test
```

---

## Administrator Login Credentials

Use the following credentials to access the secure portal on the website:
- **Username**: `admin`
- **Password**: `Admin@123`

---

## Links

| Platform     | Link                                                                                                                   |
| ------------ | ---------------------------------------------------------------------------------------------------------------------- |
| GitHub       | [GLMS GitHub Repository](https://github.com/AaliyahAllie/GLMS_AaliyahAllie_EAPD_ST10212542.git?utm_source=chatgpt.com) |
| YouTube Demo | [GLMS System Demonstration](https://youtu.be/qcLyIenQOZA?utm_source=chatgpt.com)                                       |
