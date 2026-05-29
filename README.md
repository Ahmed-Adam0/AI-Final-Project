# 🛋️ FurniMind AI - Backend Infrastructure

Welcome to the enterprise-grade backend of **FurniMind AI**, a premium furniture marketplace integrated with spatial artificial intelligence.

---

## 📖 Technical Documentation

We have prepared an extensive technical guide that covers every aspect of this system.

### 📥 [Click here to View the Full Technical Documentation (PDF)](./docs/Backend%20Architecture%20&%20System%20Documentation.pdf)
*(Note: If the link doesn't open directly, you can find the file in the `/docs` folder above)*

---

## 👥 Meet The Backend Team

| Name | Role | GitHub Profile |
| :--- | :--- | :--- |
| **Ahmed Adam** | Full Stack  Lead | [@Ahmed-Adam0](https://github.com/Ahmed-Adam0) |
| **Saleh** | Full Stack  Developer | [@salehmohamed99](https://github.com/salehmohamed99) |
| **Sultan** | Full Stack  Developer | [@SultanBashier](https://github.com/SultanBashier)) |
| **Mayar** | Full Stack  Developer |   [@may20030](https://github.com/may20030) |
| **Ashraf** | Full Stack Developer | [@MohamedAshraf144](https://github.com/MohamedAshraf144)|
| **Ayat** | Full Stack  Developer |  [@ayatahmed30](https://github.com/ayatahmed30) |

---
## 🛠️ Tech Stack

* **Framework:** ASP.NET Core 8.0 (C#)
* **ORM:** Entity Framework Core 10.0
* **Database:** SQL Server 2022
* **Architecture:** Onion Architecture (Clean Architecture)
* **AI Coordination:** Spatial analysis (x, y, z coordinates)

---

## 🏗️ Project Structure

* **`FurniMind.Domain`**: Core entities and business logic.
* **`FurniMind.Application`**: Services, DTOs, and Interfaces.
* **`FurniMind.Infrastructure`**: DB Context and Migrations.
* **`FurniMind.API`**: RESTful Controllers and Middleware.

---

## 🏁 How to Run

1. **Clone the Repo:** `git clone https://github.com/Ahmed-Adam0/FurniMind-AI-Backend.git`
2. **Setup Database:** Update connection string in `appsettings.json`.
3. **Run Migrations:** `dotnet ef database update`
4. **Start API:** `dotnet run --project src/FurniMind.API`
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
