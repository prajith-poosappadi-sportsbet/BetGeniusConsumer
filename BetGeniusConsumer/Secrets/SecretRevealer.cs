using BetGeniusConsumer.Interfaces;
using Microsoft.Extensions.Options;

namespace BetGeniusConsumer.Secrets;

public class SecretRevealer : ISecretRevealer
{
    private readonly BetGeniusClient _secrets;
    
    public SecretRevealer (IOptions<BetGeniusClient> secrets)
    {
        _secrets = secrets.Value ?? throw new ArgumentNullException(nameof(secrets));
    }

    public BetGeniusClient Reveal()
    {
        return _secrets;
    }
}