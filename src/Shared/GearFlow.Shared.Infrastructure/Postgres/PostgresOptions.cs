using System.ComponentModel.DataAnnotations;

namespace GearFlow.Shared.Infrastructure.Postgres;

public sealed class PostgresOptions
{
    public const string SectionName = "postgres";

    [Required]
    public string ConnectionString { get; set; } = default!;
}
