using BetGeniusConsumer.Secrets;

namespace BetGeniusConsumer.Interfaces;

public interface ISecretRevealer
{
    public BetGeniusClient Reveal();
}