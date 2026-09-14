# CLDV6212 POE - CoffeeNChill Canteen Management System

## Module Information

**Module:** Cloud Development B  
**Module Code:** CLDV6212  
**Assessment:** Portfolio of Evidence (POE) - Part 1  
**Project:** CoffeeNChill Canteen Management System  
**Part 1 Due Date:** 14 September 2026  

> **Important Part 1 Addendum:** The September 2026 addendum replaces the original Azure File Share requirement for staff documents with **Azure Blob Storage**, because Blob Storage is supported by Azurite. This root README reflects the addendum and the final implementation.

---

## Project Overview

CoffeeNChill is a cloud-enabled canteen management system developed for the CLDV6212 Cloud Development B Portfolio of Evidence.

Part 1 replaces two paper-based processes with cloud-backed APIs:

- Menu items are stored in the `MenuItems` Azure Table Storage table.
- Staff operational documents are stored in the `staff-docs` Azure Blob Storage container.
- HTTP-triggered Azure Functions expose the menu and document operations.
- Azurite provides local Azure Storage emulation.
- Postman is used for endpoint and validation testing.
- The Azure Functions application is containerised with Docker and published using a semantic `v1.0` image tag.
- GitHub feature branches preserve the contribution history of all four members.

---

# Part 1 Architecture

```text
                           ┌─────────────────────┐
                           │       Postman       │
                           │  API / Test Runner  │
                           └──────────┬──────────┘
                                      │
                                      ▼
                           ┌─────────────────────┐
                           │   Azure Functions   │
                           │   HTTP Endpoints    │
                           └──────────┬──────────┘
                                      │
                    ┌─────────────────┴─────────────────┐
                    │                                   │
                    ▼                                   ▼
         ┌─────────────────────┐             ┌─────────────────────┐
         │ Menu Storage Layer  │             │ Document Storage    │
         │ Azure.Data.Tables   │             │ Azure.Storage.Blobs │
         └──────────┬──────────┘             └──────────┬──────────┘
                    │                                   │
                    ▼                                   ▼
         ┌─────────────────────┐             ┌─────────────────────┐
         │       Azurite       │             │       Azurite       │
         │   Table Storage     │             │    Blob Storage     │
         │     MenuItems       │             │     staff-docs      │
         └─────────────────────┘             └─────────────────────┘
```

The Functions code follows a layered structure:

```text
HTTP Function
    ↓
Repository Interface
    ↓
Repository Implementation
    ↓
Azure Storage SDK
    ↓
Azurite
```

This keeps HTTP concerns, validation, DTO mapping and storage access separated.

---

# Part 1 Storage Architecture

## 1. MenuItems - Azure Table Storage

The `MenuItems` table stores menu items using the following entity design:

| Property | Purpose |
|---|---|
| `PartitionKey` | Menu category |
| `RowKey` | Unique item SKU / ID |
| `Name` | Menu item name |
| `Description` | Short menu description |
| `Price` | Item price |
| `IsAvailable` | Availability status |

Example:

```text
PartitionKey: Hot Drinks
RowKey: COF-001
Name: Espresso
Description: Single-shot espresso
Price: 28.00
IsAvailable: true
```

`Category` is used as the `PartitionKey`, allowing items in the same category to be grouped and queried efficiently.

---

## 2. staff-docs - Azure Blob Storage

Following the September 2026 assignment addendum, staff documents are stored in an Azure Blob Storage container named:

```text
staff-docs
```

Examples include:

- Barista recipe sheets
- Equipment cleaning manuals
- Health and safety policies
- Operational procedures
- Other staff PDF documentation

The implementation uses `Azure.Storage.Blobs` and a repository abstraction.

### Document safeguards

The final implementation includes:

- Stream-based upload
- Stream-based download
- PDF extension validation
- MIME type validation
- PDF file-signature validation
- Maximum file size of 10 MB
- Safe file-name validation
- Duplicate-document prevention
- File metadata retrieval
- Missing-document handling
- Storage and unexpected-error logging

The container is created with private access when required.

---

