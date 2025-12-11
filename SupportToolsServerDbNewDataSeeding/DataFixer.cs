using Microsoft.Extensions.Logging;

namespace SupportToolsServerDbNewDataSeeding;

internal sealed class DataFixer
{
    private readonly IDataFixRepository _dataFixRepository;
    private readonly ILogger _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DataFixer(ILogger logger, IDataFixRepository dataFixRepository)
    {
        _logger = logger;
        _dataFixRepository = dataFixRepository;
    }

    public bool Run()
    {
        return true;
    }
}