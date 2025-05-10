namespace Dependo.Demo.Services;

/// <summary>
/// Example service interface
/// </summary>
public interface IExampleService
{
    /// <summary>
    /// Gets example data
    /// </summary>
    /// <returns>Example data string</returns>
    string GetData();
}

/// <summary>
/// Example service implementation
/// </summary>
public class ExampleService : IExampleService
{
    /// <inheritdoc/>
    public string GetData() => "Example data from a singleton service";
}