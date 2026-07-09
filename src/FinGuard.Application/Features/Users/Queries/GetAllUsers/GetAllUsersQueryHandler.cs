using FinGuard.Application.Commons.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinGuard.Application.Features.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler :
    IRequestHandler<GetAllUsersQuery, List<GetAllUserDto>>
{
    private readonly IFinGuardDbContext _context;

    public GetAllUsersQueryHandler(IFinGuardDbContext context)
    {
        _context = context;
    }

    public async Task<List<GetAllUserDto>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Users.AsNoTracking();

        if(!string.IsNullOrEmpty(request.UserName))
        {
            query = query.Where(user => user.UserName.Contains(request.UserName));
        }

        if(!string.IsNullOrEmpty(request.Email))
        {
            query = query.Where(user => user.Email != null 
            && user.Email.EmailAddress.Contains(request.Email));
        }

        if(request.IsActive.HasValue)
        {
            query = query.Where(user => user.IsActive == request.IsActive.Value);
        }

        return await query
            .OrderByDescending(user => user.CreatedAt)
            .Select(user => new GetAllUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email != null ? user.Email.EmailAddress : null,
                ActiveStatus = user.IsActive ? "Active" : "Inactive"
            })
            .ToListAsync(cancellationToken);
    }
}
