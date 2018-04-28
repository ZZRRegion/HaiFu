using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;

namespace HaiFu.Project.Dal
{
    public class DalBase<T> where T:Model.ModelBase,new()
    {
        public bool Add(T model)
        {
            PropertyInfo[] pis = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            List<SqlParameter> lst = new List<SqlParameter>();
            string sql = $"Insert Into {typeof(T).Name}";
            string columns = string.Empty;
            string values = string.Empty;
            foreach(PropertyInfo pi in pis)
            {
                if (pi.Name == "Id")
                    continue;
                if (!string.IsNullOrEmpty(columns))
                    columns += " ,";
                if (!string.IsNullOrEmpty(values))
                    values += " ,";
                string name = pi.Name;
                object value = pi.GetValue(model);
                columns += name;
                values += $"@{name}";
                SqlParameter par = new SqlParameter($"@{name}", value);
                lst.Add(par);
            }
            sql = $"{sql}({columns}) Values({values})";
            int count = SQLServerProject.ExecuteNonQuery(sql, lst);
            return count > 0;
        }
        public bool Delete(int id)
        {
            string sql = $"Delete from {typeof(T).Name} where Id={id}";
            int count = SQLServerProject.ExecuteNonQuery(sql);
            return count > 0;
        }
        public void Update(T model)
        {
            PropertyInfo[] pis = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            List<SqlParameter> lst = new List<SqlParameter>();
            string set = string.Empty;
            foreach(PropertyInfo pi in pis)
            {
                if (pi.Name == "Id")
                    continue;
                string name = pi.Name;
                object value = pi.GetValue(model);
                if (!string.IsNullOrEmpty(set))
                    set += " ,";
                set += $"{name}=@{name}";
                SqlParameter par = new SqlParameter($"@{name}", value);
                lst.Add(par);
            }
            string sql = $"Update {typeof(T).Name} Set {set} Where Id={model.Id};";
            SQLServerProject.ExecuteNonQuery(sql, lst);
        }
        public T Find(int id)
        {
            string sql = $"Select * From {typeof(T).Name} Where Id={id}";
            DataTable dt = SQLServerProject.GetDataTable(sql);
            if(dt != null && dt.Rows.Count > 0)
            {
                PropertyInfo[] pis = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                DataRow row = dt.Rows[0];
                T model = new T();
                foreach(PropertyInfo pi in pis)
                {
                    string name = pi.Name;
                    if (dt.Columns.Contains(name))
                    {
                        object value = row[name];
                        if(value != DBNull.Value)
                            pi.SetValue(model, value);
                    }
                }
                return model;
            }
            return default(T);
        }
    }
}
