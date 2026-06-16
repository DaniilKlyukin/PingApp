using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PingApp.Domain.Aggregates.UserAggregate;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;
using PingApp.Infrastructure.Data;
using PingApp.Infrastructure.Repositories;

namespace PingApp.IntegrationTests;

public class RepositoryIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DbContextOptions<PingDbContext> _contextOptions;

    public RepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _contextOptions = new DbContextOptionsBuilder<PingDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;
    }

    [Fact]
    public async Task UserRepository_ShouldPersistAndManageUsers()
    {
        using var context = new PingDbContext(_contextOptions);
        var repository = new UserRepository(context);

        var username = Username.Create("test_integration_user").Value;
        var user = User.Create(username, isGuest: false, isAdmin: false);

        await repository.AddUserAsync(user);

        using (var assertContext = new PingDbContext(_contextOptions))
        {
            var savedUser = await assertContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            savedUser.Should().NotBeNull();
            savedUser!.Username.Value.Should().Be("test_integration_user");
        }

        var fetchedUser = await repository.GetUserByUsernameAsync(username);
        fetchedUser.Should().NotBeNull();
        fetchedUser!.Id.Should().Be(user.Id);

        fetchedUser.IsAdmin = true;
        await repository.UpdateUserAsync(fetchedUser);

        using (var assertContext = new PingDbContext(_contextOptions))
        {
            var updatedUser = await assertContext.Users.FindAsync(user.Id);
            updatedUser!.IsAdmin.Should().BeTrue();
        }

        // Act & Assert (Delte)
        await repository.DeleteUserAsync(user.Id);

        using (var assertContext = new PingDbContext(_contextOptions))
        {
            var deletedUser = await assertContext.Users.FindAsync(user.Id);
            deletedUser.Should().BeNull();
        }
    }

    [Fact]
    public async Task GlobalSettingsRepository_ShouldSaveAndRetrieveSettings()
    {
        using var context = new PingDbContext(_contextOptions);
        var repository = new GlobalSettingsRepository(context);
        var key = "TestSettingKey";
        var expectedValue = "45";

        await repository.SaveSettingAsync(key, expectedValue);

        var actualValue = await repository.GetSettingAsync(key, "default_val");
        actualValue.Should().Be(expectedValue);

        var defaultValue = await repository.GetSettingAsync("NonExistentKey", "100");
        defaultValue.Should().Be("100");
    }
}