using System.Data;
using Microsoft.Data.Sqlite;

namespace Chirp.DBFacade;

public static class SqlHelper 
{
    public static SqliteParameter NewParam(string value, string? name = null) {
        return new SqliteParameter {
            ParameterName = name ?? Guid.NewGuid().ToString("N"), // generate name if left out
            Value = value,
            DbType = DbType.String,
            SqliteType = SqliteType.Text
        };
    }

    public static SqliteParameter NewParam(long value, string? name = null) {
        return new SqliteParameter {
            ParameterName = name ?? Guid.NewGuid().ToString("N"),
            Value = value,
            DbType = DbType.Int64,
            SqliteType = SqliteType.Integer
        };
    }

    public static SqliteParameter NewParam(int? value, string? name = null) {
        return new SqliteParameter {
            ParameterName = name ?? Guid.NewGuid().ToString("N"),
            Value = value,
            DbType = DbType.Int32,
            SqliteType = SqliteType.Integer
        };
    }

    public static void Add(this SqliteCommand cmd, string value, string? name = null) {
        cmd.Parameters.Add(NewParam(value, name));
    }

    public static void Add(this SqliteCommand cmd, long value, string? name = null) {
        cmd.Parameters.Add(NewParam(value, name));
    }

    public static void Add(this SqliteCommand cmd, int? value, string? name = null) {
        cmd.Parameters.Add(NewParam(value, name));
    }
}