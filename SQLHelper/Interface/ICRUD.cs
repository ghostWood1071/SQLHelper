using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace DataLib.Interface
{
    public interface ICRUD<T>
    {
        List<T> GetList(string tableName);
        List<T> GetList(string tableName, string keyName, int key);
        T Get(int id, string tableName);
        bool Add(T model);
        bool Update(T model);
        bool Delete(T model);
        int Synchronize();
    }
}
