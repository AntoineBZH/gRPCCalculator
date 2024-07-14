using Grpc.Core;
using GrpcCalculator;

namespace Server
{
    internal class CalculatorServiceServer : CalculatorService.CalculatorServiceBase
    {
        #region Implementation of the gRPC server
        public override Task<SumReply> Sum(SumRequest request, ServerCallContext context)
        {
            Console.WriteLine($"The server received a Sum request with the following parameters: {request}");
            return Task.FromResult(new SumReply() { Result = request.FirstNumber + request.SecondNumber });
        }

        public override async Task PrimeNumberDecompose(PrimeNumberDecomposeRequest request, IServerStreamWriter<PrimeNumberDecomposeReply> responseStream, ServerCallContext context)
        {
            Console.WriteLine($"The server received a PrimeNumberDecompose request with the following parameters: {request}");
            int currentNumber = request.Number;
            int factor = 2;

            while (currentNumber > 1)
            {
                if (currentNumber % factor == 0)
                {
                    await responseStream.WriteAsync(new PrimeNumberDecomposeReply() { PrimeFactor = factor });
                    currentNumber /= factor;
                }
                else
                {
                    factor++;
                }
            }
        }

        public override async Task<ComputeAverageReply> ComputeAverage(IAsyncStreamReader<ComputeAverageRequest> requestStream, ServerCallContext context)
        {
            Console.Write("The server received a ComputeAverage request with the following parameters: ");
            int sum = 0;
            int number = 0;
            while (await requestStream.MoveNext()) {
                Console.Write($"{requestStream.Current.Number} ");
                sum += requestStream.Current.Number;
                number++;
            }
            Console.Write(Environment.NewLine);
            return await Task.FromResult(new ComputeAverageReply() { Average = (double)sum / (double)number });
        }
        #endregion
    }
}
