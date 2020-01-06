using DM_Suite.Services.LoggingServices;
using MetroLog;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;

namespace DM_Suite.Initiative_Features
{
    public class InitiativeDBHelper
    {
        public static SqliteConnection DatabaseFile { get; set; }

        
        /// <summary>
        /// Booleans are converted to old-fashioned 0/1 integers because
        /// Sqlite3 does not have a BOOLEAN type.
        /// </summary>
        /// <param name="input"></param>
        public static int ConvertBoolToInt(bool input)
        {
            if (input)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public static bool CreateDatabaseTable(string databaseName)
        {
            DatabaseFile = new SqliteConnection("Filename=" + databaseName + ".db");

            bool isSuccessful = false;

            using (SqliteConnection db = DatabaseFile)
            {
                db.Open();

                string initiativeTableCommand = "CREATE TABLE `INITIATIVE` ( `ID` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, `NAME` TEXT NOT NULL, `INITIATIVE` INTEGER NOT NULL, `ACTIVE` INTEGER NOT NULL DEFAULT 1 CHECK(ACTIVE IN ( 0 , 1 )), `SESSION` TEXT NOT NULL )";
                SqliteCommand createInitiativeTable = new SqliteCommand(initiativeTableCommand, db);

                try
                {
                    createInitiativeTable.ExecuteReader();
                    isSuccessful = true;
                }
                catch (SqliteException e)
                {
                    LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Sqlite Database Table for INITIATIVE could not be created. " + e.Message, LogLevel.Error);
                }
            }

            return isSuccessful;
        }

        public static bool AddParticipant(Participant participant)
        {
            bool isSuccessful = false;
            string commandText = "Insert into INITIATIVE (NAME, INITIATIVE, ACTIVE, SESSION) VALUES(@name, @initiative, @active, @session)";
            SqliteCommand insertCommand = new SqliteCommand();
            insertCommand.Parameters.AddWithValue("@name", participant.Name);
            insertCommand.Parameters.AddWithValue("@initiative", participant.Initiative);
            insertCommand.Parameters.AddWithValue("@active", ConvertBoolToInt(participant.Active));
            insertCommand.Parameters.AddWithValue("@session", participant.Session);
            insertCommand.CommandText = commandText;

            using (SqliteConnection db = DatabaseFile)
            {
                db.Open();
                insertCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Attempting insert...\t\t" + insertCommand.CommandText, LogLevel.Info);
                    query = insertCommand.ExecuteReader();
                    isSuccessful = true;
                    LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Successfully saved participant " + participant.Name, LogLevel.Info);
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Failed insert...\t\t" + error.Message, LogLevel.Error);
                }
                db.Close();
            }

            return isSuccessful;
        }

        public static bool UpdateParticipant(Participant participant)
        {
            bool isSuccessful = false;
            string commandText = "Update INITIATIVE SET Initiative = @initiative, Active = @active, WHERE Name = @name AND Session = @session";
            SqliteCommand updateCommand = new SqliteCommand();
            updateCommand.Parameters.AddWithValue("@name", participant.Name);
            updateCommand.Parameters.AddWithValue("@initiative", participant.Initiative);
            updateCommand.Parameters.AddWithValue("@active", ConvertBoolToInt(participant.Active));
            updateCommand.Parameters.AddWithValue("@session", participant.Session);
            updateCommand.CommandText = commandText;

            using (SqliteConnection db = DatabaseFile)
            {
                db.Open();
                updateCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Attempting update...\t\t" + updateCommand.CommandText, LogLevel.Info);
                    query = updateCommand.ExecuteReader();
                    isSuccessful = true;
                    LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Successfully updated participant " + participant.Name, LogLevel.Info);
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Failed update...\t\t" + error.Message, LogLevel.Error);
                }
                db.Close();
            }

            return isSuccessful;
        }

