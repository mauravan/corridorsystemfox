using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.IO;

namespace ConsoleApplication1
{
    public class fetch
    {
        // A GET request
        static List<String> getResponse(string link)
        {
            List<String> rVal = new List<String>();

            WebRequest wrGETURL;
            wrGETURL = WebRequest.Create(link);

            Stream objStream;
            objStream = wrGETURL.GetResponse().GetResponseStream();

            StreamReader objReader = new StreamReader(objStream);

            string sLine = "";

            while (sLine != null)
            {
                sLine = objReader.ReadLine();
                if (sLine != null)
                    rVal.Add(sLine);
            }

            return rVal;
        }

        //
        public static List<List<String>> getSchedule(String sign)
        {
            string sURL;
            sURL = "http://ju.se/student/studier/schema.html"; // The starting point

            //
            List<String> response = new List<String>();

            response = getResponse(sURL);

            // Finding the JTH-Calendar link
            foreach (String s in response)
            {
                if (s.IndexOf("class=\"jth sv\"") != -1)
                {
                    sURL = "http://ju.se";
                    sURL += s.Substring(s.IndexOf("href=") + 6, -(s.IndexOf("href=") + 6) + s.IndexOf("\">")); // This is the next link
                    break;
                }
            }

            response = getResponse(sURL);

            // Finding the "Visa schema" -button
            for (int i = 0; i < response.Count; i++)
            {
                string s = response[i];
                if (s.IndexOf(">Avancerad s") != -1)
                {
                    if (response.Count > i + 1)
                    {
                        i++;
                        while (response[i].IndexOf("<form") < 0) { i++; if (i > response.Count - 1) break; }
                        s = response[i];

                        sURL = "http://ju.se";
                        sURL += s.Substring(s.IndexOf("action=") + 8, -(s.IndexOf("action=") + 8) + s.IndexOf("\">")); // This is the next link
                    }
                    else Console.WriteLine("ERROR ERROR ERROR"); // ej sannolikt
                    break;
                }
            }

            // Adding form parameters to GET request
            DateTime today = DateTime.Today;
            string from = today.ToString("d");
            int newMonth = Convert.ToInt16(from.Substring(5, 2)) + 1; if (newMonth > 12) newMonth = 1;
            string strNewMonth = Convert.ToString(newMonth);
            if (strNewMonth.Length == 1) strNewMonth = "0" + strNewMonth;
            string to = from;
            to = to.Remove(5, 2).Insert(5, strNewMonth);

            sURL += "/?from=" + from + "&to=" + to + "&sign=" + sign + "&bolag=" + "jth" + "&lang=" + "sv";
            sURL = sURL.Remove(sURL.IndexOf(";jsessionid"), sURL.IndexOf("/?from") - sURL.IndexOf(";jsessionid") + 1);
            sURL = sURL.Insert(sURL.IndexOf("?"), "ical.php");
            // Now we can get the calendar...
            List<List<String>> llStr = new List<List<String>>();

            response = getResponse(sURL);

            for (int i = 0; i < response.Count; i++)
            {
                string s;
                List<String> _lStr = new List<String>();

                while (response.Count > i + 1 && response[i].IndexOf("BEGIN:VEVENT") < 0) i++;
                while (response.Count > i + 1 && response[i].IndexOf("DTSTART:") < 0) i++;

                if (response[i].IndexOf("END:VCALENDAR") > -1 || !(response.Count > i + 1)) break;

                s = response[i];
                _lStr.Add(s.Substring(8, 4) + "-" + s.Substring(12, 2) + "-" + s.Substring(14, 2)); // day

                int startH = Convert.ToInt16(s.Substring(17, 2));
                int startM = Convert.ToInt16(s.Substring(19, 2));

                while (response.Count > i + 1 && response[i].IndexOf("DURATION:") < 0) i++;
                s = response[i];

                int durH = Convert.ToInt16(s.Substring(s.IndexOf("PT") + 2, s.IndexOf("H") - (s.IndexOf("PT") + 2)));
                int durM = Convert.ToInt16(s.Substring(s.IndexOf("H") + 1, s.IndexOf("M") - (s.IndexOf("H") + 1)));

                string endM = Convert.ToString(startM + durM > 59 ? startM + durM - 60 : startM + durM);

                string h1 = Convert.ToString(startH); if (h1.Length == 1) h1 = h1.Insert(0, "0");
                string h2 = Convert.ToString(startH + durH > 23 ? startH + durH - 24 : startH + durH); if (h2.Length == 1) h2 = h2.Insert(0, "0");
                string m1 = Convert.ToString(startM); if (m1.Length == 1) m1 = m1.Insert(0, "0");
                if (endM.Length == 1) endM = endM.Insert(0, "0");

                _lStr.Add(
                    h1 + ":" + m1 +
                    " - " + h2 + ":" + endM
                    );

                while (response.Count > i + 1 && response[i].IndexOf("SUMMARY:") < 0) i++;
                _lStr.Add(response[i].Remove(0, 8));

                while (response.Count > i + 1 && response[i].IndexOf("LOCATION:") < 0) i++;
                _lStr.Add(response[i].Remove(0, 9));

                while (response.Count > i + 1 && response[i].IndexOf("END:VEVENT") < 0) i++;

                if (_lStr.Count > 0)
                    llStr.Add(_lStr);
            }

            return llStr;
        }
    }
}
