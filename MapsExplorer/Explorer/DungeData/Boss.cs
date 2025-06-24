using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapsExplorer
{
	public class TextLine 
	{
		public string Text;
		public int Pattern;
	}

	public class Boss
	{
		public int Num;
		public int Hp;
		public List<Ability> Abils = new List<Ability>();
		public int AbilsCount;
		public string AllAbilsStr;
		public int BossPower { get { return Abils.Count; } }
		public string Name;
		public Step Pos;
		public bool HeroesWin;
		public bool TribbleInFirst;
		public bool TribbleInMiddle;
		public bool TribbleInFinal;
		public bool TribbleInFinal2;
		public bool Escape1;
		public bool Escape2;
		public DateTime StartDateTime;
		public DateTime EndDateTime;
		public int Steps;
		public List<int[]> Hps = new List<int[]>();
		public int[] InfluencesByStep;
		public List<TextLine>[] TextLines;
		public bool CanBeRouting;
		public bool IsRouting;
		public bool IsFinal;
		public List<string> Loot = new List<string>();
		public List<string> LootParts = new List<string>();
		public List<string> PartVoices = new List<string>();
	}
}
