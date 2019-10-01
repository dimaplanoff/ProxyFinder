using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyService
{
    class Sql
    {

        private SqlConnection con = null;
        public bool connectionIsOpen = false;
        object sync = new object();

        public Sql()
        {
            try
            {
                con = new SqlConnection(Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\CSHARP").GetValue("ConnectionStringCursor").ToString());
                con.Open();
                connectionIsOpen = true;
            }
            catch { }
        }

        public void Dispose()
        {
            con.Close();
            con.Dispose();
        }

        public int Exec(string queue)
        {
            try
            {
                lock (sync)
                {
                    using (SqlCommand cmd = new SqlCommand(queue, con))
                    {
                        cmd.CommandTimeout = 30;
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { return 0; }
        }

        public DataTable GetTable(string queue)
        {
            DataTable DT = new DataTable();
            using (SqlDataAdapter adapt = new SqlDataAdapter(queue, con))
            {
                adapt.SelectCommand.CommandTimeout = 90;
                adapt.Fill(DT);
            }
            return DT;
        }

    }
}
