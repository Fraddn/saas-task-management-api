namespace ProjectSaas.Worker.Models;

public sealed class UserLookup
{
  public Guid Id { get; set; }
  public Guid OrganisationId { get; set; }
  public string Role { get; set; } = default!;
  public bool IsDisabled { get; set; }
}