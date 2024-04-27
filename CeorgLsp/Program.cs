namespace CeorgLsp
{
    public class StartLsp
    {
        public static void Main()
        {
            Message? _ = DecodeRpcMessage.Decode(Console.OpenStandardInput());
        }
    }
}
