using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using Schedule.Tasks.Proxy;

namespace Schedule.Tasks.Remoting
{
    public class SQLiteLogReader : MarshalByRefObject, ILogReader
    {
        #region ILogReader 成员

        string _ConnectionString = string.Format("Data Source={0}", System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "log4net.s3db"));

        public IList<string> Read(int index, int count)
        {
            string sql = string.Format("SELECT ID,LOGBY,LOGTYPE,LOGTIME,CONTENT,STACKTRACE FROM LOG4NET ORDER BY ID DESC LIMIT {0},{1}", index < 0 ? 0 : index, count <= 0 ? 1 : count);
            List<string> list = new List<string>();
            using (SQLiteConnection conn = new SQLiteConnection())
            {
                conn.ConnectionString = _ConnectionString;
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(string.Format("[ID]:{0}|[LOGBY]:{1}|[LOGTYPE]:{2}|[LOGTIME]:{3}|[CONTENT]:{4}|[STACKTRACE]:{5}", dr["ID"], dr["LOGBY"], dr["LOGTYPE"], dr["LOGTIME"], dr["CONTENT"], dr["STACKTRACE"]));
                    }
                }
                cmd.Dispose();
                return list;
            }
        }

        public IList<string> Read(string type, int index, int count)
        {
            string sql = string.Format("SELECT ID,LOGBY,LOGTYPE,LOGTIME,CONTENT,STACKTRACE FROM LOG4NET WHERE TRIM(LOGTYPE)='{2}' ORDER BY ID DESC LIMIT {0},{1}", index < 0 ? 0 : index, count <= 0 ? 1 : count, type);
            List<string> list = new List<string>();
            using (SQLiteConnection conn = new SQLiteConnection())
            {
                conn.ConnectionString = _ConnectionString;
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(string.Format("[ID]:{0}|[LOGBY]:{1}|[LOGTYPE]:{2}|[LOGTIME]:{3}|[CONTENT]:{4}|[STACKTRACE]:{5}", dr["ID"], dr["LOGBY"], dr["LOGTYPE"], dr["LOGTIME"], dr["CONTENT"], dr["STACKTRACE"]));
                    }
                }
                cmd.Dispose();
                return list;
            }
        }

        public void Clean()
        {
            string sql = string.Format("DELETE FROM Log4net");
            using (SQLiteConnection conn = new SQLiteConnection())
            {
                conn.ConnectionString = _ConnectionString;
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }

        public void Clean(string type)
        {
            string sql = string.Format("DELETE FROM Log4net WHERE TRIM(LOGTYPE)='{0}'", type);
            using (SQLiteConnection conn = new SQLiteConnection())
            {
                conn.ConnectionString = _ConnectionString;
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }

        public void Delete(string[] ids)
        {
            if (ids == null || ids.Length == 0)
                return;
            string sql = string.Format("DELETE FROM Log4net WHERE Id IN ({0})", string.Join(",", ids));
            using (SQLiteConnection conn = new SQLiteConnection())
            {
                conn.ConnectionString = _ConnectionString;
                conn.Open();
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
        }



        #endregion
    }
}
