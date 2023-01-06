using Dapper;
using Inc.TeamAssistant.Reviewer.All.Holidays.Model;
using Npgsql;

namespace Inc.TeamAssistant.Reviewer.All.Holidays.Internal;

internal sealed class HolidayReader : IHolidayReader
{
    private readonly string _connectionString;

    public HolidayReader(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(connectionString));

        _connectionString = connectionString;
    }

    public async Task<Dictionary<DateOnly, HolidayType>> GetAll(CancellationToken cancellationToken)
    {
        var command = new CommandDefinition(@"
SELECT
    date AS date,
    type AS type
FROM generic.holidays;",
            flags: CommandFlags.None,
            cancellationToken: cancellationToken);

        await using var connection = new NpgsqlConnection(_connectionString);

        var results = await connection.QueryAsync<Holiday>(command);
        return results.ToDictionary(r => r.Date, r => r.Type);
    }
}