using DeviceArchiving.Data.Dto;
using DeviceArchiving.Data.Dto.Users;
using DeviceArchiving.Data.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DeviceArchiving.Service.AccountServices;
using DeviceArchiving.Service;

public class AccountProcedureService : IAccountService
{
    private readonly DataAccessLayer _dataAccessLayer;
    private readonly JwtSettings _jwtSettings;

    public AccountProcedureService(IConfiguration configuration, IOptions<JwtSettings> jwtOptions)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "سلسلة الاتصال 'DefaultConnection' غير موجودة.");

        string pathDb = configuration["PathDb"]
        ?? throw new ArgumentNullException(nameof(configuration), "المفتاح 'PathDb' غير موجود في الإعدادات.");

        _dataAccessLayer = new DataAccessLayer(connectionString, pathDb);
        _jwtSettings = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
    }

    public async Task<BaseResponse<AuthenticationResponse>> AuthenticateAsync(AuthenticationRequest request)
    {
        try
        {
            var userName = request.UserName.Trim().ToLower();
            var parameters = new[] { new SqlParameter("@UserName", userName) };

            var users = await _dataAccessLayer.ExecuteQueryAsync("sp_AuthenticateUser", parameters, reader => new
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                Password = reader.GetString(reader.GetOrdinal("Password"))
            });

            var user = users.FirstOrDefault();
            if (user == null)
                return BaseResponse<AuthenticationResponse>.Failure($"المستخدم '{request.UserName}' غير موجود.");

            if (!VerifyPassword(request.Password, user.Password))
                return BaseResponse<AuthenticationResponse>.Failure("بيانات الدخول غير صحيحة.");

            var token = GenerateJwtToken(new User { Id = user.Id, UserName = user.UserName });
            var response = AuthenticationResponse.FromUser(new User { Id = user.Id, UserName = user.UserName }, new JwtSecurityTokenHandler().WriteToken(token));
            return BaseResponse<AuthenticationResponse>.SuccessResult(response, "تم تسجيل الدخول بنجاح.");
        }
        catch (SqlException ex)
        {
            return BaseResponse<AuthenticationResponse>.Failure($"خطأ أثناء تسجيل الدخول: {ex.Message}");
        }
    }

    public async Task<BaseResponse<string>> AddUserAsync(AuthenticationRequest request)
    {
        try
        {

            var email = request.UserName.Trim().ToLower();
            var hashedPassword = HashPassword(request.Password);

            var parameters = new[]
            {
                    new SqlParameter("@UserName", email),
                    new SqlParameter("@Password", hashedPassword),
                };

            await _dataAccessLayer.ExecuteNonQueryAsync("sp_AddAppUser", parameters);
            return BaseResponse<string>.SuccessResult("تم إنشاء المستخدم بنجاح.", "اكتمل التسجيل.");
        }
        catch (SqlException ex) when (ex.Message.Contains("اسم المستخدم موجود بالفعل"))
        {
            return BaseResponse<string>.Failure($"اسم المستخدم '{request.UserName}' مستخدم بالفعل.");
        }
        catch (SqlException ex)
        {
            return BaseResponse<string>.Failure($"خطأ أثناء إنشاء المستخدم: {ex.Message}");
        }
    }

    private JwtSecurityToken GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.UserName),
                new Claim(ClaimTypes.GivenName, user.UserName)
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        return new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddDays(_jwtSettings.DurationInDays),
            signingCredentials: creds
        );
    }

    private string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    private bool VerifyPassword(string input, string hashed) => BCrypt.Net.BCrypt.Verify(input, hashed);
}

