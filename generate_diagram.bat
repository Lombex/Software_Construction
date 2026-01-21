@echo off
echo ============================================
echo    Parking Management System ERD Generator
echo ============================================
echo.
echo This script will help you generate visual diagrams from the PlantUML file.
echo.
echo Options:
echo 1. Use online PlantUML server (recommended)
echo 2. Install PlantUML locally (requires Java)
echo.
echo Option 1 - Online Generation:
echo ---------------------------
echo 1. Go to: https://www.plantuml.com/plantuml
echo 2. Copy the contents of parking_system_erd.puml
echo 3. Paste into the online editor
echo 4. Click "Submit" to generate PNG/SVG diagram
echo.
echo Alternative online tools:
echo - https://planttext.com/
echo - https://www.plantuml.com/plantuml/uml/
echo.
echo Option 2 - Local Installation:
echo ----------------------------
echo 1. Install Chocolatey: https://chocolatey.org/
echo 2. Run: choco install plantuml
echo 3. Then run: plantuml parking_system_erd.puml
echo.
echo The generated diagram will show:
echo - 14 Controllers (Presentation Layer)
echo - 13 Services (Business Logic Layer)
echo - 14 Models (Data Access Layer)
echo - All database relationships from EF Core schema
echo.
pause