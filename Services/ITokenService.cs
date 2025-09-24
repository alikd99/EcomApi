using EcomApi.Entities;

namespace EcomApi.Services;

public interface ITokenService
{
    string CreateToken(User user);
}
