using AngleSharp;
using MapsExplorer;
using System.Runtime.Remoting.Contexts;

public class LogHandler
{
	public string LastError;

	protected IConfiguration _config;
	protected IBrowsingContext _context;

	public LogHandler()
	{
		_config = Configuration.Default;
		_context = BrowsingContext.New(_config);
	}
}
