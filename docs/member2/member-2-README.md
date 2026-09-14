# CLDV6212 POE - CoffeeNChill Canteen Management System

## Module Information

**Module:** Cloud Development B  
**Module Code:** CLDV6212  
**Assessment:** Portfolio of Evidence (POE)  
**Project:** CoffeeNChill Canteen Management System  
**Part 1 Due Date:** 14 September 2026  

---

## Project Overview

CoffeeNChill is a cloud-enabled canteen management system developed for the CLDV6212 Cloud Development B Portfolio of Evidence.

The system is developed incrementally across three parts and demonstrates the use of cloud development technologies including Azure Storage, Azure Functions, Docker, APIs, authentication, container orchestration, CI/CD, and production deployment.

### Part 1

Part 1 focuses on:

- Azure Functions
- Azure Table Storage
- Azure Files / Azure File Share
- Azurite local storage emulation
- Docker containerisation
- Docker Hub
- Postman API testing
- Postman Collection Runner
- GitHub version control
- Feature branches
- CircleCI continuous integration
- Technical documentation
- Video demonstration

For Part 1, CoffeeNChill replaces paper-based menu and staff document processes with cloud-based storage.

The CoffeeNChill menu is stored in an Azure Table Storage table named `MenuItems`, while staff operational documents are stored in an Azure File Share named `staff-docs`.

---

# Part 1 Architecture

The Part 1 solution follows a layered structure so that HTTP Functions, application models, validation, and Azure Storage operations are separated.

```text
                        ┌─────────────────────┐
                        │       Postman       │
                        │ Collection Runner   │
                        └──────────┬──────────┘
                                   │
                                   ▼
                        ┌─────────────────────┐
                        │   Azure Functions   │
                        │   HTTP Endpoints    │
                        └──────────┬──────────┘
                                   │
                   ┌───────────────┴────────────────┐
                   │                                │
                   ▼                                ▼
        ┌─────────────────────┐          ┌─────────────────────┐
        │ Menu Storage Layer  │          │ Document File Layer │
        │ Azure Table SDK     │          │ Azure Files SDK     │
        └──────────┬──────────┘          └──────────┬──────────┘
                   │                                │
                   ▼                                ▼
        ┌─────────────────────┐          ┌─────────────────────┐
        │      Azurite        │          │    Microsoft Azure  │
        │   Table Storage     │          │      File Share     │
        │     MenuItems       │          │      staff-docs     │
        └─────────────────────┘          └─────────────────────┘
```

---

# Part 1 Storage Architecture

## MenuItems - Azure Table Storage

The `MenuItems` table stores CoffeeNChill menu items.

Azure Table Storage is a NoSQL storage service that uses a `PartitionKey` and `RowKey` to uniquely identify an entity.

Each menu item contains:

- `PartitionKey` - Menu category
- `RowKey` - Unique menu item SKU / ID
- `Name`
- `Description`
- `Price`
- `IsAvailable`

### Example Menu Item

```text
PartitionKey: Hot Drinks
RowKey: COF-001
Name: Espresso
Description: Single-shot espresso
Price: 28.00
IsAvailable: true
```

### PartitionKey Design

The `Category` is used as the `PartitionKey`.

Example categories include:

- Hot Drinks
- Cold Drinks
- Pastries
- Sandwiches

This allows menu items belonging to the same category to be grouped and queried efficiently.

### RowKey Design

The menu item SKU or ID is used as the `RowKey`.

Examples:

```text
COF-001
COF-002
PAS-104
SAN-201
```

The combination of:

```text
PartitionKey + RowKey
```

uniquely identifies a menu item in Azure Table Storage.

Example:

```text
Hot Drinks + COF-001
```

---

## staff-docs - Azure File Share

The `staff-docs` Azure File Share stores operational documents used by CoffeeNChill staff.

Examples include:

- Barista recipe sheets
- Equipment cleaning manuals
- Health and safety policies
- Operational procedures
- Other staff documentation

Azure Files provides a centralised location where files can be stored and accessed when required.

Azurite does not emulate Azure File Shares, therefore the `staff-docs` component will use an actual Microsoft Azure Storage Account and Azure File Share.

Sensitive Azure File Share credentials must never be committed to GitHub.

---

# Part 1 API Endpoints

## Menu Endpoints

