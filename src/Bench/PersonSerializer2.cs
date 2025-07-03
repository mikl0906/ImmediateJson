namespace Bench;

public static class PersonSerializer2
{
    private static readonly ThreadLocal<char[]> BufferPool = new(() => new char[512]);

    public static string Serialize(Person person)
    {
        var buffer = BufferPool.Value!;
        var span = buffer.AsSpan();

        int written = WriteJson(person, span);

        // If buffer was too small, WriteJson returns -1
        if (written == -1)
        {
            // Estimate required size and allocate larger buffer
            int estimatedSize = EstimateSize(person);
            buffer = new char[estimatedSize];
            span = buffer.AsSpan();
            written = WriteJson(person, span);

            // Update thread-local buffer if this size is reasonable
            if (estimatedSize <= 8192)
            {
                BufferPool.Value = buffer;
            }
        }

        return new string(span[..written]);
    }

    private static int EstimateSize(Person person)
    {
        // Rough estimation: base object + friends
        int baseSize = 100; // Base JSON structure
        baseSize += person.Name.Length * 2; // Name with quotes
        baseSize += person.Address.Street.Length * 2;
        baseSize += person.Address.City.Length * 2;
        baseSize += person.Friends.Count * 50; // Approximate per friend

        foreach (var friend in person.Friends)
        {
            baseSize += friend.Name.Length * 2;
        }

        return Math.Max(baseSize * 2, 4096); // Double for safety, min 4KB
    }

    private static int WriteJson(Person person, Span<char> buffer)
    {
        int pos = 0;

        // Check if we have enough space before writing
        if (!TryWriteChar(buffer, ref pos, '{')) return -1;

        if (!TryWriteProperty(buffer, ref pos, "Name", person.Name)) return -1;
        if (!TryWriteChar(buffer, ref pos, ',')) return -1;
        if (!TryWriteProperty(buffer, ref pos, "Age", person.Age)) return -1;
        if (!TryWriteChar(buffer, ref pos, ',')) return -1;

        // Write address
        if (!TryWriteString(buffer, ref pos, "Address")) return -1;
        if (!TryWriteChar(buffer, ref pos, ':')) return -1;
        if (!TryWriteChar(buffer, ref pos, '{')) return -1;
        if (!TryWriteProperty(buffer, ref pos, "Street", person.Address.Street)) return -1;
        if (!TryWriteChar(buffer, ref pos, ',')) return -1;
        if (!TryWriteProperty(buffer, ref pos, "City", person.Address.City)) return -1;
        if (!TryWriteChar(buffer, ref pos, '}')) return -1;
        if (!TryWriteChar(buffer, ref pos, ',')) return -1;

        // Write friends array
        if (!TryWriteString(buffer, ref pos, "Friends")) return -1;
        if (!TryWriteChar(buffer, ref pos, ':')) return -1;
        if (!TryWriteChar(buffer, ref pos, '[')) return -1;

        for (int i = 0; i < person.Friends.Count; i++)
        {
            var friend = person.Friends[i];
            if (!TryWriteChar(buffer, ref pos, '{')) return -1;
            if (!TryWriteProperty(buffer, ref pos, "Name", friend.Name)) return -1;
            if (!TryWriteChar(buffer, ref pos, ',')) return -1;
            if (!TryWriteProperty(buffer, ref pos, "Age", friend.Age)) return -1;
            if (!TryWriteChar(buffer, ref pos, '}')) return -1;

            if (i < person.Friends.Count - 1)
                if (!TryWriteChar(buffer, ref pos, ',')) return -1;
        }

        if (!TryWriteChar(buffer, ref pos, ']')) return -1;
        if (!TryWriteChar(buffer, ref pos, '}')) return -1;

        return pos;
    }

    private static bool TryWriteProperty(Span<char> buffer, ref int pos, string name, string value)
    {
        return TryWriteString(buffer, ref pos, name) &&
               TryWriteChar(buffer, ref pos, ':') &&
               TryWriteString(buffer, ref pos, value);
    }

    private static bool TryWriteProperty(Span<char> buffer, ref int pos, string name, int value)
    {
        return TryWriteString(buffer, ref pos, name) &&
               TryWriteChar(buffer, ref pos, ':') &&
               TryWriteInt(buffer, ref pos, value);
    }

    private static bool TryWriteChar(Span<char> buffer, ref int pos, char c)
    {
        if (pos >= buffer.Length) return false;
        buffer[pos++] = c;
        return true;
    }

    private static bool TryWriteString(Span<char> buffer, ref int pos, string value)
    {
        int required = value.Length + 2; // quotes
        if (pos + required > buffer.Length) return false;

        buffer[pos++] = '"';
        value.AsSpan().CopyTo(buffer[pos..]);
        pos += value.Length;
        buffer[pos++] = '"';
        return true;
    }

    private static bool TryWriteInt(Span<char> buffer, ref int pos, int value)
    {
        if (value.TryFormat(buffer[pos..], out int written))
        {
            pos += written;
            return true;
        }
        return false;
    }
}