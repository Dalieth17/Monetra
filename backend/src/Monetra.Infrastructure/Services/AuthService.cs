using Microsoft.EntityFrameworkCore;
using Monetra.Application.Common.Exceptions;
using Monetra.Application.DTOs.Auth;
using Monetra.Application.Interfaces;
using Monetra.Domain.Entities;
using Monetra.Domain.Enums;
using Monetra.Infrastructure.Persistence;

namespace Monetra.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly MonetraDbContext _context;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthService(MonetraDbContext context, IJwtTokenGenerator tokenGenerator)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var emailLower = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _context.Users.AnyAsync(u => u.Email == emailLower, cancellationToken);
        if (existingUser)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Email"] = ["Ya existe un usuario registrado con este correo electrónico."]
            });
        }

        var requestedRole = string.IsNullOrWhiteSpace(request.Role) ? AppRoles.Usuario : request.Role.Trim();
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == requestedRole, cancellationToken)
                   ?? await _context.Roles.FirstAsync(r => r.Name == AppRoles.Usuario, cancellationToken);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newUser = new User
        {
            Name = request.Name.Trim(),
            Email = emailLower,
            PasswordHash = passwordHash,
            RoleId = role.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync(cancellationToken);

        // Create default starter accounts for the user (COP and USD)
        var defaultAccounts = new List<Account>
        {
            new() { UserId = newUser.Id, Name = "Efectivo COP", CurrencyCode = "COP", CurrentBalance = 0m, CreatedAt = DateTime.UtcNow },
            new() { UserId = newUser.Id, Name = "Cuenta Banco USD", CurrencyCode = "USD", CurrentBalance = 0m, CreatedAt = DateTime.UtcNow }
        };
        _context.Accounts.AddRange(defaultAccounts);
        await _context.SaveChangesAsync(cancellationToken);

        newUser.Role = role;
        var token = _tokenGenerator.GenerateToken(newUser);

        return new AuthResponseDto
        {
            Token = token,
            UserId = newUser.Id,
            Name = newUser.Name,
            Email = newUser.Email,
            Role = role.Name
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var emailLower = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == emailLower, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Credenciales inválidas. Por favor verifique su correo y contraseña.");
        }

        var token = _tokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.Name
        };
    }
}

