namespace ProjectSaas.Api.Realtime;

public static class NotificationGroupNameBuilder
{
  public static string ForRecipient(Guid organisationId, Guid userId)
      => $"tenant:{organisationId}:user:{userId}";
}