namespace ProjectSaas.Api.Application.Exceptions;

public sealed class InvalidRefreshTokenException : Exception
{
  public InvalidRefreshTokenException()
      : base("Refresh token is invalid or expired.")
  {
  }
}