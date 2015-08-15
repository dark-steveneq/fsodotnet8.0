﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Server.Database.DA
{
    public class MySqlContext : ISqlContext, IDisposable
    {
        private readonly string _connectionString;
        private DbConnection _connection;

        public MySqlContext(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public DbConnection Connection
        {
            get
            {
                if (_connection == null)
                    _connection = new MySqlConnection(_connectionString);

                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                return _connection;
            }
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
