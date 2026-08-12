# Playwright C# Automation – nopCommerce

![.NET](https://img.shields.io/badge/.NET-9.0-blueviolet)
![NUnit](https://img.shields.io/badge/TestFramework-NUnit-green)
![Playwright](https://img.shields.io/badge/Playwright-C%23-2ea44f)
![Docker](https://img.shields.io/badge/Docker-Local%20Test%20Environment-2496ED)
![Platform](https://img.shields.io/badge/Platform-nopCommerce-orange)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)
![Status](https://img.shields.io/badge/Status-In%20Progress-lightgrey)

![Profile Views](https://komarev.com/ghpvc/?username=heba-art&color=blue)
![GitHub Repo stars](https://img.shields.io/github/stars/heba-art/playwright-csharp-automation?style=social)
![GitHub forks](https://img.shields.io/github/forks/heba-art/playwright-csharp-automation?style=social)

End-to-end UI test automation project using **Playwright for .NET**, **C#**, and **NUnit**, following the **Page Object Model (POM)** design pattern.

The project automates key user journeys on **nopCommerce**, including registration, login, product search, and shopping cart functionality.

---

## ✅ Current Test Result

The current automated test suite has been successfully verified against a local nopCommerce environment running in Docker.

```text
Test summary: total: 5, failed: 0, succeeded: 5, skipped: 0
```

---

## 🧪 Why a Local Docker Environment?

The project originally ran against:

```text
https://demo.nopcommerce.com
```

During automation testing, the public demo website started displaying:

```text
Cloudflare – Verify you are human
```

This blocked Playwright from reaching the nopCommerce application and caused all existing tests to fail.

The test code itself was not the problem.

To create a more stable and controlled automation environment, nopCommerce and SQL Server were run locally using Docker.

### Before

```text
Playwright
    |
    v
demo.nopcommerce.com
    |
    v
Cloudflare
    |
    X
Tests blocked
```

### Now

```text
Playwright + NUnit
       |
       v
http://localhost:8080
       |
       v
Docker
       |
       +---- nopcommerce-local
       |
       +---- nopcommerce-sql
```

This provides a stable local environment without depending on external anti-bot protection.

---

## 📂 Project Structure

```text
PlaywrightTests/
│
├── Pages/
│   ├── CartPage.cs
│   ├── HomePage.cs
│   ├── LoginPage.cs
│   ├── ProductPage.cs
│   ├── RegisterPage.cs
│   └── SearchResultsPage.cs
│
├── Tests/
│   │
│   ├── Auth/
│   │   └── RegisterAndLoginTests.cs
│   │
│   └── Catalog/
│       ├── CartTests.cs
│       └── SearchTests.cs
│
├── Utils/
│   ├── CredentialStore.cs
│   ├── Faker.cs
│   └── TestBase.cs
│
├── appsettings.json
└── PlaywrightTests.csproj
```

### Main Components

**Pages**

Contains the Page Object Model classes used to separate page locators and actions from the test logic.

**Tests**

Contains the NUnit automated test cases organised by feature.

**Utils**

Contains reusable test utilities such as browser setup, configuration, test data generation, credentials, tracing, and cleanup.

**appsettings.json**

Contains the default test configuration such as:

- Base URL
- Browser
- Headless mode
- Viewport size

---

## ✅ Implemented Test Cases

| ID | Test Case | Type | Status |
|---|---|---|---|
| TC-001 | User Registration – Happy Path | Smoke / Functional | ✅ Passed |
| TC-002 | Login with Valid Credentials | Smoke | ✅ Passed |
| TC-003 | Login with Invalid Password | Negative | ✅ Passed |
| TC-004 | Search Product – Exact Match | Functional | ✅ Passed |
| TC-005 | Add to Cart from Product Page | Smoke / Functional | ✅ Passed |

---

## 🚧 Planned Test Cases

| ID | Test Case | Status |
|---|---|---|
| TC-006 | Update Quantity in Cart | 🟡 Planned |
| TC-007 | Remove Item from Cart | 🟡 Planned |
| TC-008 | Wishlist to Cart Flow | 🟡 Planned |
| TC-009 | Checkout as Guest – Valid Flow | 🟡 Planned |
| TC-010 | Sort Products – Price Low → High | 🟡 Planned |

---

## 🛠️ Technologies Used

- C#
- .NET 9
- Microsoft Playwright for .NET
- NUnit
- Page Object Model (POM)
- Docker Desktop
- SQL Server 2022
- Visual Studio 2022
- Git
- GitHub
- GitHub Actions

---

# 🐳 Local Test Environment with Docker

## Prerequisites

Install:

- Docker Desktop
- WSL 2 on Windows
- .NET 9 SDK
- Visual Studio 2022

Make sure Docker Desktop shows:

```text
Engine running
```

---

## 1. Pull the nopCommerce Docker Image

```powershell
docker pull nopcommerceteam/nopcommerce:latest
```

---

## 2. Start nopCommerce

```powershell
docker run -d --name nopcommerce-local -p 8080:80 nopcommerceteam/nopcommerce:latest
```

The application will be available at:

```text
http://localhost:8080
```

---

## 3. Start SQL Server

Use your own strong SQL Server password.

```powershell
docker run -d --name nopcommerce-sql `
-e "ACCEPT_EULA=Y" `
-e "MSSQL_SA_PASSWORD=<YourStrongPassword>" `
-p 1433:1433 `
mcr.microsoft.com/mssql/server:2022-latest
```

Do not commit real passwords or credentials to GitHub.

---

## 4. Configure nopCommerce

Open:

```text
http://localhost:8080
```

The nopCommerce installation page should appear.

Recommended settings:

```text
Country:
Australia

Create sample data:
Yes

Database:
Microsoft SQL Server

Create database if it doesn't exist:
Yes

Server name:
host.docker.internal,1433

Database name:
nopcommerce

SQL Username:
sa

SQL Password:
<same password used when creating the SQL Server container>
```

### Why Create Sample Data?

The automation tests use sample nopCommerce products such as:

```text
Build your own computer
```

Therefore, **Create sample data** should be selected during installation.

---

## 5. Restart nopCommerce After Installation

nopCommerce may stop automatically after completing installation.

Check the containers:

```powershell
docker ps -a
```

If `nopcommerce-local` shows:

```text
Exited (0)
```

start it again:

```powershell
docker start nopcommerce-local
```

Then open:

```text
http://localhost:8080
```

---

## 6. Check Running Containers

```powershell
docker ps
```

Both containers should be running:

```text
nopcommerce-local
nopcommerce-sql
```

---

# ▶️ Running the Playwright Tests

Navigate to the project:

```powershell
cd C:\playwright-csharp-automation\PlaywrightTests
```

Restore NuGet packages:

```powershell
dotnet restore
```

Install Playwright browsers if required:

```powershell
pwsh bin/Debug/net9.0/playwright.ps1 install
```

---

## Run All Tests

For local Docker testing, set the local nopCommerce URL:

```powershell
$env:BASE_URL="http://localhost:8080"
dotnet test
```

Expected current result:

```text
total: 5
failed: 0
succeeded: 5
```

---

## Run One Test

Example – TC-001 Registration:

```powershell
dotnet test --filter "Name~Register_HappyPath_ShowsSuccessAndLogsUserIn"
```

Expected result:

```text
total: 1
failed: 0
succeeded: 1
```

---

# ⚙️ Configuration

The default configuration is stored in:

```text
PlaywrightTests/appsettings.json
```

Example:

```json
{
  "baseUrl": "https://demo.nopcommerce.com",
  "browser": "chromium",
  "headless": false,
  "viewport": {
    "width": 1366,
    "height": 768
  }
}
```

The project also supports environment variable overrides.

Available variables:

```text
BASE_URL
BROWSER
HEADLESS
PW_TIMEOUT
```

Example:

```powershell
$env:BASE_URL="http://localhost:8080"
$env:HEADLESS="false"

dotnet test
```

Using `BASE_URL` allows the developer to use the local Docker environment without permanently changing the shared configuration.

---

# 🔍 Playwright Test Features

The framework currently includes:

- Page Object Model
- NUnit test organisation
- Browser configuration
- Environment variable configuration
- Dynamic test data generation
- Registration and login flows
- Negative login validation
- Product search
- Product configuration
- Add-to-cart validation
- Price validation
- Cart subtotal validation
- Browser context isolation
- Screenshots on failure
- Playwright tracing
- CI-friendly configuration

---

# ☁️ GitHub Actions / CI Note

A GitHub-hosted runner cannot access:

```text
http://localhost:8080
```

running on a developer's personal computer.

For CI/CD, the automation environment should use either:

1. A dedicated externally accessible test environment, or
2. Docker containers started directly inside the GitHub Actions workflow.

The public:

```text
https://demo.nopcommerce.com
```

website may occasionally display Cloudflare security verification, so it should not be considered a fully controlled automation environment.

---

# 📈 Project Roadmap

Current:

```text
TC-001 → TC-005 ✅
```

Next:

```text
TC-006 – Update Quantity in Cart
TC-007 – Remove Item from Cart
TC-008 – Wishlist to Cart
TC-009 – Guest Checkout
TC-010 – Product Sorting
```

Future improvements may include:

- Cross-browser testing
- Parallel execution
- API testing
- Test categories
- Data-driven testing
- Docker Compose
- Docker-based GitHub Actions environment
- HTML test reporting
- Retry strategy
- Additional regression scenarios

---

## 👩‍💻 Author

**Heba AL-Rubaye**

Professional portfolio project focused on **QA Automation / Test Automation Engineering**.

The project demonstrates practical experience with:

**C# · Playwright · NUnit · Page Object Model · Docker · SQL Server · GitHub Actions**