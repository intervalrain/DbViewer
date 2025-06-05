namespace Domain.History;

public class HistoryManager
{
    private readonly List<string> _history = [];
    private readonly int _maxSize;
    
    public int Count => _history.Count;
    
    public HistoryManager(int maxSize = 100)
    {
        _maxSize = maxSize;
    }
    
    public void Add(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return;
        
        _history.RemoveAll(h => h.Equals(command, StringComparison.OrdinalIgnoreCase));
        
        _history.Insert(0, command);
        
        while (_history.Count > _maxSize)
        {
            _history.RemoveAt(_history.Count - 1);
        }
    }
    
    public string GetAt(int index)
    {
        if (index >= 0 && index < _history.Count)
        {
            return _history[index];
        }
        return string.Empty;
    }
    
    public IReadOnlyList<string> GetAll()
    {
        return _history.AsReadOnly();
    }
    
    public void Clear()
    {
        _history.Clear();
    }
    
    public List<string> Search(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return [];
        
        return _history.Where(h => h.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                      .ToList();
    }
    
    public string? GetLatest()
    {
        return _history.FirstOrDefault();
    }
    
    public void SaveToFile(string filePath)
    {
        try
        {
            File.WriteAllLines(filePath, _history);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"儲存歷史記錄失敗: {ex.Message}");
        }
    }
    
    public void LoadFromFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                _history.Clear();
                
                foreach (var line in lines.Take(_maxSize))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        _history.Add(line);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"載入歷史記錄失敗: {ex.Message}");
        }
    }
}
