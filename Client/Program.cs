using Coldairarrow.DotNettyRPC;
using Common;
using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            IHello client = RPCClientFactory.GetClient<IHello>("127.0.0.1", 9999);
            var msg = client.SayHello("Hello");
            Console.WriteLine(msg);
            Console.ReadLine();
        }
    }
}
