# WK.OpenAiWrapper

`OpenAiWrapper` is an elegant .NET library engineered to offer a seamless interface for interacting with the OpenAI API. This library acts as a wrapper around the `OpenAI-DotNet` package by [RageAgainstThePixel](https://github.com/RageAgainstThePixel), aiming to simplify the intricacies involved in leveraging various OpenAI functions and facilitating smoother AI interactions. `OpenAiWrapper` enhances the good foundation laid by `OpenAI-DotNet`, focusing on making the application process more intuitive and developer-friendly, particularly for AI-assisted workflows and thread management.

## Features

With `OpenAiWrapper`, developers gain access to a suite of functionalities designed to streamline the interaction with OpenAI's AI models:

- **Thread Management**: Automate thread handling for conversational AI applications, including message sending, response retrieval, and new thread creation.
- **Customizable Assistants**: Establish AI assistants with custom instructions and behavior models tailored to fit project specifics.
- **API Integration**: Direct and efficient integration with the OpenAI API for leveraging advanced language models.
- **Result Objects**: Convenient result objects grant easy access to AI responses along with any associated errors or metadata.

The wrapper is still at an early stage of development and is constantly being expanded with new functions.

## Getting Started

### Configuration

Begin your `OpenAiWrapper` journey by obtaining an OpenAI API key from [OpenAI](https://platform.openai.com/api-keys).

#### Set your API Key:

Your API key can be configured in your `appsettings.json` as follows:

```json
{
  "OpenAi": {
    "ApiKey": "YOUR_API_KEY"
  }
}
```
After that, only the SetOpenAiApiKey extension needs to be called on the ServiceCollection. At the moment, the wrapper can only be used together with Microsoft's DependencyInjection.
You can also omit the entry in the AppSettings and transfer the ApiKey directly via the "SetOpenAiApiKey" extension.

## Usage 

The OpenAiApiClient must now be registered on the ServiceCollection via the "RegisterOpenAi" extension. This requires at least one pilot that describes an AI assistant.

`OpenAiWrapper` exposes the `IOpenAiClient` interface, simplifying your interactions with the OpenAI API:

### Key Interface Methods

There are two main functions that map the following use cases. Firstly, when a completely new conversation is started with the AI, or when an existing conversation is to be continued.  A pilot describing the AI assistant must be specified for a new conversation. However, this pilot must first be registered via the "RegisterOpenAi" extension of the ServiceCollection. When continuing a conversation, the corresponding ID must be entered. This is called ThreadId.

The underlying "OpenAiClient" object can also be created and used for the functions that are not yet mapped in the wrapper. It is important to take care of the memory release yourself after use. 

#### GetOpenAiResponseWithNewThread

- Initiates a new conversation thread and retrieves an OpenAI response, requiring `text`, `pilot`, and `user` parameters.
  
#### GetOpenAiResponse

- Fetches an OpenAI response from an existing thread with parameters for `text`, `threadId`, and an optional `pilot`.

#### GetNewOpenAiClient

- Instantiates a new OpenAI client with your provided API key.
  
### Example Usage

Below is an illustrative guide to get you started:

```csharp
using OpenAiWrapper;

// Configure services for `OpenAiWrapper`
services.SetOpenAiApiKey().RegisterOpenAi(new Pilot("HelpfulBot", "Provide accurate and respectful responses.")); 

// Utilize `IOpenAiClient` for AI interactions
var client = serviceProvider.GetService<IOpenAiClient>();

// Engage in AI interactions within an existing thread or commence a new one
var ongoingThreadResponse = await client.GetOpenAiResponse("Let's keep the conversation going...", "thread-123", pilot: "HelpfulBot");
var newThreadResponse = await client.GetOpenAiResponseWithNewThread("Summarize this document for me: [Document Text]", "HelpfulBot", "Peter.Pan");
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
