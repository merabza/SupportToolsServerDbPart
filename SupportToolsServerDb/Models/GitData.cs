using System;

namespace SupportToolsServerDb.Models;

public sealed class GitData
{
    private GitIgnoreFileType? _gitIgnoreFileTypeNavigation;
    public int Id { get; set; }
    public required string GitAddress { get; set; }
    public required string Name { get; set; }
    public int GitIgnoreFileTypeId { get; set; }

    public GitIgnoreFileType GitIgnoreFileTypeNavigation
    {
        get =>
            _gitIgnoreFileTypeNavigation ??
            throw new InvalidOperationException("Uninitialized property: " + nameof(GitIgnoreFileType));
        set => _gitIgnoreFileTypeNavigation = value;
    }
}