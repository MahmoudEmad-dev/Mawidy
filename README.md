# ⚖️ Mawidy (مـوعـدي)

**Mawidy** is a comprehensive digital platform designed to streamline and automate service bookings, queue management, and case tracking. Built with a robust **Clean Architecture**, it ensures scalability, maintainability, and a seamless user experience for managing governmental and judicial services (e.g., Courts).

---

## 🚀 Features

* **📅 Smart Booking System:** Book appointments for court sessions, document submissions, and legal consultations.
* **🔍 Real-Time Case Tracking:** Track the timeline and status of legal cases instantly.
* **🔢 Live Queue Management:** Real-time updates for active queues and waiting times (powered by SignalR).
* **🏛️ Dynamic Filtering:** Filter services and locations dynamically using AJAX and server-side rendering.
* **📱 Responsive UI:** A clean, fast, and accessible user interface tailored for Arabic users (RTL).

---

## 🛠️ Tech Stack

### **Backend Core**
* **Framework:** .NET 8 (ASP.NET Core MVC & Web API)
* **Architecture:** Clean Architecture (Domain-Driven Design principles)
* **Pattern:** CQRS (Command Query Responsibility Segregation) using **MediatR**
* **Real-time Communication:** SignalR (for live queue updates)

### **Database & Infrastructure**
* **ORM:** Entity Framework Core
* **Database:** SQL Server / In-Memory Database (for development/testing)

### **Frontend**
* **Markup/Styling:** HTML5, Custom CSS3, Razor Views (`.cshtml`)
* **Interactivity:** Vanilla JavaScript (ES6+), AJAX
* **Fonts:** Cairo & Tajawal (Google Fonts)

---

## 📂 Project Structure (Clean Architecture)

The repository is structured to strictly enforce separation of concerns:

```text
Mawidy/
├── src/
│   ├── Mawidy.Domain/          # Enterprise logic, Entities, Enums, Exceptions
│   ├── Mawidy.Application/     # Business logic, CQRS (Commands/Queries), Interfaces
│   ├── Mawidy.Infrastructure/  # EF Core DbContext, Migrations, External Services
│   └── Mawidy.Web/             # ASP.NET Core MVC, Controllers, Razor Views, wwwroot
├── tests/                      # Unit & Integration Tests (xUnit/NUnit)
└── README.md
```

---

## ⚙️ Getting Started

Follow these instructions to set up the project locally.

### Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Visual Studio 2022](https://visualstudio.microsoft.com/) or similar IDE
* SQL Server (Optional, currently uses In-Memory DB by default)

### Installation & Run

1. **Clone the repository:**
   ```bash
   git clone https://github.com/your-username/Mawidy.git
   cd Mawidy
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Update Database (If using SQL Server):**
   *Update the connection string in `appsettings.json` before running.*
   ```bash
   dotnet ef database update --project src/Mawidy.Infrastructure --startup-project src/Mawidy.Web
   ```

4. **Run the application:**
   ```bash
   cd src/Mawidy.Web
   dotnet run
   ```

5. **Access the application:**
   Open your browser and navigate to `https://localhost:<port>/Courts`

---

## 🤝 Contributing

1. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
2. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
3. Push to the Branch (`git push origin feature/AmazingFeature`)
4. Open a Pull Request

---

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.
