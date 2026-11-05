using SynchronizationPrimitives;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

await new SemaphoreDemo().RunAsync();
Console.WriteLine();

await new ProducerConsumerDemo().RunAsync();
Console.WriteLine();

new MutexDemo().Run();
Console.WriteLine();

new InterlockedDemo().Run();
