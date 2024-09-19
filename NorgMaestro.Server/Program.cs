namespace NorgMaestro.Server;

public sealed class Program
{
    public static void Main()
    {
        var server = new NeorgLspServer();
        server.Startup();
    }
}
