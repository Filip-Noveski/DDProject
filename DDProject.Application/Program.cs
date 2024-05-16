using DDProject.Application.Services;

namespace DDProject.Application;

internal class Program
{
    static async Task Main(string[] args)
    {
        ConsoleProgram program = new(args);
        await program.Run();
    }
}
