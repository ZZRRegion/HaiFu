using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace HaiFu.Project
{
    public static class SQLServerProject
    {
        private static SqlConnection GetSqlConnection()
        {
            string connection = "server=.;database=stdio;uid=sa;pwd=stdio;";
            SqlConnection con = new SqlConnection(connection);
            con.Open();
            return con;
        }
        private static void CloseSqlConnection(SqlConnection con)
        {
            con?.Close();
            con?.Dispose();
        }
        public static int ExecuteNonQuery(string sql)
        {
            SqlConnection con = GetSqlConnection();
            SqlCommand cmd = con.CreateCommand();
            cmd.CommandText = sql;
            int count = cmd.ExecuteNonQuery();
            CloseSqlConnection(con);
            return count;
        }
        public static int ExecuteNonQuery(string sql, List<SqlParameter> lstParams)
        {
            SqlConnection con = GetSqlConnection();
            SqlCommand cmd = con.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddRange(lstParams.ToArray());
            int count = cmd.ExecuteNonQuery();
            CloseSqlConnection(con);
            return count;
        }
        public static DataTable GetDataTable(string sql)
        {
            DataTable dt = new DataTable();
            SqlConnection con = GetSqlConnection();
            SqlCommand cmd = con.CreateCommand();
            cmd.CommandText = sql;
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            da.Dispose();
            CloseSqlConnection(con);
            return dt;
        }
    }
}
