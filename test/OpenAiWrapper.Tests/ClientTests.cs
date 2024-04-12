﻿using Microsoft.Extensions.DependencyInjection;
using OpenAiWrapper.Extensions;
using Xunit;

namespace OpenAiWrapper.UnitTests;

public class ClientTests
{
    [Fact]
    public void GetAssistantHandler()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.RegisterOpenAi();
        var buildServiceProvider = serviceCollection.BuildServiceProvider();
        var assistantHandler = buildServiceProvider.GetService<AssistantHandler>();
    }
}