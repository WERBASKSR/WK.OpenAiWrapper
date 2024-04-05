# OpenAiWrapper

`OpenAiWrapper` is an elegant .NET library engineered to offer a seamless interface for interacting with the OpenAI API. This library acts as a wrapper around the `OpenAI-DotNet` package by RageAgainstThePixel, aiming to simplify the intricacies involved in leveraging various OpenAI functions and facilitating smoother AI interactions. `OpenAiWrapper` enhances the good foundation laid by `OpenAI-DotNet`, focusing on making the application process more intuitive and developer-friendly, particularly for AI-assisted workflows and thread management.

## Features

With `OpenAiWrapper`, developers gain access to a suite of functionalities designed to streamline the interaction with OpenAI's AI models:

- **Thread Management**: Automate thread handling for conversational AI applications, including message sending, response retrieval, and new thread creation.
- **Customizable Assistants**: Establish AI assistants with custom instructions and behavior models tailored to fit project specifics.
- **API Integration**: Direct and efficient integration with the OpenAI API for leveraging advanced language models.
- **Result Objects**: Convenient result objects grant easy access to AI responses along with any associated errors or metadata.

## Getting Started

### Configuration

Begin your `OpenAiWrapper` journey by obtaining an OpenAI API key from [OpenAI](https://beta.openai.com/account/api-keys).

#### Set your API Key:

Your API key can be configured in your `appsettings.json` as follows:

```json
{
  "OpenAi": {
    "ApiKey": "YOUR_API_KEY"
  }
}
```

## Installation

Integrate `OpenAiWrapper` into your .NET project effortlessly:

```shell
dotnet add package OpenAiWrapper
```

## Usage 

`OpenAiWrapper` exposes the `IOpenAiClient` interface, simplifying your interactions with the OpenAI API:

### Key Interface Methods

#### GetNewOpenAiClient

- Instantiates a new OpenAI client with your provided API key.

#### GetOpenAiResponse

- Fetches an OpenAI response from an existing thread with parameters for `text`, `threadId`, and an optional `pilot`.

#### GetOpenAiResponseWithNewThread

- Initiates a new conversation thread and retrieves an OpenAI response, requiring `text`, `pilot`, and `user` parameters.

### Example Usage

Below is an illustrative guide to get you started:

```csharp
using OpenAiWrapper;

// Configure services for `OpenAiWrapper`
services.SetOpenAiApiKey() 
        .RegisterOpenAi(new Pilot("HelpfulBot", "Provide accurate and respectful responses.")); 

// Utilize `IOpenAiClient` for AI interactions
var client = serviceProvider.GetService<IOpenAiClient>();

// Engage in AI interactions within an existing thread or commence a new one
var ongoingThreadResponse = await client.GetOpenAiResponse("Let's keep the conversation going...", "thread-123", pilot: "HelpfulBot");
var newThreadResponse = await client.GetOpenAiResponseWithNewThread("Summarize this document for me: [Document Text]", "HelpfulBot", "user1");
```

## Customization

`OpenAiWrapper` allows significant customization to fit your specific requirements:

- **Define Pilots**: Tailor AI assistants with specific instructions and behaviors using the `Pilot` objects.
- **Adjust Instructions**: Modify your assistant's instructions via the `Pilot` constructor for precise functionalities.
- **Incorporate Tools**: Optimize your assistant's functionality with external tools as per OpenAI API guidelines.

## Testing

Ensure reliability with the included unit tests in the `ClientTests` class. Here is a setup example:

```csharp
ServiceCollection serviceCollection = new ();
serviceCollection.SetOpenAiApiKey("your-api-key");
serviceCollection.RegisterOpenAi(new Pilot("Helperli", "Be resourceful. Respond in German."));

ServiceProvider buildServiceProvider = serviceCollection.BuildServiceProvider();

IOpenAiClient openAiClient = buildServiceProvider.GetService<IOpenAiClient>() ?? throw new ArgumentNullException(nameof(IOpenAiClient));

Result<OpenAiResponse> result = openAiClient.GetOpenAiResponseWithNewThread("Why does Lukas have a peculiar smell?", "Helperli", "Stefan").Result;
Assert.IsTrue(result.IsSuccess);
Assert.IsNull(result.Value.Answer);
```
*Remember to replace `"your-api-key"` with your actual OpenAI API key.*

---