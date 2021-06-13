using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class DbHelper : Singleton<DbHelper>
{
    public SqliteConnection OpenConnect(string path)
    {
        try
        {
            //新建数据库连接
            SqliteConnection connection = new SqliteConnection(@"Data Source = " + path);
            //打开数据库
            connection.Open();

            return connection;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public SqliteDataReader ExecuteQuery(SqliteConnection dbConnection, string sqlQuery)//执行查询
    {
        SqliteCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = sqlQuery;
        SqliteDataReader dbReader = dbCommand.ExecuteReader();
        return dbReader;
    }
}
