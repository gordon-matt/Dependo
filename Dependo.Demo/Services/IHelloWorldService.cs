namespace Dependo.Demo.Services
{
    public interface IHelloWorldService
    {
        string HelloWorld();
    }

    public class HelloWorldService : IHelloWorldService
    {
        public string HelloWorld()
        {
            return "Hello World";
        }
    }
}