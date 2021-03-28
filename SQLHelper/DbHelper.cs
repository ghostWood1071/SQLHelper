using System;
using System.Data;
using System.Data.SqlClient;
namespace DataLib
{
    public class DbHelper
    {
        private SqlConnection connection;
        private SqlDataAdapter dataAdapter;
        private SqlCommand sqlCommand;

        public SqlCommand Command { get => sqlCommand;}

        public DbHelper(string conn)
        {
            connection = new SqlConnection(conn);
            dataAdapter = new SqlDataAdapter(null, connection);
            sqlCommand = new SqlCommand(null, connection);
            
        }


        public DataTable GetData(string command, string tableName = null)
        {
            DataTable table;
            if (tableName != null)
                table = new DataTable(tableName);
            else
                table = new DataTable();
            sqlCommand.CommandText = command;
            dataAdapter.SelectCommand = sqlCommand;
            dataAdapter.Fill(table);
            return table;
        }

        public DataTable GetData(string storedName, params object[] parameters)
        {
            connection.Open();
            DataTable table = new DataTable();
            sqlCommand.CommandText = storedName;
            sqlCommand.CommandType = CommandType.StoredProcedure;
            SqlCommandBuilder.DeriveParameters(sqlCommand);
            for(int i =1; i<sqlCommand.Parameters.Count; i++)
            {
                sqlCommand.Parameters[i].Value = parameters[i - 1];
            }
            dataAdapter.SelectCommand = sqlCommand;
            dataAdapter.Fill(table);
            connection.Close();
            return table;
        }

        public int Excute(string storedName, params object[] parameters) 
        {
            connection.Open();
            try
            {
                sqlCommand.Connection = connection;
                sqlCommand.CommandText = storedName;
                sqlCommand.CommandType = CommandType.StoredProcedure;
                SqlCommandBuilder.DeriveParameters(sqlCommand);
                for (int i = 1; i <sqlCommand.Parameters.Count; i++)
                {
                    sqlCommand.Parameters[i].Value = parameters[i - 1];
                }
                int result = sqlCommand.ExecuteNonQuery();
                connection.Close();
            return result;
            }
            catch
            {
               return -1;
            }
        }

        public int Synchronize(DataTable table)
        {

            SqlCommandBuilder builder = new SqlCommandBuilder(this.dataAdapter);
            dataAdapter.DeleteCommand = builder.GetDeleteCommand();
            dataAdapter.UpdateCommand = builder.GetUpdateCommand();
            int result =  dataAdapter.Update(table);
            return result;
        }
        
        public static int Synchronize(DataSet dataSet, string conn)
        {
            SqlDataAdapter adapter = new SqlDataAdapter(null, conn);
            SqlCommandBuilder builder = new SqlCommandBuilder();
            builder.DataAdapter = adapter;
            return adapter.Update(dataSet);
        }
    }
}
