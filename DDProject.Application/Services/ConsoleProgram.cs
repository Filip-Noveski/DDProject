using DDProject.Application.Exceptions;
using DDProject.P2P.Services;

namespace DDProject.Application.Services;

public class ConsoleProgram
{
    private CommandLineParser _consoleAppService = null!;

    public ConsoleProgram(string[] args)
    {
        string url = args[0];
        int port = int.Parse(args[1]);

        P2PService p2PService = new(url, port);
        _consoleAppService = new(p2PService);
    }

    private async Task MainLoop()
    {
        while (true)
        {
            await _consoleAppService.AwaitAndParseInput();
        }
    }

    public async Task Run()
    {
        try
        {
            await MainLoop();
        }
        catch (ShutdownApplicationException)
        {
            Console.WriteLine("Shutting down...");
            return;
        }
    }
}
