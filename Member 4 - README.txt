# CoffeeNChill Canteen Management System

## CLDV6212 - Cloud Development B

**Assessment:** Portfolio of Evidence (POE)
**Project:** CoffeeNChill Canteen Management System
**Part:** Part 1
**Part 1 Due Date:** 14 September 2026

---

# Project Overview

CoffeeNChill is a cloud-enabled canteen management system developed for the **CLDV6212 Cloud Development B Portfolio of Evidence (POE)**.

The system is developed incrementally across three parts and demonstrates the use of cloud development technologies, including:

* Azure Storage
* Azure Functions
* Azure Table Storage
* Azure File Share
* Azurite
* Docker
* Docker Hub
* REST APIs
* Postman
* GitHub
* Feature branches
* CircleCI
* Continuous Integration
* Technical documentation
* Cloud deployment

## Part 1

Part 1 focuses on implementing cloud-based storage and serverless functionality for the CoffeeNChill canteen.

The main technologies used are:

* Azure Functions
* Azure Table Storage
* Azure Files / Azure File Share
* Azurite local storage emulation
* Docker containerisation
* Docker Hub
* Postman API testing
* Postman Collection Runner
* GitHub version control
* GitHub feature branches
* CircleCI continuous integration
* Technical documentation
* Video demonstration

The purpose of Part 1 is to replace CoffeeNChill's paper-based menu and staff-document processes with cloud-based storage.

The CoffeeNChill menu is stored in an Azure Table Storage table called **`MenuItems`**, while staff operational documents are stored in an Azure File Share called **`staff-docs`**.

---

# Part 1 Architecture

The Part 1 solution follows a layered architecture so that HTTP Functions, application models, validation, and Azure Storage operations are separated.

