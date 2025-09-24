using EcomApi.Data;
using EcomApi.Dtos;
using EcomApi.Entities;
using EcomApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, ITokenService tokenSvc) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        // تأكد الإيميل مش مكرر
        if (await db.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Email already exists");

        // عمل Hash للباسورد
        var user = new User
        {
            Email = dto.Email,
            FullName = dto.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        // توليد توكن
        var token = tokenSvc.CreateToken(user);
        return new AuthResponseDto(token, user.Email, user.FullName);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        var token = tokenSvc.CreateToken(user);
        return new AuthResponseDto(token, user.Email, user.FullName);
    }
}
