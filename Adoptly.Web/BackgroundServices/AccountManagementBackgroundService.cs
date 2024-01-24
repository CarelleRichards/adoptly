using Adoptly.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Adoptly.Web.BackgroundServices;

public class AccountManagementBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AccountManagementBackgroundService> _logger;

    public AccountManagementBackgroundService(IServiceProvider services,
        ILogger<AccountManagementBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Run the background service.
                
                await DeleteAccountsAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unhandled exception when processing DeleteAccountsAsync");
            }
            finally
            {
                // Background service delay time.
                
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            }
        }
    }

    private async Task DeleteAccountsAsync(CancellationToken cancellationToken)
    {
        await using var scope = _services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<DataContext>();
        
        // Get all unverified logins.
        
        var logins = await context.Logins.Where(x => !x.Verified).ToListAsync(cancellationToken);

        // For each unverified account, add 28 days and compare it to the account's date created.
        // Check if the date is greater than now, and delete it.
        foreach (var login in from login in logins
                 let loginExpired = login.DateCreated.AddDays(28)
                 where DateTime.UtcNow > loginExpired
                 select login)
            context.Logins.Remove(login);

        await context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Account management background service completed.");
    }
}