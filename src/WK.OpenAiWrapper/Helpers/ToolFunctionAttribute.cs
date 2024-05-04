namespace WK.OpenAiWrapper.Helpers
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ToolFunctionAttribute : Attribute
    {
        public string Description { get; }

        public ToolFunctionAttribute(string description)
        {
            Description = description;
        }
    }
}