```text
                        ┌─────────────────────┐
                        │       Postman       │
                        │   Collection Runner │
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

Azure Table Storage is a NoSQL storage service that uses a **PartitionKey** and **RowKey** to uniquely identify entities.

Each menu item contains:

| Property       | Description                             |
| -------------- | --------------------------------------- |
| `PartitionKey` | Menu category                           |
| `RowKey`       | Unique menu item SKU / ID               |
| `Name`         | Menu item name                          |
| `Description`  | Menu item description                   |
| `Price`        | Menu item price                         |
| `IsAvailable`  | Whether the item is currently available |

### Example Menu Item

```text
PartitionKey: Hot Drinks
RowKey: COF-001
Name: Espresso
Description: Single-shot espresso
Price: 28.00
IsAvailable: true
```

## PartitionKey Design

The menu **Category** is used as the `PartitionKey`.

Example categories include:

* Hot Drinks
* Cold Drinks
* Pastries
* Sandwiches

This allows menu items belonging to the same category to be grouped and queried efficiently.

## RowKey Design

The menu item's SKU or ID is used as the `RowKey`.

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

For example:

```text
Hot Drinks + COF-001
```

---

# staff-docs - Azure File Share

The `staff-docs` Azure File Share stores operational documents used by CoffeeNChill staff.

Examples include:

* Barista recipe sheets
* Equipment cleaning manuals
* Health and safety policies
* Operational procedures
* Other staff documentation

Azure Files provides a centralised location where staff documents can be stored and accessed when required.

> **Important:** Azurite does not emulate Azure File Shares. Therefore, the `staff-docs` component uses an actual Microsoft Azure Storage Account and Azure File Share.

Sensitive Azure File Share credentials must never be committed to GitHub.

---

# Part 1 API Endpoints

## Menu Endpoints

| Method   | Endpoint                        | Description                                              | Owner    |
| -------- | ------------------------------- | -------------------------------------------------------- | -------- |
| `POST`   | `/api/menu`                     | Create a new menu item                                   | Member 1 |
| `GET`    | `/api/menu`                     | Retrieve all menu items                                  | Member 1 |
| `GET`    | `/api/menu?category={category}` | Retrieve menu items by category using a query string     | Member 1 |
| `GET`    | `/api/menu/category/{category}` | Retrieve menu items by category using the required route | Member 2 |
| `GET`    | `/api/menu/{category}/{id}`     | Retrieve one menu item by category and ID/SKU            | Member 1 |
| `PUT`    | `/api/menu/{category}/{id}`     | Update a menu item's price or availability               | Member 2 |
| `DELETE` | `/api/menu/{category}/{id}`     | Delete a menu item                                       | Member 2 |

### Category Filtering

The `GetMenuItems` endpoint supports:

```text
GET /api/menu?category={category}
```

as a convenience filter.

The assignment brief specifically requires:

```text
GET /api/menu/category/{category}
```

Both routes exist. The dedicated category route is the one demonstrated in testing and the video because it directly satisfies the assignment requirement.

---

# API Error Response

All menu endpoints use a consistent JSON error-response structure when an operation fails.

```json
{
  "error": "VALIDATION_ERROR",
  "message": "Category is required."
}
```

## Error Codes

| Error Code              | HTTP Status | Meaning                                              |
| ----------------------- | ----------: | ---------------------------------------------------- |
| `VALIDATION_ERROR`      |       `400` | Request was malformed or failed a business rule      |
| `INVALID_JSON`          |       `400` | Request body could not be parsed as JSON             |
| `DUPLICATE_MENU_ITEM`   |       `409` | An item with the same category and ID already exists |
| `MENU_ITEM_NOT_FOUND`   |       `404` | No item exists at the specified category and ID      |
| `INTERNAL_SERVER_ERROR` |       `500` | Unexpected application or storage failure            |

---

# Document Endpoints

| Method | Endpoint                             | Description               |
| ------ | ------------------------------------ | ------------------------- |
| `POST` | `/api/documents/upload`              | Upload a staff document   |
| `GET`  | `/api/documents`                     | List all staff documents  |
| `GET`  | `/api/documents/download/{fileName}` | Download a staff document |

---

# Group Members

| Member   | GitHub                    | Student                   | Part 1 Responsibility                                                                                    |
| -------- | ------------------------- | ------------------------- | -------------------------------------------------------------------------------------------------------- |
| Member 1 | `ST10472990MohammedMoosa` | ST10472990 Mohammed Moosa | Project foundation, MenuItems storage, full menu CRUD, validation, Postman evidence and documentation    |
| Member 2 | `Jason4x`                 | ST10472838 Kaden Remley   | Menu test automation, Postman Collection Runner, integration verification, documentation and code review |
| Member 3 | `ItzArren`                | ST10447147 Arren Naicker  | Azure File Share and staff document functionality                                                        |
| Member 4 | `ItzKirxn`                | ST10445189 Kieran Pillay  | Docker, Docker Hub and integration                                                                       |

---

# Group Contribution Requirements

Each group member is required to contribute to:

* Source code
* GitHub commit history
* README documentation
* Postman testing
* Pull Request review
* Code review
* Integration testing
* Video demonstration

Each group member must make a minimum of **five meaningful commits per part** using their own GitHub account.

## Meaningful Commit Examples

Avoid generic messages such as:

```text
fix
update
test
done
final
```

Instead, descriptive commit messages should be used, such as:

```text
feat(menu): implement CreateMenuItem endpoint
feat(storage): add MenuItems Table Storage repository
fix(menu): validate negative menu prices
test(postman): add menu validation tests
docs(readme): document Azurite startup process
```

---

# Repository Structure

The repository is organised around the following structure:

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
│       │
│       └── CoffeeNChill.slnx
│
├── docs/
│   ├── architecture/
│   ├── member1/
│   ├── member2/
│   ├── postman/
│   ├── setup/
│   └── screenshots/
│
├── .circleci/
│   └── config.yml
│
├── README.md
├── REFERENCES.md
└── .gitignore
```