| Method | Endpoint | Description | Owner |
|---|---|---|---|
| POST | `/api/menu` | Create a new menu item | Member 1 |
| GET | `/api/menu` | Retrieve all menu items | Member 1 |
| GET | `/api/menu?category={category}` | Convenience filter - retrieve menu items by category via query string | Member 1 |
| GET | `/api/menu/category/{category}` | Required route - retrieve menu items by category (matches assignment brief) | Member 2 |
| GET | `/api/menu/{category}/{id}` | Retrieve one menu item by category and ID / SKU | Member 1 |
| PUT | `/api/menu/{category}/{id}` | Update a menu item's price or availability | Member 2 |
| DELETE | `/api/menu/{category}/{id}` | Delete a menu item | Member 2 |

`GetMenuItems` supports an optional `?category=` query string as a
convenience, but the assignment brief specifically requires a
dedicated route (`GET /api/menu/category/{category}`), which is a
different URL to a query parameter. Both exist; the dedicated route
is the one demonstrated in testing and the video, since it's the one
that literally satisfies the brief.

### Error Response Shape

All menu endpoints return the same JSON shape on failure:

```json
{
  "error": "VALIDATION_ERROR",
  "message": "Category is required."
}
```

| `error` code | HTTP status | Meaning |
|---|---|---|
| `VALIDATION_ERROR` | 400 | Request was malformed or failed a business rule |
| `INVALID_JSON` | 400 | Request body could not be parsed as JSON |
| `DUPLICATE_MENU_ITEM` | 409 | An item with the same category + ID already exists |
| `MENU_ITEM_NOT_FOUND` | 404 | No item exists at the given category + ID |
| `INTERNAL_SERVER_ERROR` | 500 | Unexpected application or storage failure |

---

## Document Endpoints

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/documents/upload` | Upload a staff document |
| GET | `/api/documents` | List all staff documents |
| GET | `/api/documents/download/{fileName}` | Download a staff document |

---

# Group Members

| Member | GitHub | Student | Part 1 Responsibility |
|---|---|---|---|
| Member 1 | `ST10472990MohammedMoosa` | ST10472990 Mohammed Moosa | Project foundation, MenuItems storage architecture, full menu CRUD endpoints, validation, Postman evidence and documentation |
| Member 2 | `Jason4x` | ST10472838 Kaden Remley | Menu test automation, Postman Collection Runner, integration verification, documentation and code review |
| Member 3 | `ItzArren` | ST10447147 Arren Naicker | Azure File Share and staff document functionality |
| Member 4 | `ItzKirxn` | ST10445189 Kieran Pillay | Docker, Docker Hub and integration |

---

# Group Contribution Requirements

Each member is required to contribute to:

- Source code
- GitHub commit history
- README documentation
- Postman testing
- Pull Request review
- Code review
- Integration testing
- Video demonstration

Each group member must make a minimum of **five meaningful commits per part** using their own GitHub account.

Generic commit messages such as:

```text
fix
update
test
done
final
```

should be avoided.

Meaningful commit examples include:

```text
feat(menu): implement CreateMenuItem endpoint
feat(storage): add MenuItems Table Storage repository
fix(menu): validate negative menu prices
test(postman): add menu validation tests
docs(readme): document Azurite startup process
```

---

# Repository Structure

The repository is currently organised around the following core structure and may be expanded as other members complete their work:

```text
CLDV6212-POE/
│
├── src/
│   └── CoffeeNChill/
│       ├── CoffeeNChill.Functions/
│       │   ├── Functions/
│       │   │   ├── Menu/
│       │   │   └── Documents/
│       │   ├── Models/
│       │   ├── DTOs/
│       │   ├── Interfaces/
│       │   ├── Services/
│       │   ├── Program.cs
│       │   ├── host.json
│       │   └── CoffeeNChill.Functions.csproj
│       └── CoffeeNChill.slnx
│
├── docs/
│   └── member1/
│
├── README.md
├── REFERENCES.md
└── .gitignore
```

Additional folders such as automated tests, CircleCI configuration, Postman exports, Docker assets, and setup documentation may be added as later Part 1 work is completed.

---

# GitHub Repository

Repository:

```text
https://github.com/ST10472990MohammedMoosa/CLDV6212-POE
```

The repository is public so that the lecturer/marker can review the application, documentation, commit history, branches, and individual contributions.

---

# Git Workflow

The project uses GitHub feature branches.

Normal development work should not be performed directly on `main`.

The Part 1 feature branches are:

```text
feature/member1-menu-foundation
feature/member2-menu-crud
feature/member3-staff-docs
feature/member4-docker
```

Each feature branch must contain the project README.

Each member must also contribute to the README using their own GitHub account.

---

## Development Workflow

```text
main
 │
 ├── feature/member1-menu-foundation
 │
 ├── feature/member2-menu-crud
 │
 ├── feature/member3-staff-docs
 │
 └── feature/member4-docker
