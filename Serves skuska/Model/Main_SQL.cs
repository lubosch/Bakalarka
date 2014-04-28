using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Updates_namespace;

namespace Wordik.SQL
{
    class Main_SQL
    {

        static SqlConnection connection;
        static SqlCommand command;



        public Main_SQL()
        {
            command = new SqlCommand();
            command.CommandType = CommandType.Text;
            connection = null;
        }

        public static void OpenConnestion()
        {
            string connectionString = Tabber.Properties.Settings.Default.RotekConnectionString;
            connection = new SqlConnection(connectionString);
            connection.Open();


            command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.Connection = connection;
        }

        public static void AddCommand(string cmd)
        {
            if (connection == null || connection.State != ConnectionState.Open)
            {
                OpenConnestion();
            }
            if (cmd[0] == '~')
            {
                command.CommandText += cmd.Replace('~', ' ');
                Console.Out.Write(cmd.Replace('~', ' '));
            }
            else
            {
                command.CommandText += ";\n" + cmd;
                Console.Out.Write(";\n" + cmd);
            }
        }

        public static void AddParameter(string parameter, object premenna)
        {
            if (premenna == null)
            {
                command.Parameters.Add(new SqlParameter(parameter, DBNull.Value));
            }
            else
            {
                command.Parameters.Add(new SqlParameter(parameter, premenna));
            }
        }

        public static int Odpal()
        {
            int result = command.ExecuteNonQuery();
            connection.Close();
            return result;

        }
        public static void CloseConnection()
        {
            command.CommandText = "";
            connection.Close();
        }

        public static int Odpalovac(string cmd)
        {
            int ret;
            OpenConnestion();
            command = new SqlCommand(cmd, connection);
            ret = command.ExecuteNonQuery();
            CloseConnection();
            return ret;
        }

        public static DataSet List(string cmd)
        {
            OpenConnestion();
            SqlDataAdapter dataadapter = new SqlDataAdapter(cmd, connection);
            DataSet ds = new DataSet();
            dataadapter.Fill(ds);
            CloseConnection();
            return ds;
        }

        public static DataSet Commit_List()
        {
            command.CommandText = "BEGIN TRANSACTION;\n" + command.CommandText + "\nCOMMIT TRANSACTION;";
            SqlDataAdapter dataadapter = new SqlDataAdapter(command);
            DataSet ds = new DataSet();
            //Error.Show(command.CommandText);
            dataadapter.Fill(ds);
            CloseConnection();
            return ds;

        }

        public static object Commit_Transaction()
        {
            command.CommandText = "BEGIN TRANSACTION;\n" + command.CommandText + "\nCOMMIT TRANSACTION;";
            object s = command.ExecuteScalar();
            if (s != null && s.ToString().IndexOf("ERROR:") == 0)
            {
                command.CommandText = "";
                command.Parameters.Clear();
                CloseConnection();
                throw new Exception(s.ToString());
            }

            command.CommandText = "";
            command.Parameters.Clear();
            CloseConnection();
            return s;
        }




    }
}
