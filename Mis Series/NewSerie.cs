using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Mis_Series
{
    public partial class NewSerie : Form
    {
        DbHelper database;
        String currentCode;
        public NewSerie(DbHelper db)
        {
            InitializeComponent();
            this.database = db;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            String code = textBoxCode.Text.ToString();
            String name = textBoxName.Text.ToString();
            if (String.IsNullOrEmpty(code) || String.IsNullOrEmpty(name))
            {
                MessageBox.Show("Debes rellenar los campos");
                return;
            }
            else
            {
                int ret = database.insertSerie(name, code);
                if (ret > 0)
                {
                    
                    setCaps(code);
                }
                else
                {
                    MessageBox.Show("Error : La serie Code Ya existe o ocurrio un error al añadir serie");
                }
            }
        }

        protected void setCaps(string code)
        {

            try
            {
                this.currentCode = code;
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

                if (String.IsNullOrEmpty(html))
                {

                    MessageBox.Show("Error : No se pudieron descargar los datos");
                    return;
                }
                this.Text = "Guardando capitulos,Por Favor espere...";
                string match = "<a href='(capitulo.*?serie=(\\d+)&.*?)'>(.*?)</a>.*?<br>";
                string match2 = "<img src=(.*?) border=.*? height=.*? width=.*?>";
                Debug.WriteLine(match);
                foreach (Match m in Regex.Matches(html, match))
                {
                    List<string> langs = new List<string>();
                    Debug.WriteLine(m.Value);

                    foreach (Match mlang in Regex.Matches(m.Value, match2))
                    {
                        langs.Add(Path.GetFileNameWithoutExtension(mlang.Groups[1].Value));

                    }

                    string langsArray = String.Join(",", langs.ToArray());
                    
                    database.insertCapitulo(m.Groups[3].Value, m.Groups[2].Value, m.Groups[1].Value, 0, langsArray);

                }
                MessageBox.Show("Ok, nueva serie añadida");

                this.Text = "Nueva Serie";
                this.Close();

            }


        }

        private void NewSerie_Load(object sender, EventArgs e)
        {

        }

        private void NewSerie_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

       
    }
}