```

The expected workflow is:

```text
Create feature branch
        ↓
Develop feature
        ↓
Commit meaningful changes
        ↓
Push branch to GitHub
        ↓
CircleCI validation
        ↓
Open Pull Request
        ↓
Code review
        ↓
Resolve issues if required
        ↓
Merge into main
```

---

# Git Commit Convention

The project uses descriptive, feature-based commit messages.

Examples:

```text
chore(project): initialise CoffeeNChill Functions project

feat(menu): add MenuItem Table entity

feat(storage): implement MenuItems Azure Table repository

feat(menu): implement CreateMenuItem endpoint

feat(menu): implement GetAllMenuItems endpoint

fix(menu): improve invalid price validation

test(postman): add automated menu endpoint tests

docs(readme): add local Azurite setup instructions
```

---

# Local Development Requirements

Before running the application locally, ensure the following software is installed:

- Git
- Visual Studio Code
- .NET SDK required by the module
- Azure Functions development tools
- Docker Desktop
- WSL 2 where required
- Postman

---

# Azurite Setup

Azurite is used to emulate Azure Storage services locally.

## Pull the Azurite Image

Run:

```bash
docker pull mcr.microsoft.com/azure-storage/azurite
```

---

## Run Azurite

Run:

```bash
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
```

Azurite's default ports are:

| Service | Port |
|---|---:|
| Blob Storage | `10000` |
| Queue Storage | `10001` |
| Table Storage | `10002` |

Part 1 primarily uses the Table Storage service for the `MenuItems` table.

Part 2 will later make use of Azure Queue Storage.

---

# Local Storage Configuration

When Azurite is using its default ports, the local Azure Storage configuration can use:

```text
AzureWebJobsStorage=UseDevelopmentStorage=true
```

For Azure Functions, the setting is stored locally in `local.settings.json`.

Example:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

`local.settings.json` must not be committed to GitHub.

---

# Azure File Share Configuration

The `staff-docs` File Share uses a real Azure Storage Account.

The Azure File Share connection string must be stored locally or passed through environment variables.

A safe example configuration file may contain:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "StaffDocsConnection": "YOUR_AZURE_FILE_SHARE_CONNECTION_STRING"
  }
}
```

The placeholder must be replaced locally.

The real Azure credential must never be pushed to GitHub.

---

# Security

Sensitive information must never be committed to the repository.

The following files and credentials must remain outside source control:

```text
local.settings.json
.env
.env.*
secrets.json
Azure Storage connection strings
Azure File Share credentials
Docker Hub access tokens
CI/CD secrets
```

Safe placeholder/example configuration files may be committed.

---

# Menu Validation

Menu endpoints must validate incoming data.

Examples of invalid requests include:

- Missing category
- Missing menu item ID / SKU
- Missing item name
- Invalid JSON body
- Negative price
- Invalid field values
- Duplicate menu item
- Missing requested menu item

Appropriate HTTP response codes must be used.

Examples:

| Scenario | HTTP Status |
|---|---:|
| Menu item successfully created | `201 Created` |
| Successful GET request | `200 OK` |
| Invalid request | `400 Bad Request` |
| Missing resource | `404 Not Found` |
| Duplicate item | `409 Conflict` |
| Unexpected server error | `500 Internal Server Error` |

---

# Member 1 Menu Testing

Member 1 tested the menu endpoints using Postman.

Evidence is stored under:

```text
docs/member1/
```

Current evidence includes:

```text
commit5-create-menuitem-valid-200.png
commit5-invalid-id-400.png
commit6-create-menuitem-201.png
commit6-duplicate-menuitem-409.png
commit7-get-all-menuitems-200.png
commit7-get-menuitems-by-category-200.png
commit8-get-menuitem-by-id-200.png
commit8-menuitem-not-found-404.png
commit9-update-menuitem-200.png
commit9-update-menuitem-not-found-404.png
commit10-delete-menuitem-200.png
commit10-delete-menuitem-not-found-404.png
```

