# DbViewer - 資料庫檢視器

一個功能強大的資料庫檢視和管理工具，支援多種資料庫類型，提供 Console、Web API 和 Blazor Web 三種介面。

## 功能特色

- 🗄️ **多資料庫支援**：支援 SQL Server、MySQL、PostgreSQL 和 Oracle
- 🔌 **連線管理**：支援多種資料庫連線設定和測試
- 📊 **資料庫瀏覽**：檢視資料庫清單和資料表結構
- 💻 **SQL 執行**：執行各種 SQL 查詢和命令
- 📋 **結果顯示**：以表格形式顯示查詢結果
- ⚠️ **錯誤處理**：友善的錯誤訊息和異常處理
- 🌐 **多介面支援**：Console、Web API 和 Blazor Web

## 專案結構

```
DbViewer/
├── src/
│   ├── Application.Contracts/     # 應用程式合約介面
│   ├── Application/               # 應用程式邏輯
│   ├── Domain/                    # 領域模型
│   ├── Domain.Shared/             # 共享領域模型
│   ├── Infrastructure/            # 基礎設施層
│   ├── Common/                    # 共用工具
│   └── Presentation/              # 展示層
│       ├── Console/               # Console 應用程式
│       ├── WebApi/                # Web API 專案
│       └── BlazorWeb/             # Blazor Web 專案
└── test/                          # 測試專案
```

## 快速開始

### 前置需求

- .NET 9.0 SDK
- 支援的資料庫系統（SQL Server、MySQL、PostgreSQL 或 Oracle）

### 安裝與執行

1. **複製專案**
   ```bash
   git clone <repository-url>
   cd DbViewer
   ```

2. **還原套件**
   ```bash
   dotnet restore
   ```

3. **執行不同版本**

   **Console 版本：**
   ```bash
   cd src/Presentation
   dotnet run
   ```

   **Web API 版本：**
   ```bash
   cd src/Presentation/WebApi
   dotnet run
   ```
   然後開啟瀏覽器訪問 `https://localhost:5001/swagger` 查看 API 文件

   **Blazor Web 版本：**
   ```bash
   cd src/Presentation/BlazorWeb
   dotnet run
   ```
   然後開啟瀏覽器訪問 `https://localhost:5001`

## 使用說明

### Console 版本

1. 啟動應用程式後，選擇資料庫類型
2. 輸入連線資訊（伺服器、使用者名稱、密碼等）
3. 測試連線或直接連線到資料庫
4. 執行 SQL 查詢或瀏覽資料庫結構

### Web API 版本

Web API 提供以下端點：

- `POST /api/database/connect` - 連線到資料庫
- `POST /api/database/disconnect` - 中斷資料庫連線
- `POST /api/database/test-connection` - 測試資料庫連線
- `GET /api/database/status` - 取得連線狀態
- `GET /api/database/databases` - 取得資料庫清單
- `POST /api/database/switch-database` - 切換資料庫
- `GET /api/database/tables` - 取得資料表清單
- `POST /api/database/execute-query` - 執行 SQL 查詢

### Blazor Web 版本

1. 開啟瀏覽器訪問應用程式
2. 點選「資料庫管理」頁面
3. 在左側面板設定資料庫連線資訊
4. 點選「測試連線」或「連線」按鈕
5. 連線成功後可瀏覽資料庫和資料表清單
6. 在右側面板輸入 SQL 查詢並執行

## 設定檔

應用程式使用 `appsettings.json` 進行設定：

```json
{
  "Database": {
    "DefaultTimeout": 30,
    "MaxRetries": 3
  },
  "AppConfiguration": {
    "ConnectionsFilePath": "connections.json",
    "SettingsFilePath": "settings.json",
    "HistoryFilePath": "history.txt"
  }
}
```

## 支援的資料庫

- **SQL Server** - 使用 Microsoft.Data.SqlClient
- **MySQL** - 使用 MySql.Data
- **PostgreSQL** - 使用 Npgsql
- **Oracle** - 使用 Oracle.ManagedDataAccess.Core

## 開發

### 建置專案

```bash
dotnet build
```

### 執行測試

```bash
dotnet test
```

### 發佈應用程式

```bash
# Console 版本
dotnet publish src/Presentation/Presentation.csproj -c Release

# Web API 版本
dotnet publish src/Presentation/WebApi/WebApi.csproj -c Release

# Blazor Web 版本
dotnet publish src/Presentation/BlazorWeb/BlazorWeb.csproj -c Release
```

## 授權

此專案使用 MIT 授權條款。

## 貢獻

歡迎提交 Issue 和 Pull Request 來改善此專案。 