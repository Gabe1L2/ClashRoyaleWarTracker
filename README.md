# 🏰 Clash Royale War Tracker

A comprehensive .NET 8 Razor Pages application for automatically tracking and managing Clash Royale clan war statistics and performance metrics. **Now live and deployed!**

## 🎯 Project Overview

I am developing this project to gain more experience with full-stack .NET development and to gain exposure to technologies in the .NET ecosystem, including **Razor Pages** and **Entity Framework Core**. This project is still in progress but it is functional, and I am adding more features as time allows.

This application automatically tracks your Clash Royale clan(s) war statistics, providing updated data and comprehensive analytics for clan management and performance tracking.

**🌐 Live Demo**: The application is currently deployed and will soon be accessible with a guest login for demonstration purposes.

## 🚀 Current Status

**🟢 Fully Implemented Features:**
- ✅ **Interactive Player Dashboard** - Comprehensive statistics with advanced filtering and sorting
- ✅ **Clan Management System** - Add, update, delete, and view multiple clans
- ✅ **Real-time Data Updates** - Weekly and backlog data synchronization
- ✅ **Player Performance Analytics** - Fame/attack averages, trophy tracking, activity status
- ✅ **Advanced Filtering & Search** - Filter by clan, trophy level, activity status, and player search
- ✅ **User Authentication & Authorization** - Secure login with ASP.NET Core Identity
- ✅ **Responsive UI** - Bootstrap 5 with mobile-friendly design
- ✅ **Clean Architecture Implementation** - Proper separation of concerns
- ✅ **Database Management** - Entity Framework Core with automated migrations
- ✅ **Clash Royale API Integration** - Real-time game data retrieval
- ✅ **Automated User Seeding** - Environment-specific default users
- ✅ **Production Deployment** - Live application with CI/CD pipeline

**🟡 Enhanced Features:**
- 🔄 **Automated Data Updates** - Both weekly and backlog update options
- 🔄 **Player Status Management** - Track active/inactive players
- 🔄 **Clan Statistics Tracking** - Historical performance data
- 🔄 **Advanced User Interface** - Modern, interactive dashboard with real-time feedback

**🔴 Future Enhancements:**
- 📊 Data visualization and charts
- 📈 Performance trend analysis over time
- 🏆 Clan ranking and comparison system
- 📅 Scheduled background tasks for automatic updates

## 🛠️ Technology Stack & Deployment

