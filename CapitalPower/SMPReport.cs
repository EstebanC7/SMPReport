using System;
using System.Text;
using System.IO;
using System.Net;
using System.Windows.Forms;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;


namespace CapitalPower
{
    class SMPReport
    {
        private static void Main(string[] args)
        {
            // Gets data from URL
            string html = GetHTMLData();

            // Create CSV file of data
            CreateCSVFile(html);

            // Display the latest price
            DisplayLatestPrice();

            // Wait for the URL to update with new data
            WaitForUpdates();
        }

        //Gets the HTML Data from URL
        static string GetHTMLData()
        {
            string url = "http://ets.aeso.ca/ets_web/ip/Market/Reports/CSMPriceReportServlet";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            StreamReader reader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
            string html = reader.ReadToEnd();
            return html;
        }

        //Converts HTML to csv file
        static void CreateCSVFile(string html)
        {
            // Load HTML data
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            // Get table data
            HtmlNode tableNode = htmlDoc.DocumentNode.SelectSingleNode("//table[2]");
            HtmlNodeCollection rows = tableNode.SelectNodes("tr");

            // Write the data rows in csv
            using (StreamWriter writer = new StreamWriter("SMPReport.csv"))
            {
                foreach (HtmlNode row in rows)
                {
                    HtmlNodeCollection cells = row.SelectNodes("th|td");
                    string date = cells[0].InnerText;
                    string time = cells[1].InnerText;
                    string price = cells[2].InnerText;
                    string volume = cells[3].InnerText;
                    writer.WriteLine($"{date},{time},{price},{volume}");
                }
            }
        }

        // Displays the latest price from the CSV file
        static void DisplayLatestPrice()
        {
            string latestPrice = "";
            string latestTime = GetLatestTime();
            using (StreamReader reader = new StreamReader("SMPReport.csv"))
            {
                reader.ReadLine();
                // Finds top price
                string line = reader.ReadLine(); ;
                latestPrice = line.Split(',')[2];
            }
            Console.WriteLine("The latest price is: $" + latestPrice + " at " + latestTime);
        }

        //Gets the latest Price and returns value
        static string GetLatestPrice()
        {
            string latestPrice = "";
            using (StreamReader reader = new StreamReader("SMPReport.csv"))
            {
                reader.ReadLine();
                //Finds top price
                string line = reader.ReadLine();  
                latestPrice = line.Split(',')[2];
            }
            return latestPrice;
        }

        //Gets the latest Time and returns value
        static string GetLatestTime()
        {
            string latestTime = "";
            using (StreamReader reader = new StreamReader("SMPReport.csv"))
            {
                reader.ReadLine();
                //Finds latest time
                string line = reader.ReadLine(); ;
                latestTime = line.Split(',')[1];
            }

            // Display time of latest Price
            return latestTime;
        }

        //Waits for the URL to update with new data and triggers a visual cue
        static void WaitForUpdates()
        {
            
            Console.WriteLine("Waiting for price updates...");
            string latestPrice = GetLatestPrice();
            string latestPrice1 = GetLatestPrice();
            string latestTime = GetLatestTime();
            while (true)
            {
                if (latestPrice != latestPrice1)
                {
                    DisplayLatestPrice();
                    MessageBox.Show($"The price has changed to {latestPrice1} at {latestTime}", "Price Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    latestPrice = latestPrice1;
                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                    string html = GetHTMLData();
                    CreateCSVFile(html);
                    latestPrice1 = GetLatestPrice();
                    //Console.WriteLine("Nothing has changed. Waiting for updates...");
                }
            }
        }
    }
}


