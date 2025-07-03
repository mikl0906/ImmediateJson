namespace Bench;

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
