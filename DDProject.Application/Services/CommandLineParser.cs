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
        string targetUrl = Console.ReadLine()!;
        int targetPort = int.Parse(Console.ReadLine()!);
        string message = Console.ReadLine()!;
        await _p2pService.Send(targetUrl, targetPort, message);
    }

    public async Task AwaitAndParseInput()
    {
        string op = Console.ReadLine()!;

        Task t = op switch
        {
            "listen" => _p2pService.Listen(),
            "send" => PromptAndExecuteSend(),
            "exit" => throw new ShutdownApplicationException(),
            _ => WarnUnkownCommand(op)
        };
        await t;
    }
}
