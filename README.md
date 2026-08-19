# Playwright C# Automation – nopCommerce

![.NET](https://img.shields.io/badge/.NET-9.0-blueviolet)
![NUnit](https://img.shields.io/badge/TestFramework-NUnit-green)
![Playwright](https://img.shields.io/badge/Playwright-C%23-2ea44f)
![Docker](https://img.shields.io/badge/Docker-Local%20Test%20Environment-2496ED)
![MCP](https://img.shields.io/badge/Playwright-MCP-purple)
![Platform](https://img.shields.io/badge/Platform-nopCommerce-orange)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)
![Tests](https://img.shields.io/badge/Tests-10%2F10%20Passing-brightgreen)

End-to-end UI test automation project using **Playwright for .NET**, **C#**, and **NUnit**, following the **Page Object Model (POM)** design pattern.

The project automates realistic nopCommerce user journeys including registration, login, product search, cart operations, wishlist, guest checkout, and product sorting.

It also demonstrates **AI-assisted test exploration using Microsoft Playwright MCP**.

---

## ✅ Current Test Result

The complete automated regression suite has been successfully verified against a local nopCommerce environment running in Docker.

```text
Test summary:
total: 10
failed: 0
succeeded: 10
skipped: 0
```

**Current regression status: 10 / 10 tests passing ✅**

---

## 🧪 Why a Local Docker Environment?

The project originally ran against:

```text
https://demo.nopcommerce.com
```

During automation testing, the public demo website began displaying Cloudflare security verification:

```text
Verify you are human
```

This prevented Playwright from reliably reaching the application and caused previously working automated tests to fail.

The test code itself was not the problem.

To create a stable and controlled automation environment, nopCommerce and SQL Server are now run locally using Docker.

### Before

```text
Playwright
    ↓
demo.nopcommerce.com
    ↓
Cloudflare
    ↓
Tests blocked
```

### Current Environment

```text
Playwright + NUnit
        ↓
http://localhost:8080
        ↓
Docker
   ┌────┴─────┐
   ↓          ↓
nopCommerce  SQL Server 2022
```

This gives the automation suite a controlled test environment without depending on public-site anti-bot protection.

---

## 📂 Project Structure

```text
PlaywrightTests/
│
├── Pages/
│   ├── CartPage.cs
│   ├── CheckoutPage.cs
│   ├── HomePage.cs
│   ├── LoginPage.cs
│   ├── ProductPage.cs
│   ├── RegisterPage.cs
│   ├── SearchResultsPage.cs
│   └── WishlistPage.cs
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

Contains Page Object Model classes that separate page locators and reusable actions from test logic.

**Tests**

Contains NUnit automated test cases organised by feature.

**Utils**

Contains reusable framework utilities including browser setup, environment configuration, dynamic test data, credentials, tracing, screenshots, and cleanup.

---

# ✅ Automated Test Coverage

| ID | Test Case | Type | Status |
|---|---|---|---|
| TC-001 | User Registration – Happy Path | Smoke / Functional | ✅ Passed |
| TC-002 | Login with Valid Credentials | Smoke | ✅ Passed |
| TC-003 | Login with Invalid Password | Negative | ✅ Passed |
| TC-004 | Search Product – Exact Match | Functional | ✅ Passed |
| TC-005 | Add to Cart from Product Page | Smoke / Functional | ✅ Passed |
| TC-006 | Update Quantity in Cart | Functional | ✅ Passed |
| TC-007 | Remove Item from Cart | Functional | ✅ Passed |
| TC-008 | Wishlist to Cart Flow | End-to-End | ✅ Passed |
| TC-009 | Checkout as Guest – Valid Flow | End-to-End | ✅ Passed |
| TC-010 | Sort Products – Price Low → High | Functional | ✅ Passed |

---

# 🤖 AI-Assisted Testing with Playwright MCP

Microsoft **Playwright MCP** was introduced into the project to support AI-assisted test exploration.

Rather than using MCP simply to generate test code, it was used as an **exploration and debugging assistant**.

A practical example was **TC-009 – Checkout as Guest**.

The guest checkout contains multiple stages:

```text
Product
    ↓
Shopping Cart
    ↓
Accept Terms
    ↓
Checkout
    ↓
Checkout as Guest
    ↓
Billing Address
    ↓
Shipping Method
    ↓
Payment Method
    ↓
Payment Information
    ↓
Confirm Order
    ↓
Order Success
```

Playwright MCP was used to:

- Explore the complete checkout workflow in the live local application
- Capture fresh accessibility snapshots as application state changed
- Identify accessible role- and label-based locators
- Discover required billing, shipping, payment and confirmation controls
- Validate the actual successful checkout path
- Assist with debugging locator issues

During this exploration, MCP also revealed that the existing Add to Cart locator was tied to one specific product ID.

The original locator:

```csharp
_page.Locator("button#add-to-cart-button-1");
```

was refactored to a reusable accessible locator:

```csharp
_page.GetByRole(
    AriaRole.Button,
    new() { Name = "Add to cart", Exact = true }).First;
```

This allows the Page Object to support different products without depending on product-specific element IDs.

The final test implementation remains structured in **C# + Playwright + NUnit**.

---

# 🛠️ Technologies Used

- C#
- .NET 9
- Microsoft Playwright for .NET
- NUnit
- Page Object Model (POM)
- Microsoft Playwright MCP
- Docker Desktop
- SQL Server 2022
- Visual Studio 2022
- Visual Studio Code
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

Docker Desktop should show:

```text
Engine running
```

---

## 1. Start Existing Containers

After the initial setup, the normal daily workflow is:

```powershell
docker start nopcommerce-sql
docker start nopcommerce-local
```

Check:

```powershell
docker ps
```

Both should be running:

```text
nopcommerce-sql
nopcommerce-local
```

Then open:

```text
http://localhost:8080
```

---

## 2. Initial nopCommerce Docker Setup

Pull the image:

```powershell
docker pull nopcommerceteam/nopcommerce:latest
```

Create the nopCommerce container:

```powershell
docker run -d --name nopcommerce-local -p 8080:80 nopcommerceteam/nopcommerce:latest
```

---

## 3. SQL Server 2022

Use your own strong SQL Server password.

```powershell
docker run -d --name nopcommerce-sql `
-e "ACCEPT_EULA=Y" `
-e "MSSQL_SA_PASSWORD=<YourStrongPassword>" `
-p 1433:1433 `
mcr.microsoft.com/mssql/server:2022-latest
```

**Never commit real passwords or credentials to GitHub.**

---

## 4. nopCommerce Installation Settings

Recommended local installation settings:

```text
Country:
Australia

Create sample data:
Yes

Database:
Microsoft SQL Server

Create database if it doesn't exist:
Yes

Server:
host.docker.internal,1433

Database:
nopcommerce

Username:
sa

Password:
<YourStrongPassword>
```

Sample data is required because the automation suite uses nopCommerce sample products.

---

# ▶️ Running the Tests

Navigate to:

```powershell
cd C:\playwright-csharp-automation\PlaywrightTests
```

Set the local test environment:

```powershell
$env:BASE_URL="http://localhost:8080"
```

Optional – display the browser while tests execute:

```powershell
$env:HEADLESS="false"
```

Run the complete suite:

```powershell
dotnet test
```

Current expected result:

```text
total: 10
failed: 0
succeeded: 10
skipped: 0
```

---

## Run One Test

Example – TC-009 Guest Checkout:

```powershell
dotnet test --filter "Name~CheckoutAsGuest_ShouldCompleteOrderSuccessfully"
```

Example – TC-010 Product Sorting:

```powershell
dotnet test --filter "Name~SortProducts_PriceLowToHigh_ShouldDisplayAscendingPrices"
```

---

# ⚙️ Configuration

Default configuration is stored in:

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

Environment variables can override the shared configuration:

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

This allows local Docker testing without permanently changing the shared configuration.

---

# 🔍 Framework Features

The automation framework currently demonstrates:

- Page Object Model
- NUnit test organisation
- Async Playwright browser interactions
- Environment-based configuration
- Dynamic registration data
- Positive and negative authentication testing
- Product search
- Product configuration
- Add-to-cart validation
- Cart quantity updates
- Cart item removal
- Wishlist-to-cart workflow
- Guest checkout
- Billing and shipping automation
- Payment workflow using test data
- Product price sorting validation
- Price parsing and subtotal validation
- Accessible role- and label-based locators
- Browser context isolation
- Screenshots on failure
- Playwright tracing
- Docker-based local test environment
- SQL Server container
- AI-assisted test exploration using Playwright MCP
- CI-friendly framework configuration

---

# 🧠 Automation Lessons Demonstrated

Several framework improvements came directly from debugging real test failures.

### Generic Locators

A product-specific Add to Cart locator was replaced with a reusable accessible role-based locator.

### Synchronisation

TC-010 demonstrated the importance of waiting for the application state to update after changing product sorting instead of immediately reading stale UI data.

### Controlled Test Environment

Moving from the public nopCommerce demo to Docker removed Cloudflare as an external dependency and made test execution more predictable.

### AI-Assisted Exploration

Playwright MCP was most useful for exploring the complex Guest Checkout workflow, discovering accessible controls, and helping diagnose locator problems.

---

# ☁️ GitHub Actions / CI Note

A GitHub-hosted runner cannot access:

```text
http://localhost:8080
```

running on a developer's personal computer.

A future CI implementation can start the required Docker containers directly inside the GitHub Actions runner or execute against a dedicated test environment.

The public nopCommerce demo may display Cloudflare verification and therefore is not treated as a fully controlled automation environment.

---

# 📈 Roadmap

## Completed

```text
TC-001 → TC-010 ✅
Full regression: 10 / 10 passing ✅
Local Docker environment ✅
Playwright MCP exploration ✅
```

## Future Enhancements

```text
Cross-browser execution
        ↓
Parallel test execution
        ↓
Test tagging: Smoke / Regression
        ↓
Data-driven testing
        ↓
API testing
        ↓
HTML reporting
        ↓
Retry / flaky-test strategy
        ↓
Docker Compose
        ↓
Docker-based GitHub Actions CI
```

---

## 👩‍💻 Author

**Heba AL-Rubaye**

QA Automation portfolio project demonstrating practical experience with:

**C# · Playwright · NUnit · Page Object Model · Docker · SQL Server · Playwright MCP · Git · GitHub**