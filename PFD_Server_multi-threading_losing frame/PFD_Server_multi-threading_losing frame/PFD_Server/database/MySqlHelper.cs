using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data.Common;
using System.Collections.Generic;
using System.Text.RegularExpressions;
//using COMS_Bridge;
//using WindowsFormsApplication1;

namespace HST_Server
{
    /// <summary>
    /// 数据库连接的帮助类
    /// </summary>
    public class MysqlHelper
    {
        //数据库连接字符串(web.config来配置)，可以动态更改connectionString支持多数据库. 
        ConfigClass cf = new ConfigClass(System.Environment.CurrentDirectory + "\\Config.ini");
        public static string pwd { get; set; }       // 登录密码的属性
        string dbIpAddr;  // 数据库服务器

        string dbName;   // 数据库的名称
        string userName;    // 数据库的账户

        string passwd;   // 数据库的登录密码
        string charset;   // 字符集

        public static MysqlHelper Default = new MysqlHelper();
        public static string connectionString;

        public MysqlHelper()
        {
            dbIpAddr = cf.IniReadValue("db", "dbip");
            dbName = cf.IniReadValue("db", "dbname");
            userName = cf.IniReadValue("db", "dbid");
            passwd = cf.IniReadValue("db", "passwd");
            charset = cf.IniReadValue("db", "dbCharset");
        }  // 默认无参构造函数
        public MysqlHelper(string ipAddr, string dbName, string user, string pwd, string charSet)  // 有参构造函数
        {
            this.DbIpAddr = ipAddr;
            this.dbName = dbName;
            this.userName = user;
            this.passwd = pwd;
            this.charset = charSet;
            makeParam();
        }

        public string DbIpAddr  // 数据库IP
        {
            get { return dbIpAddr; }
            set { dbIpAddr = value; makeParam(); }
        }

        public string UserName  // root账户
        {
            get { return userName; }
            set { userName = value; makeParam(); }
        }

        public string Password  // 登录密码
        {
            get { return passwd; }
            set { passwd = value; makeParam(); }
        }

        public string Charset   // 字符集
        {
            get { return charset; }
            set { charset = value; makeParam(); }
        }

        public string DataBaseName // 数据库名称
        {
            get { return dbName; }
            set { dbName = value; makeParam(); }
        }

        // 改变参数，生成连接字符串
        void makeParam()
        {
            connectionString = string.Format("Data Source={0};User ID={1};Password={2};DataBase={3};Charset={4};",
                                                         dbIpAddr, userName, passwd, dbName, charset);
        }

        public void setDbServerIp(string ip)
        {
            dbIpAddr = ip;
            makeParam();
            //connectionString = "Data Source=" + dbIpAddr + ";" + "User ID=root;Password=123456;DataBase=optical;Charset=gb2312;";
        }

