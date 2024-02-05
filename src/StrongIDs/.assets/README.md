## Usage

Strongly typed IDs can have the following access modifiers: 

*    protected internal
*    protected
*    internal
*    public

Define a strongly typed ID using *StrongIDs*. Ensure it is readonly, partial, and a record struct. For example:

```csharp
public readonly partial record struct UserId : IStrongId<UserId>;
```

This generates the following code, using the IEntityId<T> interface.

**```csharp**
public partial record struct UserId : IEntityId<UserId>
{
    public Guid Value { get; }

    public bool HasValue => Value != Guid.Empty;
    
    private UserId(Guid value) => Value = value;
    
    public static UserId Empty => new UserId(Guid.Empty);
    
    public static UserId New() => new UserId(Guid.NewGuid());

    public static UserId Parse(string? value) =>
        Guid.TryParse(value, out Guid id) ? new UserId(id) : Empty;
}
```

The IEntityId<T> interface requires the above generated methods and properties to be generated. 