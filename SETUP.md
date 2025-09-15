# NCC ERP Workflow W2 - Backend Setup Guideline

## 1. Clone Source Code

- Clone the repository:
  ```bash
  git clone https://github.com/ncc-erp/ncc-erp-workflow-w2.git
  ```
- Open the project folder in **Visual Studio Code** (blue icon) or **Visual Studio** (purple icon).

---

## 2. Install Required Software

- **.NET SDK 6.0.428** ([Download SDK](https://dotnet.microsoft.com/download/dotnet/6.0))
  - Ensure you install the correct SDK version defined in the `global.json` file:
    ```json
    {
      "sdk": {
        "version": "6.0.428"
      }
    }
    ```
  - Check the installed version:
    ```bash
    dotnet --list-sdks
    ```
- **ASP.NET Core 6.0 Runtime** ([Download Runtime](https://dotnet.microsoft.com/download/dotnet/6.0))
- **PostgreSQL** (or your configured database)

### For Visual Studio Code:
- **Visual Studio Code** ([Download](https://code.visualstudio.com/))

### For Visual Studio:
- **Visual Studio 2022** ([Download](https://visualstudio.microsoft.com/downloads/))
- In Visual Studio Installer, install:
  - ".NET desktop development"
  - "ASP.NET and web development"

---

## 3. Checkout Development Branch

- In terminal:
  ```bash
  git checkout dev
  ```

---

## 4. Install ABP CLI Tool

- In terminal:
  ```bash
  dotnet tool install --global Volo.Abp.Cli --version 6.0.0
  ```

---

## 5. Restore NuGet Packages

- In the project root:
  ```bash
  dotnet restore
  ```

---

## 6. Install ABP Libraries

- In the project root:
  ```bash
  abp install-libs
  ```

---

## 7. Configure Database and App Settings

- Edit `src/W2.Web/appsettings.json` and `src/W2.Web/appsettings.Development.json`:
  - Update `"ConnectionStrings": { "Default": ... }` to match your database server.
  - Update other settings as needed (refer to your sample configurations).

- Edit `src/W2.DbMigrator/appsettings.json`:
  - Update `"ConnectionStrings": { "Default": ... }` and other relevant settings.

---

## 8. Database Migration

- Run database migration to create/update tables:
  ```bash
  cd src/W2.DbMigrator
  dotnet run
  ```

---

## 9. Build and Run Backend

### **A. Using Visual Studio Code**

1. **Open the folder** `ncc-erp-workflow-w2` in VS Code.

2. **Check .NET SDK Version**:
   - Ensure you have installed the correct SDK version defined in the `global.json` file:
     ```json
     {
       "sdk": {
         "version": "6.0.428"
       }
     }
     ```
   - Check the installed version:
     ```bash
     dotnet --list-sdks
     ```
   - If the correct version is not installed, download it from [Download SDK](https://dotnet.microsoft.com/download/dotnet/6.0).

3. **Build the solution**:
    ```bash
    dotnet build W2.sln
    ```

4. **Run the backend**:
    ```bash
    cd src/W2.Web
    dotnet run
    ```

5. **Debug**:  
   - Set breakpoints in the code.
   - Press `F5` or click "Run and Debug" to start debugging.

---

### **B. Using Visual Studio**

1. **Open the solution** `W2.sln` in Visual Studio.
2. **Select the startup project**: Right-click `W2.Web` → "Set as Startup Project".
3. **Build the solution**:  
   - Press `Ctrl+Shift+B` or click "Build Solution".
4. **Run the backend**:  
   - Press `F5` or click the green "Start" button.
5. **Debug**:  
   - Set breakpoints in the code.
   - Use Visual Studio's debugger for step-by-step inspection.

---

## 10. Access Application

- The backend API will be available at: [http://localhost:4433](http://localhost:4433)

---

## 11. Logs & Debugging

- Backend logs are stored in: `src/W2.Web/Logs/logs.txt`
- Debug using breakpoints in your IDE.

---

## 12. Useful Commands

- Build the solution:
  ```bash
  dotnet build W2.sln
  ```
- Run unit tests:
  ```bash
  dotnet test
  ```

---

## 13. Notes & Troubleshooting

- **Install the correct SDK version**:
  - Ensure the SDK version matches the `global.json` file:
    ```json
    {
      "sdk": {
        "version": "6.0.428"
      }
    }
    ```
  - Check the installed version:
    ```bash
    dotnet --list-sdks
    ```

- **Database connection issues**:
  - Check the connection string in `appsettings.json`.
  - Ensure PostgreSQL is running and accessible.

- **Missing packages**:
  - Run the following commands again:
    ```bash
    dotnet restore
    abp install-libs
    ```

- **Check PostgreSQL service**:
  - Ensure the PostgreSQL service is running and accessible.

---