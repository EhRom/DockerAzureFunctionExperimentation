using Puffix.ConsoleLogMagnifier;
using StackExchange.Redis;

ConsoleHelper.WriteInfo($"Welcome to the Redis test console app.");

const string redisCacheUri = "localhost:6379";
try
{
    ConsoleHelper.Write($"Initialize connection to the redis cache instance ({redisCacheUri})");
    ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisCacheUri);

    IDatabase db = redis.GetDatabase();

    const string keyName = "mykey";
    string expectedValue = $"value inserted in Redis cache at {DateTime.UtcNow}";
    ConsoleHelper.Write($"Set key {keyName}");
    await db.StringSetAsync(keyName, expectedValue);

    await Task.Delay(1000);

    string? retrievedValue = await db.StringGetAsync(keyName);

    ConsoleHelper.WriteSuccess($"Retrieved value for the key {keyName}: {retrievedValue} (expected value: {expectedValue})");
}
catch (Exception error)
{
    ConsoleHelper.WriteError("Error while testing Redis", error);
}

ConsoleHelper.Write("Press any key to quit...");
Console.ReadKey();