# OpenAiWrapper - A .NET Library for OpenAI API Integration

OpenAiWrapper is a user-friendly .NET library designed to simplify the integration and utilization of the OpenAI API in .NET applications. This library acts as a wrapper around the `OpenAI-DotNet` package by [RageAgainstThePixel](https://github.com/RageAgainstThePixel), streamlining API interactions and enhancing usability, particularly for managing AI workflows and threads.

## Features

- 🛠 **Easy Configuration**: Simplify the setup process with extension methods for IServiceCollection.
- 🤖 **Enhanced AI Operations**: Utilize various OpenAI models efficiently within your .NET applications.
- 🧵 **Thread Management**: Manage threads for continuous conversations or data tracking.
- 🚀 **Advanced Pilot Management**: Configure and use different pilots with specific capabilities and tools.
- 📝 **Support for Multiple AI Features**: Get text, image, and audio responses easily.

## Installation

To start using OpenAiWrapper, add the package to your .NET project using NuGet:

```bash
dotnet add package OpenAiWrapper
```

## Configuration

### Setup in `Startup.cs` or `Program.cs`

```csharp
public void ConfigureServices(IServiceCollection services)
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

    // Register OpenAI services with API key and optional pilots
    services.RegisterOpenAi("your_openai_api_key_here", new Pilot("default", "Default Pilot Instructions"));
}
```

## Usage Examples

### Getting AI Responses

```csharp
public async Task<string> GetResponseFromOpenAi(string userInput)
{
    var client = serviceProvider.GetService<IOpenAiClient>();
    var response = await client.GetOpenAiResponseWithNewThread(userInput, "default", "user1");

    if (response.IsSuccess)
    {
        return response.Value.Answer;
    }
    else
    {
        throw new Exception("Failed to get response from OpenAI");
    }
}
```

### Working with Images

```csharp
public async Task<string> GetImageUrlFromOpenAi(string prompt)
{
    var client = serviceProvider.GetService<IOpenAiClient>();
    var response = await client.GetOpenAiImageResponse(prompt);

    if (response.IsSuccess)
    {
        return response.Value.Url;
    }
    else
    {
        throw new Exception("Failed to generate image from OpenAI");
    }
}
```

## Advanced Configuration

### Defining Pilots

Pilots can be defined with specific tools and models to tailor the AI responses according to the application's needs.

```csharp
var advancedPilot = new Pilot("advanced", "Provide detailed analysis", "gpt-4-turbo")
{
    Tools = new List<Tool>
    {
        new Tool("AnalysisTool", "Detailed Analysis")
    }
};

services.RegisterOpenAi("your_openai_api_key_here", advancedPilot);
```

## Contribution

Contributions are welcome! If you would like to improve the OpenAiWrapper, feel free to fork the repository and submit a pull request.

## About Us

**Werbas** continues to innovate and improve upon tools and technologies that enhance your experience with artificial intelligence. Visit us at [www.werbas.com](http://www.werbas.com) to learn more about our contributions to the tech community and how we can help you achieve your business goals.

## License

OpenAiWrapper is released under the MIT License. See the `LICENSE` file for more information.

---

Enjoy building with OpenAiWrapper, and unleash the power of AI in your .NET applications seamlessly! 🚀✨
