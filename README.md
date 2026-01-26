#  Parking Management System
 
A comprehensive parking management solution built with ASP.NET Core, featuring NFC payments, role-based access control, and automated CI/CD pipelines.
 
## Quick Start
 
### Prerequisites
- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- **SQLite** (comes with .NET)
- **Git** for version control
 
### Get Started in 3 Steps
 
```bash
# 1. Clone the repository
git clone <repository-url>
cd Software_Construction
 
# 2. Build and run the API
cd "CSharp Parking API"
dotnet restore
dotnet run
 
# 3. Run tests (optional)
cd "../CSharp Parking Tests"
dotnet test
```
 
The API will start on `http://localhost:5000` (or configured port).
 
##  Project Structure
 
```
Software_Construction/
├── CSharp Parking API/           # Main ASP.NET Core API
│   ├── Controllers/             # REST API endpoints
│   ├── Services/                # Business logic layer
│   ├── Models/                  # Data models & DTOs
│   ├── Database/                # SQLite database setup
│   └── Migrations/              # EF Core migrations
├── CSharp Parking Tests/         # Comprehensive test suite
│   ├── API Tests/               # Integration tests
│   └── Services/                # Unit tests
├── Python Parking API/          # Legacy Python implementation
├── Test/                        # Integration tests
├── .github/workflows/           # CI/CD pipelines
└── docs/                        # Technical documentation
```
 
## 🛠️ Development Commands
 
### API Development
```bash
# Restore dependencies
dotnet restore
 
# Build in debug mode
dotnet build
 
# Run API locally
dotnet run
 
# Run with hot reload (development)
dotnet watch run
```
 
### Database Operations
```bash
# Add new migration
dotnet ef migrations add "MigrationName"
 
# Update database
dotnet ef database update
 
# Generate SQL script
dotnet ef migrations script
```
 
### Testing
```bash
# Run all tests
dotnet test
 
# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
 
# Run specific test class
dotnet test --filter "TestClassName"
 
# Run tests in watch mode
dotnet watch test
```
 
### Code Quality
```bash
# Format code
dotnet format
 
# Run static analysis
dotnet build /p:RunAnalyzersDuringBuild=true
```
 
## Authentication & Roles
 
The system uses **JWT-based authentication** with **3 user roles**:
 
- **ParkingUser**: Regular users (book parking, view reservations)
- **ParkingLotAdmin**: Location managers (manage specific parking lot)
- **SuperAdmin**: System administrators (full access)
 
### Testing Authentication
```bash
# Get auth token (replace with actual credentials)
curl -X POST http://localhost:5000/api/v2/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"youruser","password":"yourpass"}'
```
 
##  Testing Strategy
 
### Test Coverage: 84.37% Branch Coverage
 
**Test Types:**
- **API Tests**: End-to-end testing via HTTP client
- **Service Tests**: Unit tests for business logic
- **Integration Tests**: Full workflow testing
 
### Run Tests Locally
```bash
cd "CSharp Parking Tests"
dotnet test --verbosity normal --logger "trx;LogFileName=test-results.trx"
```
 
### Coverage Report
```bash
# Generate coverage report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator "-reports:test-results/**/*.xml" "-targetdir:coveragereport"
```
 
##  Deployment
 
### CI/CD Pipeline
 
**Triggers:** Push to any branch
- **Staging**: Automatic deployment for feature branches
- **Production**: Manual deployment for `master` branch only
 
### Manual Deployment
```bash
# Build for production
dotnet publish -c Release -o ./publish
 
# Create deployment package
tar -czf api-package.tar.gz -C ./publish .
 
# Deploy (configure your target environment)
# Azure App Service, Docker, AWS, etc.
```
 
### Environment Variables
```bash
# Database
ConnectionStrings__DefaultConnection="Data Source=parking.db"
 
# JWT Configuration
Jwt__Key="your-secret-key-here"
Jwt__Issuer="parking-api"
Jwt__Audience="parking-users"
 
# Server Configuration
ASPNETCORE_URLS="http://+:5000"
ASPNETCORE_ENVIRONMENT="Production"
```
 
## 🔧 Key Features
 
### 💳 NFC Payment System
- Contactless parking payments
- Hotel discount integration (20% off for hotel guests)
- Balance verification before payment
- Automatic session creation
 
### 🏨 Hotel Integration
- Guest registration system
- Automatic discount application
- Check-in/check-out management
 
### 📊 Advanced Analytics
- Real-time performance monitoring
- Test execution timing reports
- Coverage metrics tracking
 
### 🔒 Security Features
- JWT token authentication
- Role-based access control (RBAC)
- Token revocation (logout)
- Secure password hashing
 
## 🐛 Troubleshooting
 
### Common Issues
 
**Port already in use:**
```bash
# Find process using port 5000
netstat -ano | findstr :5000
# Kill the process
taskkill /PID <PID> /F
```
 
**Database connection errors:**
```bash
# Ensure database exists
dotnet ef database update
# Check connection string in appsettings.json
```
 
**Test failures:**
```bash
# Clean and rebuild tests
dotnet clean
dotnet restore
dotnet build
dotnet test --no-build
```
 
### Debug Mode
```bash
# Run with detailed logging
dotnet run --environment Development --verbose
```
 
## Documentation
 
- **Technical Documentation**: See detailed docs in project folders
- **API Endpoints**: Swagger UI at `/swagger` when running locally
- **Database Schema**: `database_schema.sql`
- **Test Coverage**: `TESTS_CICD.md`
 
##  Contributing
 
### Development Workflow
1. **Create feature branch**: `git checkout -b feature/your-feature`
2. **Make changes** and **write tests**
3. **Run tests**: `dotnet test`
4. **Commit**: `git commit -m "feat: your feature description"`
5. **Push**: `git push origin feature/your-feature`
6. **Create PR** to trigger CI/CD
 
### Code Standards
- Follow C# naming conventions
- Write comprehensive unit tests
- Maintain 80%+ code coverage
- Use meaningful commit messages
 
### Pull Request Requirements
- ✅ All tests pass
- ✅ Code coverage maintained
- ✅ Security scan passes
- ✅ Code review approved
 
##  Support
 
For questions or issues:
1. Check existing documentation
2. Review CI/CD logs for errors
3. Create an issue with detailed reproduction steps
 
---
 
**Built with ❤️ using ASP.NET Core 8.0, SQLite, and modern DevOps practices**