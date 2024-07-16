using Grpc.Core;
using GrpcCalculator;

namespace Server
{
    internal class Program
    {
        private const string GRPC_ADDRESS = "localhost";
        private const int GRPC_PORT = 8800;

        static async Task Main(string[] args)
        {
            Console.Title = "gRPC client";

            string clientCertificate = File.ReadAllText(Path.Combine(".", "ssl", "client.crt"));
            string clientKey = File.ReadAllText(Path.Combine(".", "ssl", "client.key"));
            string caCertificate = File.ReadAllText(Path.Combine(".", "ssl", "ca.crt"));

            Channel? channel = new Channel(GRPC_ADDRESS, GRPC_PORT, new SslCredentials(caCertificate, new KeyCertificatePair(clientCertificate, clientKey)));
            await channel.ConnectAsync(deadline: DateTime.UtcNow.AddMinutes(2)).ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                {
                    Console.WriteLine($"Connection successful with the server {GRPC_ADDRESS}:{GRPC_PORT}.");
                }
            });
            CalculatorService.CalculatorServiceClient client = new CalculatorService.CalculatorServiceClient(channel);

            try
            {
                Sum(10, 5, client);
                await PrimeNumberDecompose(120, client);
                await ComputeAverage(client, Enumerable.Range(1, 4).ToArray());
                await FindMaximum(client, 1, 5, 3, 6, 2, 20);
            }
            catch (RpcException ex)
            {
                Console.WriteLine(ex.Status.Detail);
            }

            await channel.ShutdownAsync();
        }

        #region Private methods
        /// <summary>
        /// Implements a gRPC unitary API to sum two number.
        /// </summary>
        /// <param name="firstNumber">First number to sum.</param>
        /// <param name="secondNumber">Second number to sum.param>
        /// <param name="client">gRPC client for the calculator service.</param>
        static private void Sum(int firstNumber, int secondNumber, CalculatorService.CalculatorServiceClient client)
        {
            Console.WriteLine($"The result of the sum {firstNumber} + {secondNumber} is: " + client.Sum(new SumRequest() { FirstNumber = firstNumber, SecondNumber = secondNumber }).Result);
        }

        /// <summary>
        /// Implements an gRPC server streaming API to get the prime number decomposition.
        /// </summary>
        /// <param name="number">Number for which to obtain the decomposition into prime numbers.</param>
        /// <param name="client">gRPC client for the calculator service.</param>
        /// <returns>The asynchronous task to get the prime number decomposition.</returns>
        static private async Task PrimeNumberDecompose(int number, CalculatorService.CalculatorServiceClient client)
        {
            Console.Write($"The decomposition of {number} in prime number is: ");
            var response = client.PrimeNumberDecompose(new PrimeNumberDecomposeRequest() { Number = number });
            bool isFirstResponse = true;
            while (await response.ResponseStream.MoveNext())
            {
                Console.Write($"{(isFirstResponse ? "" : "*")}{response.ResponseStream.Current.PrimeFactor}");
                isFirstResponse = false;
            }
            Console.Write(Environment.NewLine);
        }

        /// <summary>
        /// Implements an gRPC client streaming API to get the average of numbers.
        /// </summary>
        /// <param name="client">gRPC client for the calculator service.</param>
        /// <param name="numbers">Numbers for which to obtain the average.</param>
        /// <returns>The asynchronous task to get the average.</returns>
        static private async Task ComputeAverage(CalculatorService.CalculatorServiceClient client, params int[] numbers)
        {
            Console.Write("The average of");
            var stream = client.ComputeAverage();
            foreach (int number in numbers)
            {
                Console.Write($" {number}");
                await stream.RequestStream.WriteAsync(new ComputeAverageRequest() { Number = number });
            }
            await stream.RequestStream.CompleteAsync();
            Console.Write($" is {stream.ResponseAsync.Result.Average}.");

            Console.Write(Environment.NewLine);
        }

        /// <summary>
        /// Implements an gRPC bidirectional streaming API to get the maximum of numbers.
        /// </summary>
        /// <param name="client">gRPC client for the calculator service.</param>
        /// <param name="numbers">Numbers for which to obtain the maximum.</param>
        /// <returns>The asynchronous task to get the maximum.</returns>
        static private async Task FindMaximum(CalculatorService.CalculatorServiceClient client, params int[] numbers)
        {
            var stream = client.FindMaximum();
            Task receivingTask = Task.Run(async () =>
            {
                while (await stream.ResponseStream.MoveNext())
                {
                    Console.WriteLine($"The maximum found is {stream.ResponseStream.Current.Maximum}");
                }
            });

            foreach(int number in numbers)
            {
                await stream.RequestStream.WriteAsync(new FindMaximumRequest() { Number = number });
            }
            await stream.RequestStream.CompleteAsync();
            await receivingTask;
        }
        #endregion
    }
}