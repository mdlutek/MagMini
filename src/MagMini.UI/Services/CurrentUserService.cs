using MagMini.Application.Common.Interfaces;

namespace MagMini.UI.Services;

public class CurrentUserService : ICurrentUserService
{
    public int? UserId { get; private set; }
    public string? Username { get; private set; }
    public bool IsAuthenticated => UserId.HasValue;

    public void SetUser(int userId, string username)
    {
        UserId = userId;
        Username = username;
    }

    public void Clear()
    {
        UserId = null;
        Username = null;
    }
}