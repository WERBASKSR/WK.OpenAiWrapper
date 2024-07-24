using Microsoft.Extensions.Options;
using WK.OpenAiWrapper.Extensions;
using WK.OpenAiWrapper.Interfaces;
using WK.OpenAiWrapper.Models;
using WK.OpenAiWrapper.Options;
using WK.OpenAiWrapper.Result;

namespace WK.OpenAiWrapper;

public class PilotConfig : IOpenAiPilotConfig
{
    private readonly OpenAiOptions _options;

    public PilotConfig(IOptions<OpenAiOptions> options)
    {
        _options = options.Value;
    }
    
    public async Task<Result<Pilot?>> GetPilot(string pilotName) => _options.GetPilot(pilotName);

    public async Task<Result<Pilot>> AddPilot(Pilot pilot)
    {
        try
        {
            _options.AddPilot(pilot);
            return pilot;
        }
        catch (Exception e)
        {
            return Result<Pilot>.Error(e.Message);
        }
    }

    public async Task<Result<Pilot>> UpdatePilot(Pilot pilot)
    {
        try
        {
            await _options.UpdatePilotAsync(pilot);
            return pilot;
        }
        catch (Exception e)
        {
            return Result<Pilot>.Error(e.Message);
        }
    }

    public async Task<Result<Pilot?>> DeletePilot(string pilotName)
    {
        try
        {
            Pilot? pilot = await _options.DeletePilotAsync(pilotName);
            return pilot ?? throw new Exception($"{pilotName} could not be deleted because he was not found.");
        }
        catch (Exception e)
        {
            return Result<Pilot?>.Error(e.Message);
        }
    }
}