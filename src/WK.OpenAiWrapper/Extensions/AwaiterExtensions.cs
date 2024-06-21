using System.Runtime.CompilerServices;

namespace WK.OpenAiWrapper.Extensions;

public static class AwaiterExtensions
{
    public static TaskAwaiter GetAwaiter(this int milliseconds) => Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).GetAwaiter();
}