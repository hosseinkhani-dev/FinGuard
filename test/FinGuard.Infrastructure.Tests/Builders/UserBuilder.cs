using FinGuard.Domain.Entities;
using FinGuard.Domain.Enums;
using FinGuard.Domain.ValueObjects;

namespace FinGuard.IntegrationTests.Builders;

public class UserBuilder
{
    private string _userName = "defaultUser";
    private string _password = "defaultPassword";
    private UserRole _role = UserRole.Admin;
    private Email? _email = null;

    public UserBuilder WithUserName(string userName)
    {
        _userName = userName;
        return this;
    }
    public UserBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }
    public UserBuilder WithRole(UserRole role)
    {
        _role = role;
        return this;
    }
    public UserBuilder WithEmail(string email)
    {
        _email = new Email(email);
        return this;
    }

    public User Build()
    {
        return new User(_userName, _password, _role, _email);
    }
}