### **Production Environment**
- **Hosting Provider**: [MonsterASP.NET](https://www.monsterasp.net/) - Professional ASP.NET hosting
- **Application Hosting**: Windows-based shared hosting with .NET 8 support
- **Database**: SQL Server hosting through MonsterASP.NET
- **Deployment**: Automated CI/CD pipeline using GitHub Actions with WebDeploy
- **Environment Management**: Separate Development and Production configurations

### **Frontend Technologies**
- **Razor Pages** - Server-side page-based programming model
- **Bootstrap 5** - Responsive UI framework with modern components
- **FontAwesome** - Professional icon library
- **jQuery** - Enhanced client-side interactivity
- **Custom JavaScript** - Advanced filtering, sorting, and modal management

### **Backend Technologies**
- **.NET 8** - Latest LTS version of the .NET framework
- **Entity Framework Core** - Code-first ORM with automated migrations
- **ASP.NET Core Identity** - Complete authentication and authorization system
- **SQL Server** - Production-grade relational database
- **HttpClient** - Robust API integration with retry policies

### **Architecture & Patterns**
- **Clean Architecture** - Clear separation of concerns across layers
- **Repository Pattern** - Abstracted data access layer
- **Dependency Injection** - Built-in .NET DI container
- **Result Pattern** - Consistent error handling and operation results
- **SOLID Principles** - Maintainable and extensible code design

### **External Integrations**
- **Clash Royale API** - Official Supercell API for real-time game data
- **GitHub Actions** - Automated build, test, and deployment pipeline
- **WebDeploy** - Seamless deployment to production hosting

### **DevOps & Deployment**
- **GitHub Actions Workflows** - Separate pipelines for development and production
- **Environment-specific Configurations** - Secure secrets management
- **Automated Database Migrations** - Zero-downtime deployments
- **Health Checks & Monitoring** - Application startup validation

## 🏗️ Project Structure

```
ClashRoyaleWarTracker/
├── .github/workflows/                  # CI/CD pipelines
│   ├── main.yml                        # Production deployment
│   └── dev-deploy.yml                  # Development deployment
├── ClashRoyaleWarTracker.Web/             # Razor Pages presentation layer
│   ├── Pages/                          # Razor pages and page models
│   ├── Areas/Identity/                 # Authentication pages
│   ├── wwwroot/                        # Static files (CSS, JS, images)
│   ├── Properties/launchSettings.json  # Development profiles
│   └── Program.cs                      # Application entry point
├── ClashRoyaleWarTracker.Application/     # Business logic and interfaces
│   ├── Interfaces/                     # Service contracts
│   ├── Models/                         # Domain models and DTOs
│   ├── Services/                       # Business logic implementation
│   └── Helpers/                        # Utility classes
├── ClashRoyaleWarTracker.Infrastructure/  # Data access and external services
│   ├── Repositories/                   # Data access implementation
│   ├── Http/                           # API clients
│   ├── Migrations/                     # EF Core database migrations
│   ├── Services/                       # Infrastructure services
│   └── Configuration/                  # Configuration models
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
```
git clone https://github.com/Gabe1L2/ClashRoyaleWarTracker.git
cd ClashRoyaleWarTracker
```

2. **Set up User Secrets for development:**
```
cd ClashRoyaleWarTracker.Web
dotnet user-secrets init
dotnet user-secrets set "ClashRoyaleApi:ApiKey" "YOUR_ACTUAL_API_KEY"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING"
```

3. **Configure default users (optional):**
```
dotnet user-secrets set "DefaultUsers:0:Username" "admin"
dotnet user-secrets set "DefaultUsers:0:Password" "YourPassword123!"
dotnet user-secrets set "DefaultUsers:0:Role" "Admin"
```

4. **Update the database:**
```
dotnet ef database update --project ClashRoyaleWarTracker.Infrastructure --startup-project ClashRoyaleWarTracker.Web
```

5. **Run the application:**
```
dotnet run --project ClashRoyaleWarTracker.Web
```

6. **Access the application:**
   - Navigate to `https://localhost:7108` (or the URL shown in your terminal)
   - Use the configured admin credentials to log in

## 🎮 How to Get a Clash Royale API Key

1. Visit the [Clash Royale Developer Portal](https://developer.clashroyale.com/)
2. Create an account or log in with your Supercell ID
3. Create a new API key:
   - **Name**: Your project name
   - **Description**: Brief description of your project
   - **IP Addresses**: Add your development/production IP address(es)
4. Copy the generated API key and add it to your configuration

## 🔧 Configuration

The application supports multiple environments with different configuration files:

- **Development**: Uses `appsettings.Development.json` with development database and API keys
- **Production**: Uses environment variables and secrets for secure configuration
- **Launch Profiles**: Separate profiles for Development and Production environments

## 🎯 Key Features in Detail

### **Interactive Dashboard**
- Real-time player statistics with fame/attack averages
- Advanced filtering by clan, trophy level, and activity status
- Sortable columns with visual indicators
- Player search functionality
- One-click data updates (weekly or backlog)

### **Clan Management**
- Add clans using Clash Royale clan tags
- Automatic clan validation and data retrieval
- Support for multiple clans simultaneously
- Real-time clan statistics tracking

### **Data Updates**
- **Weekly Update**: Fetches the last week of clan war data
- **Backlog Update**: Fetches the last 10 weeks for comprehensive historical data
- Progress feedback and error handling
- Automatic player status tracking

### **User Management**
- Role-based authentication (Admin/Member)
- Environment-specific default users
- Secure password policies
- Session management

## 🚀 Deployment & Production

The application is deployed using a modern DevOps pipeline:

### **Hosting Stack**
- **MonsterASP.NET**: Professional Windows-based ASP.NET hosting
- **SQL Server**: Fully managed database hosting
- **Custom Domain**: Production-ready domain configuration
- **SSL/TLS**: Secure HTTPS encryption

### **CI/CD Pipeline**
- **Automated Builds**: GitHub Actions for continuous integration
- **Environment Separation**: Distinct development and production pipelines
- **Secrets Management**: Secure handling of API keys and connection strings
- **Zero-Downtime Deployment**: WebDeploy for seamless updates

### **Monitoring & Maintenance**
- Application health checks during startup
- Comprehensive logging with structured output
- Error handling with user-friendly feedback
- Database migration automation

## 🎯 Learning Outcomes

Through this project, I gained comprehensive experience with:

- **Full-Stack .NET Development** - End-to-end application development
- **Production Deployment** - Real-world hosting and CI/CD implementation
- **Clean Architecture** - Scalable and maintainable code organization
- **Modern Web UI** - Responsive design with advanced user interactions
- **API Integration** - External service consumption with error handling
- **Database Design** - Relational modeling and migration strategies
- **Authentication Systems** - Security implementation and user management
- **DevOps Practices** - Automated deployment and environment management
- **Performance Optimization** - Efficient data loading and UI responsiveness

## 🤝 Contributing

This project showcases modern .NET development practices and is open for feedback and suggestions! If you'd like to contribute:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/new-feature`)
3. Commit your changes (`git commit -am 'Add new feature'`)
4. Push to the branch (`git push origin feature/new-feature`)
5. Create a Pull Request

## 📝 License

This project is open source and available under the [MIT License](LICENSE).

---

**✨ This project demonstrates a complete, production-ready .NET 8 application with modern development practices, clean architecture, and professional deployment infrastructure.**

For questions, suggestions, or to see the live application, feel free to open an issue or reach out!