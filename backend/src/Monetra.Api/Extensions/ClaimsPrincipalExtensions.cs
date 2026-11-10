using System.Security.Claims;
using Monetra.Application.Common.Exceptions;

namespace Monetra.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out var userId))
        {
            return userId;
        }

        throw new UnauthorizedException("Identificador de usuario inválido en el token de autenticación.");
    }
}
