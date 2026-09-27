# 🛒 Advanced Point of Sale (POS) System

## 📌 Introduction

Welcome to the **Advanced POS System**, a high-performance desktop application engineered with **C# .NET**. This project has been significantly modified and optimized to provide a seamless experience for managing sales, inventory, and user authentication. It is designed to be lightweight, portable, and ready for modern retail environments.

## 🚀 Key Features

### 💻 Sales & POS Interface

* **Real-time Transactions:** Process sales quickly with an intuitive cart system.
* **Dynamic Search:** Find products instantly by barcode or product code.
* **Automated Calculation:** Precise calculation of totals and change.
* **Cashier Handover:** Dedicated interface for cashier roles with restricted permissions.

### 📦 Inventory & Stock Control

* **Product Management:** Full CRUD (Create, Read, Update, Delete) for products, brands, and categories.
* **Smart Stock-In:** Efficiently update inventory levels through the dedicated Stock-In module.
* **Low Stock Alerts:** Monitor reorder levels to prevent inventory shortages.
* **Inventory Tracking:** Detailed logs of stock adjustments and historical data.

### 👥 User Security & Roles

* **Role-Based Access Control (RBAC):** Distinct permissions for **Administrators** and **Cashiers**.
* **Secure Authentication:** Encrypted-ready login system to protect business data.
* **Profile Management:** Store detailed employee information, including full names and active status.

### 📊 Reporting & Analytics

* **Sales History:** View and analyze past transactions with date filtering.
* **Top Selling Items:** Identify which products are driving revenue.
* **Data Export:** Export critical business reports for external analysis.

## 🛠 Prerequisites

To run this application, ensure your environment meets these requirements:

* **OS:** Windows 10 or 11.
* **Framework:** .NET Framework 4.7.2 or higher.
* **IDE:** Visual Studio 2022 (Recommended).
* **Database:** System.Data.SQLite (Included in project references).

## ⚙️ Installation

1. **Clone the Repository:**
```bash
git clone https://github.com/sanija123t/PosSystem.git

```


2. **Open the Project:**
Launch `PosSystem.sln` in Visual Studio.
3. **Restore Packages:**
Visual Studio will automatically restore the SQLite NuGet packages.
4. **Database Setup:**
The system uses an **Auto-Initialization** engine. On the first run, the system will automatically create the `PosDB.db` file and all required tables in the `bin/Debug/DATABASE/` directory.

## 📖 Usage

1. **First Launch:** Use the default Administrator credentials to log in.
2. **Configure System:** Navigate to the "Maintenance" section to add your Brands and Categories.
3. **Add Products:** Enter your inventory items in the Product module.
4. **Stock-In:** Add quantities to your products to make them available for sale.
5. **Start Selling:** Open the POS terminal to begin processing customer transactions.

## 🔒 License & Ownership

**Copyright © 2024 - All Rights Reserved.**

This project is the private property of the repository owner. It has been extensively modified and customized for specific business logic. Unauthorized copying, modification, or distribution of this software is strictly prohibited.