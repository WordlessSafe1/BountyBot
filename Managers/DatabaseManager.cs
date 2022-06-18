﻿using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BountyBot.Entities;
using Npgsql;

namespace BountyBot.Managers
{
    internal class DatabaseManager
    {
        const string HOST = "localhost";
        const string LOGIN = "postgres";
        const string PWD = "INSERT_HERE"; // Read from/point to file
        const string DB = "testdb";
        const string CON_ARGS = $"Host={HOST};Username={LOGIN};Password={PWD};Database={DB}";

        public static void CreateTables()
        {
            using var con = new NpgsqlConnection(CON_ARGS);
            using var cmd = new NpgsqlCommand() { Connection = con };

            con.Open();

            // Create Bounties table
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS bounties(pk SERIAL PRIMARY KEY, id INT, target VARCHAR(255), createdAt TIMESTAMP, value INT, status INT, assignedTo BIGINT[], author BIGINT, reviewer BIGINT)";
            cmd.ExecuteNonQuery();

            // Create Proposed Bounties table
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS proposedBounties(pk SERIAL PRIMARY KEY, id INT, target VARCHAR(255), createdAt TIMESTAMP, value INT, status INT, assignedTo BIGINT[], author BIGINT, reviewer BIGINT)";
            cmd.ExecuteNonQuery();

            // Create Archived Bounties table
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS archivedBounties(pk SERIAL PRIMARY KEY, id INT, target VARCHAR(255), createdAt TIMESTAMP, value INT, status INT, assignedTo BIGINT[], author BIGINT, reviewer BIGINT)";
            cmd.ExecuteNonQuery();
            con.Close();
        }

        public static async void AddBountyToDB(Bounty bounty)
        {
            using var con = new NpgsqlConnection(CON_ARGS);
            using var cmd = new NpgsqlCommand() { Connection = con };

            con.Open();

            cmd.CommandText = "INSERT INTO bounties(id, target, createdAt, value, status, assignedTo, author, reviewer)" +
                "VALUES(@id, @target, @createdAt, @value, @status, @assignedTo, @author, @reviewer)";
            GetBountyParams(bounty).ForEach(x => cmd.Parameters.Add(x));
            await cmd.ExecuteNonQueryAsync();
            await con.CloseAsync();
        }

        private static List<NpgsqlParameter> GetBountyParams(Bounty bounty)
        {
            List<NpgsqlParameter> paramList = new();
            var assignedTo = bounty.AssignedTo.Select(x => (BigInteger)x).ToArray();
            paramList.Add(new("id", bounty.ID));
            paramList.Add(new("target", bounty.Target));
            paramList.Add(new("createdAt", bounty.CreatedAt));
            paramList.Add(new("value", bounty.Value));
            paramList.Add(new("status", ((int)bounty.Status)));
            paramList.Add(new("assignedTo", assignedTo));
            paramList.Add(new("author", (BigInteger)bounty.Author));
            paramList.Add(new("reviewer", (BigInteger)bounty.Reviewer));
            return paramList;
        }

        public static async Task<Bounty[]> GetBountiesAsync()
        {
            using NpgsqlConnection con = new(CON_ARGS);
            con.Open();
            using NpgsqlCommand cmd = new() { Connection = con };

            cmd.CommandText = "SELECT * FROM bounties";
            await using var reader = await cmd.ExecuteReaderAsync();
            List<Bounty> bounties = new();
            while (await reader.ReadAsync())
            {
                bounties.Add(new(
                    (int)                           reader[1],
                    (string)                      reader[2],
                    (DateTime)                reader[3],
                    (int)                           reader[4],
                    (Bounty.StatusLevel) reader[5],
                    (ulong)(long)             reader[7],
                    (ulong)(long)             reader[8],
                    ((long[])                     reader[6]).Select(x => (ulong)x).ToArray()
                    ));
            }
            return bounties.ToArray();
        }

        public static Bounty[] GetBounties()
        {
            using NpgsqlConnection con = new(CON_ARGS);
            con.Open();
            using NpgsqlCommand cmd = new() { Connection = con };

            cmd.CommandText = "SELECT * FROM bounties";
            using var reader = cmd.ExecuteReader();
            List<Bounty> bounties = new();
            while (reader.Read())
            {
                bounties.Add(new(
                    (int)reader[1],
                    (string)reader[2],
                    (DateTime)reader[3],
                    (int)reader[4],
                    (Bounty.StatusLevel)reader[5],
                    (ulong)(long)reader[7],
                    (ulong)(long)reader[8],
                    ((long[])reader[6]).Select(x => (ulong)x).ToArray()
                    ));
            }
            return bounties.ToArray();
        }
    }
}
