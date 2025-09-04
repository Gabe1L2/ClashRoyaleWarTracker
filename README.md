# 🏰 Clash Royale War Tracker

A comprehensive .NET 8 Razor Pages application for automatically tracking and managing Clash Royale clan war statistics and performance metrics.

## 🎯 Project Overview

I am creating this project to gain more experience with full-stack .NET development and to gain exposure to technologies I have not previously worked with, including **Razor Pages** and **Entity Framework Core**. This project is still in progress with only some pieces working right now, but I am actively working on it as I have time.

**What this project will eventually allow you to do** is automatically keep track of your Clash Royale clan(s)'s war stats, which need to be updated weekly to maintain accurate historical data and performance analytics.

## 🚀 Current Status

**🟢 Working Features:**
- ✅ Clan management (Add, Update, Delete, View)
- ✅ Integration with Clash Royale API
- ✅ Entity Framework Core with SQL Server
- ✅ User authentication with ASP.NET Core Identity
- ✅ Clean Architecture implementation
- ✅ Database migrations and seeding

**🟡 In Development:**
- 🔄 Automated weekly war statistics updates
- 🔄 Historical clan performance tracking
- 🔄 Player statistics and averages
- 🔄 War participation analytics
- 🔄 Administrative dashboard

**🔴 Planned Features:**
- 📅 Scheduled background tasks for weekly updates
- 📊 Data visualization and charts
- 📈 Performance trend analysis
- 🏆 Clan ranking system
- 📱 Responsive mobile interface

## 🛠️ Technologies & Learning Objectives

This project serves as a learning platform for:

### **Frontend Technologies**
- **Razor Pages** - Server-side page-based programming model
- **Bootstrap 5** - Responsive UI framework
- **jQuery** - Client-side interactivity

### **Backend Technologies**
- **.NET 8** - Latest version of the .NET framework
- **Entity Framework Core** - Object-relational mapping (ORM)
- **ASP.NET Core Identity** - Authentication and authorization
- **SQL Server** - Relational database management

### **Architecture & Patterns**
- **Clean Architecture** - Separation of concerns and dependency inversion
- **Repository Pattern** - Data access abstraction
- **Dependency Injection** - Inversion of control
- **Unit Testing** - Test-driven development practices

### **External Integrations**
- **Clash Royale API** - Real-time game data retrieval
- **HttpClient** - RESTful API consumption

## 🏗️ Project Structure

```text
ClashRoyaleWarTracker/
├── ClashRoyaleWarTracker.Web/             # Razor Pages presentation layer
│   ├── Pages/                          # Razor pages and page models
│   ├── wwwroot/                        # Static files (CSS, JS, images)
│   └── Program.cs                      # Application entry point
├── ClashRoyaleWarTracker.Application/     # Business logic and interfaces
│   ├── Interfaces/                     # Service contracts
│   ├── Models/                         # Domain models
│   ├── Services/                       # Business logic implementation
│   └── Helpers/                        # Utility classes
├── ClashRoyaleWarTracker.Infrastructure/  # Data access and external services
│   ├── Repositories/                   # Data access implementation
│   ├── Http/                           # API clients
│   ├── Migrations/                     # EF Core database migrations
│   └── Services/                       # Infrastructure services
└── ClashRoyaleWarTracker.Tests/           # Integration and unit tests
```

## 🚀 Getting Started

### Prerequisites

- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** or **SQL Server LocalDB**
- **Clash Royale API Key** - [Get one here](https://developer.clashroyale.com/)
- **Visual Studio 2022** or **VS Code** (recommended)

### Installation
1. **Clone the repository:**
```powershell
git clone https://github.com/Gabe1L2/ClashRoyaleWarTracker.git cd ClashRoyaleWarTracker
```
2. **Set up User Secrets for development:**
```powershell
cd ClashRoyaleWarTracker.Web dotnet user-secrets init dotnet user-secrets set "ClashRoyaleApi:ApiKey" "YOUR_ACTUAL_API_KEY" dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING"
```
3. **Configure default users (optional):**
```powershell
dotnet user-secrets set "DefaultUsers:0:Email" "admin@example.com" dotnet user-secrets set "DefaultUsers:0:Password" "YourPassword123!"
```
4. **Update the database:**
```powershell
dotnet ef database update --project ClashRoyaleWarTracker.Infrastructure --startup-project ClashRoyaleWarTracker.Web
```
5. **Run the application:**
```powershell
dotnet run --project ClashRoyaleWarTracker.Web
```
6. **Access the application:**
   - Navigate to `https://localhost:7236` (or the URL shown in your terminal)

## 🎮 How to Get a Clash Royale API Key

1. Visit the [Clash Royale Developer Portal](https://developer.clashroyale.com/)
2. Create an account or log in with your Supercell ID
3. Create a new API key:
   - **Name**: Your project name
   - **Description**: Brief description of your project
   - **IP Addresses**: Add your development IP address(es)
4. Copy the generated API key and add it to your User Secrets


**Test Features:**
- Full integration testing with real Clash Royale API
- Database integration testing
- User Secrets integration for secure testing

## 🎯 Learning Outcomes

Through this project, I'm gaining hands-on experience with:

- **Razor Pages architecture** vs. traditional MVC
- **Entity Framework Core** relationships and migrations
- **Clean Architecture** principles in .NET
- **API integration** and HTTP client management
- **Authentication and authorization** in web applications
- **Database design** and normalization
- **Asynchronous programming** patterns
- **Dependency injection** and service lifetimes
- **Integration testing** strategies

## 🤝 Contributing

This is primarily a learning project, but suggestions and feedback are welcome! If you'd like to contribute:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/new-feature`)
3. Commit your changes (`git commit -am 'Add new feature'`)
4. Push to the branch (`git push origin feature/new-feature`)
5. Create a Pull Request

## 📝 License

This project is open source and available under the [MIT License](LICENSE).

---

**Note**: This project is actively under development as a learning exercise. Some features may be incomplete or subject to change as I continue to learn and improve the codebase.

For questions or suggestions, feel free to open an issue or reach out!
