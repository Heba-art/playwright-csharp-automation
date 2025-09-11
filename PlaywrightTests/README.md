# Playwright C# Automation – nopCommerce

![.NET](https://img.shields.io/badge/.NET-6.0%2B-blueviolet)
![NUnit](https://img.shields.io/badge/TestFramework-NUnit-green)
![Playwright](https://img.shields.io/badge/Playwright-C%23-2ea44f)
![Platform](https://img.shields.io/badge/Platform-nopCommerce%20Demo-orange)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)
![Status](https://img.shields.io/badge/Status-In%20Progress-lightgrey)


![Profile Views](https://komarev.com/ghpvc/?username=heba-art/&color=blue)
![GitHub Repo stars](https://img.shields.io/github/stars/heba-art/playwright-csharp-automation?style=social)
![GitHub forks](https://img.shields.io/github/forks/heba-art/playwright-csharp-automation?style=social)


End-to-end test automation project using **Playwright** with **C# (NUnit)**.  
Target application: [nopCommerce Demo](https://demo.nopcommerce.com)

---


## 📂 Project Structure
```
PlaywrightTests/
│
├── 📁 Pages/
│ ├── 📄 HomePage.cs # Represents the Home Page. Contains selectors and methods for interacting with elements (e.g., search bar, navigation links).
│ ├── 📄 LoginPage.cs # Represents the Login Page. Handles interactions like filling username/password fields and clicking the submit button.
│ └── 📄 RegisterPage.cs # Represents the User Registration Page. Manages form filling and submission logic.
│
├── 📁 Tests/
│ └── 📁 Auth/
│ └── 📄 RegisterAndLoginTests.cs # Contains the actual test methods ([Test]). Uses Page Objects + NUnit assertions for registration/login scenarios.
│
├── 📁 Utils/
│ ├── 📄 Faker.cs # Helper class for generating random test data (unique emails, strong passwords).
│ └── 📄 TestBase.cs # Base class for setup/teardown (launching browser, new page context, closing browser).
│
├── 📄 appsettings.json # Config file (baseUrl, browser, environment details).
│
└── 📄 README.md # Project documentation (purpose, setup, run instructions).
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
