using System.Collections.Generic;

namespace SupportToolsServerDb.Models;

public sealed class GitIgnoreFileType
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Content { get; set; }

    public ICollection<GitData> GitData { get; set; } = new List<GitData>();
}