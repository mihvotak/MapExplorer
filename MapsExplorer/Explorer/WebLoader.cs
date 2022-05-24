using System;
using System.IO;
using System.Text;
using System.Net;

namespace MapsExplorer
{
	public class WebLoader
	{
		public static string GetContent(string address)
        {
            StringBuilder builder = new StringBuilder();
            WebRequest request = WebRequest.Create(address);
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line = "";
                    while ((line = reader.ReadLine()) != null)
                    {
                        builder.Append(line);
                    }
                }
            }
            response.Close();
            return builder.ToString();
        }

    }
}
