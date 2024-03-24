using Logger;
using Logger.Abstracts;

namespace LoggerTests
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class Tests : PageTest
    {
        [Test]
        public async Task DebugAsync_LogToFileFromTenThreads_CorrectLogging()
        {
            var logger = new LoggerFabric()
                .SetFile("logs.log")
                .SetLogLevel(LogLevel.Debug)
                .Build();

            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    var procces = new Thread(async () => await Procces(logger));
                    procces.Start();
                }
            });
        }

        private async Task Procces(ILogger logger)
        {
            for (int i = 0; i < 100; i++)
                await logger.DebugAsync($"thread: {Thread.CurrentThread.ManagedThreadId} | i: {i}");
        }
    }
}
