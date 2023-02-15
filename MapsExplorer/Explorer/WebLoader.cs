using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading;

namespace MapsExplorer
{
	public class WebLoader
	{
		public static string GetContent(string address, out string error)
        {
			error = "";
			StringBuilder builder = new StringBuilder();
			WebResponse response = null;
			int count = 0;
			int limit = 500;
			while (count < limit)
			{
				try
				{
					WebRequest request = WebRequest.Create(address);
					response = request.GetResponse();
					break;
				}
				catch (WebException e)
				{
					count++;
					Thread.Sleep(5000);
					if (count == limit)
					{
						error = "WebLoader error with address " + address + " : " + e.Message;
						return null;
					}
				}
			}
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
