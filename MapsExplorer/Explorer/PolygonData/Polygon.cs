
using MapsExplorer;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;

public class Polygon
{
	public GodResult[] GodResults = new GodResult[4];

	public LogLine LogLine { get; private set; }

	public List<PolyMaps4> Maps = new List<PolyMaps4>();
	public PolyMaps4 GetMaps4(int stepNum) => Maps[stepNum - 1];

	public Polygon(LogLine logLine)
	{
		LogLine = logLine;
	}
}
