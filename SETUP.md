# NCC ERP Workflow W2 - Backend Setup Guideline

## 1. Clone Source Code

- Clone the repository:
  ```
  git clone https://github.com/ncc-erp/ncc-erp-workflow-w2.git
  ```
- Open the project folder in **Visual Studio Code** (màu xanh) hoặc **Visual Studio** (màu tím).

---

## 2. Install Required Software

- **.NET SDK 6.0.428** ([Download SDK](https://dotnet.microsoft.com/download/dotnet/6.0))
- **ASP.NET Core 6.0 Runtime** ([Download Runtime](https://dotnet.microsoft.com/download/dotnet/6.0))
- **PostgreSQL** (or your configured DB)

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
  ```
  git checkout dev
  ```

---

## 4. Install ABP CLI Tool

- In terminal:
  ```
  dotnet tool install --global Volo.Abp.Cli --version 6.0.0
  ```

---

## 5. Restore NuGet Packages

- In project root:
  ```
  dotnet restore
  ```

---

## 6. Install ABP Libs

- In project root:
  ```
  abp install-libs
  ```

---

## 7. Configure Database and App Settings

- Edit `src/W2.Web/appsettings.json` and `src/W2.Web/appsettings.Development.json`:
  - Update `"ConnectionStrings": { "Default": ... }` to match your DB server.
  - Update other settings as needed (see your sample configs above).

- Edit `src/W2.DbMigrator/appsettings.json`:
  - Update `"ConnectionStrings": { "Default": ... }` and other relevant settings.

---

## 8. Database Migration

- Run database migration to create/update tables:
  ```
  cd src/W2.DbMigrator
  dotnet run
  ```

---

## 9. Build and Run Backend

### **A. Using Visual Studio Code**

1. **Open folder** `ncc-erp-workflow-w2` in VS Code.
2. **Build solution**:
    ```
    dotnet build W2.sln
    ```
3. **Run backend**:
    ```
    cd src/W2.Web
    dotnet run
    ```
4. **Debug**:  
   - Set breakpoints in code.
   - Press `F5` or click "Run and Debug" to start debugging.

---

### **B. Using Visual Studio**

1. **Open solution** `W2.sln` in Visual Studio.
2. **Select startup project**: Right-click `W2.Web` → "Set as Startup Project".
3. **Build solution**:  
   - Press `Ctrl+Shift+B` or click "Build Solution".
4. **Run backend**:  
   - Press `F5` or click the green "Start" button.
5. **Debug**:  
   - Set breakpoints in code.
   - Use Visual Studio's debugger for step-by-step inspection.

---

## 10. Access Application

- Backend API will be available at: [http://localhost:4433](http://localhost:4433)

---

## 11. Logs & Debugging

- Backend logs are stored in: `src/W2.Web/Logs/logs.txt`
- Debug using breakpoints in your IDE.

---

## 12. Useful Commands

- Build solution:
  ```
  dotnet build W2.sln
  ```
- Run unit tests:
  ```
  dotnet test
  ```

---

## 13. Notes & Troubleshooting

- If you get SDK version errors, check `global.json` and installed SDKs (`dotnet --list-sdks`).
- If you get DB connection errors, verify connection string and DB access.
- If you get missing package errors, run `dotnet restore` and `abp install-libs` again.
- Make sure your PostgreSQL service is running and accessible.

---
