using BetGeniusConsumer.Interfaces;
using Microsoft.Extensions.Hosting;

namespace BetGeniusConsumer;

public class Worker : BackgroundService
{
    private readonly IConsumer _consumer;

    public Worker(IConsumer consumer)
    {
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _consumer.Listen();
    }
}