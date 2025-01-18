using Netconf;

namespace Demo;

internal static class Program
{
    public static async Task Main()
    {
        // await NetconfDemo.Run();
        await RestconfDemo.Run();
        Console.WriteLine("Done!");
    }
    
    public const string Host = "devnetsandboxiosxe.cisco.com";
    public const string Username = "admin";
    public const string Password = "C1sco12345";
    public const int NetconfPort = 830;
    public const int RestconfPort = 443;


    public static void WriteException(RpcException exception)
    {
        Console.WriteLine($"Error: {exception.Message}");
        Console.WriteLine($"Type: {exception.Type}");
        if (exception.Tag is { } tag)
            Console.WriteLine($"Tag: {tag}");
        Console.WriteLine($"Severity: {exception.Severity}");
        if (exception.AppTag is { } appTag)
            Console.WriteLine($"App Tag: {appTag}");
        if (exception.Path is { } path)
            Console.WriteLine($"Path: {path}");
        if (exception.MessageLanguage is { } language)
            Console.WriteLine($"Message Language: {language}");
        if (exception.Info is { } info)
            Console.WriteLine($"Info: {info}");
    }


}


