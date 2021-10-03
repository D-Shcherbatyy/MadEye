using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodyNotes.Entities.Authentication.Entities;
using FoodyNotes.Infrastructure.Interfaces.Authentication;
using FoodyNotes.Infrastructure.Interfaces.Persistence;
using FoodyNotes.UseCases.Authentication.Commands;
using FoodyNotes.UseCases.Exceptions;
using MediatR;

namespace FoodyNotes.UseCases.Authentication.Handlers
{
  public class RevokeTokenCommandHandler : AsyncRequestHandler<RevokeTokenCommand>
  {
    private readonly IRepositoryBase<User, string> _userRepo;
    private readonly IRefreshTokenService _refreshTokenService;

    public RevokeTokenCommandHandler(IRepositoryBase<User, string> userRepo, IRefreshTokenService refreshTokenService)
    {
      _userRepo = userRepo;
      _refreshTokenService = refreshTokenService;
    }

    protected override async Task Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
      var user = await _refreshTokenService.GetUserByRefreshTokenAsync(request.RefreshToken);
      var refreshToken = user.RefreshTokens.Single(x => x.Token == request.RefreshToken);

      if (!refreshToken.IsActive)
        throw new AppException("Invalid token");

      // revoke token and save
      _refreshTokenService.RevokeRefreshToken(refreshToken, request.IpAddress, "Revoked without replacement");
      
      _userRepo.Update(user);
      
      await _userRepo.SaveChangesAsync(cancellationToken);
    }
  }
}