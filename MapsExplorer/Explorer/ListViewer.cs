using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Parser;

namespace MapsExplorer
{
	public class ListViewer
	{
        private double _rnd;
        private List<DungeLine> _res = new List<DungeLine>();
        private List<string> _totals = new List<string>();
        private int _total;
        private IConfiguration _config;
        private IBrowsingContext _context;
        private string _error;

        private void SetError(string error)
        {
            if (string.IsNullOrEmpty( _error))
                _error = error;
        }

        private void GenerateRnd()
        {
            _rnd = new Random().NextDouble();
            if (_rnd < .1)
                _rnd += .1;
        }

        private string GetAddress(DateTime beginDate, DateTime endDate, string add, int page = 1)
        {
            string address = "https://gv.erinome.net/duels/log/?act=search";

            address += "&t=2";  //  подземелье
            address += "&b=" + beginDate.ToString("dd.MM.yyyy"); //13.05.2020
            address += "&e=" + endDate.ToString("dd.MM.yyyy");
            address += add;
            //address += "&r=1";  //  успех
            NumberFormatInfo nfi = new NumberFormatInfo { NumberDecimalSeparator = "."};
            address += "&s=" + _rnd.ToString(nfi);
            if (page > 1)
                address += "&p=" + page;
            return address;
        }


        public void StartView(DateTime beginDate, DateTime endDate, string add, System.Action<string, List<DungeLine>> onReady, System.Action<int> onProgress)
        {
            _res.Clear();
            _totals.Clear();
            _config = Configuration.Default;
            _context = BrowsingContext.New(_config);
            TimeSpan full = endDate - beginDate;
            TimeSpan interval = new TimeSpan(3, 0, 0, 0);
            int parts = (int)Math.Ceiling((double)full.Ticks / interval.Ticks);
            int index = 0;
            for (DateTime date = beginDate; date < endDate; date += interval)
            {
                DateTime d2 = date + interval;
                if (d2 > endDate)
                    d2 = endDate;
                LoadList(date, d2, add, onProgress, index, parts);
				if (!string.IsNullOrEmpty(_error))
					break;
                index++;
                onProgress((int)((double)index / parts * 100));
            }
            onReady(_error, _res);
        }

        private void LoadList(DateTime beginDate, DateTime endDate, string add, System.Action<int> onProgress, int partIndex, float parts)
        {
            GenerateRnd();
            string doc = null;
            _total = -1;
            int page = 1;
            while (_total == -1 || ((page - 1) * 20 < _total))
            {
                string address = GetAddress(beginDate, endDate, add, page);
                Console.WriteLine(page + ") " + address);
                doc = WebLoader.GetContent(address, out _error);
				if (string.IsNullOrEmpty(doc))
					break;
                AddPageList(_context, doc);
                if (_total > 0)
                {
                    double part = (double)1 / parts;
                    double progress = partIndex * part + part * (double)page / (_total / 20 + 1);
                    //onProgress((int)(progress * 100));
                }
                page++;
                //await Task.Delay(1000);
            }
            _totals.Add(_total.ToString());
        }


        private async void AddPageList(IBrowsingContext context, string docStrFull)
        {
            if (_total == -1)
                _total = 0;
            string[] parts = docStrFull.Split('$');
            if (parts.Length == 2)
            {
                int.TryParse(parts[0], out _total);
                string docStr = "<table>" + parts[1] + "</table>";
                if (docStr.Contains("Ничего не найдено"))
                {
                    return;
                }
                var document = await context.OpenAsync(req => req.Content(docStr));
                var trs = document.QuerySelectorAll("tr");
                foreach (var tr in trs)
                {
                    DungeLine line = new DungeLine();
                    var tds = tr.QuerySelectorAll("td");
                    line.Hash = tds[0].QuerySelector("a").TextContent;
					if (line.Hash.Contains("_") || line.Hash.Contains("f9kysecen"))
						continue;
                    var t1 = tds[1].TextContent;    // Тип данжа и 1/2/3/vault
                    string t11 = "";
                    string t12 = "";
                    var t11s = tds[1].QuerySelectorAll("sup");
                    foreach (var t11El in t11s)
                    {
                        string su = t11El.TextContent;
                        int suIndex = t1.IndexOf(su);
                        if (suIndex > 0)
                            t1 = t1.Substring(0, suIndex);
                        if (su != "vault" && t11s.Length > 1)
                            continue;
                        t11 = su;
                        if (t11 == "vault")
                        {
                            t11 = "";
                            t12 = "v";
                        }
                    }
					if (t11 != "")
					{
						if (t11 == "custom")
							line.Custom = true;
						else
							line.Category = t11 == "3" ? Category.Аква : (t11 == "2" ? Category.Конюшня : Category.Ромашка);
					}
                    line.Vault = t12 == "v";
                    if (t1.Length > 11)// подземелье 
                    {
                        t1 = t1.Substring(11, t1.Length - 11);
						int byIndex = t1.IndexOf("от ");
						if (byIndex == 0)
							line.Kind = DungeKind.Обыденности;
						else if (byIndex != -1)
							t1 = t1.Substring(0, byIndex);
						if (byIndex != -1)
							line.Custom = true;

					}
                    if (line.Kind != DungeKind.Обыденности && !Enum.TryParse(t1, out line.Kind))
                    {
                        line.Kind = DungeKind.Обыденности;
                        //SetError("Неизвестный тип подземелья");
                    }
                    var t2 = tds[2].TextContent;    //  боги
                    line.Gods = new List<string>(t2.Split(','));
                    for (int i = 0; i < line.Gods.Count; i++)
                    {
                        string god = line.Gods[i];
                        if (god[0] == ' ')
                            line.Gods[i] = god.Substring(1, god.Length - 1);
                    }
                    var t3 = tds[3].TextContent;    //  успех
                    line.Success = t3 == "успех";
                    var t4 = tds[4].TextContent;    //  дата время
                    line.DateTime = Utils.ParseDateTime(t4);
                    _res.Add(line);
                }
            }
        }
    }
}
