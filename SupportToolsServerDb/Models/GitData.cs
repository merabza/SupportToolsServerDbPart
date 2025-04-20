using System;

namespace SupportToolsServerDb.Models;

public sealed class GitData
{
    private GitIgnoreFileType? _gitIgnoreFileTypeNavigation;
    public int GdId { get; set; }
    public required string GdGitAddress { get; set; }
    public required string GdName { get; set; }
    public required string GdFolderName { get; set; }
    public int GitIgnoreFileTypeId { get; set; }

    public GitIgnoreFileType GitIgnoreFileTypeNavigation
    {
        get =>
            _gitIgnoreFileTypeNavigation ??
            throw new InvalidOperationException("Uninitialized property: " + nameof(GitIgnoreFileType));
        set => _gitIgnoreFileTypeNavigation = value;
    }
}