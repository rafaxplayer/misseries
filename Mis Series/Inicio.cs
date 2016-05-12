
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mis_Series
{
    public partial class InicioForm : Form
    {
        DbHelper db;
        bool disableItemCheckEvent = false;
        string currentCode;
        private ContextMenu tray_menu;
        private ContextMenu listSeries_menu;

        public InicioForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            createContextMenuIcon();
            notifyIcon1.Visible = true;
            notifyIcon1.Text = "Mis Series";
            notifyIcon1.ContextMenu = tray_menu;
            timer1.Enabled = true;
            listViewSeries.ContextMenu = listSeries_menu;
            db = new DbHelper();
            checkStartup();
            checkData();

        }

        private void setBadgeNum(int num)
        {

            Icon ic = Helper.GetIconNum(num);
            this.Icon = ic;
            notifyIcon1.Icon = ic;
        }

        private void timer_tick(object sender, EventArgs e)
        {

            checkData();
        }

        private void checkStartup()
        {
            String[] arguments = Environment.GetCommandLineArgs();
            if (arguments.Length > 1)
            {
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }

            if (Helper.rkApp.GetValue("MisSeries") == null)
            {

                checkBox1.Checked = false;

            }
            else
            {

                checkBox1.Checked = true;
            }
        }

        private void checkData()
        {
            listseries();
            LoadData dat = new LoadData(db);
            dat.Run();
            List<News> ls = dat.checkNews();
            if (ls.Count > 0)
            {
                setBadgeNum(ls.Count);
                StringBuilder sb = new StringBuilder();
                foreach (News n in ls)
                {
                    sb.Append(n.Title);
                    sb.Append(Environment.NewLine);
                    foreach (ListViewItem it in listViewSeries.Items)
                    {
                        if (it.SubItems[1].Text.ToString() == n.Serie_Code)
                        {

                            it.BackColor = Color.Lime;
                        }

                    }

                }

                showNotify("Hay capitulos nuevos en tus series :", sb.ToString());
                db.setAllAvisados();
            }
        }

        private void listseries()
        {
            SQLiteDataReader reader = db.loadDataWithTable(DbHelper.TABLE_SERIES);
            if (reader != null)
            {
                listViewSeries.Items.Clear();
                while (reader.Read())
                {
                    ListViewItem it = new ListViewItem(reader["name"].ToString());
                    it.BackColor = Color.FromArgb(42, 42, 42);

                    it.SubItems.Add(reader["serie_code"].ToString());

                    it.Tag = reader["id"].ToString();
                    listViewSeries.Items.Add(it);
                }
                reader.Close();
                reader.Dispose();
            }
        }

        private void listarCapitulos(string code)
        {
            SQLiteDataReader reader = db.loadCapitulos(code);
            if (reader != null)
            {
                dataGridView1.Rows.Clear();
                while (reader.Read())
                {
                    bool check = Convert.ToInt32(reader["capitulo_visto"]) == 0 ? false : true;
                    int ind = dataGridView1.Rows.Add();
                    ((DataGridViewCheckBoxCell)dataGridView1.Rows[ind].Cells["Column1"]).Value = check;
                    dataGridView1.Rows[ind].Cells["Column2"].Value = new ItemData(reader["name"].ToString(), Convert.ToInt32(reader["id"]), reader["capitulo_url"].ToString());
                    Bitmap bmp = Helper.CombineLangBitmap(reader["capitulo_language"].ToString());
                    dataGridView1.Rows[ind].Cells["Column3"].Value = bmp;
                }

                reader.Close();
                reader.Dispose();
            }
        }

        private void nuevaSerieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewSerie dlg = new NewSerie(db);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                listseries();
            }
        }

        private void listViewSeries_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection breakfast =
        (sender as ListView).SelectedItems;
            foreach (ListViewItem item in breakfast)
            {
                string code = item.SubItems[1].Text.ToString();
                labelSerie.Text = item.SubItems[0].Text.ToString();
                this.disableItemCheckEvent = true;
                listarCapitulos(code);
                this.currentCode = code;
            }

            this.disableItemCheckEvent = false;
            setBadgeNum(0);
        }

        public class ItemData
        {
            public string Name;
            public int Id;
            public string url;
            public ItemData(string name, int id, string url)
            {
                this.Name = name;
                this.Id = id;
                this.url = url;
            }

            public override string ToString()
            {
                return this.Name;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {

                int ch = (sender as CheckBox).Checked ? 1 : 0;

                if (!String.IsNullOrEmpty(currentCode))
                {
                    this.disableItemCheckEvent = true;
                    int ret = db.setCheckAllCapitulos(this.currentCode, ch);
                    if (ret > 0)
                    {
                        listarCapitulos(this.currentCode);
                    }
                    this.disableItemCheckEvent = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        private void createContextMenuIcon()
        {

            tray_menu = new ContextMenu();
            tray_menu.MenuItems.Add(0,
                new MenuItem("Refrescar Datos", new System.EventHandler(Refresh_Click)));
            tray_menu.MenuItems.Add(1,
                new MenuItem("Capitulos No vistos", new System.EventHandler(noVistosShow)));
            tray_menu.MenuItems.Add(2,
                new MenuItem("Mostrar", new System.EventHandler(Show_Click)));
            tray_menu.MenuItems.Add(3,
                new MenuItem("Minimizar", new System.EventHandler(Hide_Click)));
            tray_menu.MenuItems.Add(4,
                new MenuItem("Salir De Mis Series", new System.EventHandler(Exit_Click)));

            listSeries_menu = new ContextMenu();
            listSeries_menu.MenuItems.Add(0,
                new MenuItem("Eliminar Serie", new System.EventHandler(DeleteSerie_Click)));
            listSeries_menu.MenuItems.Add(1,
                new MenuItem("Actualizar Series", new System.EventHandler(Refresh_Click)));
            listSeries_menu.MenuItems.Add(2,
                new MenuItem("Capitulos No vistos", new System.EventHandler(noVistosShow)));
        }

        protected void noVistosShow(Object sender, System.EventArgs e)
        {
            int noVistos = db.noVistosOrNoAvisadosCount(DbHelper.CAPITULO_VISTO);
            if (noVistos > 0)
            {
                Nuevos dlg = new Nuevos();
                dlg.ShowDialog(this);
            }
            else
            {
                MessageBox.Show("No hay capitulos \"No Vistos\"", "Mis Series");
            }

        }

        protected void Exit_Click(Object sender, System.EventArgs e)
        {
            Environment.Exit(0);
        }
        protected void Hide_Click(Object sender, System.EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
            this.ShowInTaskbar = false;
        }
        protected void Show_Click(Object sender, System.EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.Show();
            this.ShowInTaskbar = true;
        }
        protected void Refresh_Click(Object sender, System.EventArgs e)
        {
            checkData();

        }

        protected void DeleteSerie_Click(Object sender, System.EventArgs e)
        {
            try
            {
                if (listViewSeries.SelectedItems.Count > 0)
                {
                    String name = (listViewSeries.SelectedItems[0] as ListViewItem).Text.ToString();
                    if (MessageBox.Show("¿Seguro Quieres eliminar la serie seleccionada?", "Eliminar " + name, MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        string code = (listViewSeries.SelectedItems[0] as ListViewItem).SubItems[1].Text.ToString();
                        if (db.DeleteData(DbHelper.TABLE_SERIES, DbHelper.SERIE_CODE, code) > 0)
                        {
                            listseries();
                            MessageBox.Show("Ok , Serie eliminada");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void showNotify(string title, string text)
        {
            notifyIcon1.BalloonTipTitle = title;
            notifyIcon1.BalloonTipText = text;
            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.ShowBalloonTip(50000);
        }

        private void checkBox1_CheckedChanged_1(object sender, EventArgs e)
        {
            if ((sender as CheckBox).Checked)
            {
                Helper.rkApp.SetValue("MisSeries", string.Format(@"{0} /fromStartup", Application.ExecutablePath));
            }
            else
            {
                Helper.rkApp.DeleteValue("MisSeries", false);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.ShowInTaskbar = false;
            this.Hide();
            if (Mis_Series.Properties.Settings.Default.MiniMizeMessage)
            {
                showNotify("Minimizar mis series", "La aplicacion seguira activa aqui en area de notificaciones");
            }
            Mis_Series.Properties.Settings.Default.MiniMizeMessage = false;
            Mis_Series.Properties.Settings.Default.Save();
            e.Cancel = true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (db != null)
            {
                db.close();
            }
        }

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                string url = (dataGridView1.Rows[e.RowIndex].Cells[1].Value as ItemData).url;
                Process.Start(Helper.URLBASE_CAPITULO + url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (!disableItemCheckEvent)
                {

                    if (e.ColumnIndex == 0 && e.RowIndex != -1)
                    {
                        Boolean res = (Boolean)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                        int ch = res ? 1 : 0;
                        ItemData it = (dataGridView1.Rows[e.RowIndex].Cells[1].Value as ItemData);

                        int ret = db.setCheckCapitulo(it.Id, ch);
                        if (ret > 0)
                        {
                            Debug.WriteLine(it.Id + " " + ch);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void futureButton1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void futureButton2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
