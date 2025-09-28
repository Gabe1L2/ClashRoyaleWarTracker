# 🏰 Clash Royale War Tracker

A comprehensive .NET 8 Razor Pages application for automatically tracking and managing Clash Royale clan war statistics and performance metrics. **Now live and deployed with advanced features!**

## 🎯 Project Overview

This project demonstrates full-stack .NET development expertise, showcasing modern web application patterns, clean architecture, and production deployment. The application automatically tracks Clash Royale clan(s) war statistics, providing updated data and comprehensive analytics for clan management and performance tracking.

**🌐 Live Demo**: [crwartracker.com](https://crwartracker.com) - Guest access available inside the application for demonstration purposes.

## 🚀 Current Status

**🟢 Fully Implemented Features:**
- ✅ **Advanced War History Spreadsheet** - Interactive grid with sticky columns, real-time filtering, and sortable data
- ✅ **Player Notes System** - Add, edit, and manage player notes with tooltip display
- ✅ **Trophy Level Management** - Separate 5k+ and sub-5k trophy level tracking and analytics
- ✅ **Roster Management System** - Automated roster assignments with in-clan status tracking
- ✅ **User Management System** - Role-based access control with admin interface
- ✅ **Scheduled Task Automation** - External API endpoint for automated weekly updates
- ✅ **Central Time Zone Service** - Consistent CST/CDT time handling across the application
- ✅ **Clan Management System** - Add, update, delete, and view multiple clans with configurable war history
- ✅ **Real-time Data Updates** - Weekly and configurable backlog data synchronization
- ✅ **Player Performance Analytics** - Fame/attack averages, trophy tracking, activity status management
- ✅ **Advanced Filtering & Search** - Filter by clan, trophy level, activity status, and player search
- ✅ **User Authentication & Authorization** - Secure multi-role login system
- ✅ **Responsive UI** - Bootstrap 5 with mobile-friendly design and modern interactions
- ✅ **Clean Architecture Implementation** - Proper separation of concerns with SOLID principles
- ✅ **Database Management** - Entity Framework Core with automated migrations
- ✅ **Clash Royale API Integration** - Real-time game data retrieval with robust error handling
- ✅ **Production Deployment** - Live application with CI/CD pipeline

**🟡 Enhanced Features:**
- 🔄 **Player Status Tracking** - Active/Inactive/L2W status management with visual indicators
- 🔄 **War History Editing** - Inline editing of war statistics with validation
- 🔄 **Automated Roster Generation** - Algorithm-based clan assignments by performance
- 🔄 **In-Clan Status Verification** - API-based verification of player clan membership
- 🔄 **Performance-based Sorting** - Default sorting by fame/attack averages
- 🔄 **Multi-level Permission System** - Admin/Management/Coleader/Member/Guest role hierarchy

**🔴 Future Enhancements:**
- 📊 Data visualization and charts
- 📈 Performance trend analysis over time
- 🏆 Clan ranking and comparison system
- 📱 Mobile app companion

## 🛠️ Technology Stack & Architecture

### **Production Environment**
- **Hosting Provider**: [MonsterASP.NET](https://www.monsterasp.net/) - Professional ASP.NET hosting
- **Database**: SQL Server hosting with automated backups
- **Deployment**: Automated CI/CD pipeline using GitHub Actions with WebDeploy
- **Environment Management**: Separate Development and Production configurations
- **Time Zone Management**: Centralized CST/CDT time service

### **Frontend Technologies**
- **Razor Pages** - Server-side page-based programming model with modern UI patterns
- **Bootstrap 5** - Responsive UI framework with custom components
- **FontAwesome** - Professional icon library
- **jQuery** - Enhanced client-side interactivity and AJAX operations
- **Custom JavaScript** - Advanced filtering, sorting, modal management, and real-time updates

### **Backend Technologies**
- **.NET 8** - Latest LTS version with performance optimizations
- **Entity Framework Core** - Code-first ORM with complex migrations
- **ASP.NET Core Identity** - Multi-role authentication and authorization system
- **SQL Server** - Production-grade relational database with optimized queries
- **HttpClient** - Robust API integration with retry policies and error handling

### **Architecture & Patterns**
- **Clean Architecture** - Clear separation across Application, Infrastructure, and Web layers
- **Repository Pattern** - Abstracted data access with complex query optimization
- **Dependency Injection** - Built-in .NET DI container with scoped services
- **Result Pattern** - Consistent error handling and operation results
- **SOLID Principles** - Maintainable and extensible code design
- **Time Zone Abstraction** - Centralized time handling service

### **External Integrations**
- **Clash Royale API** - Official Supercell API for real-time game data
- **GitHub Actions** - Automated build, test, and deployment pipeline
- **WebDeploy** - Seamless deployment to production hosting
- **Scheduled Task Integration** - External automation endpoints with security keys

### **DevOps & Deployment**
- **GitHub Actions Workflows** - Separate pipelines for development and production
- **Environment-specific Configurations** - Secure secrets management
- **Automated Database Migrations** - Zero-downtime deployments
- **Health Checks & Monitoring** - Application startup validation and logging

## 🏗️ Project Structure

```
ClashRoyaleWarTracker/
├── .github/workflows/                  # CI/CD pipelines
├── ClashRoyaleWarTracker.Web/             # Razor Pages presentation layer
│   ├── Pages/                          # Razor pages and page models
│   │   ├── Index.cshtml                # War history spreadsheet
│   │   ├── Rosters.cshtml              # Roster management
│   │   ├── UserManagement.cshtml       # User administration
│   │   └── ScheduledTask.cshtml        # Automation endpoint
│   ├── Areas/Identity/                 # Authentication pages
│   ├── wwwroot/                        # Static files (CSS, JS, images)
│   └── Program.cs                      # Application entry point
├── ClashRoyaleWarTracker.Application/     # Business logic and interfaces
│   ├── Interfaces/                     # Service contracts
│   ├── Models/                         # Domain models and DTOs
│   └── Services/                       # Business logic implementation
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
```bash
git clone https://github.com/Gabe1L2/ClashRoyaleWarTracker.git
cd ClashRoyaleWarTracker
```

2. **Set up User Secrets for development:**
```bash
cd ClashRoyaleWarTracker.Web
dotnet user-secrets init
dotnet user-secrets set "ClashRoyaleApi:ApiKey" "YOUR_ACTUAL_API_KEY"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING"
```

3. **Configure default users (optional):**
```bash
dotnet user-secrets set "DefaultUsers:0:Username" "admin"
dotnet user-secrets set "DefaultUsers:0:Password" "YourPassword123!"
dotnet user-secrets set "DefaultUsers:0:Role" "Admin"
```

4. **Configure scheduled task security (optional):**
```bash
dotnet user-secrets set "ScheduledTask:SecurityKey" "YOUR_SECURITY_KEY"
```

5. **Update the database:**
```bash
dotnet ef database update --project ClashRoyaleWarTracker.Infrastructure --startup-project ClashRoyaleWarTracker.Web
```

6. **Run the application:**
```bash
dotnet run --project ClashRoyaleWarTracker.Web
```

7. **Access the application:**
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

### **Scheduled Tasks**
Configure external automation with security keys:
```json
{
  "ScheduledTask": {
    "SecurityKey": "your-secure-key-here"
  }
}
```

## 🎯 Key Features in Detail

### **War History Spreadsheet**
- **Interactive Grid**: Sticky columns with horizontal and vertical scrolling
- **Real-time Filtering**: Multi-criteria filtering by clan, status, and player search
- **Performance Sorting**: Default sorting by fame/attack averages (descending)
- **Trophy Level Separation**: Toggle between 5k+ and sub-5k trophy brackets
- **Visual Status Indicators**: Color-coded rows for Active/Inactive/L2W players
- **Editable War Data**: Inline editing of fame, attacks, and boat attacks with validation

### **Player Management**
- **Player Notes**: Add, edit, and view player-specific notes (max 100 characters)
- **Status Management**: Active/Inactive/L2W status tracking with visual indicators
- **Performance Analytics**: Fame/attack averages calculated across configurable weeks
- **Trophy Bracket Analysis**: Separate tracking for 5k+ and sub-5k trophy levels

### **Roster Management**
- **Automated Assignments**: Algorithm-based roster generation by performance metrics
- **In-Clan Status Tracking**: Real-time verification of player clan membership
- **Bulk Operations**: Multi-select operations for efficient roster management
- **Clan-based Organization**: Sortable rosters grouped by clan with collapsible sections

### **Clan Management**
- **Multi-Clan Support**: Add and manage multiple clans simultaneously
- **Configurable History**: Specify weeks of war history to fetch (0-10 weeks)
- **Automatic Validation**: Real-time clan verification through Clash Royale API
- **Historical Tracking**: Comprehensive clan war history with trophy progression

### **User Management & Security**
- **Role-based Access Control**: Admin/Management/Coleader/Member/Guest hierarchy
- **User Administration**: Create, edit, and delete users with role assignments
- **Permission System**: Feature-specific permissions (manage clans, modify data, etc.)
- **Secure Authentication**: ASP.NET Core Identity with customizable password policies

### **Data Updates & Automation**
- **Weekly Updates**: Fetch the last week of clan war data with configurable player average weeks
- **Backlog Updates**: Comprehensive historical data updates (up to 10 weeks)
- **Player Average Updates**: Trophy-level specific average recalculations
- **Scheduled Tasks**: External API endpoints for automated updates with security keys

### **Advanced UI Features**
- **Responsive Design**: Mobile-friendly Bootstrap 5 interface
- **Interactive Modals**: Tabbed interfaces for player actions and clan management
- **Real-time Search**: Instant filtering without page reloads
- **Sortable Tables**: Multi-column sorting with visual indicators
- **Tooltip System**: Contextual help and player notes display
- **Color-coded Performance**: Visual performance indicators throughout the interface

## 🚀 Deployment & Production

### **Hosting Stack**
- **Live Website**: [crwartracker.com](https://crwartracker.com) with guest access available
- **MonsterASP.NET**: Professional Windows-based ASP.NET hosting
- **SQL Server**: Fully managed database hosting with automated backups
- **SSL/TLS**: Secure HTTPS encryption with custom domain
- **Time Zone Consistency**: Centralized CST/CDT time handling

### **CI/CD Pipeline**
- **Automated Builds**: GitHub Actions for continuous integration
- **Environment Separation**: Distinct development and production pipelines
- **Secrets Management**: Secure handling of API keys and connection strings
- **Zero-Downtime Deployment**: WebDeploy for seamless updates
- **Database Migrations**: Automated schema updates on deployment

### **Monitoring & Maintenance**
- **Application Health Checks**: Startup validation and dependency verification
- **Comprehensive Logging**: Structured logging with performance tracking
- **Error Handling**: User-friendly error messages with detailed logging

### **External Automation**
- **Scheduled Task Endpoints**: Secure API endpoints for external automation
- **Security Key Authentication**: Protected access for automated systems

## 🎯 Learning Outcomes

Through this project, I gained comprehensive experience with:

- **Full-Stack .NET Development** - End-to-end application development with modern patterns
- **Production Deployment** - Real-world hosting and CI/CD implementation
- **Clean Architecture** - Scalable and maintainable code organization
- **Advanced UI Development** - Responsive design with complex user interactions
- **API Integration** - External service consumption with robust error handling
- **Database Design** - Complex relational modeling and migration strategies
- **Authentication Systems** - Multi-role security implementation and user management
- **DevOps Practices** - Automated deployment and environment management
- **Performance Optimization** - Efficient data loading and UI responsiveness
- **Time Zone Management** - Consistent time handling across distributed systems
- **Complex Business Logic** - Algorithm development for roster assignments and analytics

## 🚀 API Endpoints

### **Scheduled Tasks** (Authentication Required)
- `GET /ScheduledTask?task=weekly&weeks=4&key=YOUR_KEY` - Automated weekly update
- Parameters:
  - `task`: Task type (weekly/weeklyupdate)
  - `weeks`: Number of weeks for player averages (1-10)
  - `key`: Security key for authentication

## 📝 License

This project is open source and available under the [MIT License](LICENSE).

---

**✨ This project demonstrates a complete, production-ready .NET 8 application with modern development practices, clean architecture, advanced features, and professional deployment infrastructure.**

**🌐 Experience it live at [crwartracker.com](https://crwartracker.com) with guest access available for demonstration!**

For questions, suggestions, or to see the live application, feel free to open an issue or reach out!