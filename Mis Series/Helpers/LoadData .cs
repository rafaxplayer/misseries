using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Mis_Series
{
    class LoadData
    {
        DbHelper db;
        string currentCode;

        public LoadData(DbHelper database)
        {
            this.db = database;
        }


        public void Run()
        {

            SQLiteDataReader reader = db.loadDataWithTable(DbHelper.TABLE_SERIES);
            if (reader != null)
            {
                while (reader.Read())
                {
                    this.currentCode = reader["serie_code"].ToString();

                    setCaps(reader["serie_code"].ToString());
                }
                reader.Close();
                reader.Dispose();
            }
        }

        protected void setCaps(string code)
        {
            try
            {
                WebClient client = new WebClient();
                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadStringCallback);
                client.DownloadStringAsync(new Uri(Helper.URLBASE_SERIE + code));

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void DownloadStringCallback(Object sender, DownloadStringCompletedEventArgs e)
        {

            if (!e.Cancelled && e.Error == null)
            {
                string html = (string)e.Result;
                File.WriteAllText("log.txt", html);
                string match = "<a href='(capitulo.*?serie=(\\d+)&.*?)'>(.*?)</a>.*?<br>";
                string match2 = "<img src=(.*?) border=.*? height=.*? width=.*?>";
                //Debug.WriteLine(match);
                foreach (Match m in Regex.Matches(html, match))
                {
                    
                    List<string> langs = new List<string>();
                    
                    foreach (Match mlang in Regex.Matches(m.Value, match2))
                    {
                        langs.Add(Path.GetFileNameWithoutExtension(mlang.Groups[1].Value));

                    }

                    string langsArray = String.Join(",", langs.ToArray());

                    db.insertCapitulo(m.Groups[3].Value, m.Groups[2].Value, m.Groups[1].Value, 0, langsArray);

                }

            }

        }

        public List<News> checkNews()
        {
            SQLiteDataReader reader = db.loadDataWithQuery("SELECT * FROM tbl_Capitulos WHERE capitulo_visto = 0 AND capitulo_avisado = 0");
            var theNews = new List<News>();
            if (reader != null)
            {

                while (reader.Read())
                {
                    var n = new News() { Title = reader["name"].ToString(), Serie_Code = reader["serie_code"].ToString(), id = Convert.ToInt32(reader["id"]) };
                    theNews.Add(n);

                }
            }
            return theNews;
        }


    }
}
