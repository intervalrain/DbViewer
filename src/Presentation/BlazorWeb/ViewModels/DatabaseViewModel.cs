using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using Application.Contracts.Dtos;
using Application.Contracts.Services;
using Domain.Database.Entities;
using DomainConnectionInfo = Domain.Database.Entities.ConnectionInfo;
using DomainAppSettings = Domain.Configuration.Entities.AppSettings;

namespace Presentation.BlazorWeb.ViewModels;

public class DatabaseViewModel : INotifyPropertyChanged
{
    private readonly IDatabaseService _databaseService;
    private readonly DomainAppSettings _appSettings;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler? ConnectionStateChanged;
    public event EventHandler<DataTable>? QueryResultChanged;
    public event EventHandler<int>? AffectedRowsChanged;
    public event EventHandler<List<DatabaseInfo>>? DatabasesLoaded;
    public event EventHandler<List<string>>? TablesLoaded;

    // Private fields
    private string _server = "localhost";
    private string _username = string.Empty;
    private string _password = string.Empty;
    private int _port = 5432;
    private string _selectedDatabase = string.Empty;
    private string _sqlQuery = string.Empty;
    private DataTable? _queryResult;
    private int _affectedRows;
    private List<DatabaseInfo> _databases = new();
    private List<string> _tables = new();
    private bool _isBusy;
    private string? _errorMessage;
    private List<DomainConnectionInfo> _savedConnections = new();
    private string _connectionName = string.Empty;
    private bool _savePassword = true;

    // Pagination properties
    private int _currentPage = 1;
    private int _pageSize = 50;
    private int _totalRecords = 0;
    private DataTable? _fullQueryResult;

    public DatabaseViewModel(IDatabaseService databaseService, DomainAppSettings appSettings)
    {
        _databaseService = databaseService;
        _appSettings = appSettings;
    }

