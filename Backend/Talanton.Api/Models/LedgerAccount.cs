using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talanton.Api.Models;

[Table("ledger_accounts", Schema = "public")]
public class LedgerAccount
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("name")]
    [Required]
    public string Name { get; set; } = string.Empty;

    [Column("account_type")]
    [Required]
    public string AccountType { get; set; } = string.Empty;

    [Column("current_balance")]
    public decimal CurrentBalance { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
