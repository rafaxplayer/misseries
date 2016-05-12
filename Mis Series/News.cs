using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Mis_Series
{
    public partial class Nuevos : Form
    {
        DbHelper db;
        public Nuevos()
        {
            InitializeComponent();
            db = new DbHelper();
            loadNoVistosGrid();

        }

        private void loadNoVistosGrid()
        {
            try
            {
                DataTable dat = db.loadNoVistos();
                dataGridView1.AutoGenerateColumns = false;
                dataGridView1.Columns["id"].DataPropertyName = "capituloid";
                dataGridView1.Columns["Capitulo"].DataPropertyName = "capituloname";
                dataGridView1.Columns["Serie"].DataPropertyName = "seriename";
                dataGridView1.Columns["url"].DataPropertyName = "capitulourl";

                if (dat.Rows.Count > 0)
                {

                    dataGridView1.DataSource = dat;
                }
            }catch(Exception e){

                Debug.WriteLine(e.Message);
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int id=Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString());
                string cap = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                string url = Helper.URLBASE_CAPITULO + dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
                if (MessageBox.Show("Deseas ver el Capitulo " + cap, "", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {

                    Process.Start(url);
                    db.setCheckCapitulo(id, 1);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        private void futureButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