    // Connection Properties
    public string Server
    {
        get => _server;
        set
        {
            if (SetProperty(ref _server, value))
            {
                ClearError();
                OnPropertyChanged(nameof(CanConnect));
                OnPropertyChanged(nameof(CanTestConnection));
            }
        }
    }

    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                ClearError();
                OnPropertyChanged(nameof(CanConnect));
                OnPropertyChanged(nameof(CanTestConnection));
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                ClearError();
            }
        }
    }

    public int Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }

    public string SelectedDatabase
    {
        get => _selectedDatabase;
        set => SetProperty(ref _selectedDatabase, value);
    }

    // Query Properties
    public string SqlQuery
    {
        get => _sqlQuery;
        set
        {
            if (SetProperty(ref _sqlQuery, value))
            {
                ClearError();
                OnPropertyChanged(nameof(CanExecuteQuery));
            }
        }
    }

    public DataTable? QueryResult
    {
        get => _queryResult;
        private set
        {
            if (SetProperty(ref _queryResult, value))
            {
                QueryResultChanged?.Invoke(this, value!);
            }
        }
    }

    public int AffectedRows
    {
        get => _affectedRows;
        private set
        {
            if (SetProperty(ref _affectedRows, value))
            {
                AffectedRowsChanged?.Invoke(this, value);
            }
        }
    }

    // Collections
    public List<DatabaseInfo> Databases
    {
        get => _databases;
        private set
        {
            if (SetProperty(ref _databases, value))
            {
                DatabasesLoaded?.Invoke(this, value);
            }
        }
    }

    public List<string> Tables
    {
        get => _tables;
        private set
        {
            if (SetProperty(ref _tables, value))
            {
                TablesLoaded?.Invoke(this, value);
            }
        }
    }

    // Status
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(CanConnect));
                OnPropertyChanged(nameof(CanDisconnect));
                OnPropertyChanged(nameof(CanTestConnection));
                OnPropertyChanged(nameof(CanExecuteQuery));
            }
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsConnected => _databaseService.IsConnected;
    public string CurrentDatabase => _databaseService.CurrentDatabase;

    // Validation Properties
    public bool CanConnect => !string.IsNullOrWhiteSpace(Server) && !string.IsNullOrWhiteSpace(Username) && !IsBusy;
    public bool CanDisconnect => IsConnected && !IsBusy;
    public bool CanTestConnection => !string.IsNullOrWhiteSpace(Server) && !string.IsNullOrWhiteSpace(Username) && !IsBusy;
    public bool CanExecuteQuery => IsConnected && !string.IsNullOrWhiteSpace(SqlQuery) && !IsBusy;

    // Connection Records Properties
    public List<DomainConnectionInfo> SavedConnections
    {
        get => _appSettings.SavedConnections;
    }

    public string ConnectionName
    {
        get => _connectionName;
        set => SetProperty(ref _connectionName, value);
    }

    public bool SavePassword
    {
        get => _savePassword;
        set => SetProperty(ref _savePassword, value);
    }

    // Pagination Properties
    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (SetProperty(ref _currentPage, value))
            {
                UpdatePagedResult();
                OnPropertyChanged(nameof(CanGoToPreviousPage));
                OnPropertyChanged(nameof(CanGoToNextPage));
                OnPropertyChanged(nameof(PageInfo));
            }
        }
    }

    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (SetProperty(ref _pageSize, value) && value > 0)
            {
                CurrentPage = 1; // 重置到第一頁
                UpdatePagedResult();
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(PageInfo));
            }
        }
    }

    public int TotalRecords
    {
        get => _totalRecords;
        private set
        {
            if (SetProperty(ref _totalRecords, value))
            {
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(CanGoToPreviousPage));
                OnPropertyChanged(nameof(CanGoToNextPage));
                OnPropertyChanged(nameof(PageInfo));
            }
        }
    }

    public int TotalPages => TotalRecords > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 0;

    public bool CanGoToPreviousPage => CurrentPage > 1;
    public bool CanGoToNextPage => CurrentPage < TotalPages;

    public string PageInfo => TotalRecords > 0 
        ? $"第 {CurrentPage} 頁，共 {TotalPages} 頁 (總計 {TotalRecords} 筆記錄)"
        : "無資料";

    // Commands
    public async Task ConnectAsync()
    {
        if (!CanConnect) return;

        await ExecuteAsync(async () =>
        {
            var connectionInfo = new ConnectionInfoDto
            {
                Host = Server,
                Username = Username,
                Password = Password,
                Port = Port,
                DefaultDatabase = SelectedDatabase
            };

            var result = await _databaseService.ConnectAsync(connectionInfo, SelectedDatabase);
            result.Switch(
                () => {
                    OnPropertyChanged(nameof(IsConnected));
                    OnPropertyChanged(nameof(CurrentDatabase));
                    OnPropertyChanged(nameof(CanDisconnect));
                    ConnectionStateChanged?.Invoke(this, EventArgs.Empty);

                    // 自動保存連線記錄（如果有連線名稱）
                    if (!string.IsNullOrWhiteSpace(ConnectionName))
                    {
                        SaveCurrentConnection();
                    }
                }, error => ErrorMessage = error.ErrorMessage);

            if (result.IsSuccess)
            {
                await LoadDatabasesAsync();
                await LoadTablesAsync();
            }
        });
    }

    public async Task DisconnectAsync()
    {
        if (!CanDisconnect) return;

        await ExecuteAsync(async () =>
        {
            var result = await _databaseService.DisconnectAsync();
            result.Switch(
                () => {
                    Databases = [];
                    Tables = [];
                    QueryResult = null;
                    AffectedRows = 0;
                    
                    OnPropertyChanged(nameof(IsConnected));
                    OnPropertyChanged(nameof(CurrentDatabase));
                    OnPropertyChanged(nameof(CanDisconnect));
                    ConnectionStateChanged?.Invoke(this, EventArgs.Empty);
                }, error => ErrorMessage = error.ErrorMessage);
        });
    }

    public async Task TestConnectionAsync()
    {
        if (!CanTestConnection) return;

        await ExecuteAsync(async () =>
        {
            var connectionInfo = new ConnectionInfoDto
            {
                Host = Server,
                Username = Username,
                Password = Password,
                Port = Port,
                DefaultDatabase = SelectedDatabase
            };

            var result = await _databaseService.TestConnectionAsync(connectionInfo);
            result.Switch(
                () => ErrorMessage = null,
                error => ErrorMessage = error.ErrorMessage);
        });
    }

    public async Task ExecuteQueryAsync()
    {
        if (!CanExecuteQuery) return;

        await ExecuteAsync(async () =>
        {
            // 清除之前的結果
            QueryResult = null;
            AffectedRows = 0;
            _fullQueryResult = null;
            TotalRecords = 0;
            CurrentPage = 1;

            var result = await _databaseService.ExecuteQueryAsync(SqlQuery);
            
            if (result.IsSuccess)
            {
                if (result.Value != null)
                {
                    _fullQueryResult = result.Value.Data;
                    TotalRecords = _fullQueryResult.Rows.Count;
                    UpdatePagedResult();
                }
                else
                {
                    AffectedRows = result.Value.AffectedRows;
                }
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
            }
        });
    }

    public async Task LoadDatabasesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await _databaseService.GetDatabasesAsync();
            result.Switch(
                databases => Databases = databases,
                error => ErrorMessage = error.ErrorMessage);
        });
    }

    public async Task LoadTablesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await _databaseService.GetTablesAsync();
            result.Switch(
                tables => Tables = tables,
                error => ErrorMessage = error.ErrorMessage);
        });
    }

    public async Task SwitchDatabaseAsync(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName) || !IsConnected) return;

        await ExecuteAsync(async () =>
        {
            var result = await _databaseService.SwitchDatabaseAsync(databaseName);
            result.Switch(
                () =>
                {
                    SelectedDatabase = databaseName;
                    OnPropertyChanged(nameof(CurrentDatabase));
                },
                error => ErrorMessage = error.ErrorMessage);

            if (result.IsSuccess)
            {
                await LoadTablesAsync();
            }
        });
    }

    public void SetSelectQuery(string tableName)
    {
        SqlQuery = $"SELECT * FROM \"{tableName}\" LIMIT 10;";
    }

    public void ClearQueryResults()
    {
        QueryResult = null;
        AffectedRows = 0;
        _fullQueryResult = null;
        TotalRecords = 0;
        CurrentPage = 1;
        ClearError();
    }

    public void LoadSavedConnections()
    {
        try
        {
            OnPropertyChanged(nameof(SavedConnections));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"載入連線記錄失敗: {ex.Message}";
        }
    }

    public async Task LoadConnection(DomainConnectionInfo connection)
    {
        Server = connection.Host;
        Port = connection.Port;
        Username = connection.Username;
        if (connection.SavePassword)
        {
            Password = connection.Password;
        }
        SelectedDatabase = connection.DefaultDatabase;
        ConnectionName = connection.DisplayName;
        SavePassword = connection.SavePassword;

        // 更新最後使用時間
        connection.LastUsed = DateTime.Now;
        _appSettings.AddOrUpdateConnection(connection);
        _appSettings.Save();
        OnPropertyChanged(nameof(SavedConnections));
        await ConnectAsync();
    }

    public void SaveCurrentConnection()
    {
        if (string.IsNullOrWhiteSpace(ConnectionName))
        {
            ErrorMessage = "請輸入連線名稱";
            return;
        }

        var connection = new DomainConnectionInfo
        {
            DisplayName = ConnectionName,
            Host = Server,
            Port = Port,
            Username = Username,
            Password = SavePassword ? Password : string.Empty,
            DefaultDatabase = SelectedDatabase,
            SavePassword = SavePassword,
            LastUsed = DateTime.Now
        };

        try
        {
            _appSettings.AddOrUpdateConnection(connection);
            _appSettings.Save();
            OnPropertyChanged(nameof(SavedConnections));
            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"儲存連線記錄失敗: {ex.Message}";
        }
    }

    public void DeleteConnection(DomainConnectionInfo connection)
    {
        try
        {
            _appSettings.RemoveConnection(connection);
            _appSettings.Save();
            OnPropertyChanged(nameof(SavedConnections));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"刪除連線記錄失敗: {ex.Message}";
        }
    }

    private async Task ExecuteAsync(Func<Task> operation)
    {
        IsBusy = true;
        ClearError();

        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearError()
    {
        ErrorMessage = null;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    // Pagination Methods
    public void GoToFirstPage()
    {
        CurrentPage = 1;
    }

    public void GoToPreviousPage()
    {
        if (CanGoToPreviousPage)
        {
            CurrentPage--;
        }
    }

    public void GoToNextPage()
    {
        if (CanGoToNextPage)
        {
            CurrentPage++;
        }
    }

    public void GoToLastPage()
    {
        CurrentPage = TotalPages;
    }

    public void GoToPage(int pageNumber)
    {
        if (pageNumber >= 1 && pageNumber <= TotalPages)
        {
            CurrentPage = pageNumber;
        }
    }

    private void UpdatePagedResult()
    {
        if (_fullQueryResult == null || TotalRecords == 0)
        {
            QueryResult = null;
            return;
        }

        // 計算分頁範圍
        var startIndex = (CurrentPage - 1) * PageSize;
        var endIndex = Math.Min(startIndex + PageSize, TotalRecords);

        // 創建分頁結果
        var pagedTable = _fullQueryResult.Clone();
        for (int i = startIndex; i < endIndex; i++)
        {
            pagedTable.ImportRow(_fullQueryResult.Rows[i]);
        }

        QueryResult = pagedTable;
    }
} 