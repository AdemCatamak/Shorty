namespace Shorty.Application.Services;

public interface IRandomGenerator
{
    string Generate(int? minLength = null, int? maxLength = null);
}

public class RandomGenerator : IRandomGenerator
{
    private readonly Random _random;

    public RandomGenerator()
    {
        _random = new Random();
    }

    public string Generate(int? minLength = null, int? maxLength = null)
    {
        minLength ??= 5;
        maxLength ??= 10;
        return Generate(minLength.Value, maxLength.Value);
    }

    private string Generate(int minLength, int maxLength)
    {
        // const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const string chars = "ABCDEFGHJKMNPQRSTUVWXYZ0123456789";
        var length = _random.Next(minLength, maxLength);

        var random = Enumerable.Repeat(chars, length).Select(s => s[_random.Next(s.Length)]).ToArray();
        return new string(random);
    }
}