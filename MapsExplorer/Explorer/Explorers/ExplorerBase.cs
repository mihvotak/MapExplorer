using MapsExplorer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

public class ExplorerBase
{
	protected List<DungeLine> _resultLines = new List<DungeLine>();
	protected LogHandler _logHandler;
	protected ExploreMode _exploreMode;
	protected BackgroundWorker _backgroundWorker;
	protected bool _customCheckBoxChecked;
	protected bool _checkBoxMinRouteChecked;

	public string TableText = "";
	public string ResultText = "";

	public void Init(ExploreMode exploreMode, LogHandler logHandler, List<DungeLine> resultLines, BackgroundWorker backgroundWorker, bool customCheckBoxChecked, bool checkBoxMinRouteChecked)
	{
		_exploreMode = exploreMode;
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
		_backgroundWorker.ReportProgress((int)((double)(lineIndex + 1) / _resultLines.Count * 100));
	}
}