These tests verify request validation, successful creation, duplicate prevention, retrieval, category filtering, retrieval by ID, missing-resource handling, updates, deletion, and repeated deletion handling.

---

# Document Validation

Staff document endpoints must validate file uploads.

Validation should include:

- File exists
- File name validation
- Allowed MIME type
- Allowed file extension
- Maximum file size
- Safe file names
- Missing file handling

For stronger implementation quality, file transfers should use streams instead of unnecessarily loading entire files into memory.

Document listing should include useful metadata such as:

- File name
- File size
- Last modified / upload date

---

# Postman Testing

Postman is used to test all Part 1 API endpoints.

One shared Postman Collection will contain all CoffeeNChill Part 1 requests.

Recommended structure:

```text
CLDV6212 CoffeeNChill - Part 1
│
├── Menu
│   ├── Create Menu Item - Valid
│   ├── Create Menu Item - Invalid
│   ├── Get All Menu Items
│   ├── Get Menu Items By Category
│   ├── Get Menu Item By ID
│   ├── Update Menu Item
│   └── Delete Menu Item
│
├── Documents
│   ├── Upload Staff Document
│   ├── List Staff Documents
│   └── Download Staff Document
│
└── Validation
    ├── Missing Required Fields
    ├── Invalid Price
    ├── Duplicate Menu Item
    ├── Invalid File Type
    └── Missing Document
```

---

# Postman Environment

A Postman environment named:

```text
CLDV6212 Local
```

will contain:

```text
baseUrl = http://localhost:7077/api
```

All requests must use:

```text
{{baseUrl}}
```

Example:

```text
{{baseUrl}}/menu
```

Hardcoded URLs should not be used throughout the collection.

---

# Automated Postman Tests

Each Postman request should contain saved automated assertions.

Example:

```javascript
pm.test("Status is 200 OK", function () {
    pm.response.to.have.status(200);
});
```

Example response validation:

```javascript
pm.test("Response contains expected fields", function () {
    const body = pm.response.json();

    pm.expect(body).to.have.property("name");
    pm.expect(body).to.have.property("price");
    pm.expect(body).to.have.property("isAvailable");
});
```

The complete collection will be run using the Postman Collection Runner so that all tests can be executed together.

The exported collection and environment will be committed under:

```text
docs/postman/
```

---

# CircleCI Continuous Integration

CircleCI will be used to provide continuous integration for the project.

The initial Part 1 pipeline will perform:

```text
GitHub Push
     ↓
Checkout Repository
     ↓
Restore .NET Dependencies
     ↓
Build Project
     ↓
Run Automated Tests
     ↓
Pipeline Pass / Fail
```

The pipeline will help identify build or test failures before Pull Requests are merged.

The CircleCI configuration will be stored in:

```text
.circleci/config.yml
```

The CI/CD process may be expanded as later POE parts are developed.

---

# Docker - Part 1

Part 1 uses standalone Docker containers.

Docker Compose is not used for Part 1.

Docker Compose will be introduced in Part 2.

The Functions application will use a Dockerfile and will be published to a public Docker Hub repository.

The required Functions image will use semantic versioning.

Example:

```text
<dockerhub-username>/coffeenchill-functions:v1.0
```

The final Part 1 demonstration must show the containers being started using `docker run`.

---

# Part 1 Testing Requirements

Before Part 1 is considered complete, the group must verify:

- Azure Functions project builds successfully
- Azurite starts successfully
- MenuItems table can be created
- Menu items can be inserted
- All menu items can be retrieved
- Menu items can be filtered by category
- Individual menu items can be retrieved
- Menu items can be updated
- Menu items can be deleted
- Invalid menu input is handled
- Missing menu items return appropriate responses
- Staff documents can be uploaded
- Staff documents can be listed
- Staff documents can be downloaded
- Invalid files are rejected
- Missing files are handled
- Postman automated tests pass
- Postman Collection Runner passes
- Docker image builds
- Docker image is available on Docker Hub
- Standalone Docker containers run successfully
- CircleCI pipeline passes
- README instructions are accurate
- All members have sufficient meaningful GitHub commits

---

# Part 1 Group Responsibilities

## Member 1 - Project Foundation and Menu CRUD

**Student:** ST10472990 Mohammed Moosa  
**GitHub:** `ST10472990MohammedMoosa`

### Completed Responsibilities

