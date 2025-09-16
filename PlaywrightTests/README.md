# Playwright C# Automation – nopCommerce

![.NET](https://img.shields.io/badge/.NET-6.0%2B-blueviolet)
![NUnit](https://img.shields.io/badge/TestFramework-NUnit-green)
![Playwright](https://img.shields.io/badge/Playwright-C%23-2ea44f)
![Platform](https://img.shields.io/badge/Platform-nopCommerce%20Demo-orange)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)
![Status](https://img.shields.io/badge/Status-In%20Progress-lightgrey)
[![CI](https://github.com/Heba-art/playwright-csharp-automation/actions/workflows/dotnet-playwright-ci.yaml/badge.svg?branch=main)](https://github.com/Heba-art/playwright-csharp-automation/actions/workflows/dotnet-playwright-ci.yaml)



![Profile Views](https://komarev.com/ghpvc/?username=heba-art&color=blue)
![GitHub Repo stars](https://img.shields.io/github/stars/heba-art/playwright-csharp-automation?style=social)
![GitHub forks](https://img.shields.io/github/forks/heba-art/playwright-csharp-automation?style=social)


End-to-end test automation project using **Playwright** with **C# (NUnit)**.  
Target application: [nopCommerce Demo](https://demo.nopcommerce.com)

---


## 📂 Project Structure
```
PlaywrightTests/
│
├── 📁 Pages/                         # Page Object Models (POM)
│   ├── 📄 CartPage.cs                # Cart page interactions (view items, totals, update/remove).
│   ├── 📄 HomePage.cs                # Home page actions (navigate, search, menus).
│   ├── 📄 LoginPage.cs               # Login page (username, password, submit).
│   ├── 📄 ProductPage.cs             # Product details page (add to cart, select options).
│   ├── 📄 RegisterPage.cs            # User registration page.
│   └── 📄 SearchResultsPage.cs       # Search results listing page.
│
├── 📁 Tests/                         # Test Suites organized by feature
│   ├── 📁 Auth/
│   │   └── 📄 RegisterAndLoginTests.cs 
│   │                                 # Tests for user registration & login flows.
│   │
│   └── 📁 Catalog/
│       ├── 📄 CartTests.cs           # Tests for adding/removing products from cart, price checks.
│       └── 📄 SearchTests.cs         # Tests for product search and results validation.
│
├── 📁 Utils/                         # Shared utilities & helpers
│   ├── 📄 CredentialStore.cs         # Handles test credentials (safe storage for login data).
│   ├── 📄 Faker.cs                   # Generates random test data (emails, usernames, passwords).
│   └── 📄 TestBase.cs                # Common setup/teardown (browser, context, page).
│
├── 📄 appsettings.json               # Config (baseUrl, browser, environment details).
│
└── 📄 README.md                      # Documentation (setup, run instructions, project details).

```
---

## ✅ Test Cases (Phase 1)

| **ID**   | **Title**                          | **Preconditions**         | **Test Data**                   | **Expected Result**                     | **Priority** | **Type**                | **Status** |
|----------|-------------------------------------|---------------------------|---------------------------------|------------------------------------------|--------------|-------------------------|------------|
| TC-001   | User Registration – Happy Path      | Not logged in             | Unique email, valid details     | Success message; user logged in          | 🔴 High      | Smoke / Functional      | Passed     |
| TC-002   | Login with Valid Credentials        | Registered user (TC-001)  | Valid email/password            | Login succeeds; My Account visible       | 🔴 High      | Smoke                   | Passed     |
| TC-003   | Login with Invalid Password         | Registered user           | Valid email, wrong password     | Error shown; login fails                 | 🔴 High      | Negative                | Passed     |
| TC-004   | Search Product – Exact Match        | None                      | “Build your own computer”       | Product appears in results               | 🟡 Medium    | Functional              | Passed     |
| TC-005   | Add to Cart from Product Page       | None                      | “Build your own computer”       | Item in cart; correct name, price        | 🔴 High      | Smoke / Functional      | Passed     |
| TC-006   | Update Quantity in Cart             | Item already in cart      | Qty=2                           | Subtotal recalculated                    | 🟡 Medium    | Functional              | Passed     |
| TC-007   | Remove Item from Cart               | Item already in cart      | —                               | Cart empty message                       | 🟡 Medium    | Functional              | Passed     |
| TC-008   | Wishlist to Cart Flow               | Logged in user            | Any product                     | Item moved/added to cart                 | 🟢 Low       | Regression              | Passed     |
| TC-009   | Checkout as Guest – Valid Flow      | Cart has item, logged out | Guest billing + shipping info   | Order placed successfully                | 🔴 High      | Regression / Functional | Passed     |
| TC-010   | Sort Products – Price Low → High    | None                      | Category: Computers → Desktops  | Products sorted ascending                | 🟡 Medium    | Regression              | Passed     |

---

## ▶️ How to Run

### 🔹 From CLI
```bash
# Navigate to project folder
cd playwright-csharp-automation/PlaywrightTests

# Restore dependencies
dotnet restore

# Install Playwright drivers
pwsh bin/Debug/net*/playwright.ps1 install

# Run all tests
dotnet test

🔹 From Visual Studio 2022
Open PlaywrightTests.csproj in Visual Studio 2022.
Open Test Explorer (Test → Test Explorer).
Click Run All Tests ▶️.
See results (pass/fail) directly in IDE.

🛠️ Tools & Frameworks
Playwright for .NET
NUnit
.NET 6/7

Visual Studio 2022

👩‍💻 Author: Heba
📌 Purpose: Professional portfolio project for Test Automation Engineer role
