using Grpc.Core;
using GrpcCalculator;

namespace Server
{
    internal class Program
    {
        private const string GRPC_HOST = "localhost";
        private const int GRPC_PORT = 8800;

        static void Main(string[] args)
        {
            Console.Title = "gRPC server";

            Grpc.Core.Server? server = null;
            string serverCertificate = File.ReadAllText(Path.Combine(".", "ssl", "server.crt"));
            string serverKey = File.ReadAllText(Path.Combine(".", "ssl", "server.key"));
            string caCertificate = File.ReadAllText(Path.Combine(".", "ssl", "ca.crt"));

            try
            {
                server = new Grpc.Core.Server()
                {
                    Ports = { new ServerPort(GRPC_HOST, GRPC_PORT, new SslServerCredentials([new KeyCertificatePair(serverCertificate, serverKey)], caCertificate, true)) },
                    Services = { CalculatorService.BindService(new CalculatorServiceServer()) }
                };
                server.Start();
                Console.WriteLine($"The server is now listening {GRPC_HOST}:{GRPC_PORT}.");
                Console.WriteLine("Press any key to stop the server.");
                Console.WriteLine();
                Console.ReadKey();
            }
            catch (IOException ex)
            {
                Console.WriteLine($"An error occured during the server initialisation: {ex.Message}");
            }
            finally
            {
                server?.ShutdownAsync().Wait();
            }
        }
    }
}