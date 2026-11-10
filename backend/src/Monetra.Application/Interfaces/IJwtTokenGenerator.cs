using Monetra.Domain.Entities;

namespace Monetra.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}

