namespace Ashutosh.RemoteTuning
{
    internal interface IConfigValidator
    {
        ValidationResult Validate(string rawJson, out ConfigDocument document);
    }
}