# Part 1 API Endpoints

## Menu Endpoints

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/menu` | Create a new menu item |
| `GET` | `/api/menu` | Retrieve all menu items |
| `GET` | `/api/menu?category={category}` | Convenience category filter |
| `GET` | `/api/menu/category/{category}` | Dedicated category-filter route required by the assignment |
| `GET` | `/api/menu/{category}/{id}` | Retrieve one menu item by category and ID / SKU |
| `PUT` | `/api/menu/{category}/{id}` | Update price and/or availability |
| `DELETE` | `/api/menu/{category}/{id}` | Delete a menu item |

### Menu HTTP responses

| Scenario | Status |
|---|---:|
| Successful creation | `201 Created` |
| Successful retrieval/update/delete | `200 OK` |
| Validation error | `400 Bad Request` |
| Missing resource | `404 Not Found` |
| Duplicate item | `409 Conflict` |
| Unexpected storage/application error | `500 Internal Server Error` |

---

## Staff Document Endpoints

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/documents/upload` | Upload a PDF staff document |
| `GET` | `/api/documents` | List staff documents and metadata |
| `GET` | `/api/documents/download/{fileName}` | Stream an existing PDF document |

### Document validation behaviour

The document API rejects:

- Missing files
- Empty files
- File paths embedded in file names
- Non-PDF extensions
- Unsupported MIME types
- Files larger than 10 MB
- Renamed files that do not contain a valid PDF signature
- Duplicate document names

The API returns structured JSON errors for validation, missing documents, conflicts and storage failures.

---

# Group Members and Final Part 1 Contributions

| Member | GitHub | Student | Final Part 1 Contribution |
|---|---|---|---|
| Member 1 | `ST10472990MohammedMoosa` | ST10472990 Mohammed Moosa | Project foundation, Azure Table architecture, menu repository, CRUD Functions, validation, logging, manual Postman evidence and documentation |
| Member 2 | `Jason4x` | ST10472838 Kaden Remley | Dedicated category route, automated menu testing, validation/edge-case coverage, Collection Runner work, integration verification and documentation |
| Member 3 | `ItzArren` | ST10447147 Arren Naicker | Azure Blob Storage staff-document repository, upload/list/download Functions, document validation, metadata, logging, Postman tests and evidence |
| Member 4 | `ItzKirxn` | ST10445189 Kieran Pillay | Multi-stage Functions Dockerfile, `.dockerignore`, container/network integration, Docker Hub `v1.0` image, standalone Docker execution and integration evidence |

## Feature Branches

```text
feature/member1-menu-foundation
feature/member2-menu-crud
feature/member3-staff-docs
feature/member4-docker
```

All feature work is merged into `main` through the group workflow. Individual feature branches remain useful for reviewing contribution history.

---

# Repository Structure

```text
CLDV6212-POE/
│
├── src/
│   └── CoffeeNChill/
│       ├── CoffeeNChill.Functions/
│       │   ├── DTOs/
│       │   ├── Functions/
│       │   │   ├── Menu/
│       │   │   └── Documents/
│       │   ├── Interfaces/
│       │   ├── Models/
│       │   ├── Services/
│       │   ├── Dockerfile
│       │   ├── .dockerignore
│       │   ├── Program.cs
│       │   ├── host.json
│       │   └── CoffeeNChill.Functions.csproj
│       └── CoffeeNChill.slnx
│
├── docs/
│   ├── member1/
│   ├── member2/
│   ├── member 3/
│   └── member4/
│
├── README.md
├── REFERENCES.md
└── .gitignore
```

> For the final submission, the exported Postman collection(s) should preferably also be copied into a single `docs/postman/` folder so that the repository directly matches the addendum deliverable wording.

---

# Local Development Requirements

Install:

- Git
- Visual Studio / Visual Studio Code
- .NET 10 SDK
- Azure Functions development tooling
- Docker Desktop
- Postman

The current Functions project targets:

```text
.NET 10
Azure Functions v4
dotnet-isolated worker model
```

---

# Local Setup - Visual Studio / Functions Host