- Repository and project foundation
- Azure Functions solution structure
- Azurite Table Storage configuration
- `MenuItemEntity` Azure Table entity
- Menu request/response DTO architecture
- `IMenuItemRepository`
- Azure Table Storage repository implementation
- `POST /api/menu`
- `GET /api/menu`
- `GET /api/menu?category={category}`
- `GET /api/menu/{category}/{id}`
- `PUT /api/menu/{category}/{id}`
- `DELETE /api/menu/{category}/{id}`
- Request validation
- Duplicate-item handling
- Missing-resource handling
- Structured API error responses
- Logging
- Postman manual testing
- Member 1 screenshot evidence
- README contribution
- References documentation
- GitHub feature branch development

### Member 1 Git Branch

```text
feature/member1-menu-foundation
```

### Member 1 Commit Sequence

```text
1. chore(project): initialise CoffeeNChill Azure Functions solution
2. feat(menu): add MenuItem entity and API DTO models
3. feat(storage): implement MenuItems Azure Table repository
4. feat(menu): add CreateMenuItem HTTP function structure
5. feat(validation): add CreateMenuItem input validation
6. feat(menu): persist menu items and prevent duplicates
7. feat(menu): add GetMenuItems retrieval endpoint
8. feat(menu): add GetMenuItemById endpoint
9. feat(menu): add UpdateMenuItem endpoint
10. feat(menu): add DeleteMenuItem endpoint
```

---

## Member 2 - Menu Test Automation and Integration

**Student:** ST10472838 Kaden Remley  
**GitHub:** `Jason4x`

Member 1 (Mohammed Moosa) implemented the CoffeeNChill menu CRUD
endpoints, storage layer, and DTOs. Member 2's contribution builds on
top of that implementation rather than duplicating it: automated
testing, validation/edge-case coverage, integration verification, and
documentation for the menu module.

Completed work (Commits 1-6 on `feature/member2-menu-crud`):

- Reviewed Member 1's menu implementation (see
  `docs/member2/code-review-member1.md`), including discovering and
  documenting the actual `GetMenuItemById` route
  (`/api/menu/item/{category}/{id}`) during integration testing
- Built a single, self-contained Postman collection
  (`CLDV6212 CoffeeNChill - Part 1`) covering the full menu lifecycle:
  create, read, category filter, update, delete, and 9 validation /
  edge-case scenarios (empty fields, boundary values, malformed JSON,
  duplicate detection, missing resources)
- Added a paired `CLDV6212 Local` Postman environment (`baseUrl`
  variable) rather than hardcoding URLs
- Made the full suite Collection-Runner compatible: test data is
  generated per run (timestamp-based IDs) so the entire collection can
  be re-run against the same table with no manual setup or cleanup
- Verified the complete suite end-to-end via Collection Runner:
  48/48 assertions passed, 0 errors, 0 failures
- Documented all menu endpoints (request/response examples, shared
  error shape) in this README
- Added Harvard Anglia references for testing/tooling documentation
  actually used, under this section in `REFERENCES.md`

Evidence (screenshots, exported collection/environment JSON, and
detailed testing notes) is stored under `docs/member2/` and
`docs/postman/`.

### Member 2 Branch

```text
feature/member2-menu-crud
```

The branch name is retained for the existing project workflow even
though the CRUD implementation itself was completed by Member 1;
Member 2's commits on this branch are testing, validation, and
documentation work only, kept clearly separate from Member 1's
implementation commits.

---

## Member 3 - Azure File Share and Staff Documents

**Student:** ST10447147 Arren Naicker  
**GitHub:** `ItzArren`

Responsibilities:

- Azure Storage Account/File Share configuration
- `staff-docs` File Share
- `POST /api/documents/upload`
- `GET /api/documents`
- `GET /api/documents/download/{fileName}`
- Stream-based file transfers
- MIME validation
- File-size validation
- File metadata
- Error logging
- Assigned Postman tests
- README contribution
- Code review
- Video contribution

---

## Member 4 - Docker and Integration

**Student:** ST10445189 Kieran Pillay  
**GitHub:** `ItzKirxn`

Responsibilities:

- Functions Dockerfile
- Multi-stage Docker build
- Docker image configuration
- Docker Hub repository
- `coffeenchill-functions:v1.0`
- Standalone container execution
- Environment-variable configuration
- Integration testing
- Docker documentation
- Final combined Postman verification
- README contribution
- Code review
- Video contribution

---

# Pull Request and Code Review Process

Before a feature is merged:

