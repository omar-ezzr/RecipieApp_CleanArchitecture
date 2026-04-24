# 🍽️ Recipe App — Clean Architecture (Full-Stack)

A full-stack web application for managing recipes, built with **.NET 8** and **Angular 18**, following **Clean Architecture principles**.

This project focuses on building a structured, maintainable, and scalable application — not just basic CRUD.

---

## 🚀 Features

### 🔐 Authentication

* JWT Authentication
* Role-based access (Admin / User)
* Protected routes (Angular Guards)

### 📦 Recipes Management

* Create / Update / Delete recipes (Admin)
* View recipes (User)
* Structured data:

  * Ingredients
  * Steps
  * Categories

### 🔍 Data Handling

* Pagination
* Filtering (search, difficulty)
* Sorting

### 🎨 Frontend (Angular 18)

* Standalone components
* Feature-based folder structure
* Interceptors (auto attach token)
* Responsive UI

---

## 🧠 Architecture

### Backend — Clean Architecture

```
Core.Domain        → Entities
Core.Application   → DTOs, Interfaces, Services
Infrastructure     → EF Core, Repositories
API                → Controllers, JWT, Middleware
```

### Flow

```
Angular → API → Service → Repository → Database
```

---

## 🛠️ Tech Stack

### Backend

* .NET 8
* ASP.NET Core Web API
* Entity Framework Core
* SQL Server (Docker)

### Frontend

* Angular 18
* TypeScript
* RxJS

---

## ⚙️ Getting Started

### 1. Clone the repository

```
git clone https://github.com/omar-ezzr/RecipieApp_CleanArchitecture.git
cd RecipieApp_CleanArchitecture
```

---

### 2. Backend setup

```
cd API
dotnet restore
dotnet run
```

Swagger:

```
https://localhost:5001/swagger
```

---

### 3. Frontend setup

```
cd app
npm install
ng serve
```

App runs on:

```
http://localhost:4200
```

---

## 📸 Screenshots

<img width="1363" height="567" alt="Capture d’écran du 2026-04-24 17-15-14" src="https://github.com/user-attachments/assets/3dc00569-d2f4-457e-ba61-a584667fec86" />
<img width="1363" height="567" alt="Capture d’écran du 2026-04-24 17-15-07" src="https://github.com/user-attachments/assets/d16114db-f783-400a-b4d1-73e4bba840a3" />
<img width="1363" height="567" alt="Capture d’écran du 2026-04-24 17-14-53" src="https://github.com/user-attachments/assets/4111f711-1125-4914-8a80-dd8711060cc0" />
<img width="1363" height="494" alt="Capture d’écran du 2026-04-24 17-14-33" src="https://github.com/user-attachments/assets/9312b6e3-d76a-49c6-be67-2d626d97c3bf" />

---

## 🧪 Future Improvements

* Redis caching
* RabbitMQ (async processing)
* File upload (images)
* Deployment (Docker + Cloud)
* Unit testing

---

## 📌 Notes

This is Version 1 of the project.
The focus was on **architecture, data handling, and clean structure**, not only features.

---

## 👨‍💻 Author

**Omar Ezzr**
