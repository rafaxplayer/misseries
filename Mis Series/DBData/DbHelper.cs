using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;


namespace Mis_Series
{
    public class DbHelper
    {
        public SQLiteConnection m_dbConnection = null;
        private SQLiteCommand command = null;
        public static String DATABASE_NAME = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BDSeries.sqlite");

        //Tablas nombres...
        public static String TABLE_SERIES = "tbl_Series";
        public static String TABLE_CAPITULOS = "tbl_Capitulos";

        //Campos generales...
        public static String ID = "id";
        public static String NAME = "name";
        public static String SERIE_CODE = "serie_code";
        public static String CAPITULO_URL = "capitulo_url";
        public static String CAPITULO_VISTO = "capitulo_visto";
        public static String CAPITULO_AVISADO = "capitulo_avisado";
        public static String CAPITULO_LANG = "capitulo_language";

        private static String SqlCreateTable_Series = "CREATE TABLE IF NOT EXISTS "
                + TABLE_SERIES + "(" + ID + " INTEGER PRIMARY KEY,"
                + SERIE_CODE + " TEXT UNIQUE NOT NULL, "
                + NAME + " TEXT UNIQUE)";

        private static String SqlCreateTable_Capitulos = "CREATE TABLE IF NOT EXISTS "
                + TABLE_CAPITULOS + "(" + ID + " INTEGER PRIMARY KEY,"
                + NAME + " TEXT  UNIQUE, "
                + SERIE_CODE + " TEXT, "
                + CAPITULO_URL + "  TEXT, "
                + CAPITULO_VISTO + "  INTEGER DEFAULT 0, "
                + CAPITULO_AVISADO + " INTEGER DEFAULT 0, "
                + CAPITULO_LANG + " TEXT)";

        private static String SqlCreateTrigger_OnDeleteSerie = " CREATE  TRIGGER IF NOT EXISTS ONDELETE_SERIE BEFORE DELETE "
                + " ON " + TABLE_SERIES
                + " FOR EACH ROW "
                + " BEGIN "
                + " DELETE FROM " + TABLE_CAPITULOS + " WHERE " + SERIE_CODE + " = OLD.serie_code;"
                + " END; ";

        private static String SqlCreateTrigger_OnUpdateCapituloVisto = " CREATE  TRIGGER IF NOT EXISTS ONUPDATE_CAPITULOVISTO BEFORE UPDATE "
                + " ON " + TABLE_CAPITULOS
                + " WHEN new.capitulo_visto = 1"
                + " BEGIN "
                + " UPDATE " + TABLE_CAPITULOS + " SET capitulo_avisado = 1 WHERE id = OLD.id;"
                + " END; ";

        //construir base de datos .....
        public DbHelper()
        {
            try
            {
                if (!File.Exists(DATABASE_NAME))
                {

                    SQLiteConnection.CreateFile(DATABASE_NAME);

                    m_dbConnection = new SQLiteConnection(String.Format(Helper.CONNECTION_STRING, DATABASE_NAME));
                    m_dbConnection.Open();
                    Debug.Write("Open data base");

                    command = new SQLiteCommand(SqlCreateTable_Series, m_dbConnection);
                    command.ExecuteNonQuery();

                    command = new SQLiteCommand(SqlCreateTable_Capitulos, m_dbConnection);
                    command.ExecuteNonQuery();

                    command = new SQLiteCommand(SqlCreateTrigger_OnDeleteSerie, m_dbConnection);
                    command.ExecuteNonQuery();

                    command = new SQLiteCommand(SqlCreateTrigger_OnUpdateCapituloVisto, m_dbConnection);
                    command.ExecuteNonQuery();

                }
                else
                {
                    m_dbConnection = new SQLiteConnection(String.Format(Helper.CONNECTION_STRING, DATABASE_NAME));
                    m_dbConnection.Open();
                    Debug.Write("Open Data base", "mis_series");
                }
            }
            catch (SQLiteException ex)
            {
                Debug.Write("Error : " + ex.Message);
            }

        }

        //############################# Generic methods #############################
        public void close()
        {
            if (m_dbConnection.State.ToString().Equals("Open"))
            {
                m_dbConnection.Close();
                Debug.Write("Close data base", "mis_series");
            }
        }

        public int DeleteData(String tableName, String campo, String value)
        {
            int ret = 0;
            try
            {
                String sql = "delete from " + tableName + " where " + campo + " = " + value;
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                ret = command.ExecuteNonQuery();
                return ret;
            }
            catch (SQLiteException ex)
            {
                Debug.Write("Error : " + ex.Message, "mis_series");

            }
            return ret;
        }

        public SQLiteDataReader loadDataWithID(String tableName, int ID)
        {
            SQLiteDataReader reader = null;
            try
            {
                String sql = "select * from " + tableName + " where id=" + ID + " order by " + NAME + " asc";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                reader = command.ExecuteReader();
                return reader;
            }
            catch (SQLiteException ex)
            {
                Debug.Write("Error : " + ex.Message, "mis_series");

            }
            return reader;
        }

