using ImmediateJson;

namespace Bench;

[GenerateImmediateJsonSerializer]
public partial class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public Address Address { get; set; } = new Address();
    public List<Person> Friends { get; set; } = [];
}

[GenerateImmediateJsonSerializer]
public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}