using Dapper;
using System.Data.SqlClient;
using System.Configuration;
using System;
using AuthenticationApi.Model;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Collections;

namespace AuthenticationApi.Helper
{
    public class DbHelper
    {
        private string connStr;
        public DbHelper(IConfiguration configuration)
        {
            connStr = configuration.GetConnectionString("DbConnectionString");
        }

        #region CheckUser
        public bool CheckUser(string Username, string Password)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                String selectQuery = @"SELECT COUNT(*) FROM [dbo].[users] WHERE Username = @Username and Password = @Password";
                return conn.Query<bool>(selectQuery, new { Username = Username, Password = Password }).FirstOrDefault();
            }
        }
        #endregion CheckUser
    }
}