Additional folders may be added as the project develops.

---

# GitHub Repository

The official project repository is:

**Repository:**
https://github.com/ST10472990MohammedMoosa/CLDV6212-POE

The repository is public so that the lecturer/marker can review:

* Application source code
* Documentation
* Commit history
* Feature branches
* Pull Requests
* Individual contributions
* Testing evidence

---

# Git Workflow

The project uses GitHub feature branches.

Normal development work should **not** be performed directly on `main`.

## Part 1 Feature Branches

```text
feature/member1-menu-foundation
feature/member2-menu-crud
feature/member3-staff-docs
feature/member4-docker
```

Each feature branch must contain the project README.

Each member must also contribute to the README using their own GitHub account.

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

The expected development workflow is:

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
chore(project): initialise CoffeeNChill Azure Functions solution

feat(menu): add MenuItem entity and API DTO models

feat(storage): implement MenuItems Azure Table repository

feat(menu): implement CreateMenuItem endpoint

fix(menu): improve invalid price validation

test(postman): add automated menu endpoint tests

docs(readme): add local Azurite setup instructions
```

---

# Local Development Requirements

Before running the application locally, ensure the following software is installed:

* Git
* Visual Studio Code
* .NET SDK required by the module
* Azure Functions development tools
* Docker Desktop
* WSL 2 where required
* Postman

---

# Azurite Setup

Azurite is used to emulate Azure Storage services locally.

## Pull the Azurite Image

```bash
docker pull mcr.microsoft.com/azure-storage/azurite
```

## Run Azurite

```bash
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
```

## Default Azurite Ports

| Service       |    Port |
| ------------- | ------: |
| Blob Storage  | `10000` |
| Queue Storage | `10001` |
| Table Storage | `10002` |

Part 1 primarily uses the **Table Storage** service.

---

# Local Storage Configuration

When Azurite is running on its default ports, the local Azure Storage configuration can use:

```text
AzureWebJobsStorage=UseDevelopmentStorage=true
```

For Azure Functions, the setting is stored in:

```text
local.settings.json
```

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

> **Important:** `local.settings.json` must not be committed to GitHub.

---

# Azure File Share Configuration

The `staff-docs` File Share uses a real Azure Storage Account.

The Azure File Share connection string must be stored locally or passed through environment variables.

Example:

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

The placeholder must be replaced with the actual connection string locally.

> **Never commit the real Azure credential to GitHub.**

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

Menu endpoints validate incoming data before processing requests.

Examples of invalid requests include:

* Missing category
* Missing menu item ID/SKU
* Missing item name
* Invalid JSON body
* Negative price
* Invalid field values
* Duplicate menu item
* Missing requested menu item

Appropriate HTTP response codes are returned.

| Scenario                       |                 HTTP Status |
| ------------------------------ | --------------------------: |
| Menu item successfully created |               `201 Created` |
| Successful GET request         |                    `200 OK` |
| Invalid request                |           `400 Bad Request` |
| Missing resource               |             `404 Not Found` |
| Duplicate item                 |              `409 Conflict` |
| Unexpected server error        | `500 Internal Server Error` |

---

# Member 1 - Project Foundation and Menu CRUD

**Student:** ST10472990 Mohammed Moosa
**GitHub:** `ST10472990MohammedMoosa`

## Completed Responsibilities

* Repository and project foundation
* Azure Functions solution structure
* Azurite Table Storage configuration
* `MenuItemEntity` Azure Table entity
* Menu request/response DTO architecture
* `IMenuItemRepository`
* Azure Table Storage repository implementation
* `POST /api/menu`
* `GET /api/menu`
* `GET /api/menu?category={category}`
* `GET /api/menu/{category}/{id}`
* `PUT /api/menu/{category}/{id}`
* `DELETE /api/menu/{category}/{id}`
* Request validation
* Duplicate-item handling
* Missing-resource handling
* Structured API error responses
* Logging
* Postman manual testing
* Member 1 screenshot evidence
* README contribution
* References documentation
* GitHub feature branch development

## Member 1 Branch

```text
feature/member1-menu-foundation
```

## Member 1 Commit Sequence

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

# Member 2 - Menu Test Automation and Integration

**Student:** ST10472838 Kaden Remley
**GitHub:** `Jason4x`

Member 1 implemented the CoffeeNChill menu CRUD endpoints, storage layer and DTOs.

Member 2's contribution builds on that implementation rather than duplicating it. The focus is on automated testing, validation and edge-case coverage, integration verification, and documentation.

## Completed Work

* Reviewed Member 1's menu implementation
* Documented the code review in `docs/member2/code-review-member1.md`
* Discovered and documented the actual `GetMenuItemById` route during integration testing:

  ```text
  /api/menu/item/{category}/{id}
  ```
* Built a single self-contained Postman collection
* Added full menu lifecycle testing
* Added validation and edge-case scenarios
* Added paired `CLDV6212 Local` Postman environment
* Used the `baseUrl` variable instead of hardcoded URLs
* Made the collection Collection-Runner compatible
* Generated timestamp-based test IDs
* Verified the complete suite end-to-end
* Achieved:

  ```text
  48/48 assertions passed
  0 errors
  0 failures
  ```
* Documented all menu endpoints in the README
* Added Harvard Anglia references to `REFERENCES.md`
* Added screenshots and testing evidence
* Added exported Postman collection/environment files

## Member 2 Branch

```text
feature/member2-menu-crud
```

The branch name is retained for the existing project workflow even though the CRUD implementation itself was completed by Member 1.

Member 2's commits focus on testing, validation, integration and documentation.

---

# Member 3 - Azure File Share and Staff Documents

**Student:** ST10447147 Arren Naicker
**GitHub:** `ItzArren`

## Completed Responsibilities

* Azure Storage Account/File Share configuration
* `staff-docs` File Share
* `POST /api/documents/upload`
* `GET /api/documents`
* `GET /api/documents/download/{fileName}`
* Stream-based file transfers
* MIME validation
* File-size validation
* File metadata
* Error logging
* Assigned Postman tests
* README contribution
* Code review
* Video contribution

## Member 3 Branch

```text
feature/member3-staff-docs
```

## Member 3 Commit Sequence

```text
1. feat(documents): add staff document service abstraction
2. feat(documents): implement streamed document upload
3. feat(documents): implement document metadata listing
4. feat(documents): implement streamed document download
5. feat(validation): add document MIME and filename validation
6. test(postman): add document workflow tests
7. docs(readme): document staff-docs setup and endpoints
```

---

# Member 4 - Docker and Integration

**Student:** ST10445189 Kieran Pillay
**GitHub:** `ItzKirxn`

## Responsibilities

Member 4 is responsible for:

* Docker containerisation
* Docker Hub
* Container configuration
* Integration testing
* Environment-variable configuration
* Docker documentation
* Final combined Postman verification
* README contribution

## Completed Responsibilities

* Created multi-stage Dockerfile for Azure Functions
* Configured container build and runtime environment
* Built the CoffeeNChill Functions image
* Published `coffeenchill-functions:v1.0` to Docker Hub
* Verified standalone container execution
* Tested environment-variable configuration
* Performed menu integration testing
* Tested `GET /api/menu`
* Tested `POST /api/menu`
* Created Docker network
* Connected Azurite to the Docker network
* Added Docker documentation
* Added Postman collection and environment files

## Member 4 Branch

```text
feature/member4-docker
```

---

# Docker - Part 1

Part 1 uses **standalone Docker containers**.

Docker Compose is **not used for Part 1**.

Docker Compose will be introduced in Part 2.

The Functions application uses a Dockerfile and is published to Docker Hub using semantic versioning.

Example:

```text
<dockerhub-username>/coffeenchill-functions:v1.0
```

The final Part 1 demonstration must show the containers being started using `docker run`.

---

# 🛠️ Docker Commands

## Build the Docker Image

```bash
docker build -t kirxn19/coffeenchill-functions:v1.0 .
```

## Create the Docker Network

```bash
docker network create cldv6212-network
```

## Run Azurite

```bash
docker run -d --name cldv6212-azurite -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
```

## Connect Azurite to the Network

```bash
docker network connect cldv6212-network cldv6212-azurite
```

## Run the Functions Container

```bash
docker run -d --name coffeechill-function -p 7071:80 -e "AzureWebJobsStorage=DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://cldv6212-azurite:10000/devstoreaccount1;QueueEndpoint=http://cldv6212-azurite:10001/devstoreaccount1;TableEndpoint=http://cldv6212-azurite:10002/devstoreaccount1;" --network cldv6212-network coffeechill-function:latest
```

## Push the Image to Docker Hub

```bash
docker push kirxn19/coffeenchill-functions:v1.0
```

---

# Docker Hub

## CoffeeNChill Functions Image

**Image:**

```text
kirxn19/coffeenchill-functions:v1.0
```

**Docker Hub:**

https://hub.docker.com/r/kirxn19/coffeenchill-functions

The Docker image is published using the `v1.0` semantic version tag for Part 1.

---

# Part 1 Testing Requirements

Before Part 1 is considered complete, the group must verify the following:

* [ ] Azure Functions project builds successfully
* [ ] Azurite starts successfully
* [ ] `MenuItems` table can be created
* [ ] Menu items can be inserted
* [ ] All menu items can be retrieved
* [ ] Menu items can be filtered by category
* [ ] Individual menu items can be retrieved
* [ ] Menu items can be updated
* [ ] Menu items can be deleted
* [ ] Invalid menu input is handled
* [ ] Missing menu items return appropriate responses
* [ ] Staff documents can be uploaded
* [ ] Staff documents can be listed
* [ ] Staff documents can be downloaded
* [ ] Invalid files are rejected
* [ ] Missing files are handled
* [ ] Postman automated tests pass
* [ ] Postman Collection Runner passes
* [ ] Docker image builds successfully
* [ ] Docker image is available on Docker Hub
* [ ] Standalone Docker containers run successfully
* [ ] CircleCI pipeline passes
* [ ] README instructions are accurate
* [ ] All members have sufficient meaningful GitHub commits

---

# Document Validation

Staff document endpoints validate uploaded files.

Validation includes:

* File existence
* File name validation
* Allowed MIME type
* Allowed file extension
* Maximum file size
* Safe file names
* Missing file handling

For stronger implementation quality, file transfers use streams instead of unnecessarily loading entire files into memory.

Document listing provides useful metadata such as:

* File name
* File size
* Last modified/upload date

---

# Postman Testing

Postman is used to test all Part 1 API endpoints.

One shared Postman Collection contains the CoffeeNChill Part 1 requests.

## Collection Structure

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

The Postman environment is named:

```text
CLDV6212 Local
```

It contains:

```text
baseUrl = http://localhost:7071/api
```

Requests should use:

```text
{{baseUrl}}
```

For example:

```text
{{baseUrl}}/menu
```

Hardcoded URLs should not be used throughout the collection.

---

# Automated Postman Tests

Each Postman request contains saved automated assertions.

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

The complete collection can be executed using the **Postman Collection Runner** so that the tests can be executed together.

The exported collection and environment are stored under:

```text
docs/postman/
```

---

# CircleCI Continuous Integration

CircleCI is used to provide continuous integration for the project.

The initial Part 1 pipeline performs:

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

The pipeline helps identify build or test failures before Pull Requests are merged.

The CircleCI configuration is stored in:

```text
.circleci/config.yml
```

The CI/CD process may be expanded during later POE parts.

---

# Member 1 Postman Testing Evidence

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

These tests verify:

* Request validation
* Successful creation
* Duplicate prevention
* Retrieval
* Category filtering
* Retrieval by ID
* Missing-resource handling
* Updates
* Deletion
* Repeated deletion handling

---

# Pull Request and Code Review Process

Before a feature is merged:

1. The member completes the assigned functionality.
2. The project must compile.
3. Relevant tests must pass.
4. The member pushes the feature branch.
5. A Pull Request is opened.
6. Another group member reviews the code.
7. Identified issues are resolved.
8. CI checks should pass.
9. The Pull Request can then be merged into `main`.

---

# Definition of Done

A feature is considered complete when:

* [ ] Code compiles
* [ ] Required functionality works
* [ ] Validation works
* [ ] Appropriate HTTP status codes are returned
* [ ] Errors are handled
* [ ] Logging is present where appropriate
* [ ] Relevant Postman tests exist
* [ ] Automated tests pass
* [ ] README documentation is updated
* [ ] No secrets are committed
* [ ] Meaningful commits exist
* [ ] Another member has reviewed the change

---

# Documentation

Additional project documentation is stored under:

```text
docs/
```

The documentation includes:

```text
docs/
├── architecture/
├── postman/
├── setup/
└── screenshots/
```

Documentation may include:

* Architecture diagrams
* Postman testing evidence
* Setup instructions
* Screenshots
* Code review evidence
* Integration testing evidence

---

# References

Academic and technical references used by the project are stored in:

```text
REFERENCES.md
```

References follow the **Harvard Anglia referencing style** required by the module.

---

# Video Demonstration

Part 1 requires an **unlisted YouTube video demonstration**.

The video will demonstrate:

* Project architecture
* GitHub repository
* Individual contributions
* Running Azurite
* Running the Azure Functions application
* Azure Table Storage functionality
* Azure File Share functionality
* Menu CRUD operations
* Staff document operations
* Validation and error handling
* Postman Collection Runner
* Docker containers
* Docker Hub image
* CircleCI pipeline
* Important design decisions

> **AI-generated voices will not be used.**

## Part 1 YouTube Video

**To be added**

---

# Docker Hub

**CoffeeNChill Functions Image:**

https://hub.docker.com/r/kirxn19/coffeenchill-functions

---

# AI Tool Usage Disclosure

AI-assisted tools may be used during development for activities such as:

* Planning
* Debugging assistance
* Code review
* Documentation assistance
* Proofreading
* Research support

All submitted work must be reviewed, understood, tested and verified by the group members.

AI-generated code must not be submitted without the group's own analysis, understanding, testing and implementation decisions.

Any AI use relevant to the assessment will be disclosed as required by the module assessment instructions.

---

# Part 1 Submission Checklist

Before submission, the group must confirm:

* [ ] Public GitHub repository is accessible
* [ ] All four members have accepted repository collaboration access
* [ ] Every member has contributed using their own GitHub account
* [ ] Every member has at least five meaningful commits
* [ ] Commit activity is spread throughout development
* [ ] README is complete
* [ ] Every member has contributed to the README
* [ ] `/docs` folder is complete
* [ ] No credentials or secrets are committed
* [ ] `MenuItems` Table Storage works
* [ ] All menu endpoints work
* [ ] `staff-docs` Azure File Share works
* [ ] All three document endpoints work
* [ ] Validation and error handling work
* [ ] Postman collection includes all endpoints
* [ ] Postman environment uses `{{baseUrl}}`
* [ ] Automated Postman tests pass
* [ ] Collection Runner passes
* [ ] Dockerfile builds successfully
* [ ] Docker Hub image is public
* [ ] Docker image uses the `v1.0` tag
* [ ] Standalone `docker run` execution works
* [ ] CircleCI build succeeds
* [ ] Unlisted YouTube demonstration is complete
* [ ] YouTube link is included in this README
* [ ] Docker Hub link is included in this README
* [ ] Final application compiles and runs

---

# Future POE Development

## Part 2

Part 2 will extend CoffeeNChill with:

* Azure Queue Storage
* Asynchronous order processing
* Queue-triggered Azure Functions
* Orders Azure Table
* Order status lifecycle
* Docker Compose
* Docker Hub `v2.0`
* Updated Postman tests
* Updated documentation

The Part 2 architecture will introduce Docker Compose for managing multiple containers.

---

# Part 3 / Final POE

The final POE will introduce:

* ASP.NET Core Web API Gateway
* Swagger / OpenAPI
* User profiles
* Azure Table Storage authentication data
* Password hashing
* JWT Bearer authentication
* Role-based authorization
* Linux VPS deployment
* Production Docker Compose
* Updated Postman authentication tests
* Optional automated CI/CD deployment pipeline

---

# Current Project Status

| Component                                      | Status            |
| ---------------------------------------------- | ----------------- |
| Part 1                                         | 🟡 In Development |
| Member 1 - Menu Work                           | ✅ Completed       |
| Member 2 - Testing, Validation & Documentation | ✅ Completed       |
| Member 3 - Azure File Share & Documents        | ✅ Completed       |
| Member 4 - Docker, Docker Hub & Integration    | ✅ Completed       |
| Part 2                                         | ⏳ Not Started     |
| Part 3                                         | ⏳ Not Started     |

---

# Individual Contributions

## Member 1

**ST10472990 Mohammed Moosa**

Project foundation, Azure Table Storage architecture, MenuItems CRUD, validation, Postman evidence and documentation.

## Member 2

**ST10472838 Kaden Remley**

Menu testing automation, Collection Runner, validation coverage, integration verification, documentation and code review.

## Member 3

**ST10447147 Arren Naicker**

Azure File Share, staff document upload/list/download functionality, validation, testing and documentation.

## Member 4

**ST10445189 Kieran Pillay**

Docker containerisation, Docker Hub, Docker networking, integration testing, Postman integration, Docker documentation and final README contribution.

---

# Important Part 1 Design Decisions

The following design decisions were made during Part 1:

### Azure Table Storage

Azure Table Storage was selected for menu items because the menu data is structured around categories and unique item IDs while not requiring a traditional relational database.

### PartitionKey

The menu category is used as the `PartitionKey` so that items belonging to the same category can be grouped.

### RowKey

The menu SKU/ID is used as the `RowKey` to uniquely identify an item within its category.

### Azure File Share

Azure File Share is used for staff operational documents because it provides centralised cloud-based file storage.

### Azurite

Azurite is used during local development to emulate Azure Storage services without requiring all Table Storage development to take place against a live Azure environment.

### Docker

Docker provides a consistent environment for running the Azure Functions application and allows the application to be tested independently from the developer's local environment.

### Docker Compose

Docker Compose is intentionally not used in Part 1. It will be introduced in Part 2 when the project requires multiple coordinated services.

### Feature Branches

GitHub feature branches allow each member to develop their assigned functionality separately before the work is reviewed and merged.

### Postman Collection Runner

Postman Collection Runner allows the group to execute the API test suite repeatedly and verify that the application continues to behave correctly after integration.

---

# Conclusion

CoffeeNChill Part 1 establishes the foundation for a cloud-enabled canteen management system using Azure Functions, Azure Storage, Docker and API-based communication.

The project demonstrates the group's ability to develop collaboratively using GitHub feature branches, implement cloud storage services, create and test HTTP APIs, containerise an application, perform integration testing and document the development process.

Part 1 provides the foundation for the asynchronous order-processing functionality planned for Part 2 and the authenticated API gateway and production deployment planned for the final POE.

---

**Project:** CoffeeNChill Canteen Management System
**Module:** CLDV6212 - Cloud Development B
**Part:** Part 1
**Due Date:** 14 September 2026
**Last Updated:** 10 September 2026
