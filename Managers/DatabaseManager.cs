using System;
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
        static readonly string conArgPath = System.IO.Directory.GetCurrentDirectory() + "\\psql.conf";
        static readonly string CON_ARGS = System.IO.File.ReadAllText(conArgPath); 
        
        /// <summary>
        /// Creates the bounties table in the database specified by <see cref="DB"/>.
        /// </summary>
        public static void CreateTables()
        {
            using var con = new NpgsqlConnection(CON_ARGS);
            using var cmd = new NpgsqlCommand() { Connection = con };

            con.Open();

            // Create Bounties table
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS bounties(id SERIAL PRIMARY KEY, target VARCHAR(255), createdAt TIMESTAMP, value INT, status INT, assignedTo BIGINT[], author BIGINT, reviewer BIGINT)";
            cmd.ExecuteNonQuery();

            //// Create Proposed Bounties table
            //cmd.CommandText = "CREATE TABLE IF NOT EXISTS proposedBounties(pk SERIAL PRIMARY KEY, id INT, target VARCHAR(255), createdAt TIMESTAMP, value INT, status INT, assignedTo BIGINT[], author BIGINT, reviewer BIGINT)";
            //cmd.ExecuteNonQuery();

            //// Create Archived Bounties table
            //cmd.CommandText = "CREATE TABLE IF NOT EXISTS archivedBounties(pk SERIAL PRIMARY KEY, id INT, target VARCHAR(255), createdAt TIMESTAMP, value INT, status INT, assignedTo BIGINT[], author BIGINT, reviewer BIGINT)";
            //cmd.ExecuteNonQuery();
            con.Close();
        }
        /// <summary>
        /// <b>*Warning*</b> Irrevocably deletes bounties table in database!
        /// </summary>
        public static void DropTables()
        {
            using var con = new NpgsqlConnection(CON_ARGS);
            using var cmd = new NpgsqlCommand() { Connection = con };

            con.Open();
            cmd.CommandText = "DROP TABLE bounties";
            cmd.ExecuteNonQuery();
            con.Close();
        }

        public static async void AddBountyToDBAsync(Bounty bounty)
        {
            using var con = new NpgsqlConnection(CON_ARGS);
            using var cmd = new NpgsqlCommand() { Connection = con };

            con.Open();

            cmd.CommandText = "INSERT INTO bounties(target, createdAt, value, status, assignedTo, author, reviewer)" +
                "VALUES(@target, @createdAt, @value, @status, @assignedTo, @author, @reviewer)";
            GetBountyParams(bounty).ForEach(x => cmd.Parameters.Add(x));
            await cmd.ExecuteNonQueryAsync();
            await con.CloseAsync();
        }
        public static Bounty AddBountyToDB(Bounty bounty)
        {
            using var con = new NpgsqlConnection(CON_ARGS);
            using var cmd = new NpgsqlCommand() { Connection = con };

            con.Open();

            cmd.CommandText = "INSERT INTO bounties(target, createdAt, value, status, assignedTo, author, reviewer)" +
                "VALUES(@target, @createdAt, @value, @status, @assignedTo, @author, @reviewer)" +
                "RETURNING id";
            GetBountyParams(bounty).ForEach(x => cmd.Parameters.Add(x));
            bounty.ID = (int)cmd.ExecuteScalar();
            con.Close();
            return bounty;
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
            con.Close();
            return bounties.ToArray();
        }

        public static Bounty[] GetBounties()
        {
            using NpgsqlConnection con = new(CON_ARGS);
            con.Open();
            using NpgsqlCommand cmd = new() { Connection = con };

            cmd.CommandText = "SELECT * FROM bounties ORDER BY id";
            using var reader = cmd.ExecuteReader();
            List<Bounty> bounties = new();
            while (reader.Read())
            {
                bounties.Add(new(
                    (int)reader[0],
                    (string)reader[1],
                    (DateTime)reader[2],
                    (int)reader[3],
                    (Bounty.StatusLevel)reader[4],
                    (ulong)(long)reader[6],
                    (ulong)(long)reader[7],
                    ((long[])reader[5]).Select(x => (ulong)x).ToArray()
                    ));
            }
            con.Close();
            return bounties.ToArray();
        }

        public static Bounty GetBountyByID(int id)
        {
            using NpgsqlConnection con = new(CON_ARGS);
            con.Open();
            using NpgsqlCommand cmd = new() { Connection = con };
            cmd.Parameters.Add(new("id", id));
            cmd.CommandText = "SELECT * FROM bounties WHERE id=@id LIMIT 1";
            using var reader = cmd.ExecuteReader();
            Bounty bounty = null;
            if (reader.Read())
            {
                bounty = new(
                    (int)reader[0],
                    (string)reader[1],
                    (DateTime)reader[2],
                    (int)reader[3],
                    (Bounty.StatusLevel)reader[4],
                    (ulong)(long)reader[6],
                    (ulong)(long)reader[7],
                    ((long[])reader[5]).Select(x => (ulong)x).ToArray()
                    );
            }
            con.Close();
            return bounty;
        }
        public static Bounty[] GetBountiesByStatus(Bounty.StatusLevel status)
        {
            using NpgsqlConnection con = new(CON_ARGS);
            con.Open();
            using NpgsqlCommand cmd = new() { Connection = con };
            cmd.Parameters.Add(new("status", (int)status));
            cmd.CommandText = "SELECT * FROM bounties WHERE status=@status";
            using var reader = cmd.ExecuteReader();
            List<Bounty> bounties = new();
            while (reader.Read())
            {
                bounties.Add(new(
                    (int)reader[0],
                    (string)reader[1],
                    (DateTime)reader[2],
                    (int)reader[3],
                    (Bounty.StatusLevel)reader[4],
                    (ulong)(long)reader[6],
                    (ulong)(long)reader[7],
                    ((long[])reader[5]).Select(x => (ulong)x).ToArray()
                    ));
            }
            con.Close();
            return bounties.ToArray();
        }

        public static Bounty[] GetBountiesAssignedToPlayer(ulong player)
        {
            using NpgsqlConnection con = new(CON_ARGS);
            con.Open();
            using NpgsqlCommand cmd = new() { Connection = con };
            cmd.Parameters.Add(new("player", (BigInteger)player));
            cmd.CommandText = "SELECT * FROM bounties WHERE  @player=ANY(assignedTo)";
            using var reader = cmd.ExecuteReader();
            List<Bounty> bounties = new();
            while (reader.Read())
            {
                bounties.Add(new(
                    (int)reader[0],
                    (string)reader[1],
                    (DateTime)reader[2],
                    (int)reader[3],
                    (Bounty.StatusLevel)reader[4],
                    (ulong)(long)reader[6],
                    (ulong)(long)reader[7],
                    ((long[])reader[5]).Select(x => (ulong)x).ToArray()
                    ));
            }
            con.Close();
            return bounties.ToArray();
        }

        public static Bounty UpdateBounty(int id, Bounty bounty)
        {
            using NpgsqlConnection con = new(CON_ARGS);
            con.Open();
            using NpgsqlCommand cmd = new() { Connection = con };

            GetBountyParams(bounty).ForEach(x => cmd.Parameters.Add(x));

            cmd.CommandText = "UPDATE bounties SET target = @target, " +
                "createdAt = @createdAt, " +
                "value = @value, " +
                "status = @status, " +
                "assignedTo = @assignedTo, " +
                "author = @author, " +
                "reviewer = @reviewer " +
                "WHERE id = @id";

            cmd.ExecuteNonQuery();
            con.Close();
            return bounty;
        }
    }
}
