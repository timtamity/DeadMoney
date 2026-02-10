@echo off
cls
echo ======================================================
echo   DEAD MONEY - Database Sync Tool
echo ======================================================
echo.

:: PHASE 1: Build Check
echo [PHASE 1] Building Solution...
dotnet build
if %errorlevel% neq 0 (
    echo.
    echo ERROR: Build failed. Please fix compilation errors before migrating.
    pause
    exit /b %errorlevel%
)
echo SUCCESS: Project builds.
echo.

:: PHASE 2: Generate Migration
echo [PHASE 2] Generating EF Migration (InitialLeagueSetup)...
dotnet ef migrations add InitialLeagueSetup ^
    --project DeadMoney.Data ^
    --startup-project DeadMoney.Web ^
    --context DeadMoneyDbContext ^
    --output-dir Migrations
if %errorlevel% neq 0 (
    echo.
    echo ERROR: Migration generation failed. 
    echo Check if the Migrations folder already exists or if DbContext is locked.
    pause
    exit /b %errorlevel%
)
echo SUCCESS: Migration files created in DeadMoney.Data/Migrations.
echo.

:: PHASE 3: Update Database
echo [PHASE 3] Applying Schema to SQL Server...
echo (Using connection string from DeadMoney.Web User Secrets)
dotnet ef database update ^
    --project DeadMoney.Data ^
    --startup-project DeadMoney.Web ^
    --context DeadMoneyDbContext
if %errorlevel% neq 0 (
    echo.
    echo ERROR: Database update failed. 
    echo Ensure your SQL Server is running and the connection string is correct.
    pause
    exit /b %errorlevel%
)
echo.
echo ======================================================
echo   DATABASE SYNC COMPLETE!
echo   Schemas 'League' and 'Auth' are now live.
echo ======================================================
pause