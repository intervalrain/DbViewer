# Result Pattern - C# Error Handling Framework

一個受 Rust Result 和 ErrorOr 啟發的 C# 錯誤處理框架，提供類型安全、函數式的錯誤處理機制。

## 目錄

- [特點](#特點)
- [快速開始](#快速開始)
- [核心概念](#核心概念)
- [基本用法](#基本用法)
- [函數式操作](#函數式操作)
- [HTTP 響應整合](#http-響應整合)
- [錯誤處理最佳實踐](#錯誤處理最佳實踐)
- [從 Try-Catch 遷移](#從-try-catch-遷移)
- [進階用法](#進階用法)
- [API 參考](#api-參考)

## 特點

- **類型安全**：編譯時確保錯誤處理
- **函數式風格**：支援 Map、Bind、Match 等操作
- **零性能開銷**：基於 struct 實現，無額外分配
- **豐富的錯誤資訊**：支援錯誤代碼、描述、類型和中繼資料
- **HTTP 整合**：無縫轉換為 HTTP 響應
- **可組合性**：輕鬆組合多個可能失敗的操作
- **向後相容**：可與現有 try-catch 代碼共存

## 快速開始

### 1. 定義服務方法

```csharp
public class UserService
{
    public Result<User> GetUser(int id)
    {
        if (id <= 0)
        {
            return Errors.Validation.Required(nameof(id));
        }
        
        return _repository.GetById(id) ?? Errors.NotFound.ById("User", id);
    }
}
```

### 2. 在 Controller 中使用

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var result = await _userService.GetUserAsync(id);
    return result.ToActionResult("Get userinfo successfully.");
}
```

### 3. 函數式鏈式操作
+ As Is:
```csharp
public async Task<Result<UserDto>> GetUserProfileAsync(int id)
{
    var result = await GetUserAsymc(id);
    if (result.IsFailure) return result.Errors;

    var user = result.Value;
    var dto = _mapper.Map<UserDto>(user);

    _logger.LogInformation("User {UserId} is loaded", dto.Id);

    var isValid = await ValidateUserAsync(dto);
    if (!isValid) return Errors.InvalidUser();

    return dto;
}
```

+ To Be:
```csharp
public async Task<Result<UserDto>> GetUserProfileAsync(int id)
{
    return await GetUserAsync(id)
        .Map(user => _mapper.Map<UserDto>(user))
        .TapAsync(dto => _logger.LogInformation("User {UserId} is loaded", dto.Id))
        .EnsureAsync(dto => ValidateUserAsync(dto), Errors.InvalidUser());
}
```

## 核心概念

### Result&lt;T&gt;

表示一個可能成功（包含值）或失敗（包含錯誤）的操作結果。

```csharp
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public T Value { get; }
    public List<Error> Errors { get; }
    public string ErrorMessage { get; }
}
```

### Error

不可變的錯誤表示，包含完整的錯誤資訊。

```csharp
public sealed record Error
{
    public string Code { get; }
    public string Description { get; }
    public string Message { get; }
    public ErrorType Type { get; }
    public Dictionary<string, object> Metadata { get; }
}
```

### ErrorType

預定義的錯誤類型，對應不同的 HTTP 狀態碼。

```csharp
public enum ErrorType
{
    Failure,        // 500 Internal Server Error
    Validation,     // 400 Bad Request
    NotFound,       // 404 Not Found
    Conflict,       // 409 Conflict
    Unauthorized,   // 401 Unauthorized
    Forbidden       // 403 Forbidden
}
```

## 基本用法

### 創建 Result

```csharp
// 成功結果
Result<string> success = "Hello World";
Result<User> userResult = Result<User>.Success(user);

// 失敗結果
Result<User> failure = Error.NotFound("User.NotFound", "User not found.");
Result<User> multipleErrors = new List<Error> { error1, error2 };
```

### 檢查結果

```csharp
var result = GetUser(123);

if (result.IsSuccess)
{
    Console.WriteLine($"Username: {result.Value.Name}");
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"- {error.Code}: {error.Description}");
    }
}
```

### 模式匹配

```csharp
var message = result.Match(
    onSuccess: user => $"Welcome: {user.Name}！",
    onFailure: errors => $"Invalid operation: {errors.ErrorMessage}"
);
```

## 函數式操作

### Map - 轉換成功值

```csharp
Result<UserDto> dtoResult = userResult.Map(user => new UserDto
{
    Id = user.Id,
    Name = user.Name,
    Email = user.Email
});
```

### Bind - 鏈接可能失敗的操作

```csharp
Result<Order> orderResult = GetUser(userId)
    .Bind(user => ValidateUser(user))
    .Bind(user => CreateOrder(user, orderData));
```

### Tap - 執行副作用

```csharp
var result = GetUser(id)
    .Tap(user => _logger.LogInformation("User found: {UserName}", user.Name))
    .TapError(errors => _logger.LogWarning("User not found: {Errors}", 
        string.Join(", ", errors.Select(e => e.Code))));
```

### Ensure - 添加驗證

```csharp
var result = GetUser(id)
    .Ensure(user => user.IsActive, UserErrors.InactiveUser())
    .Ensure(user => !user.IsBlocked, UserErrors.BlockedUser());
```

### 異步操作

```csharp
Result<UserProfile> profileResult = await GetUserAsync(id)
    .MapAsync(async user => await _mapper.MapAsync<UserProfile>(user))
    .BindAsync(async profile => await EnrichProfileAsync(profile))
    .TapAsync(async profile => await LogProfileAccessAsync(profile.UserId));
```

## HTTP 響應整合

### 基本轉換

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetUser(int id)
{
    var result = await _userService.GetUserAsync(id);
    
    // 自動轉換為適當的 HTTP 響應
    return result.ToActionResult("User query succeed.");
}
```

### 不同 HTTP 狀態碼

```csharp
[HttpPost]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
    var result = await _userService.CreateUserAsync(request);
    
    return result.ToCreatedAtActionResult(
        actionName: nameof(GetUser),
        controllerName: null,
        routeValues: new { id = result.IsSuccess ? result.Value.Id : 0 },
        successMessage: "User created."
    );
}

[HttpDelete("{id}")]
public async Task<IActionResult> DeleteUser(int id)
{
    var result = await _userService.DeleteUserAsync(id);
    
    return result.ToActionResult(
        successMessage: "User deleted.",
        successStatusCode: HttpStatusCode.NoContent
    );
}
```

### 分頁響應

```csharp
[HttpGet]
public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
{
    var usersResult = await _userService.GetUsersAsync(page, pageSize);
    var countResult = await _userService.GetUserCountAsync();

    if (usersResult.IsFailure) return usersResult.ToActionResult();
    if (countResult.IsFailure) return countResult.Errors.ToErrorActionResult();

    return usersResult.ToPaginatedResult(page, pageSize, countResult.Value, "Query user list succeed");
}
```

### 響應格式

所有 HTTP 響應都遵循統一格式：

```json
{
  "success": true,
  "data": { ... },
  "message": "Opeartion succeed.",
  "errors": null,
  "metadata": {
    "timestamp": "2025-06-01T10:00:00Z",
    "requestId": "abc123",
    "version": "1.0"
  }
}
```

錯誤響應：

```json
{
  "success": false,
  "data": null,
  "message": "Invalid operation.",
  "errors": [
    {
      "code": "User.NotFound",
      "description": "User not found.",
      "field": "id",
      "metadata": null
    }
  ],
  "metadata": { ... }
}
```

## 錯誤處理最佳實踐

### 1. 使用預定義錯誤

```csharp
// ✅ 好的做法
return Errors.NotFound.ById("User", id);
return Errors.Validation.Required("Email");

// ❌ 避免的做法
return Error.Failure("ERROR001", "Something went wrong");
```

### 2. 創建領域特定錯誤

```csharp
public static class UserErrors
{
    public static Error EmailAlreadyExists(string email) =>
        Error.Conflict("User.EmailAlreadyExists", $"Email '{email}' has been used");

    public static Error WeakPassword() =>
        Error.Validation("User.WeakPassword", "Password is not strong enough");

    public static Error AccountLocked(DateTime unlockTime) =>
        Error.Forbidden("User.AccountLocked", $"The account has been locked, try after: {unlockTime:yyyy-MM-dd HH:mm}");
}
```

### 3. 收集多個驗證錯誤

```csharp
public Result<User> ValidateUser(CreateUserRequest request)
{
    var errors = new List<Error>();

    if (string.IsNullOrEmpty(request.Name))
        errors.Add(Errors.Validation.Required(nameof(request.Name)));

    if (string.IsNullOrEmpty(request.Email))
        errors.Add(Errors.Validation.Required(nameof(request.Email)));
    else if (!IsValidEmail(request.Email))
        errors.Add(Errors.Validation.InvalidFormat(nameof(request.Email)));

    if (request.Age < 18)
        errors.Add(Errors.AgeRestriction(18));

    return errors.Any() ? errors : Result<User>.Success(new User(request));
}
```

### 4. 錯誤中繼資料

```csharp
public Result<User> GetUser(int id)
{
    var user = _repository.GetById(id);
    if (user == null)
    {
        return Errors.NotFound.ById("User", id)
            .WithMetadata("searchedAt", DateTime.UtcNow)
            .WithMetadata("searchMethod", "ById");
    }
    
    return user;
}
```

## 從 Try-Catch 遷移

### 原始代碼

```csharp
public async Task<AuthResultDto> LoginAsync(LoginDto loginDto)
{
    try
    {
        if (!await ValidateCredentialsAsync(loginDto.Username, loginDto.Password))
        {
            return new AuthResultDto
            {
                IsSuccess = false,
                ErrorMessage = "用戶名或密碼錯誤"
            };
        }

        var userInfo = await GetUserInfoAsync(loginDto.Username);
        var token = GenerateToken(userInfo);
        
        return new AuthResultDto
        {
            IsSuccess = true,
            Token = token,
            Username = userInfo.Username
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "登入過程中發生錯誤");
        return new AuthResultDto
        {
            IsSuccess = false,
            ErrorMessage = "登入過程中發生錯誤"
        };
    }
}
```

### 遷移後的代碼

```csharp
public async Task<Result<AuthToken>> LoginAsync(LoginDto loginDto)
{
    return await ValidateCredentialsAsync(loginDto.Username, loginDto.Password)
        .BindAsync(async isValid => isValid 
            ? await GetUserInfoAsync(loginDto.Username)
            : Result<UserInfo>.Failure(AuthErrors.InvalidCredentials()))
        .BindAsync(async userInfo => await GenerateTokenAsync(userInfo))
        .TapAsync(async authToken => 
        {
            _logger.LogInformation("用戶 {Username} 登入成功", authToken.Username);
        })
        .TapError(errors => 
        {
            _logger.LogWarning("登入失敗：{Errors}", string.Join(", ", errors.Select(e => e.Code)));
        });
}
```

## 進階用法

### 組合多個結果

```csharp
public async Task<Result<UserProfile>> GetCompleteProfile(int userId)
{
    var userTask = GetUserAsync(userId);
    var settingsTask = GetUserSettingsAsync(userId);
    var preferencesTask = GetUserPreferencesAsync(userId);

    await Task.WhenAll(userTask, settingsTask, preferencesTask);

    var combinedResult = Result<object>.Combine(
        userTask.Result,
        settingsTask.Result,
        preferencesTask.Result
    );

    return combinedResult.Map(results => new UserProfile
    {
        User = (User)results[0],
        Settings = (UserSettings)results[1],
        Preferences = (UserPreferences)results[2]
    });
}
```

### 自定義擴展方法

```csharp
public static class UserResultExtensions
{
    public static Result<User> EnsureActive(this Result<User> result)
    {
        return result.Ensure(user => user.IsActive, UserErrors.InactiveUser());
    }

    public static Result<User> EnsureNotBlocked(this Result<User> result)
    {
        return result.Ensure(user => !user.IsBlocked, UserErrors.BlockedUser());
    }

    public static Result<User> LogAccess(this Result<User> result, ILogger logger)
    {
        return result.Tap(user => logger.LogInformation("用戶 {UserId} 已訪問", user.Id));
    }
}

// 使用
var result = GetUser(id)
    .EnsureActive()
    .EnsureNotBlocked()
    .LogAccess(_logger);
```

### 條件式操作

```csharp
public Result<User> GetUserWithPermissions(int id, bool checkPermissions = true)
{
    var result = GetUser(id);
    
    if (checkPermissions)
    {
        result = result.Bind(user => ValidatePermissions(user));
    }
    
    return result;
}
```

### 重試機制

```csharp
public static class ResultRetryExtensions
{
    public static async Task<Result<T>> RetryAsync<T>(
        this Func<Task<Result<T>>> operation,
        int maxRetries,
        TimeSpan delay)
    {
        for (int i = 0; i <= maxRetries; i++)
        {
            var result = await operation();
            if (result.IsSuccess || i == maxRetries)
                return result;
                
            await Task.Delay(delay);
        }
        
        return Error.Failure("Retry.MaxAttemptsReached", "已達到最大重試次數");
    }
}

// 使用
var result = await (() => CallExternalApiAsync())
    .RetryAsync(maxRetries: 3, delay: TimeSpan.FromSeconds(1));
```

## API 參考

### Result&lt;T&gt; 方法

| 方法 | 描述 | 範例 |
|------|------|------|
| `Map<TOut>(Func<T, TOut>)` | 轉換成功值 | `result.Map(user => user.Name)` |
| `Bind<TOut>(Func<T, Result<TOut>>)` | 鏈接可能失敗的操作 | `result.Bind(user => ValidateUser(user))` |
| `Match<TOut>(Func<T, TOut>, Func<List<Error>, TOut>)` | 模式匹配 | `result.Match(success, failure)` |
| `Tap(Action<T>)` | 執行副作用（成功時） | `result.Tap(user => Log(user))` |
| `TapError(Action<List<Error>>)` | 執行副作用（失敗時） | `result.TapError(errors => Log(errors))` |
| `Ensure(Func<T, bool>, Error)` | 添加驗證條件 | `result.Ensure(user => user.IsActive, error)` |
| `UnwrapOr(T)` | 獲取值或預設值 | `result.UnwrapOr(defaultUser)` |

### Error 工廠方法

| 方法 | 描述 | HTTP 狀態碼 |
|------|------|-------------|
| `Error.Validation(code, description)` | 驗證錯誤 | 400 |
| `Error.NotFound(code, description)` | 找不到資源 | 404 |
| `Error.Conflict(code, description)` | 資源衝突 | 409 |
| `Error.Unauthorized(code, description)` | 未授權 | 401 |
| `Error.Forbidden(code, description)` | 禁止存取 | 403 |
| `Error.Failure(code, description)` | 一般失敗 | 500 |

### Errors 快捷方法

```csharp
// 驗證錯誤
Errors.Validation.Required("fieldName")
Errors.Validation.InvalidFormat("fieldName")
Errors.Validation.TooLong("fieldName", maxLength)

// 找不到資源
Errors.NotFound.ById("EntityName", id)
Errors.NotFound.ByProperty("EntityName", "property", value)

// 衝突錯誤
Errors.Conflict.AlreadyExists("EntityName", "property", value)
Errors.Conflict.ConcurrencyConflict("EntityName")

// 認證錯誤
Errors.Authentication.InvalidCredentials()
Errors.Authentication.TokenExpired()
Errors.Authentication.InsufficientPermissions()
```
