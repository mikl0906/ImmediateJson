using System.Text;
using System.Text.Json;

using Bench;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

// var p = new Person
// {
//     Name = "John Doe",
//     Age = 30,
//     Address = new Address
//     {
//         Street = "123 Main St",
//         City = "Anytown"
//     },
//     Friends =
//             [
//                 new() { Name = "Jane Smith", Age = 28 },
//                 new() { Name = "Alice Brown", Age = 32 },
//                 new() { Name = "Charlie Davis", Age = 40 },
//                 new() { Name = "Eve White", Age = 45 },
//                 new() { Name = "Frank Green", Age = 50 },
//                 new() { Name = "Grace Lee", Age = 29 },
//                 new() { Name = "Hank Miller", Age = 22 },
//                 new() { Name = "Ivy Clark", Age = 27 },
//                 new() { Name = "Jack Wilson", Age = 31 },
//                 new() { Name = "Bob Johnson", Age = 35 },
//                 new() { Name = "Alice Brown", Age = 32 },
//                 new() { Name = "Charlie Davis", Age = 40 },
//                 new() { Name = "Eve White", Age = 45 },
//                 new() { Name = "Frank Green", Age = 50 },
//                 new() { Name = "Grace Lee", Age = 29 },
//                 new() { Name = "Hank Miller", Age = 22 },
//                 new() { Name = "Ivy Clark", Age = 27 },
//                 new() { Name = "Jack Wilson", Age = 31 },
//                 new() { Name = "Bob Johnson", Age = 35 },
//                 new() { Name = "Alice Brown", Age = 32 },
//                 new() { Name = "Charlie Davis", Age = 40 },
//                 new() { Name = "Eve White", Age = 45 },
//                 new() { Name = "Frank Green", Age = 50 },
//                 new() { Name = "Grace Lee", Age = 29 },
//                 new() { Name = "Hank Miller", Age = 22 },
//                 new() { Name = "Ivy Clark", Age = 27 },
//                 new() { Name = "Jack Wilson", Age = 31 },
//                 new() { Name = "Bob Johnson", Age = 35 },
//                 new() { Name = "Bob Johnson", Age = 35 }
//             ]
// };
// Console.WriteLine(PersonSerializer.Serialize(p));

BenchmarkRunner.Run<SerializationBenchmark>();

[MemoryDiagnoser]
public class SerializationBenchmark
{
    private Person _person = null!;

    [GlobalSetup]
    public void Setup()
    {
        _person = new Person
        {
            Name = "John Doe",
            Age = 30,
            Address = new Address
            {
                Street = "123 Main St",
                City = "Anytown"
            },
            Friends =
            [
                new() { Name = "Jane Smith", Age = 28 },
                new() { Name = "Alice Brown", Age = 32 },
                new() { Name = "Charlie Davis", Age = 40 },
                new() { Name = "Eve White", Age = 45 },
                new() { Name = "Frank Green", Age = 50 },
                new() { Name = "Grace Lee", Age = 29 },
                new() { Name = "Hank Miller", Age = 22 },
                new() { Name = "Ivy Clark", Age = 27 },
                new() { Name = "Jack Wilson", Age = 31 },
                new() { Name = "Bob Johnson", Age = 35 },
                new() { Name = "Alice Brown", Age = 32 },
                new() { Name = "Charlie Davis", Age = 40 },
                new() { Name = "Eve White", Age = 45 },
                new() { Name = "Frank Green", Age = 50 },
                new() { Name = "Grace Lee", Age = 29 },
                new() { Name = "Hank Miller", Age = 22 },
                new() { Name = "Ivy Clark", Age = 27 },
                new() { Name = "Jack Wilson", Age = 31 },
                new() { Name = "Bob Johnson", Age = 35 },
                new() { Name = "Alice Brown", Age = 32 },
                new() { Name = "Charlie Davis", Age = 40 },
                new() { Name = "Eve White", Age = 45 },
                new() { Name = "Frank Green", Age = 50 },
                new() { Name = "Grace Lee", Age = 29 },
                new() { Name = "Hank Miller", Age = 22 },
                new() { Name = "Ivy Clark", Age = 27 },
                new() { Name = "Jack Wilson", Age = 31 },
                new() { Name = "Bob Johnson", Age = 35 },
                new() { Name = "Bob Johnson", Age = 35 }
            ]
        };
    }

    [Benchmark]
    public string ImmediateSerialization()
    {
        return PersonSerializer2.Serialize(_person);
    }

    [Benchmark]
    public string SystemTextSerialization()
    {
        return JsonSerializer.Serialize(_person);
    }
}
