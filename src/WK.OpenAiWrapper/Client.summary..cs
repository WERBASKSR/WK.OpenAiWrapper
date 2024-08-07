﻿using WK.OpenAiWrapper.Result;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Interfaces.Services;

namespace WK.OpenAiWrapper;

internal partial class Client
{
    internal readonly ISummaryService SummaryService;
        
    public Task<Result<OpenAiResponse>> GetConversationSummaryResponse(string threadId, int messageCount = 10) 
        => SummaryService.GetConversationSummaryResponse(threadId, messageCount);
}