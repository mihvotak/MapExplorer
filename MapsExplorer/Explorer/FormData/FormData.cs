using System;
using System.IO;
using Newtonsoft.Json;

namespace MapsExplorer
{
	[Serializable]
	public partial class FormData
	{
		public DateTime StartDate;
		public DateTime EndDate;
		public bool Success;
		public bool Special;
		public bool Custom;
        public AvantureKind AvantureKind;
		public DungeonExploreMode DungeonExploreMode;
		public PolygonExploreMode PolygonExploreMode;
		public SeaExploreMode SeaExploreMode;
		public string Hash;

		public static FormData ReadFromFile(string fileName)
		{
			FormData data = null;
			if (File.Exists(fileName))
			{
				string text = File.ReadAllText(fileName);
				data = JsonConvert.DeserializeObject<FormData>(text);
			}
			else
			{
				data = new FormData();
				data.EndDate = DateTime.Now.Date;
				data.StartDate = data.EndDate + TimeSpan.FromDays(-3);
				data.Hash = "";
			}
			return data;
		}

		override public string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}
	}
}
