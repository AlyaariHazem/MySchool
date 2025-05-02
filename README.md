# 🎓 MySchool – School Management System

## 📌 Overview

**MySchool** is a comprehensive **school management system** built to streamline and automate the administration of educational institutions. It integrates multiple academic and administrative functions into a unified platform to ensure accuracy, efficiency, and accessibility for all stakeholders: **administrators**, **teachers**, and **guardians**.

This system centralizes:
- Student data management
- Academic structure and curriculum organization
- Grade and attendance tracking
- Fee management and reporting

All features are accessed through a secure and role-based web application.

---

## 🏗️ System Architecture

MySchool is based on a **three-tier architecture**:
1. **Presentation Layer** – Angular v18 frontend with PrimeNG and PrimeFlex
2. **Business Logic Layer** – ASP.NET Core Web API services and repositories
3. **Data Access Layer** – Entity Framework Core + SQL Server database

---

## 🔧 Technologies and Dependencies

| Layer              | Stack / Tools                        |
|--------------------|--------------------------------------|
| Frontend           | Angular 18, PrimeNG, PrimeFlex, RxJS |
| Backend            | ASP.NET Core Web API, JWT Auth       |
| Database           | SQL Server, EF Core                  |
| API Documentation  | Swagger (Swashbuckle)                |
| Authentication     | JSON Web Tokens (JWT)                |

---

## 📚 Key Features

### 👨‍🎓 Student Management
- Student profiles with academic year linkage
- Guardian assignment and record tracking

### 🏫 Academic Structure
- Manage stages, classes, and sections
- Link curriculum to class levels

### 💰 Fee Management
- Define fee classes and assign to students
- Apply discounts and monitor mandatory fees

### 📊 Grade Tracking
- Monthly and term-wise grade entry
- Detailed grade reports by subject and student

### 📘 Course and Curriculum Management
- Dynamic course linking by class level
- View and assign subjects per academic year

---

## 🧑‍💼 User Roles

- **Admin**: Full access to all modules
- **Teacher**: Access to grades and student data
- **Guardian**: Read-only access to children’s academic and financial data

---

## 🌍 Internationalization

The application supports multiple languages including **Arabic** with **RTL layout support**, thanks to PrimeNG’s built-in features.

---

## 🚀 Getting Started

> Prerequisites:
- .NET 7 SDK or later
- Node.js v18+
- SQL Server instance

```bash
# Backend
cd MySchool.API
dotnet restore
dotnet ef database update
dotnet run

# Frontend
cd MySchool.UI
npm install
ng serve --open
