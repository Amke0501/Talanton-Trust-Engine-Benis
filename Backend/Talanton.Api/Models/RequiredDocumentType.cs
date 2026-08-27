namespace Talanton.Api.Models;

public class RequiredDocumentType
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string ApplicantType { get; set; } = "Both";

    public bool IsMandatory { get; set; }

    public string? AllowedMimeTypes { get; set; }

    public int MaxFileSizeMb { get; set; }

    public bool IsCollateralDocumentType { get; set; }

    public Guid? SaccoId { get; set; }

    public Sacco? Sacco { get; set; }
}