## 1. Clone the repository

```bash
git clone https://github.com/ST10472990MohammedMoosa/CLDV6212-POE.git
cd CLDV6212-POE
```

## 2. Start Azurite

Create the container the first time:

```bash
docker run -d \
  --name cldv6212-azurite \
  -p 10000:10000 \
  -p 10001:10001 \
  -p 10002:10002 \
  mcr.microsoft.com/azure-storage/azurite
```

On later runs:

```bash
docker start cldv6212-azurite
```

Standard Azurite ports used by the project:

| Service | Port |
|---|---:|
| Blob | `10000` |
| Queue | `10001` |
| Table | `10002` |

## 3. Configure `local.settings.json`

Create/maintain this file locally inside the Functions project:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "StaffDocumentsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

`local.settings.json` must not be committed to GitHub.

## 4. Run the Functions project

The Visual Studio development profile uses:

```text
http://localhost:7077
```

Postman base URL for local Visual Studio testing:

```text
http://localhost:7077/api
```

---

# Docker - Part 1

Part 1 uses standalone Docker containers. Docker Compose is not required for this part.

## Multi-stage Dockerfile

The final Dockerfile:

1. Uses the .NET 10 SDK image to restore and publish the Functions project.
2. Copies only the published application into the Azure Functions .NET isolated runtime image.
3. Enables the Functions worker runtime in the container.

## Build the Functions image

Run from:

```text
src/CoffeeNChill/CoffeeNChill.Functions
```

Command:

```bash
docker build -t kirxn19/coffeenchill-functions:v1.0 .
```

## Create a shared Docker network

```bash
docker network create cldv6212-network
```

If the network already exists, no further action is required.

## Run Azurite on the network

For a fresh container:

```bash
docker run -d \
  --name cldv6212-azurite \
  --network cldv6212-network \
  -p 10000:10000 \
  -p 10001:10001 \
  -p 10002:10002 \
  mcr.microsoft.com/azure-storage/azurite
```

If the existing Azurite container was created previously:

```bash
docker start cldv6212-azurite
docker network connect cldv6212-network cldv6212-azurite
```

If Docker reports that the container is already connected, that message can be ignored.

## Run the Functions container

The application container must address Azurite by its Docker container name instead of `localhost`.

Use the standard Azurite development account connection string for both Functions host storage and the document repository.

PowerShell example:

```powershell
$azurite = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://cldv6212-azurite:10000/devstoreaccount1;QueueEndpoint=http://cldv6212-azurite:10001/devstoreaccount1;TableEndpoint=http://cldv6212-azurite:10002/devstoreaccount1;"

docker run -d `
  --name coffeenchill-functions `
  --network cldv6212-network `
  -p 7071:80 `
  -e "AzureWebJobsStorage=$azurite" `
  -e "StaffDocumentsStorage=$azurite" `
  -e "FUNCTIONS_WORKER_RUNTIME=dotnet-isolated" `
  kirxn19/coffeenchill-functions:v1.0