        public static bool DeleteParticipant(Participant participant)
        {
            bool isSuccessful = false;
            string commandText = "Delete from INITIATIVE where Name = @name and Session = @session";
            SqliteCommand deleteCommand = new SqliteCommand();
            deleteCommand.Parameters.AddWithValue("@name", participant.Name);
            deleteCommand.Parameters.AddWithValue("@session", participant.Session);
            deleteCommand.CommandText = commandText;

            using (SqliteConnection db = DatabaseFile)
            {
                db.Open();
                deleteCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Attempting delete...\t\t" + deleteCommand.CommandText, LogLevel.Info);
                    query = deleteCommand.ExecuteReader();
                    isSuccessful = true;
                    LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Successfully deleted participant " + participant.Name, LogLevel.Info);
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Failed delete...\t\t" + error.Message, LogLevel.Error);
                }
                db.Close();
            }

            return isSuccessful;
        }

        /// <summary>
        /// Queries a list of participants in a given session
        /// </summary>
        /// <param name="session"></param>
        /// <returns><returns>
        public static List<Participant> GetSession(string session)
        {
            string commandText = "Select * from INITIATIVE where SESSION = @session";
            SqliteCommand selectCommand = new SqliteCommand();
            selectCommand.Parameters.AddWithValue("@session", session);
            string logger = "Input values:\t\tSession == " + session;

            selectCommand.CommandText = commandText;
            List<Participant> results = new List<Participant>();
                
                using (SqliteConnection db = DatabaseFile)
                {
                    db.Open();
                    selectCommand.Connection = db;
                    SqliteDataReader query;
                    try
                    {
                        LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Attempting query...\t\t" + selectCommand.CommandText, LogLevel.Info);
                        LoggingServices.Instance.WriteLine<InitiativeDBHelper>(logger, LogLevel.Info);
                        query = selectCommand.ExecuteReader();
                    }
                    catch (SqliteException error)
                    {
                        LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Error fetching database entries: " + error.Message, LogLevel.Error);
                        return results;
                    }
                    while (query.Read())
                    {
                        Participant participant = new Participant(query);
                        results.Add(participant);
                    }
                    db.Close();
                }
                
            results = results.OrderBy(participant => participant.Initiative).ToList();
            LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Found results: " + results.Count, LogLevel.Info);
            return results;
        }


        /// <summary>
        /// Queries a list of unique sessions across the entire table
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSessionsList()
        {
            string commandText = "Select distinct SESSION from INITIATIVE";
            SqliteCommand selectCommand = new SqliteCommand();
            selectCommand.CommandText = commandText;
            List<string> results = new List<string>();

            using (SqliteConnection db = DatabaseFile)
            {
                db.Open();
                selectCommand.Connection = db;
                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Attempting query...\t\t" + selectCommand.CommandText, LogLevel.Info);
                    query = selectCommand.ExecuteReader();
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Error fetching database entries: " + error.Message, LogLevel.Error);
                    return results;
                }
                while (query.Read())
                {
                    string session = query.GetValue(0).ToString();
                    results.Add(session);
                }
                db.Close();
            }

            LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Found results: " + results.Count, LogLevel.Info);
            return results;
        }

        public static bool DeleteSession(string session)
        {
            bool isSuccessful = false;
            string commandText = "Delete from INITIATIVE where Session = @session";
            SqliteCommand deleteCommand = new SqliteCommand();
            deleteCommand.Parameters.AddWithValue("@session", session);
            deleteCommand.CommandText = commandText;

            using (SqliteConnection db = DatabaseFile)
            {
                db.Open();
                deleteCommand.Connection = db;

                SqliteDataReader query;
                try
                {
                    LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Attempting delete...\t\t" + deleteCommand.CommandText, LogLevel.Info);
                    query = deleteCommand.ExecuteReader();
                    isSuccessful = true;
                    LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Successfully deleted session " + session, LogLevel.Info);
                }
                catch (SqliteException error)
                {
                    LoggingServices.Instance.WriteLine<InitiativeDBHelper>("Failed delete...\t\t" + error.Message, LogLevel.Error);
                }
                db.Close();
            }

            return isSuccessful;
        }
    }
}
