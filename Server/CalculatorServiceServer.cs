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

            if (currentNumber < 1)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "The argument must be positive."));

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

        public override async Task FindMaximum(IAsyncStreamReader<FindMaximumRequest> requestStream, IServerStreamWriter<FindMaximumReply> responseStream, ServerCallContext context)
        {
            Console.Write("The server received a FindMaximum request with the following parameters: ");
            int currentMaximum = int.MinValue;
            while (await requestStream.MoveNext())
            {
                Console.Write($"{requestStream.Current.Number} ");
                if (currentMaximum < requestStream.Current.Number)
                {
                    currentMaximum = requestStream.Current.Number;
                    await responseStream.WriteAsync(new FindMaximumReply() { Maximum = currentMaximum });
                }
            }
            Console.Write(Environment.NewLine);
        }
        #endregion
    }
}
