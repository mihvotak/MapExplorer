using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Plot2d
{
	private int _half = 20;
	private int _graphSize;
	private int[,] _arr;

	public Plot2d(int half = 20)
	{
		_half = half;
		_graphSize = _half * 2 + 1;
		_arr = new int[_graphSize, _graphSize];

	}

	public void Inc(int x, int y)
	{
		_arr[x + _half, y + _half]++;
	}

	public string GetRes(int drawHalf)
	{
		string s = "\t";
		for (int x = -drawHalf; x <= drawHalf; x++)
			s += x + "\t";
		s += "\n";

		for (int y = drawHalf; y >= -drawHalf; y--)
		{
			s += y + "\t";
			for (int x = -drawHalf; x <= drawHalf; x++)
			{
				s += _arr[x + _half, y + _half] + "\t";
			}
			s += "\n";
		}
		s += "\n";
		return s;
	}

	public string GetRes4(int drawHalf)
	{
		string s = "";
		for (int y = drawHalf; y >= 0; y--)
		{
			s += y + "\t";
			for (int x = 0; x <= drawHalf; x++)
			{
				s += (_arr[x + _half, y + _half] +
					_arr[x + _half, -y + _half] +
					_arr[-x + _half, y + _half] +
					_arr[-x + _half, -y + _half]) + "\t";
			}
			s += "\n";
		}
		s += "\t";
		for (int x = 0; x <= drawHalf; x++)
			s += x + "\t";
		s += "\n";
		s += "\n";
		return s;
	}

	public string GetRes8(int drawHalf)
	{
		string s = "";
		for (int y = drawHalf; y >= 0; y--)
		{
			s += y + "\t";
			for (int x = 0; x <= drawHalf; x++)
			{
				int v = _arr[x + _half, y + _half] +
					_arr[x + _half, -y + _half] +
					_arr[-x + _half, y + _half] +
					_arr[-x + _half, -y + _half];
				v += _arr[y + _half, x + _half] +
					_arr[y + _half, -x + _half] +
					_arr[-y + _half, x + _half] +
					_arr[-y + _half, -x + _half];
				if (x > y)
					v = 0;
				s += (v) + "\t";
			}
			s += "\n";
		}
		s += "\t";
		for (int x = 0; x <= drawHalf; x++)
			s += x + "\t";
		s += "\n";
		return s;
	}
}
