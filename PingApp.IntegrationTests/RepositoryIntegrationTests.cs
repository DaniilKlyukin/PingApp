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

        await repository.AddUserAsync(user, TestContext.Current.CancellationToken);

        using (var assertContext = new PingDbContext(_contextOptions))
        {
            var savedUser = await assertContext.Users.FirstOrDefaultAsync(
                u => u.Id == user.Id,
                TestContext.Current.CancellationToken);

            savedUser.Should().NotBeNull();
            savedUser!.Username.Value.Should().Be("test_integration_user");
        }

        var fetchedUser = await repository.GetUserByUsernameAsync(username, TestContext.Current.CancellationToken);
        fetchedUser.Should().NotBeNull();
        fetchedUser!.Id.Should().Be(user.Id);

        fetchedUser.IsAdmin = true;
        await repository.UpdateUserAsync(fetchedUser, TestContext.Current.CancellationToken);

        using (var assertContext = new PingDbContext(_contextOptions))
        {
            var updatedUser = await assertContext.Users.FindAsync(
                new object[] { user.Id },
                TestContext.Current.CancellationToken);

            updatedUser!.IsAdmin.Should().BeTrue();
        }

        await repository.DeleteUserAsync(user.Id, TestContext.Current.CancellationToken);

        using (var assertContext = new PingDbContext(_contextOptions))
        {
            var deletedUser = await assertContext.Users.FindAsync(
                new object[] { user.Id },
                TestContext.Current.CancellationToken);

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

        await repository.SaveSettingAsync(key, expectedValue, TestContext.Current.CancellationToken);

        var actualValue = await repository.GetSettingAsync(key, "default_val", TestContext.Current.CancellationToken);
        actualValue.Should().Be(expectedValue);

        var defaultValue = await repository.GetSettingAsync("NonExistentKey", "100", TestContext.Current.CancellationToken);
        defaultValue.Should().Be("100");
    }
}