```

Docker API base URL:

```text
http://localhost:7071/api
```

## Push to Docker Hub

```bash
docker push kirxn19/coffeenchill-functions:v1.0
```

Docker Hub repository:

```text
https://hub.docker.com/r/kirxn19/coffeenchill-functions
```

---

# Postman Testing

## Local environment

The shared environment is:

```text
CLDV6212 Local
```

with:

```text
baseUrl = http://localhost:7077/api
```

When testing the Docker container, change `baseUrl` to:

```text
http://localhost:7071/api
```

Requests should use:

```text
{{baseUrl}}
```

rather than hardcoded URLs.

## Menu coverage

Member 1 and Member 2 evidence covers:

- Valid menu creation
- Missing/invalid values
- Duplicate item conflict
- Retrieve all
- Category filtering
- Retrieve by ID
- Update price
- Update availability
- Invalid update
- Missing item
- Delete
- Repeat delete / missing delete
- Collection Runner assertions

## Staff document coverage

Member 3's Postman collection covers:

- Valid PDF upload
- Upload without file
- List staff documents
- Download existing PDF
- Download missing PDF
- Unsupported file type
- Duplicate document
- Invalid PDF content

## Important final Postman verification

The final source route for retrieving one menu item is:

```text
GET /api/menu/{category}/{id}
```

Before final submission, confirm that every exported Postman request uses this route. Older Member 2 exports used:

```text
/api/menu/item/{category}/{id}
```

which no longer matches the final source code.

The addendum requires the exported Postman collection JSON to be committed in the `/docs` area and to verify all HTTP endpoints. For the cleanest final submission, consolidate the menu and document requests into one final collection under:

```text
docs/postman/
```

Recommended files:

```text
docs/postman/CoffeeNChill-Part1.postman_collection.json
docs/postman/CLDV6212-Local.postman_environment.json
```

---

# Evidence

## Member 1

```text
docs/member1/
```

Contains manual Postman evidence for menu create, read, update, delete, validation and missing-resource scenarios.

## Member 2

```text
docs/member2/
member-2-POSTMAN/
```

Contains automated menu test evidence, edge-case screenshots, Collection Runner work and exported menu Postman files.

## Member 3

```text
docs/member 3/
Member 3 -Postman/
```

Contains staff-document endpoint evidence and the exported document Postman collection.

## Member 4

```text
docs/member4/
```

Contains Docker build/run evidence, Docker image evidence and final integration testing screenshots.

---

# Security

Do not commit:

```text
local.settings.json
.env
.env.*
secrets.json
production Azure connection strings
Docker Hub access tokens
CI/CD secrets
```

The Azurite `devstoreaccount1` development key is the standard emulator credential and is only used for local/containerised development.

---

# Pull Request and Code Review Workflow

```text
Feature branch
    ↓
Implementation
    ↓
Meaningful commits
    ↓
Build and test
    ↓
Push
    ↓
Pull Request
    ↓
Peer review
    ↓
Resolve issues
    ↓
Merge into main
```

The project requirement is a minimum of five meaningful commits per group member.

---

# Definition of Done - Part 1

Part 1 should only be submitted when all of the following are verified:

- [✓] Public GitHub repository is accessible
- [✓] All four members have at least five meaningful commits
- [✓] Root README is the final authoritative README
- [✓] `/docs` evidence is complete
- [✓] No secrets are committed
- [✓] Project builds successfully
- [✓] Azurite starts successfully
- [✓] `MenuItems` Table Storage CRUD works
- [✓] Dedicated category route works
- [✓] Menu validation and custom errors work
- [✓] `staff-docs` Blob Storage upload works
- [✓] Staff document listing returns metadata
- [✓] Staff document download streams successfully
- [✓] Invalid/missing/duplicate document scenarios are handled
- [✓] Final Postman collection contains every required endpoint
- [✓] Final Postman collection uses the correct Get-by-ID route
- [✓] Postman environment uses `{{baseUrl}}`
- [✓] Saved automated tests pass
- [✓] Exported Postman JSON is committed under `docs/`
- [✓] Multi-stage Docker image builds successfully
- [✓] Functions container runs independently
- [✓] Functions container communicates with Azurite
- [✓] Public Docker Hub image is available with the `v1.0` tag
- [✓] Unlisted YouTube demonstration is complete
- [✓] YouTube link is added below

---

# Submission Links

## GitHub

```text
https://github.com/ST10472990MohammedMoosa/CLDV6212-POE
```

## Docker Hub

```text
https://hub.docker.com/r/kirxn19/coffeenchill-functions
```

## YouTube Demonstration

```text
https://youtu.be/sGtIo8GpJbE
```

---

# References

The group's technical references are maintained in:

```text
REFERENCES.md
```

References should be consolidated into the final `REFERENCES.md` and formatted consistently in the referencing style required by the module.

---

# Current Status

**Part 1:** Final integration / submission preparation  
**Member 1:** Completed  
**Member 2:** Completed, with final Postman route verification still required  
**Member 3:** Completed using Azure Blob Storage in accordance with the addendum  
**Member 4:** Completed Docker/containerisation work and the video. 
