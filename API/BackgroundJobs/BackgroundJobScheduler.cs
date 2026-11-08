using Application.Features.Reservations.Jobs;

namespace API.BackgroundJobs;

public sealed class BackgroundJobScheduler(
    IServiceProvider _serviceProvider,
    ILogger<BackgroundJobScheduler> _logger)
    : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("🚀 Arka Plan İşleri (Job Scheduler) başlatıldı.");

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        // 🚨 1. JOB: Pending Payments (Kendi scope'u ile)
        using (var scope1 = _serviceProvider.CreateScope())
        {
          var pendingJob = scope1.ServiceProvider.GetRequiredService<ExpirePendingPaymentsJob>();
          await pendingJob.ExecuteAsync(stoppingToken);
        } // scope1 bitti, DbContext dispose edildi

        // 🚨 2. JOB: Deposit Locks (Kendi scope'u ile)
        using (var scope2 = _serviceProvider.CreateScope())
        {
          var depositJob = scope2.ServiceProvider.GetRequiredService<ExpireDepositLocksJob>();
          await depositJob.ExecuteAsync(stoppingToken);
        } // scope2 bitti, DbContext dispose edildi
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "❌ Arka plan işleri çalışırken hata oluştu.");
      }

      // 1 dakika bekle
      await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    }
  }
}

/* Task.WhenAll => 
      1., 2. ve 3. sorguyu aynı anda (hepsini tek bir hamlede) fırlatırsın.
      Hepsini tek bir çatı altında toplar ve "hepsi bitene kadar beklesinler" dersin.
      En uzun süren hangisiyse (diyelim ki 2 saniye süren) sistem o kadar sürer.

*/