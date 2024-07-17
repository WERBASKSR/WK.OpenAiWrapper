using WK.OpenAiWrapper.Helpers;

namespace WK.OpenAiWrapper.Tests;

public class WeatherCalls
{
    [ToolFunction("""
                  Gets the weather.
                  """)]
    public string GetWeather(string location)
    {
        return string.Empty;
    }
}