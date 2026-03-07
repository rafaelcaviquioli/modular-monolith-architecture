using Monolith.Modules.Users.Domain.Entities;
using Xunit;

namespace Monolith.Tests.Modules.Users.Domain;

public class UserTests
{
    [Fact]
    public void Create_ShouldReturnUserWithCorrectProperties()
    {
        var (user, _) = User.Create("john@example.com", "John Doe");

        Assert.Equal("john@example.com", user.Email);
        Assert.Equal("John Doe", user.FullName);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public void Create_ShouldRaiseUserCreatedDomainEvent()
    {
        var (user, domainEvent) = User.Create("jane@example.com", "Jane Doe");

        Assert.Equal(user.Id, domainEvent.UserId);
        Assert.Equal("jane@example.com", domainEvent.Email);
        Assert.Equal("Jane Doe", domainEvent.FullName);
    }
}
