using AngleSharp;
using MapsExplorer;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Linq;

public class SeaLogHandler : LogHandler
{
    private SeaExploreMode _mode;

    public SeaLogHandler() : base()
    {

    }

    public Sea GetSea(LogLine line, SeaExploreMode mode = SeaExploreMode.None)
    {
        _mode = mode;
        LastError = "";
        string dir = Paths.SeasBaseDir + "/" + Utils.GetDateFolderString(line.DateTime);
        string localPath = dir + "/" + line.Hash + ".html";
        string content = null;
        if (File.Exists(localPath))
        {
            content = File.ReadAllText(localPath);
        }
        else
        {
            string address = Paths.GetLogPath(line.Hash);
            content = WebLoader.GetContent(address, out LastError);
            if (string.IsNullOrEmpty(content))
                return null;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(localPath, content);
        }

        if (string.IsNullOrEmpty(content))
            return null;
        Sea sea = new Sea(line);
        Parse(content, sea, false);
        if (!string.IsNullOrEmpty(LastError))
            return null;
        return sea;
    }

    private async void Parse(string html, Sea sea, bool isDesc)
    {
    }
}

