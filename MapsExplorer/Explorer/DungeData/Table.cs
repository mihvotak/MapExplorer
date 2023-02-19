using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

public class Table
{
	class TableLine
	{
		public Dictionary<string, string> Dict	= new Dictionary<string, string>();
	}

	private List<string> _headers = new List<string>();
	private List<TableLine> _lines = new List<TableLine>();
	private TableLine _currentLine;

	public void AddRow()
	{
		_lines.Add(new TableLine());
		_currentLine = _lines[_lines.Count - 1];
	}

	public void Add(string header, string value)
	{
		if (_currentLine == null)
			return;
		if (!_headers.Contains(header))
			_headers.Add(header);
		_currentLine.Dict[header] = value;
	}

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < _headers.Count; i++)
		{
			sb.Append(_headers[i]);
			if (i < _headers.Count - 1)
				sb.Append("\t");
		}
		for (int j = 0; j < _lines.Count; j++)
		{
			sb.Append("\n");
			TableLine line = _lines[j];
			for (int i = 0; i < _headers.Count; i++)
			{
				string header = _headers[i];
				if (line.Dict.ContainsKey(header))
					sb.Append(line.Dict[header]);
				if (i < _headers.Count - 1)
					sb.Append("\t");
			}
		}
		return sb.ToString();
	}
}