        public SQLiteDataReader loadDataWithQuery(string query)
        {
            SQLiteDataReader reader = null;
            try
            {
                SQLiteCommand command = new SQLiteCommand(query, m_dbConnection);
                reader = command.ExecuteReader();
                return reader;
            }
            catch (SQLiteException ex)
            {
                Debug.Write("Error : " + ex.Message, "mis_series");

            }
            return reader;
        }
        public int noVistosOrNoAvisadosCount(string colummn)
        {
            int ret = 0;

            try
            {
                string sql = "SELECT COUNT(id) FROM tbl_Capitulos WHERE " + colummn + " = 0";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);

                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (SQLiteException ex)
            {
                Debug.Write("Error : " + ex.Message, "mis_series");

            }
            return ret;
        }

        public SQLiteDataReader loadDataWithTable(String tableName)
        {
            SQLiteDataReader reader = null;
            try
            {
                String sql = "select * from " + tableName + " ORDER by name ASC";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                reader = command.ExecuteReader();
                return reader;
            }
            catch (SQLiteException ex)
            {
                Debug.Write("Error : " + ex.Message, "mis_series");

            }
            return reader;
        }
        //############################# Expecific methods #############################

        public SQLiteDataReader loadCapitulos(String code)
        {
            SQLiteDataReader reader = null;
            try
            {
                String sql = "select * from " + TABLE_CAPITULOS + " WHERE serie_code =" + code + " ORDER BY name ASC";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                reader = command.ExecuteReader();
                return reader;
            }
            catch (SQLiteException ex)
            {
                Debug.Write("Error : " + ex.Message, "mis_series");

            }
            return reader;
        }


        public int insertSerie(String name, String code)
        {
            int ret = 0;
            try
            {
                String sql = "insert or ignore into " + TABLE_SERIES + " (" + NAME + " ,"
                    + SERIE_CODE + ") values ('" + name + "','" + code + "')";

                command = new SQLiteCommand(sql, m_dbConnection);
                ret = command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine(ex.Message, "mis_series");
            }
            return ret;
        }

        public int insertCapitulo(String name, String code, String url, int visto, string langs)
        {
            int ret = 0;
            try
            {
                String sql = "insert or ignore into " + TABLE_CAPITULOS + " (" + NAME + " ,"
                        + SERIE_CODE + " ,"
                        + CAPITULO_URL + " ,"
                        + CAPITULO_VISTO + " ,"
                        + CAPITULO_LANG + ") values ('" + name + "','" + code + "','" + url + "','" + visto + "','" + langs + "')";

                command = new SQLiteCommand(sql, m_dbConnection);
                ret = command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return ret;
        }

        public int setCheckCapitulo(int id, int check)
        {
            int ret = 0;
            try
            {
                string sql = "UPDATE " + TABLE_CAPITULOS + " SET capitulo_visto=" + check + " WHERE " + ID + "=" + id;
                command = new SQLiteCommand(sql, m_dbConnection);
                ret = command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {

                Debug.WriteLine(ex.Message, "mis_series");
            }
            return ret;
        }

        public DataTable loadNoVistos()
        {
            DataSet st = new DataSet();
            DataTable dt = new DataTable();
            try
            {
                String sql = "SELECT tbl_Capitulos.id AS capituloid, tbl_Capitulos.name As capituloname, tbl_Capitulos.capitulo_url AS capitulourl, tbl_Capitulos.serie_code, tbl_Series.serie_code,tbl_Series.name as seriename FROM tbl_Capitulos LEFT JOIN tbl_Series ON tbl_Capitulos.serie_code=tbl_Series.serie_code WHERE tbl_Capitulos.capitulo_visto=0";
                SQLiteDataAdapter dataAdapter =
                        new SQLiteDataAdapter(sql, m_dbConnection);
                dataAdapter.Fill(st);
                dt = st.Tables[0];

                return dt;
            }
            catch (SQLiteException ex)
            {
                Debug.Write("Error : " + ex.Message);

            }
            return dt;
        }

        public int setCheckAllCapitulos(string code, int check)
        {
            int ret = 0;
            try
            {
                string sql = "UPDATE " + TABLE_CAPITULOS + " SET capitulo_visto=" + check + " WHERE " + SERIE_CODE + "=" + code;
                command = new SQLiteCommand(sql, m_dbConnection);
                ret = command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {

                Debug.WriteLine(ex.Message);
            }
            return ret;
        }

        public int setAllAvisados()
        {
            int ret = 0;
            try
            {
                string sql = "UPDATE " + TABLE_CAPITULOS + " SET capitulo_avisado= 1 WHERE " + ID + " > 0";
                command = new SQLiteCommand(sql, m_dbConnection);
                ret = command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {

                Debug.WriteLine(ex.Message);
            }
            return ret;
        }

    }

}
