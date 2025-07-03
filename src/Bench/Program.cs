using System.Text;
using System.Text.Json;

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

    private TextWriter TextWriter => new StringWriter();

    [Benchmark]
    public void ImmediateSerializationWithWriter()
    {
        PersonSerializer.Serialize(_person, TextWriter);
    }

    [Benchmark]
    public string ImmediateSerialization()
    {
        return PersonSerializer.Serialize(_person);
    }

    [Benchmark]
    public string SystemTextSerialization()
    {
        return JsonSerializer.Serialize(_person);
    }
}

public static class PersonSerializer
{
    private static readonly ThreadLocal<char[]> BufferPool = new(() => new char[2048]);

    public static string Serialize(Person person)
    {
        var buffer = BufferPool.Value!;
        var span = buffer.AsSpan();

        int written = WriteJson(person, span);

        // Only allocation: creating the result string
        return new string(span[..written]);
    }

    public static void Serialize(Person person, TextWriter writer)
    {
        // Write directly to stream without intermediate allocations
        var buffer = BufferPool.Value!;
        var span = buffer.AsSpan();

        int written = WriteJson(person, span);

        // Write the result directly to the TextWriter
        writer.Write(span[..written]);
    }

    private static int WriteJson(Person person, Span<char> buffer)
    {
        int pos = 0;

        // Write person object
        buffer[pos++] = '{';
        pos += WriteProperty(buffer[pos..], "Name", person.Name);
        buffer[pos++] = ',';
        pos += WriteProperty(buffer[pos..], "Age", person.Age);
        buffer[pos++] = ',';

        // Write address
        pos += WriteString(buffer[pos..], "Address");
        buffer[pos++] = ':';
        buffer[pos++] = '{';
        pos += WriteProperty(buffer[pos..], "Street", person.Address.Street);
        buffer[pos++] = ',';
        pos += WriteProperty(buffer[pos..], "City", person.Address.City);
        buffer[pos++] = '}';
        buffer[pos++] = ',';

        // Write friends array
        pos += WriteString(buffer[pos..], "Friends");
        buffer[pos++] = ':';
        buffer[pos++] = '[';

        for (int i = 0; i < person.Friends.Count; i++)
        {
            var friend = person.Friends[i];
            buffer[pos++] = '{';
            pos += WriteProperty(buffer[pos..], "Name", friend.Name);
            buffer[pos++] = ',';
            pos += WriteProperty(buffer[pos..], "Age", friend.Age);
            buffer[pos++] = '}';

            if (i < person.Friends.Count - 1)
                buffer[pos++] = ',';
        }

        buffer[pos++] = ']';
        buffer[pos++] = '}';

        return pos;
    }

    private static int WriteProperty(Span<char> buffer, string name, string value)
    {
        int pos = 0;
        pos += WriteString(buffer[pos..], name);
        buffer[pos++] = ':';
        pos += WriteString(buffer[pos..], value);
        return pos;
    }

    private static int WriteProperty(Span<char> buffer, string name, int value)
    {
        int pos = 0;
        pos += WriteString(buffer[pos..], name);
        buffer[pos++] = ':';
        pos += WriteInt(buffer[pos..], value);
        return pos;
    }

    private static int WriteString(Span<char> buffer, string value)
    {
        buffer[0] = '"';
        value.AsSpan().CopyTo(buffer[1..]);
        buffer[value.Length + 1] = '"';
        return value.Length + 2;
    }

    private static int WriteInt(Span<char> buffer, int value)
    {
        return value.TryFormat(buffer, out int written) ? written : 0;
    }
}

public class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public Address Address { get; set; } = new Address();
    public List<Person> Friends { get; set; } = [];
}

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}