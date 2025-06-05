using Common.Ddd.Domain.Entities;

namespace Domain.Database.Entities;

public class DatabaseInfo(Guid id) : Entity<Guid>(id)
{
    public string Name { get; set; } = "";
    public string Owner { get; set; } = "";
    public string Encoding { get; set; } = "";
    public string Collate { get; set; } = "";
    public string CType { get; set; } = "";
    public long Size { get; set; }
    public string Description { get; set; } = "";
    
    public string FormattedSize
    {
        get
        {
            if (Size == 0) return "N/A";
            
            string[] sizes = ["B", "KB", "MB", "GB", "TB"];
            double len = Size;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
} 