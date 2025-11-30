using System;

namespace SupportToolsServerDb.Models;

public sealed class GitData
{
    public int GdId { get; init; }
    public required string GdGitAddress { get; init; }
    public required string GdName { get; init; }
    public required string GdFolderName { get; init; }
    public int GitIgnoreFileTypeId { get; init; }

    public GitIgnoreFileType GitIgnoreFileTypeNavigation
    {
        get => field ?? throw new InvalidOperationException("Uninitialized property: " + nameof(GitIgnoreFileType));
        init;
    }
}