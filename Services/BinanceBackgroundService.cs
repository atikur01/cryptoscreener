namespace CryptoScreener.Services
{
    public class BinanceBackgroundService : BackgroundService
    {
        private readonly BinanceService _binanceService;

        public BinanceBackgroundService(BinanceService service)
        {
            _binanceService = service;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _binanceService.UpdateDataAsync();
                await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
            }
        }
    }
}
