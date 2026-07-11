using GearFlow.Modules.Users.Core.Auth.DTO;
using GearFlow.Modules.Users.Core.Entities;
using GearFlow.Shared.Abstractions.Enums;
using GearFlow.Modules.Users.Core.Exceptions;
using GearFlow.Modules.Users.Core.Repositories;
using GearFlow.Modules.Users.Core.Security;
using GearFlow.Shared.Abstractions.Time;
using GearFlow.Shared.Infrastructure.Postgres;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace GearFlow.Modules.Users.Core.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordManager _passwordManager;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptions<AuthOptions> _authOptions;
    private readonly IClock _clock;

    public AuthService(IUserRepository userRepository, ICustomerRepository customerRepository, IRefreshTokenRepository refreshTokenRepository, 
        IPasswordManager passwordManager, ITokenService tokenService, IOptions<AuthOptions> authOptions, IUnitOfWork unitOfWork, IClock clock)
    {
        _userRepository = userRepository;
        _customerRepository = customerRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordManager = passwordManager;
        _tokenService = tokenService;
        _authOptions = authOptions;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<AuthResponse> SignInAsync(SignInRequest dto, CancellationToken cancellationToken)
    {
        var now = _clock.Current();

        var user = await _userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (user == null || !_passwordManager.Validate(dto.Password, user.PasswordHash))
            throw new InvalidCredentialsException();


        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = CreateRefreshToken(user.Id, now);

        _refreshTokenRepository.Add(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token
        };
    }

    public async Task LogoutAsync(LogoutRequest dto, CancellationToken cancellationToken)
    {
        await _refreshTokenRepository.RevokeAsync(dto.RefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    // todo: add logout from all devices method

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest dto, CancellationToken cancellationToken)
    {
        AuthResponse? response = null;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var now = _clock.Current();

            var token = await _refreshTokenRepository.GetByTokenAsync(dto.RefreshToken, cancellationToken);
            if (token == null || token.IsRevoked || token.IsExpired(now))
                throw new InvalidRefreshTokenException();

            var user = await _userRepository.GetByIdAsync(token.UserId, cancellationToken);
            if (user == null)
                throw new UserNotFoundException(token.UserId);

            token.Revoke();

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = CreateRefreshToken(user.Id, now);
            _refreshTokenRepository.Add(newRefreshToken);

            response = new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token
            };
        }, cancellationToken);

        return response!;
    }

    public async Task<AuthResponse> SignUpAsync(SignUpRequest dto, CancellationToken cancellationToken)
    {
        AuthResponse? response = null;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {

            if (await _userRepository.ExistsByEmailAsync(dto.Email, cancellationToken))
                throw new EmailAlreadyInUseException(dto.Email);

            var hashedPassword = _passwordManager.Secure(dto.Password);
            var now = _clock.Current();

            var user = new UserAccount(dto.Email, hashedPassword, Role.Customer, now);

            var exsistingCustomer = await _customerRepository.GetByEmailAsync(dto.Email, cancellationToken);

            if (exsistingCustomer != null)
                user.AttachCustomer(exsistingCustomer); // todo: need email verification to do so
            else
            {
                var customer = new Customer(dto.FirstName, dto.LastName, dto.Email, dto.PhoneNumber, now);
                user.AttachCustomer(customer);
                _customerRepository.Add(customer);
            }

            _userRepository.Add(user);

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = CreateRefreshToken(user.Id, now);
            _refreshTokenRepository.Add(refreshToken);

            response = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };
        }, cancellationToken);

        return response!;
    }

    private RefreshToken CreateRefreshToken(Guid userId, DateTime utcNow)
        => RefreshToken.Create(
            token: Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            expiresAt: utcNow.Add(_authOptions.Value.ExpiryRefreshToken),
            createdAt: utcNow,
            userId: userId
            );

    //public async Task<AuthResponse> RegisterEmployeeAsync(RegisterEmployeeRequest dto, CancellationToken cancellationToken)
    //{
    //    if (await _userRepository.ExistsByEmailAsync(dto.Email, cancellationToken).Result)
    //    {
    //        throw new Exception("User with this email already exists.");
    //    }

    //    var hashedPassword = _passwordManager.Secure(dto.Password);

    //    var user = new User(dto.Email, hashedPassword, Role.Customer);

    // todo: register employee user account by email and send password  
    //}
}