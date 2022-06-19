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
        /// <summary>
        /// Inserts a bounty into the database asynchonously.
        /// </summary>
        /// <param name="bounty">The <see cref="Bounty"/> to insert into the database.</param>
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
        /// <summary>
        /// Inserts a bounty into the database.
        /// </summary>
        /// <param name="bounty">The <see cref="Bounty"/> to insert into the database.</param>
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
        /// <summary>
        /// Creates a set of <see cref="NpgsqlParameter"/> objects using a bounty.
        /// </summary>
        /// <param name="bounty">The <see cref="Bounty"/> used to generate parameters.</param>
        /// <returns>An <see cref="Array"/> of <see cref="NpgsqlParameter"/> objects containing the values of <paramref name="bounty"/>.</returns>
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
        /// <summary>
        /// Retreives all bounties from the database asynchonously.
        /// </summary>
        /// <returns>An <see cref="Array"/> of <see cref="Bounty"/> objects, ordered by <seealso cref="Bounty.ID"/>.</returns>
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
        /// <summary>
        /// Retreives all bounties from the database.
        /// </summary>
        /// <returns>An <see cref="Array"/> of <see cref="Bounty"/> objects, ordered by <seealso cref="Bounty.ID"/>.</returns>
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
        /// <summary>
        /// Retreives a bounty from the database using <see cref="Bounty.ID"/>.
        /// </summary>
        /// <param name="id">The <see cref="Bounty.ID"/> of the bounty to retrieve.</param>
        /// <returns>A <see cref="Bounty"/> object.</returns>
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
        /// <summary>
        /// Retreives all bounties from the database with the <see cref="Bounty.StatusLevel"/> of <paramref name="status"/>.
        /// </summary>
        /// <param name="status">The <see cref="Bounty.StatusLevel"/> to filter results by.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Retreives all bounties from the database assigned to <paramref name="player"/>.
        /// </summary>
        /// <param name="player">The UID of the player to search for.</param>
        /// <returns>An <see cref="Array"/> of <see cref="Bounty"/> objects, ordered by <seealso cref="Bounty.ID"/>.</returns>
        public static Bounty[] GetBountiesAssignedToPlayer(ulong player)
        {
            using NpgsqlConnection con = new(CON_ARGS);
            con.Open();
            using NpgsqlCommand cmd = new() { Connection = con };
            cmd.Parameters.Add(new("player", (BigInteger)player));
            cmd.CommandText = "SELECT * FROM bounties WHERE  @player=ANY(assignedTo) ORDER BY id";
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
        /// <summary>
        /// Updates a bounty in the database.
        /// </summary>
        /// <param name="id">The primary key of the entity to update.</param>
        /// <param name="bounty">The updated <see cref="Bounty"/> entity.</param>
        /// <returns>The updated <see cref="Bounty"/> entity.</returns>
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
