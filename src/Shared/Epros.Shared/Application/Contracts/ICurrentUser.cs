namespace Epros.Shared.Application.Contracts
{
    public interface ICurrentUser
    {
        string? GetUserId();
        string? GetUserName();
        string? GetUserEmail();
    }
}
