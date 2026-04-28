# AI-Final-Project
# 🚀 Project Name: المشروع النهائي (Final Project)
## 🖥️ Dashboard (ASP.NET Core MVC) & Backend API

---

## 📌 Project Overview
> [سيتم كتابة وصف المشروع هنا فور تحديد الفكرة النهائية]

---

## 👥 Meet The Team
* **آدم (Adam)**
* **صالح (Saleh)**
* **سلطان (Sultan)**
* **ميار (Mayar)**
* **أشرف (Ashraf)**
* **آيات (Ayat)**

---

## 🏗️ Technical Architecture: Onion Architecture
يعتمد المشروع على **Onion Architecture** لضمان فصل الاهتمامات (Separation of Concerns) وسهولة الاختبار والصيانة:

1. **Domain Layer:** تحتوي على الـ Entities والـ Core logic.
2. **Application Layer:** تحتوي على الـ Interfaces والـ DTOs والـ Features/Use Cases.
3. **Infrastructure Layer:** تحتوي على الـ Data Access (Entity Framework) والـ External Services.
4. **Presentation Layer:** تشمل الـ API و الـ MVC Dashboard.

---

## 🛠️ Tech Stack
* **Framework:** ASP.NET Core (Onion Architecture Approach)
* **API:** RESTful API
* **Database:** SQL Server / Entity Framework Core
* **Identity:** ASP.NET Core Identity & JWT

---

## 📂 Folder Structure (Onion Layers)
```text
├── 1. Core
│   ├── Project.Domain
│   └── Project.Application
├── 2. Infrastructure
│   ├── Project.Persistence (Database context)
│   └── Project.Infrastructure (External services)
├── 3. Presentation
│   ├── Project.API
│   └── Project.Web (MVC Dashboard)
└── Docs (Architecture Diagrams)
