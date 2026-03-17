# 📦 ISDN (Island Link Integrated Sales and Distribution Management System)

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Entity Framework Core](https://img.shields.io/badge/EF_Core-8.0-388E3C?style=for-the-badge&logo=nuget&logoColor=white)
![MySQL](https://img.shields.io/badge/MySQL-4479A1?style=for-the-badge&logo=mysql&logoColor=white)
![Visual Studio](https://img.shields.io/badge/Visual_Studio-2026-5C2D91?style=for-the-badge&logo=visual-studio&logoColor=white)

## 📖 Executive Summary
The **ISDN (Island Link Integrated Sales and Distribution Management System)** is a comprehensive, enterprise-scale web platform designed to streamline the entire supply chain and consumer experience. At its core, the system features an integrated order management portal that provides extensive customer services—allowing users to seamlessly browse available products, manage their shopping carts, execute secure payments, track their orders through every lifecycle phase, and manage their account profiles. 

Operating powerfully in the background are extensive administrative toolsets that oversee user clearance, secure financing, and operational logistics. This includes dedicated field services for transport personnel, such as real-time GPS tracking for order deliveries. By coupling these front-end customer experiences with robust logistics, inventory, and point-of-sale management systems, ISDN provides a centralized hub capable of handling complex hierarchical B2B business operations, Head Office oversight, Regional Distribution Centers (RDC), and multi-tier customer endpoints.

---

## 📑 Table of Contents
1. [System Architecture & Tech Stack](#1-system-architecture--tech-stack)
2. [Identity & Access Management (RBAC)](#2-identity--access-management-rbac)
3. [Client Typology & B2B Hierarchy](#3-client-typology--b2b-hierarchy)
4. [Core Business Modules](#4-core-business-modules)
5. [Prerequisites & System Requirements](#5-prerequisites--system-requirements)
6. [Local Environment & Database Setup](#6-local-environment--database-setup)
7. [Project Repository Structure](#7-project-repository-structure)
8. [Auditing & Compliance](#8-auditing--compliance)
9. [Support & Maintainers](#9-support--maintainers)

---

## 1. System Architecture & Tech Stack

This architecture is built for high concurrency, reliable relational data mapping, and strict state execution using the Microsoft .NET ecosystem paired with a highly relational MySQL database.

* **Backend Framework:** .NET 8.0 (C#)
* **Web UI Framework:** ASP.NET Core Razor Pages
* **ORM:** Entity Framework Core (EF Core) via Pomelo MySQL provider
* **Primary Database:** MySQL 8.x
* **Frontend Assets:** HTML5, CSS3, JavaScript, jQuery
* **Version Control:** Git & GitHub

---

## 2. Identity & Access Management (RBAC)

Security and separation of concerns are critical to the platform's integrity. The platform is strictly segmented into **8 main departments/roles**. Access boundaries are hard-enforced at the controller level via authorization middleware.

| Role | Department & System Access Level |
| :--- | :--- |
| **`SYSTEM_ADMIN`** | Highest clearance level. Oversees total system configuration, global user management, and has exclusive access to the system-wide historical auditing and error logging modules. |
| **`HEAD_OFFICE`** | Executive customer management. Oversees customer account activation, deactivation, suspension, and disapproval workflows across all 4 customer types. Ensures platform compliance and top-level administrative gating. |
| **`FINANCE`** | Handles overarching financial ledgers, oversees payment transaction authentications, generates corporate invoices, and processes financial reconciliations. |
| **`RDC_STAFF`** | Manages Regional Distribution Center workflows. Oversees picking/packing of customer orders, handovers to the logistics team for dispatch, and processes incoming return and refund requests from customers. |
| **`SALES_REP`** | Analytical overview team. Responsible for comprehensive sales reports generation and overseeing sales metrics over monthly or custom date ranges to track business trends. |
| **`LOGISTICS`** | Fleet and dispatch management. Handles assigning drivers to orders and utilizes a cross-performance view function to monitor real-time in-transit deliveries and audit completed routes. |
| **`DRIVER`** | Field operators utilizing the lightweight portal. Receives delivery assignments, updates transit stages, utilizes real-time GPS tracking for delivery routing, and processes on-site confirmations. |
| **`CUSTOMER`** | Client-facing portal. Granted access to browsing catalogs, adding items to carts, making secure payments, placing orders, tracking live order statuses, and managing complex account profile settings. |

---

## 3. Client Typology & B2B Hierarchy

The ISDN platform supports a highly sophisticated B2B Customer framework capable of managing multi-branch infrastructures. Account structures are rigidly divided into 4 typologies:

* **SOB (Single Business Owner):** A traditional standalone corporate account running a singular operational branch.
* **PBOS (Primary Business Owner - Single Type):** A master account handling specialized solo-industry operations with a centralized control structure.
* **PBOM (Primary Business Owner - Multiple Type):** A complex master account running various distinct business verticals under one corporate umbrella.
* **BM (Branch Manager):** Subordinate accounts that cannot register independently. They must register using a secure, system-generated code.

**Unique Code Generation & Branch Clustering:**
For **PBOS** and **PBOM** entities, unique system code generation is an optional preference chosen by the primary business owner. This generates a **single code** that is used to loop and retrieve database records for all registered branches as a unified cluster. While the shared code groups the branches under one primary employer, each specific branch is uniquely identified in the system via a separate, distinct `CustomerID` generated upon branch registration.

---

## 4. Core Business Modules

The software is heavily decoupled into distinct, highly cohesive operational domains:

### 📊 Headquarters & Customer Management
Governed by the Head Office logics, this handles the onboarding pipeline for the complex PBOS/PBOM and BM hierarchy. It enforces security by gating account activations, suspensions, and verifications before new businesses can trade on the network.

### 💳 Finance & Secure Payment Gateways
Managed by the `PaymentsController`, this module secures all external transaction ledgers. It processes completed checkouts, handles financial status updates across the business, and oversees invoice generation upon final delivery confirmation.

### 🚚 Logistics & Fleet Dispatch
Governed by the `LogisticsController`, providing cross-performance visibility of the entire delivery pipeline. It transitions finalized packages from the RDC to active Drivers, monitors real-time in-transit states, and logs proof of delivery.

### 🏢 RDC (Regional Distribution Center) Operations
A dedicated operational hub for regional staff. Manages the physical packing of customer orders, staging for logistics handover, and the critical auditing of returned items and refund validations.

### 🛍 Integrated Order Lifecycle Management
The primary state-machine of the application. It maps an order seamlessly from: `Cart -> Secure Payment -> Placed -> Packed (RDC) -> Assigned (Logistics) -> In-Transit (Driver API / GPS) -> Delivered`.

---

## 5. Prerequisites & System Requirements

To compile, debug, and contribute to this repository locally, engineers must ensure the following environment configurations are met:

1. **Integrated Development Environment:** [Visual Studio 2022 or 2026](https://visualstudio.microsoft.com/) 
   *(Required Workload: ASP.NET & Web Development)*
2. **Framework:** [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
3. **Database Server:** **MySQL Server (v8.0+)** 
   *(Highly Recommended: MySQL Workbench for UI-based server management).*
4. **Version Control:** Git CLI

---

## 6. Local Environment & Database Setup

Follow these exact standard operating procedures (SOPs) to initialize the platform locally:

### Step 1: Clone the Repository
Launch PowerShell or Terminal and execute:

git clone https://github.com/THINUX-Official/ISDN.git cd "ISDN 3/ISDN 3"


### Step 2: Configure Application Secrets
Open `appsettings.json` in the active project directory. Update the `DefaultConnection` string to point to your local MySQL instance. 

"ConnectionStrings": { "DefaultConnection": "server=localhost;port=3306;database=isdn_distribution_db;user=root;password=YourSecurePassword;SslMode=none;AllowPublicKeyRetrieval=true" }


### Step 3: Database Initialization (Using the Provided MySQL Dump)
To ensure all engineers share the exact same structural data, utilize the provided database dump over standard EF migrations.

**Option A: Using MySQL Command Line**
1. Open your terminal and log into MySQL CLI: `mysql -u root -p`
2. Create an empty database: `CREATE DATABASE isdn_distribution_db;`
3. Exit CLI, then import the `.sql` dump file located in the repository:

mysql -u root -p isdn_distribution_db < Database/isdn_database_dump.sql


**Option B: Using MySQL Workbench**
1. Open MySQL Workbench and connect to your local server instance.
2. Click the **"Create a new schema"** icon and name it `isdn_distribution_db`.
3. Navigate to **Server > Data Import**.
4. Select **Import from Self-Contained File** and route it to the `.sql` dump file inside the project.
5. Set the Default Target Schema to `isdn_distribution_db` and click **Start Import**.

### Step 4: Build & Run
Open the solution file in Visual Studio. Select the web project as your startup project, restore any missing NuGet packages, and press `F5` to boot the application.

---

## 7. Project Repository Structure

To maintain a clean working methodology, the codebase adheres to the following structural conventions:

ISDN/ │ ├── /Constants/          # Immutable system states (e.g., UserRoles.cs) ├── /Controllers/        # Route controllers (Logistics, HeadOffice, Finance, Orders) ├── /Data/               # EF DbContext bindings & Startup Initializers ├── /Database/           # MySQL Dump files and Database SOPs (*.sql, *.md) ├── /Migrations/         # Historical schema states (For future EF tracking only) ├── /Models/             # Domain Entities (Customers, Orders, AuditLogs, Evidence) ├── /Repositories/       # Data Access Layer abstractions (e.g., CustomerRepository) ├── /Views/              # Razor Pages/Views grouped by subsystem (RdcStaff, Orders) ├── /wwwroot/            # Static Assets (Compiled CSS, jQuery, internal scripts, libs) │ ├── appsettings.json     # Encrypted configuration properties └── Program.cs           # WebHost Builder, Services Injection, Pipeline configuration


---

## 8. Auditing & Compliance

To maintain enterprise accountability, this system utilizes a strict backend logging architecture. 
* High-level `POST`, `PUT`, and `DELETE` requests affecting customer statuses, refund validations, or delivery confirmations are securely logged.
* Changes track the `UserID`, `Timestamp`, `ActionType`, and `Delta State`.
* This ensures zero data discrepancies and complete historical traceability for the Head Office and Finance teams.

---

## 9. Support & Maintainers

This robust infrastructure was architected and is actively maintained by the **THINUX Engineering Division**. 


*Internal Document | **THINUX-Official** B2B Software Division | Proprietary Logic*