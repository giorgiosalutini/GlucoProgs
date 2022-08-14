﻿using gamon;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace GlucoMan
{
    internal partial class DL_Sqlite : DataLayer
    {
        internal override int? SaveOneAlarm(Alarm Alarm)
        {
            int? IdAlarm = null;
            try
            {
                if (Alarm.IdAlarm == null || Alarm.IdAlarm == 0)
                {
                    Alarm.IdAlarm = GetNextTablePrimaryKey("Alarms", "IdAlarm");
                    Alarm.TimeStart.DateTime = DateTime.Now;
                    // INSERT new record in the table
                    InsertAlarm(Alarm);
                }
                else
                {   // update existing record 
                    UpdateAlarm(Alarm);
                }
                return Alarm.IdAlarm;
            }
            catch (Exception ex)
            {
                Common.LogOfProgram.Error("Sqlite Datalayer | SaveOneAlarm", ex);
                return null;
            }
            return IdAlarm; 
        }
        private void UpdateAlarm(Alarm alarm)
        {
            throw new NotImplementedException();
        }
        private int? InsertAlarm(Alarm alarm)
        {
            try
            {
                using (DbConnection conn = Connect())
                {
                    int? secondsOfInterval = (int?)((TimeSpan)alarm.Interval).TotalSeconds;
                    int? secondsOfDuration = (int?)((TimeSpan)alarm.Duration).TotalSeconds;

                    DbCommand cmd = conn.CreateCommand();
                    string query = "INSERT INTO Alarms" +
                    "(" +
                    "IdAlarm,TimeStart,TimeAlarm,Interval,Duration," +
                    "IsRepeated,IsEnabled";
                    query += ")VALUES(" +
                    SqliteHelper.Int(alarm.IdAlarm) + "," +
                    SqliteHelper.Date(alarm.TimeStart.DateTime) + "," +
                    SqliteHelper.Date(alarm.TimeAlarm.DateTime) + "," +
                    SqliteHelper.Int(secondsOfInterval) + "," +
                    SqliteHelper.Int(secondsOfDuration) + "," +
                    SqliteHelper.Bool(alarm.IsRepeated) + "," +
                    SqliteHelper.Bool(alarm.IsEnabled) + ""; 
                    query += ");";
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                return alarm.IdAlarm;
            }
            catch (Exception ex)
            {
                Common.LogOfProgram.Error("Sqlite_AlarmMeasurement | InsertAlarm", ex);
                return null;
            }
        }
        internal override List<Alarm> ReadAllAlarms()
        {
            List<Alarm> alarms = new List<Alarm>();
            try
            {
                DbDataReader dRead;
                DbCommand cmd;
                using (DbConnection conn = Connect())
                {
                    string query = "SELECT *" +
                        " FROM Alarms"; 
                    query += " ORDER BY IdAlarm DESC";
                    query += ";";
                    cmd = new SqliteCommand(query);
                    cmd.Connection = conn;
                    dRead = cmd.ExecuteReader();
                    while (dRead.Read())
                    {
                        Alarm f = GetAlarmFromRow(dRead);
                        alarms.Add(f);
                    }
                    dRead.Dispose();
                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            {
                Common.LogOfProgram.Error("Sqlite_AlarmManagement | ReadAllAlarms", ex);
            }
            return alarms;
        }
        private Alarm GetAlarmFromRow(DbDataReader Row)
        {
            Alarm m = new Alarm(); 
            try
            {
                m.IdAlarm = Safe.Int(Row["IdAlarm"]);
                m.TimeStart.DateTime = Safe.DateTime(Row["TimeStart"]);
                m.TimeAlarm.DateTime= Safe.DateTime(Row["TimeAlarm"]);
                m.Interval = Safe.TimeSpanFromSeconds(Row["Interval"]);
                m.Duration = Safe.TimeSpanFromMinutes(Row["Duration"]);
                m.IsEnabled = Safe.Bool(Row["IsEnabled"]);                
                m.IsRepeated = Safe.Bool(Row["IsRepeated"]);
            }
            catch (Exception ex)
            {
                Common.LogOfProgram.Error("Sqlite_AlarmManagement | GetAlarmFromRow", ex);
            }
            return m;
        }
    }
}