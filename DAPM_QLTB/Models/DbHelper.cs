using System.Configuration;
using System.Data.SqlClient;

namespace QLTB.Models
{
    public static class DbHelper
    {
        public static SqlConnection GetConnection()
        {
            string connStr = ConfigurationManager.ConnectionStrings["QuanLyThietBi"].ConnectionString;
            return new SqlConnection(connStr);
        }
    }
}
