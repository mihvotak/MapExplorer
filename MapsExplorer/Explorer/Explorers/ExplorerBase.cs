using MapsExplorer;
using System.Collections.Generic;
using System.ComponentModel;

public class ExplorerBase
{
	protected List<LogLine> _resultLines = new List<LogLine>();
	protected LogHandler _logHandler;
	protected DungeonLogHandler DungeonLogHandler => _logHandler as DungeonLogHandler;
	protected PolygonLogHandler PolygonLogHandler => _logHandler as PolygonLogHandler;
	protected SeaLogHandler SeaLogHandler => _logHandler as SeaLogHandler;
	protected AvantureKind _avantureKind;
	protected DungeonExploreMode _dungeonExploreMode;
	protected PolygonExploreMode _polygonExploreMode;
	protected SeaExploreMode _seaExploreMode;
	protected BackgroundWorker _backgroundWorker;
	protected bool _customCheckBoxChecked;
	protected bool _checkBoxMinRouteChecked;

	public string TableText = "";
	public string ResultText = "";

	public void Init(AvantureKind avantureKind, DungeonExploreMode dungeonExploreMode, PolygonExploreMode polygonExploreMode, SeaExploreMode seaExploreMode, LogHandler logHandler, List<LogLine> resultLines, BackgroundWorker backgroundWorker, bool customCheckBoxChecked, bool checkBoxMinRouteChecked)
	{
		_avantureKind = avantureKind;
		_dungeonExploreMode = dungeonExploreMode;
		_polygonExploreMode = polygonExploreMode;
        _seaExploreMode = seaExploreMode;
        _logHandler = logHandler;
		_resultLines = resultLines;
		_backgroundWorker = backgroundWorker;
		_customCheckBoxChecked = customCheckBoxChecked;
		_checkBoxMinRouteChecked = checkBoxMinRouteChecked;
	}

	virtual public void Work()
	{
		//
	}

	protected void ReportProgress(int lineIndex)
	{
		if (_resultLines == null || _resultLines.Count == 0)
			return;
		_backgroundWorker.ReportProgress((int)((double)(lineIndex + 1) / _resultLines.Count * 100));
	}
}
