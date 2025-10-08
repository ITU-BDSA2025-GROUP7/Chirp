using System.Text.Json.Serialization.Metadata;
using Microsoft.Data.Sqlite;

namespace Chirp.DBFacade;

public static class Queries {
    /** Return [username, text, pub_date] */
    public static string ReadQuery(SqliteParameter? p = null) {
        const string query = "SELECT username, text, pub_date " +
                             "FROM message JOIN user ON author_id=user_id " +
                             "ORDER BY pub_date desc, username ";
        if (p is null) return query;
        return query + $"LIMIT @{p.ParameterName}";
    }

    public static string ReadPageQuery(SqliteParameter p)
    {
        const string query = "SELECT username, text, pub_date " +
                             "FROM message JOIN user ON author_id=user_id " +
                             "ORDER BY pub_date desc, username ";
        
        return query + $"LIMIT @{p.ParameterName},32";
        ;
    }
    
    public static string ReadPageQueryByName(SqliteParameter name ,SqliteParameter? startingEntryPer = null)
    {
        string query = "";
        query += "SELECT username, text, pub_date ";
        query += "FROM message JOIN user ON author_id=user_id ";
        query += $"WHERE user.username = @{name.ParameterName} ";
        query += "ORDER BY pub_date desc, user.username ";
        if (startingEntryPer is not null)
        {
            query += $"LIMIT @{startingEntryPer.ParameterName},32 ";
        }
        else
        {
            Console.WriteLine("StartingEntryPer is null");
            query += $"LIMIT 32 ";
        }
        return query;
    }

    public static string PrepareInsertionCommand(string[] p) {
        return "INSERT OR ROLLBACK INTO message (author_id, text, pub_date) " +
               $"VALUES ((SELECT user_id FROM user WHERE username=@{p[0]} LIMIT 1), @{p[1]}, @{p[2]})";
    }

    public static string CountMatchingUsernames(SqliteParameter user) =>
        $"SELECT COUNT(user_id) FROM user WHERE username=@{user.ParameterName}";

    public static string InsertUser(SqliteParameterCollection p) {
        return "INSERT OR ROLLBACK INTO user (username, email, pw_hash) VALUES (" +
               $"@{p[0].ParameterName},@{p[1].ParameterName},@{p[2].ParameterName})";
    }

    public const string CountMessages = "SELECT COUNT(*) FROM message";
    public const string CountUsers = "SELECT COUNT(*) FROM user";
    public const string SelectAllUsers = "SELECT * FROM user";
}