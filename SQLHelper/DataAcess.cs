using System;
using System.Collections.Generic;
using System.Reflection;
using System.Data;
using DataLib;
using DataLib.Interface;

namespace DataLib
{
    public class DataAcess<T> : ICRUD<T>
    {
        protected DbHelper dbHelper;

        private static DataTable table;

        /// <summary>
        /// Create instance of DatataAcess with string connection
        /// </summary>
        /// <param name="conn">The System.string that contains connection string</param>
        public DataAcess(string conn)
        {
            dbHelper = new DbHelper(conn);
            table = new DataTable();
        }

        /// <summary>
        /// Add a model to table 
        /// </summary>
        /// <param name="model">The System.Generic.T that contains info to add to the table</param>
        /// <returns></returns>
        public bool Add(T model)
        {
            if (table.Rows.Count >= 0)
            {
                Type type = typeof(T);
                PropertyInfo[] properties = type.GetProperties();
                DataRow row = table.NewRow();
                for (int i = 0; i < properties.Length; i++)
                {
                    object obj = properties[i].GetValue(model);
                    row[i] = obj;
                }
                table.Rows.Add(row);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Delete a model from table
        /// </summary>
        /// <param name="model">model</param>
        /// <returns></returns>
        public bool Delete(T model)
        {
            if (model == null)
                return false;

            bool result = false;
            int id = GetId(model);
            if (table.Rows != null)
            {
                int index = GetIndex(id);
                if (index >= 0)
                {
                    DataView dataView = new DataView(table);
                    dataView.AllowDelete = true;
                    dataView.Delete(index);
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Delete rows from database with specified filter;
        /// </summary>
        /// <param name="filter">string</param>
        /// <returns>bool</returns>
        public bool Delete(string filter)
        {
            DataView dataView = new DataView(table);
            dataView.RowFilter = filter;
            dataView.AllowDelete = true;
            while (dataView.Count > 0)
            {
                dataView.Delete(0);
            }
            return true;
        }

        public void LoadData(string tableName)
        {
            table = dbHelper.GetData($"SELECT * FROM {tableName}", tableName);
        }

        /// <summary>
        /// Get an instance of object from the table
        /// </summary>
        /// <param name="id"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public T Get(int id, string tableName)
        {
            if(table.Rows.Count == 0)
                table = dbHelper.GetData($"SELECT * FROM {tableName}", tableName);

            DataView view = new DataView(table);
            DataTable thisTable = view.ToTable();

            Type type = typeof(T);
            object obj = Activator.CreateInstance(type);
            int index = GetIndex(id);
            if (index >= 0)
            {
                DataRow row = thisTable.Rows[index];
                PropertyInfo[] properties = type.GetProperties();

                for (int i = 0; i < row.ItemArray.Length; i++)
                    if (row.ItemArray[i] is DBNull)
                        properties[i].SetValue(obj, null);
                    else
                        properties[i].SetValue(obj, row.ItemArray[i]);
            }
            return (T)obj;

        }

        /// <summary>
        /// Get a List of model with specify type and table name
        /// </summary>
        /// <param name="tableName">the name of table in sqlserver</param>
        /// <returns>a list of model</returns>
        public List<T> GetList(string tableName)
        {
            if(table.Rows.Count == 0)
                table =  dbHelper.GetData($"SELECT * FROM {tableName}", tableName);

            Type myType = typeof(T);
            return Convert(myType, table);
        }

        /// <summary>
        /// Get data from database with specified table's name, condition is foreign key and value of key
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="keyName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> GetList(string tableName, string keyName, int key)
        {
            if (table.Rows.Count == 0)
                table = dbHelper.GetData($"SELECT * FROM {tableName} WHERE {keyName} = {key}", tableName);

            Type myType = typeof(T);
            return Convert(myType, table);
        }

        /// <summary>
        /// Get data from database with filter, sort option by specified field 
        /// </summary>
        /// <param name="filter">string</param>
        /// <param name="sortOption">string</param>
        /// <returns></returns>
        public List<T> GetList(string filter, string sortOption = null)
        {
            DataView dataView = new DataView(table);
            dataView.RowFilter = filter;
            dataView.Sort = sortOption;
            Type type = typeof(T);
            return Convert(type, dataView.ToTable());
        }

        /// <summary>
        /// Modify a model from datatable
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Update(T model)
        {
            if (model == null)
                return false;

            DataView view = new DataView(table);
            view.AllowEdit = true;

            int index = GetIndex(GetId(model));
            if (index >= 0)
            {
                Type type = typeof(T);
                PropertyInfo[] properties = type.GetProperties();
                for(int i = 0; i<properties.Length; i++)
                    if(properties[i].GetValue(model) !=null)
                        view[index][i] = properties[i].GetValue(model);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Update rows to database by filter with specified field and parameters
        /// </summary>
        /// <param name="filter">string</param>
        /// <param name="parameters">params object[]</param>
        /// <returns></returns>
        public bool Update(string filter, params object[] parameters)
        {
            DataView dataView = new DataView(table);
            dataView.AllowEdit = true;
            dataView.RowFilter = filter;
            for (int i = 0; i < dataView.Count; i++)
            {
                for (int j = 0; j < parameters.Length; j++)
                {
                    if(parameters[j] != null)
                        dataView[i][j] = parameters[j];
                }
            }
            return true;
        }

        protected int GetIndex(int id)
        {
            int l = 0, r = table.Rows.Count - 1;
            while (r>=l)
            {
                int mid = l + (r - l) / 2;
                int midId = GetId(mid);
                if (midId == id)
                    return mid;
                if (midId > id)
                    r = mid - 1;
                if (midId < id)
                    l = mid + 1;
            }
            return -1;
        }

        protected int GetId(T model)
        {
            Type myType = typeof(T);
            PropertyInfo propertyInfo = myType.GetProperties()[0];
            return (int)propertyInfo.GetValue(model);
        }

        protected int GetId(int index)
        {
            return (int) table.Rows[index].ItemArray[0];
        }

        protected List<T> Convert(Type myType, DataTable table)
        {
            PropertyInfo[] properties = myType.GetProperties();
            List<T> result = new List<T>();


            DataView view = new DataView(table);

            //get data from view 
            DataTable thisTable = view.ToTable();

            foreach (DataRow row in thisTable.Rows)
            {
                object newItem = Activator.CreateInstance(myType);
                for (int i = 0; i < row.ItemArray.Length; i++)
                    if (row.ItemArray[i] is DBNull)
                        properties[i].SetValue(newItem, null);
                    else
                        properties[i].SetValue(newItem, row.ItemArray[i]);
                result.Add((T)newItem);
            }
            return result;
        }

        /// <summary>
        /// synchronize data from datatable to database
        /// </summary>
        /// <returns></returns>
        public int Synchronize()
        {
          int result =  dbHelper.Synchronize(table);
            return result;
        }

       
    }
}