        /// <summary> 
        /// 执行SQL语句，返回影响的记录数 
        /// </summary> 
        /// <param name="SQLString">SQL语句</param> 
        /// <returns>影响的记录数</returns>
        public void ExecuteNonQuery(string SQLString)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        //return rows;
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e)
                    {
                        connection.Close();
                        //throw e;
                    }
                }
            }
        }


        /// <summary> 
        /// 执行SQL语句，返回影响的记录数 
        /// </summary> 
        /// <param name="SQLString">SQL语句</param> 
        /// <returns>影响的记录数</returns> 
        public int ExecuteNonQuery(string SQLString, params MySqlParameter[] cmdParms)  //未使用
        {

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e)
                    {
                        throw e;
                    }
                }
            }
        }

        /// <summary> 
        /// 执行多条SQL语句，实现数据库事务。 
        /// </summary> 
        /// 2017-8-3
        /// zhangx
        /// <param name="SQLStringList">多条SQL语句</param> 
        public bool ExecuteNoQueryTran(List<String> SQLStringList)
        {
            makeParam();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                MySqlTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    for (int n = 0; n < SQLStringList.Count; n++)
                    {
                        string strsql = SQLStringList[n];
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            PrepareCommand(cmd, conn, tx, strsql, null);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    return false;
                }
            }
        }

        /// <summary> 
        /// 执行一条计算查询结果语句，返回查询结果（object）。 
        /// </summary> 
        /// <param name="SQLString">计算查询结果语句</param> 
        /// <returns>查询结果（object）</returns> 
        public object ExecuteScalar(string SQLString)
        {
            makeParam();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        return obj;
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }

        /// <summary> 
        /// 执行一条计算查询结果语句，返回查询结果（object）。 
        /// </summary> 
        /// <param name="SQLString">计算查询结果语句</param> 
        /// <returns>查询结果（object）</returns> 
        public object ExecuteScalar(string SQLString, params MySqlParameter[] cmdParms)  //未使用
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e)
                    {
                        throw e;
                    }
                }
            }
        }

        /// <summary> 
        /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close ) 
        /// </summary> 
        /// <param name="strSQL">查询语句</param> 
        /// <returns>MySqlDataReader</returns> 
        public MySqlDataReader ExecuteReader(string strSQL)   //未使用
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand(strSQL, connection);
            try
            {
                connection.Open();
                MySqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return myReader;
            }
            catch (MySql.Data.MySqlClient.MySqlException e)
            {
                throw e;
            }
        }

        /// <summary> 
        /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close ) 
        /// </summary> 
        /// <param name="strSQL">查询语句</param> 
        /// <returns>MySqlDataReader</returns> 
        public MySqlDataReader ExecuteReader(string SQLString, params MySqlParameter[] cmdParms)  //未使用
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand cmd = new MySqlCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                MySqlDataReader myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (MySql.Data.MySqlClient.MySqlException e)
            {
                throw e;
            }
        }

        /// <summary> 
        /// 执行查询语句，返回DataTable 
        /// </summary> 
        /// <param name="SQLString">查询语句</param> 
        /// <returns>DataTable</returns> 
        public DataTable ExecuteDataTable(string SQLString)
        {
            makeParam();
            Console.WriteLine("mysql connection:{0},\n {1} ", connectionString, SQLString);
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    MySqlDataAdapter command = new MySqlDataAdapter(SQLString, connection);
                    command.Fill(ds, "ds");
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    Console.WriteLine("数据库出错:{0}", ex.ToString());
                    return null;
                }
                return ds.Tables[0];
            }
        }


        /// <summary> 
        /// 执行查询语句，返回DataSet 
        /// </summary> 
        /// <param name="SQLString">查询语句</param> 
        /// <returns>DataTable</returns> 
        public DataTable ExecuteDataTable(string SQLString, params MySqlParameter[] cmdParms)  //未使用
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = new MySqlCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                {
                    DataSet ds = new DataSet();
                    try
                    {
                        da.Fill(ds, "ds");
                        cmd.Parameters.Clear();
                    }
                    catch (MySql.Data.MySqlClient.MySqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return ds.Tables[0];
                }
            }
        }
        //获取起始页码和结束页码 
        public DataTable ExecuteDataTable(string cmdText, int startResord, int maxRecord)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                DataSet ds = new DataSet();
                try
                {
                    connection.Open();
                    MySqlDataAdapter command = new MySqlDataAdapter(cmdText, connection);
                    command.Fill(ds, startResord, maxRecord, "ds");
                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                return ds.Tables[0];
            }
        }

        /// <summary> 
        /// 获取分页数据 在不用存储过程情况下 
        /// </summary> 
        /// <param name="recordCount">总记录条数</param> 
        /// <param name="selectList">选择的列逗号隔开,支持top num</param> 
        /// <param name="tableName">表名字</param> 
        /// <param name="whereStr">条件字符 必须前加 and</param> 
        /// <param name="orderExpression">排序 例如 ID</param> 
        /// <param name="pageIdex">当前索引页</param> 
        /// <param name="pageSize">每页记录数</param> 
        /// <returns></returns> 
        public DataTable getPager(out int recordCount, string selectList, string tableName, string whereStr, string orderExpression, int pageIdex, int pageSize)
        {
            int rows = 0;
            DataTable dt = new DataTable();
            //MatchCollection matchs = Regex.Matches(selectList, @"top\s+\d{1,}", RegexOptions.IgnoreCase);//含有top 
            //string sqlStr = sqlStr = string.Format("select {0} from {1} where 1=1 {2}", selectList, tableName, whereStr);
            //if (!string.IsNullOrEmpty(orderExpression)) { sqlStr += string.Format(" Order by {0}", orderExpression); }
            //if (matchs.Count > 0) //含有top的时候 
            //{
            //    DataTable dtTemp = ExecuteDataTable(sqlStr);
            //    rows = dtTemp.Rows.Count;
            //}
            //else //不含有top的时候 
            //{
            //    string sqlCount = string.Format("select count(*) from {0} where 1=1 {1} ", tableName, whereStr);
            //    //获取行数 
            //    object obj = ExecuteScalar(sqlCount);
            //    if (obj != null)
            //    {
            //        rows = Convert.ToInt32(obj);
            //    }
            //}
            //dt = ExecuteDataTable(sqlStr, (pageIdex - 1) * pageSize, pageSize);
            recordCount = rows;
            return dt;
        }

        private void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, string cmdText, MySqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;

            if (trans != null)
            {
                cmd.Transaction = trans;
                cmd.CommandType = CommandType.Text;//cmdType; 
            }

            if (cmdParms != null)
            {
                foreach (MySqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                    (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }
    }

    //public class database
    //   {
    //       /// <summary> 
    //       /// 执行查询语句，返回DataTable 
    //       /// </summary> 
    //       /// <param name="SQLString">查询语句</param> 
    //       /// <returns>DataTable</returns> 
    //       public DataTable ExecuteDataTable(string connectionString, string SQLString)
    //       {


    //           using (MySqlConnection conn = new MySqlConnection(connectionString))
    //           {
    //               DataSet ds = new DataSet();
    //               try
    //               {
    //                   conn.Open();
    //                   MySqlDataAdapter command = new MySqlDataAdapter(SQLString, conn);
    //                   command.Fill(ds, "ds");
    //               }
    //               catch (Exception ex)
    //               {
    //                   return null;
    //               }
    //               return ds.Tables[0];
    //           }
    //       }

    //       /// <summary> 
    //       /// 执行SQL语句，返回影响的记录数 
    //       /// </summary> 
    //       /// <param name="SQLString">SQL语句</param> 
    //       /// <returns>影响的记录数</returns>
    //       public int ExecuteNonQuery(string SQLString)
    //       {
    //           using (MySqlConnection conn = new MySqlConnection(connectionString))
    //           {
    //               using (MySqlCommand cmd = new MySqlCommand(SQLString, conn))
    //               {
    //                   //try
    //                   //{
    //                       conn.Open();
    //                       int rows = cmd.ExecuteNonQuery();
    //                       return rows;
    //                   //}
    //                   //catch (MySql.Data.MySqlClient.MySqlException e)
    //                   //{
    //                   //    //conn.Close();
    //                   //    //throw e;
    //                   //}
    //               }
    //           }
    //       }



    //       /// <summary> 
    //       /// 执行一条计算查询结果语句，返回查询结果（object）。 
    //       /// </summary> 
    //       /// <param name="SQLString">计算查询结果语句</param> 
    //       /// <returns>查询结果（object）</returns> 
    //       public object ExecuteScalar(string SQLString, string connectionString)
    //       {
    //           using (MySqlConnection connection = new MySqlConnection(connectionString))
    //           {
    //               using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
    //               {
    //                   try
    //                   {
    //                       connection.Open();
    //                       object obj = cmd.ExecuteScalar();
    //                       return obj;
    //                   }
    //                   catch (MySql.Data.MySqlClient.MySqlException e)
    //                   {
    //                       connection.Close();
    //                       throw e;
    //                   }
    //               }
    //           }
    //       }
}



