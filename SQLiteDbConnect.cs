using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Windows.Forms;



namespace wujinprt
{
    public class DbConnect
    {

        private string dbPath = "";
        private string db_name = "materListCode.db";
        public DataSet dbset = new DataSet();
        public SQLiteConnection cnn = new SQLiteConnection();
        
        public DbConnect()
        {
            dbPath = $@"Data Source={Application.StartupPath}\\{db_name}";
            cnn = new SQLiteConnection(dbPath);
        }

        public DbConnect(string strDbName)
        {

            dbPath = $@"Data Source={Application.StartupPath}\\{strDbName}";
            cnn = new SQLiteConnection(dbPath);
        }

        public void dbOpen()
        {
            cnn = new SQLiteConnection(dbPath);
            if (cnn.State == ConnectionState.Closed)
            {
                cnn.Open();
            }
        }


        public void dbClose()
        {
            cnn = new SQLiteConnection(dbPath);
            if (cnn != null)
            {
                if (cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                }
                cnn.Dispose();
            }
        }

        /// <summary>
        /// 查询表是否存在
        /// </summary>
        /// <param name="strSql">sql查询语句</param>
        /// <returns>是否存在 true or false </returns>
        public bool tableExists(string strSql)
        {
            SQLiteCommand command = new SQLiteCommand();
            try
            {
                dbOpen();
                command = new SQLiteCommand(strSql, cnn);
                return command.ExecuteReader().Read();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                command.Dispose();
                dbClose();
            }
        }

        /// <summary>
        /// 查询 (DataSet)
        /// </summary>
        /// <param name="strSql">查询sql</param>
        /// <returns>查询结果</returns>
        public DataSet GetDataSet(string strSql)
        {
            SQLiteDataAdapter sda = new SQLiteDataAdapter();
            try
            {
                dbOpen();
                sda = new SQLiteDataAdapter(strSql, cnn);
                sda.Fill(dbset);
                sda.Update(dbset);
                return dbset;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                sda.Dispose();
                dbClose();
            }
        }
        /// <summary>
        /// 查询 (DataSet) 带表名
        /// </summary>
        /// <param name="strSql">sql</param>
        /// <param name="tableName">表名</param>
        /// <returns>查询结果</returns>
        public DataSet GetDataSet(string strSql, string tableName)
        {
            SQLiteDataAdapter sda = new SQLiteDataAdapter();
            try
            {
                dbOpen();
                sda = new SQLiteDataAdapter(strSql, cnn);
                sda.Fill(dbset, tableName);
                return dbset;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                sda.Dispose();
                dbClose();
            }
        }

        /// <summary>
        /// 查询 datatable
        /// </summary>
        /// <param name="strSql">sql</param>
        /// <returns>查询结果</returns>
        public DataTable GetDataTable(string strSql)
        {
            return GetDataSet(strSql).Tables[0];
        }
        /// <summary>
        /// 查询 datatable 带表名
        /// </summary>
        /// <param name="strSql">sql</param>
        /// <param name="tableName">表名</param>
        /// <returns>查询结果datatable</returns>
        public DataTable GetDataTable(string strSql, string tableName)
        {
            return GetDataSet(strSql, tableName).Tables[tableName];
        }

        /// <summary>
        /// 查询（DataView）
        /// </summary>
        /// <param name="strSql">SQL语句</param>
        /// <returns>查询结果</returns>
        public DataView GetDataView(string strSql)
        {
            return GetDataSet(strSql).Tables[0].DefaultView;
        }

        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="strSql">sql语句</param>
        public void doSql(string strSql)
        {
            SQLiteCommand command = new SQLiteCommand();
            try
            {
                dbOpen();
                command.CommandType = CommandType.Text;
                command = new SQLiteCommand(strSql, cnn);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                dbClose();
            }
        }

        public bool Cnn()
        {
            //cnn = new SQLiteConnection(dbPath);
            if (cnn.State == ConnectionState.Open)
            {
                return true;
            }
            else
            {
                try
                {
                    dbOpen();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("连接失败   " + ex.Message);
                    return false;
                }
                return true;
            }
        }
        /// <summary>
        /// 保存数据到数据库中
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="tbname">表名</param>
        /// <returns>返回成功或失败</returns>
        public string saveData(string sql, string tbname)
        {
            DataTable getChangeTable;
            getChangeTable = dbset.Tables[tbname].GetChanges();
            if (getChangeTable == null)
            {
                return "数据未改变";
            }
            try
            {

                dbOpen();
                DataSet tmpSet0 = new DataSet();//存放有变更的数据
                getChangeTable.TableName = tbname;
                tmpSet0.Tables.Add(getChangeTable);
                if (Cnn() == false)
                {
                    return "打开连接失败";
                }

                sql = System.Text.RegularExpressions.Regex.Split(sql.ToLower(), " where ")[0] + " where 6=7 ";
                DataSet tmpset = new DataSet();
                SQLiteCommand AdoCmd = new SQLiteCommand(sql, cnn);
                SQLiteDataAdapter dapt = new SQLiteDataAdapter();
                dapt.SelectCommand = AdoCmd;
                SQLiteCommandBuilder my_Builder = new SQLiteCommandBuilder(dapt);
                dapt.Fill(tmpset, tbname);
                tmpset.Merge(tmpSet0);
                dapt.Update(tmpset, tbname);
                dbset.Tables[tbname].AcceptChanges();
                return "处理完毕";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                dbClose();
            }
        }

    }
}
