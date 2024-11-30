using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using CleanArchitecture.Identity.Application.Dtos;
using CleanArchitecture.Identity.Repository.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tenon.AspNetCore.Abstractions.Application;
using Tenon.AspNetCore.Configuration;
using Tenon.AspNetCore.Extensions;
using Tenon.AspNetCore.Identity.Extensions;
using Tenon.DistributedId.Abstractions;
using Tenon.EntityFrameworkCore.Extensions;
using Tenon.Mapper.Abstractions;
using Tenon.Models.Dtos;
using Tenon.Repository.EfCore.Transaction;

namespace CleanArchitecture.Identity.Application.Services.Impl;

public sealed class UserService : ServiceBase, IUserService
{
    private readonly IDGenerator _idGenerator;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<UserService> _logger;
    private readonly IObjectMapper _mapper;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly RoleManager<Role> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;

    public UserService(ILogger<UserService> logger, UserManager<User> userManager, RoleManager<Role> roleManager,
        IPasswordHasher<User> passwordHasher, IUnitOfWork unitOfWork, IObjectMapper mapper,
        IOptionsMonitor<JwtOptions> jwtOptions, IDGenerator idGenerator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(_userManager));
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(_roleManager));
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _idGenerator = idGenerator;
        _jwtOptions = jwtOptions.CurrentValue;
    }

    public async Task<ServiceResult<UserLoginResultDto>> LoginAsync(UserLoginDto input)
    {
        var loginUser = await _userManager.FindByEmailAsync(input.Email);
        if (loginUser == null)
            return Problem(HttpStatusCode.BadRequest, "Incorrect username or password");

        if (!await _userManager.CheckPasswordAsync(loginUser, input.Password))
            return Problem(HttpStatusCode.Forbidden, "Invalid credentials");

        var claims = new Claim[2];
        var jtiValue = Guid.NewGuid().ToString("N");
        claims[0] = new Claim(JwtRegisteredClaimNames.Sub, loginUser.Id.ToString());
        claims[1] = new Claim(JwtRegisteredClaimNames.Jti, jtiValue);
        var accessToken = _jwtOptions.CreateAccessToken(claims.ToArray());
        var refreshToken = _jwtOptions.CreateRefreshToken(claims.ToArray());
        var tokenInfo = new UserLoginResultDto(accessToken.Token, accessToken.Expire, refreshToken.Token,
            refreshToken.Expire);
        return tokenInfo;
    }

    public async Task<ServiceResult<UserTokenInfoDto>> RefreshAccessTokenAsync(UserRefreshTokenDto input)
    {
        var userClaims = _jwtOptions.GetClaimsFromRefreshToken(input.RefreshToken);
        if (!(userClaims?.Any() ?? false))
            return Problem(HttpStatusCode.Forbidden);

        var nameIdClaim = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (nameIdClaim == null)
            return Problem(HttpStatusCode.Forbidden);
        if (!long.TryParse(nameIdClaim.Value, out var subId))
        {
            return Problem(HttpStatusCode.Forbidden);
        }
        var loginUser = await _userManager.FindByIdAsync(subId.ToString());
        if (loginUser == null)
            return Problem(HttpStatusCode.Forbidden);
        var claims = new Claim[2];
        var jtiValue = Guid.NewGuid().ToString("N");
        claims[0] = new Claim(JwtRegisteredClaimNames.Sub, loginUser.Id.ToString());
        claims[1] = new Claim(JwtRegisteredClaimNames.Jti, jtiValue);
        var accessToken = _jwtOptions.CreateAccessToken(claims.ToArray());
        var refreshToken = _jwtOptions.CreateRefreshToken(claims.ToArray());
        var tokenInfo = new UserTokenInfoDto(accessToken.Token, accessToken.Expire, refreshToken.Token,
            refreshToken.Expire);
        return tokenInfo;
    }

    public async Task<ServiceResult<long>> CreateAsync(UserCreationDto input)
    {
        var existUser = await _userManager.FindByNameAsync(input.Name);
        if (existUser != null)
            return Problem(HttpStatusCode.BadRequest, $"UserName:{input.Name} is exist");
        existUser = await _userManager.FindByEmailAsync(input.Email);
        if (existUser != null)
            return Problem(HttpStatusCode.BadRequest, $"Email:{input.Email} is exist");
        _unitOfWork.BeginTransaction();
        var user = _mapper.Map<User>(input);
        user.PasswordHash = _passwordHasher.HashPassword(user, input.Password);
        user.Id = _idGenerator.GetNextId();
        user.SecurityStamp = Guid.NewGuid().ToString();
        var createdResult = await _userManager.CreateAsync(user);
        if (!createdResult.Succeeded)
            return Problem(HttpStatusCode.BadRequest, createdResult.GetErrorMessage());

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Email, input.Email),
            new(JwtRegisteredClaimNames.Birthdate, input.Birthday.ToString()),
            new(JwtRegisteredClaimNames.Gender, input.Sex.ToString()),
            new(IdentityRegisteredClaimNames.DeptId, input.DeptId.ToString())
        };
        var addClaimsResult = await _userManager.AddClaimsAsync(user, claims);
        if (!addClaimsResult.Succeeded)
        {
            await _unitOfWork.RollbackAsync();
            return Problem(HttpStatusCode.BadRequest, $"Create user:{input.Name} failed");
        }

        await _unitOfWork.CommitAsync();
        return user.Id;
    }

    public async Task<ServiceResult> UpdateAsync(long id, UserUpdationDto input)
    {
        var user = _mapper.Map<User>(input);
        user.Id = id;
        await _userManager.UpdateAsync(user);
        return CreateServiceResult();
    }

    public async Task<ServiceResult> DeleteAsync(long id)
    {
        var existUser = await _userManager.FindByIdAsync(id.ToString());
        if (existUser == null)
            return Problem(HttpStatusCode.BadRequest, $"UserId:{id} is not exist");
        await _userManager.DeleteAsync(existUser);
        return CreateServiceResult();
    }

    public async Task<PagedResultDto<UserDto>> GetPagedAsync(UserSearchPagedDto search)
    {
        var userPagedList = await _userManager.Users
            .WhereIf(!string.IsNullOrWhiteSpace(search.Account),
                x => EF.Functions.Like(x.Account, $"%{search.Account}%"))
            .WhereIf(!string.IsNullOrWhiteSpace(search.Name), x => EF.Functions.Like(x.UserName, $"%{search.Name}%"))
            .ToPagedListAsync(search.PageIndex, search.PageSize);

        var userDtos = _mapper.Map<List<UserDto>>(userPagedList.Data);
        if (userDtos?.Any() ?? false)
            foreach (var user in userPagedList.Data)
            {
                var userClaims = await _userManager.GetClaimsAsync(user);
                var userDto = userDtos.FirstOrDefault(c => c.Account == user.Account);
                if (userDto == null) continue;
                userDto.Email = userClaims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)
                    ?.ToString();
                userDto.Sex =
                    Convert.ToInt32(userClaims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Gender)?.Value);
                userDto.Birthday =
                    Convert.ToDateTime(userClaims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Birthdate)
                        ?.Value);
                userDto.Sex =
                    Convert.ToInt32(userClaims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Gender)?.Value);
                userDto.DeptId =
                    Convert.ToInt64(
                        userClaims.FirstOrDefault(c => c.Type == IdentityRegisteredClaimNames.DeptId)?.Value);
            }

        return new PagedResultDto<UserDto>(userPagedList.CurrentPage, userPagedList.PageSize, userDtos,
            userPagedList.TotalCount);
    }
}