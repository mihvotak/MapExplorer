using AngleSharp;

public class LogHandler
{
	public string LastError;

	protected IConfiguration _config;
	protected IBrowsingContext _context;

    protected bool _offlineMode;

    public LogHandler()
	{
		_config = Configuration.Default;
		_context = BrowsingContext.New(_config);
	}

    public void Prepare(bool offlineMode)
    {
        _offlineMode = offlineMode;
    }
}
