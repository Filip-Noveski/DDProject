using DDProject.Application.Exceptions;
using DDProject.P2P.Services;

namespace DDProject.Application.Services;

public class CommandLineParser
{
    private readonly P2PService _p2pService;

    public CommandLineParser(P2PService p2pService)
    {
        _p2pService = p2pService;
    }

    private static Task WarnUnkownCommand(string command)
    {
        Console.WriteLine($"The command '{command}' was not recognised.");
        return Task.CompletedTask;
    }

    private async Task PromptAndExecuteSend()
    {
        Console.Write("Target url: ");
        string targetUrl = Console.ReadLine()!;

        Console.Write("Target port: ");
        int targetPort = int.Parse(Console.ReadLine()!);

        Console.Write("Message: ");
        string message = Console.ReadLine()!;

        string response = await _p2pService.Send(targetUrl, targetPort, message);

        if (response is "")
        {
            Console.WriteLine($"Ack received\n");
            return;
        }

        Console.WriteLine($"Received response: {response}\n");
    }

    private async Task Listen()
    {
        Console.WriteLine("Listening...");
        string response = await _p2pService.Listen();
        Console.WriteLine($"Received response: {response}\n");
    }

    public async Task AwaitAndParseInput()
    {
        Console.Write("Operation: ");
        string op = Console.ReadLine()!;

        Task t = op switch
        {
            "listen" => Listen(),
            "send" => PromptAndExecuteSend(),
            "exit" => throw new ShutdownApplicationException(),
            _ => WarnUnkownCommand(op)
        };
        await t;
    }
}
