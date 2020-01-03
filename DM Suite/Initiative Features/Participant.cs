using DM_Suite.Services.LoggingServices;
using MetroLog;
using Microsoft.Data.Sqlite;
using System;

namespace DM_Suite.Initiative_Features
{
    public class Participant
    {
        public string Name { get; set; }
        public int Initiative { get; set; }
        public bool Alive { get; set; }
        public string Session { get; set; }

        public Participant() { }

        public Participant(SqliteDataReader query)
        {
            if (query.FieldCount == 5)
            {
                try
                {
                    Name = query.GetValue(1).ToString();
                    Initiative = Convert.ToInt32(query.GetValue(2));
                    if (Convert.ToInt32(query.GetValue(3)) > 0)
                    {
                        Alive = true;
                    }
                    else
                    {
                        Alive = false;
                    }
                    Session = query.GetValue(4).ToString();
                }
                catch (Exception error)
                {
                    LoggingServices.Instance.WriteLine<Participant>("Error mapping Participant from query: " + error.Message, LogLevel.Info);
                }
            }
            else
            {
                LoggingServices.Instance.WriteLine<Participant>("Invalid query result for mapping Participant. Expected FieldCount of 5, found FieldCount of " + query.FieldCount, LogLevel.Info);
            }
        }

        public void Kill()
        {
            Alive = false;
        }

    }
}
