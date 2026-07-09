using FinGuard.Application.Commons.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinGuard.Application.Features.Users.Queries.GetUser;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, GetUserDto?>
{
    private readonly IFinGuardDbContext _context;

    public GetUserQueryHandler(IFinGuardDbContext context)
    {
        _context = context;
    }

    public async Task<GetUserDto?> Handle(
        GetUserQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == request.Id)
            .Select(u => new GetUserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email != null ? u.Email.EmailAddress : null,
                ActiveStatus = u.IsActive ? "Active" : "Inactive"
            }).FirstOrDefaultAsync(cancellationToken);
    }
}
