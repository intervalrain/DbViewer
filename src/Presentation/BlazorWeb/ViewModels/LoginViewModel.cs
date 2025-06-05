using System.ComponentModel;
using System.Runtime.CompilerServices;
using Application.Contracts.Dtos;
using Application.Contracts.Services;

namespace Presentation.BlazorWeb.ViewModels;

public class LoginViewModel : INotifyPropertyChanged
{
    private readonly IAuthService _authService;
    private readonly ICookieService _cookieService;
    
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<LoginStateChangedEventArgs>? LoginStateChanged;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _rememberMe;
    private bool _isBusy;
    private string? _errorMessage;

    public LoginViewModel(IAuthService authService, ICookieService cookieService)
    {
        _authService = authService;
        _cookieService = cookieService;
    }

    public string Username
    {
        get => _username;
        set
        {
            if (SetProperty(ref _username, value))
            {
                ClearError();
                OnPropertyChanged(nameof(CanLogin));
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
                OnPropertyChanged(nameof(CanLogin));
            }
        }
    }

    public bool RememberMe
    {
        get => _rememberMe;
        set => SetProperty(ref _rememberMe, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(CanLogin));
            }
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool CanLogin => !string.IsNullOrWhiteSpace(Username) && 
                           !string.IsNullOrWhiteSpace(Password) && 
                           !IsBusy;

    public async Task InitializeAsync()
    {
        try
        {
            // 等待一下確保 JavaScript 完全載入
            await Task.Delay(100);
            
            // 重試機制：最多嘗試 3 次
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                Console.WriteLine($"InitializeAsync 嘗試第 {attempt} 次檢查 token...");
                
                // 檢查是否有儲存的 token
                var tokenResult = await _cookieService.GetAsync("auth_token");
                
                if (tokenResult.IsSuccess && !string.IsNullOrWhiteSpace(tokenResult.Value))
                {
                    Console.WriteLine($"第 {attempt} 次嘗試成功找到 token: {tokenResult.Value.Substring(0, Math.Min(10, tokenResult.Value.Length))}...");
                    
                    // 驗證 token 是否有效
                    var validateResult = await _authService.ValidateTokenAsync(tokenResult.Value);
                    if (validateResult.IsSuccess && validateResult.Value)
                    {
                        Console.WriteLine("Token 驗證成功，自動登入");
                        // 如果有有效的 token，自動導航到主頁
                        LoginStateChanged?.Invoke(this, new LoginStateChangedEventArgs
                        {
                            IsSuccess = true,
                            RedirectUrl = "/"
                        });
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Token 驗證失敗，清除無效 token");
                        // Token 無效，清除它
                        await _cookieService.RemoveAsync("auth_token");
                        break;
                    }
                }
                else
                {
                    Console.WriteLine($"第 {attempt} 次嘗試沒有找到有效的 token");
                    
                    // 如果是最後一次嘗試，就停止
                    if (attempt == 3)
                    {
                        Console.WriteLine("所有嘗試都沒有找到 token，用戶需要手動登入");
                        break;
                    }
                    
                    // 等待一下再重試
                    await Task.Delay(200);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InitializeAsync 錯誤: {ex.Message}");
            // 發生錯誤時不做任何事，讓用戶正常登入
        }
    }

    public async Task LoginAsync()
    {
        if (!CanLogin) return;

        IsBusy = true;
        ClearError();

        try
        {
            var loginDto = new LoginDto
            {
                Username = Username,
                Password = Password,
                RememberMe = RememberMe
            };

            var result = await _authService.LoginAsync(loginDto);
            
            if (result.IsSuccess)
            {
                var authResult = result.Value;
                
                // 儲存 token 到 Cookie
                if (RememberMe && !string.IsNullOrEmpty(authResult.Token))
                {
                    var cookieResult = await _cookieService.SetAsync("auth_token", authResult.Token, TimeSpan.FromDays(30));
                    if (!cookieResult.IsSuccess)
                    {
                        // 記錄 Cookie 設定失敗，但不阻止登入
                        Console.WriteLine($"Cookie 設定失敗: {cookieResult.ErrorMessage}");
                    }
                }
                
                LoginStateChanged?.Invoke(this, new LoginStateChangedEventArgs
                {
                    IsSuccess = true,
                    RedirectUrl = "/"
                });
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
                LoginStateChanged?.Invoke(this, new LoginStateChangedEventArgs
                {
                    IsSuccess = false
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "登入過程中發生錯誤: " + ex.Message;
            LoginStateChanged?.Invoke(this, new LoginStateChangedEventArgs
            {
                IsSuccess = false
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            Console.WriteLine("LoginViewModel.LogoutAsync 開始...");
            
            // 清除 AuthService 中的狀態
            await _authService.LogoutAsync();
            Console.WriteLine("AuthService.LogoutAsync 完成");
            
            // 檢查登出前的 Cookie 狀態
            var beforeResult = await _cookieService.GetAsync("auth_token");
            Console.WriteLine($"登出前 Cookie 狀態: {(beforeResult.IsSuccess ? "存在" : "不存在")}");
            
            // 清除 Cookie 中的 token
            var result = await _cookieService.RemoveAsync("auth_token");
            if (!result.IsSuccess)
            {
                Console.WriteLine($"清除 Cookie 失敗: {result.ErrorMessage}");
            }
            else
            {
                Console.WriteLine("Cookie 清除成功");
            }
            
            // 檢查登出後的 Cookie 狀態
            var afterResult = await _cookieService.GetAsync("auth_token");
            Console.WriteLine($"登出後 Cookie 狀態: {(afterResult.IsSuccess ? "仍然存在" : "已清除")}");
            
            Console.WriteLine("LoginViewModel.LogoutAsync 完成：AuthService 狀態和 Cookie 都已清除");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LogoutAsync 錯誤: {ex.Message}");
            throw; // 重新拋出異常讓調用者知道
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
}

public class LoginStateChangedEventArgs : EventArgs
{
    public bool IsSuccess { get; set; }
    public string? RedirectUrl { get; set; }
} 