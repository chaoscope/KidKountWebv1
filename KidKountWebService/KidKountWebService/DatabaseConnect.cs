using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;

namespace KidKountWebService
{
    public class DatabaseConnect
    {
        
        //SINGLETON START 
        private static DatabaseConnect instance;

        public static DatabaseConnect Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DatabaseConnect();
                }
                return instance;
            }
        }
        //SINGLETON END

        public MySqlConnection connection;

        private DatabaseConnect()
        {
            Initialize();
        }

        private void Initialize()
        {
            var connectionString = "SERVER=" + GetServer() + ";" + "DATABASE="
                + Constants.DATABASE_NAME + ";" + "UID=" + GetUid() + ";" + "PASSWORD=" +
                GetPassword() + ";";

            connection = new MySqlConnection(connectionString);
        }

        private string GetServer()
        {
            return DevelopmentInfo.server;
        }

        private string GetUid()
        {
            return DevelopmentInfo.user;
        }

        private string GetPassword()
        {
            return DevelopmentInfo.password;
        }


        /*
         *  PUBLIC FUNCTIONS 
         * 
         */

        public bool OpenConnection()
        {
            var connected = false;

            try
            {
                if (connection.State != System.Data.ConnectionState.Open) 
                {
                    connection.Open();
                }
                connected = true;
            }
            catch (MySqlException exception)
            {
                //TODO
            }

            return connected;
        }

        public bool CloseConnection()
        {
            bool connectionBroke = false;

            try
            {
                connection.Close();
                connectionBroke = true;
            }
            catch (MySqlException exception)
            {
                //TODO
            }

            return connectionBroke;
        }

        public bool TestConnection()
        {
            var test = OpenConnection();
            CloseConnection();

            return test;
        }

        public void NonQueryQuickExecute(string query)
        {
            if (OpenConnection() == true)
            {
                var cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                CloseConnection();
            }
        }
        
    }
}