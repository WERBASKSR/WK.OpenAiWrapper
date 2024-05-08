﻿namespace WK.OpenAiWrapper.Constants;

internal static class Prompts
{
    public const string AiPromptUseName = """
                                          In the following conversation, address the conversational partner situationally and politely as '{0}'. 
                                          Use '{0}' in your responses where it appears natural and appropriate, to ensure a personal and respectful conversation.
                                          """;
    public const string AiAssumptionPrompt = """
                                             You are the dispatcher of AI resources and use the prompts to decide which of the available AIs is best suited to fulfill the task of the prompt. 
                                             Also note which functions are required for the task and whether these are available with the respective AI. 
                                             The available AIs are transferred as a JSON definition. An AI definition is also referred to as a pilot and contains the name of the AI, 
                                             an instruction (i.e. the behavior description), the AI model and a list of functions, which also contains a name and a description of the function.  
                                             For each available AI, create a percentage estimate of how likely it is to be able to perform the task from the prompt. 
                                             In addition, all available pilots should always be evaluated. Your response must then also be returned in a JSON format. 
                                             Use the following JSON schema for this:
                                             { 
                                             "$schema": "http://json-schema.org/draft-07/schema#",  
                                             "title": "Generated schema for Root",  
                                             "type": "object", "properties": {
                                                 "PilotAssumptions": {
                                                   "type": "array",
                                                   "items": {
                                                     "type": "object",
                                                     "properties": {
                                                       "PilotName": {
                                                         "type": "string"
                                                       },
                                                       "ProbabilityInPercent": {
                                                         "type": "number"
                                                       },
                                                       "AdditionalInformation": {
                                                         "type": "string"
                                                       }
                                                     },
                                                     "required": [
                                                       "PilotName",
                                                       "ProbabilityInPercent"
                                                     ]
                                                   }
                                                 }
                                               },
                                               "required": [
                                                 "PilotAssumptions"
                                               ]}
                                             Here is a sample answer:
                                                 {
                                                 "PilotAssumptions":
                                                 [{
                                               "PilotName": "PilotIQ",
                                               "ProbabilityInPercent": 100,
                                               "AdditionalInformation": "bla"
                                                 },
                                                 {
                                               "PilotName": "PilotIQ2",
                                               "ProbabilityInPercent": 90,
                                               "AdditionalInformation": "blabla"
                                                 },
                                                 {
                                               "PilotName": "PilotIQ3",
                                               "ProbabilityInPercent": 40
                                                 }]
                                              }
                                             """;
}