1. The member must complete the assigned functionality.
2. The project must compile.
3. Relevant tests must pass.
4. The member must push the feature branch.
5. A Pull Request must be opened.
6. Another group member must review the code.
7. Any identified issues must be resolved.
8. CI checks should pass.
9. The Pull Request can then be merged.

---

# Definition of Done

A feature is only considered complete when:

- Code compiles
- Required functionality works
- Validation works
- Appropriate HTTP status codes are returned
- Errors are handled
- Logging is present where appropriate
- Relevant Postman tests exist
- Automated tests pass
- README documentation is updated
- No secrets are committed
- Meaningful commits exist
- Another member has reviewed the change

---

# Documentation

Additional project documentation will be stored under:

```text
docs/
```

This includes:

```text
docs/
├── architecture/
├── postman/
├── setup/
└── screenshots/
```

---

# References

Academic and technical references used by the project are stored in:

```text
REFERENCES.md
```

References should follow the Harvard Anglia referencing style required by the module.

---

# Video Demonstration

Part 1 requires an unlisted YouTube video demonstration.

The video will demonstrate:

- Project architecture
- GitHub repository
- Individual contributions
- Running Azurite
- Running the Azure Functions application
- Azure Table Storage functionality
- Azure File Share functionality
- Menu CRUD operations
- Staff document operations
- Validation and error handling
- Postman Collection Runner
- Docker containers
- Docker Hub image
- CI pipeline
- Important design decisions

AI-generated voices will not be used.

**Part 1 YouTube Video:** To be added

---

# Docker Hub

**CoffeeNChill Functions Image:** To be added

Expected image format:

```text
<dockerhub-username>/coffeenchill-functions:v1.0
```

---

# AI Tool Usage Disclosure

AI-assisted tools may be used during development for activities such as:

- Planning
- Debugging assistance
- Code review
- Documentation assistance
- Proofreading
- Research support

All submitted work must be reviewed, understood, tested, and verified by the group members.

AI-generated code must not be submitted without the group's own analysis, understanding, testing, and implementation decisions.

Any AI use relevant to the assessment will be disclosed as required by the module assessment instructions.

---

# Part 1 Submission Checklist

Before submission, the group must confirm:

- [ ] Public GitHub repository is accessible
- [ ] All four members have accepted repository collaboration access
- [ ] Every member has contributed using their own GitHub account
- [ ] Every member has at least five meaningful commits
- [ ] Commit activity is spread throughout development
- [ ] README is complete
- [ ] Every member has contributed to the README
- [ ] `/docs` folder is complete
- [ ] No credentials or secrets are committed
- [ ] `MenuItems` Table Storage works
- [ ] All five menu endpoints work
- [ ] `staff-docs` Azure File Share works
- [ ] All three document endpoints work
- [ ] Validation and error handling work
- [ ] Postman collection includes all endpoints
- [ ] Postman environment uses `{{baseUrl}}`
- [ ] Automated Postman tests pass
- [ ] Collection Runner passes
- [ ] Dockerfile builds successfully
- [ ] Docker Hub image is public
- [ ] Docker image uses the `v1.0` tag
- [ ] Standalone `docker run` execution works
- [ ] CircleCI build succeeds
- [ ] Unlisted YouTube demonstration is complete
- [ ] YouTube link is included in this README
- [ ] Docker Hub link is included in this README
- [ ] Final application compiles and runs

---

# Future POE Development

## Part 2

Part 2 will extend CoffeeNChill with:

- Azure Queue Storage
- Asynchronous order processing
- Queue-triggered Azure Functions
- Orders Azure Table
- Order status lifecycle
- Docker Compose
- Docker Hub `v2.0`
- Updated Postman tests
- Updated documentation

## Part 3 / Final POE

The final POE will introduce:

- ASP.NET Core Web API Gateway
- Swagger / OpenAPI
- User profiles
- Azure Table Storage authentication data
- Password hashing
- JWT Bearer authentication
- Role-based authorization
- Linux VPS deployment
- Production Docker Compose
- Updated Postman authentication tests
- Optional automated CI/CD deployment pipeline

---

# Current Status

**Part 1:** In Development  
**Member 1 Menu Work:** Completed  
**Member 2:** Testing, validation coverage, Collection Runner suite, and documentation completed (Commits 1-6 on `feature/member2-menu-crud`)  
**Part 2:** Not Started  
**Part 3:** Not Started  

Last updated: 1 September 2026
