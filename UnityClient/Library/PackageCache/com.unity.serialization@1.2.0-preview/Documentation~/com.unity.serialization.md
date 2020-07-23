# Introduction to Unity Serialization

Unity Serialization is a general purpose serialization library written entirely in C#. It currently supports JSON and Binary formats.

Serialization makes use of the `Unity.Properties` package to efficiently traverse data containers at runtime in order to serialize and deserialize data. 

## Describing types for serialization

This section gives a brief overview of how to use Unity.Properties to describe types for serialization. 

_Note:  This is not a comprehensive guide on properties but rather what you need to know to use it for serialization. If you want to learn more about properties check out the package documentation._

In order for the serialization package to traverse types they must first be described using properties. This can be done in one of two ways:

**Reflection** - By including the `Unity.Properties.Reflection` assembly in your project. This will allow properties to lazily evaluate types at runtime. **IMPORTANT: This is unavailable on AOT platforms.**

**CodeGen** - By instrumenting your types with `Unity.Properties.GeneratePropertyBagAttribute`. This will be consumed by a PostProcessor to generate the necessary code for visitation. _Note: CodeGen is only triggered in player builds._

By default _public field members_ and members with the `Unity.Properties.CreatePropertyAttribute` are serialized.

```csharp
[GeneratePropertyBag]
struct Data
{
    public int IntField;
    
    [CreateProperty]
    int m_Value;
}
```

### Additional attributes

* `[Unity.Serialization.FormerNameAttribute]` and `[UnityEngine.Serialization.FormerlySerializedAsAttribute]` These attributes can be used to perform basic migration. The former can be used on _fields_, _net properties_ and _types_ while the latter will only work on _fields_. If you need more complex migration see the section on **Implementing Migration for a Type**.

* `[Unity.Serialization.DontSerializeAttribute]` and `[System.NonSerializedAttribute]` This can be used to have the serialization system ignore a specific member. The former works on _fields_ and _net properties_ while the latter will only work on _fields_. This only affects serialization and **not** deserialization.

* `[DontCreateProperty]` This can be used to have properties ignore a specific member all together.  This will affect serialization **and** deserialization. _Note: When using this attribute it affects all systems using properties and not just serialization. This can affect other systems which rely on properties (e.g. Inspector UI)._

## JSON 

The package provides both a high level and low level API for serialization. If you are just looking to quickly get started on saving and loading some data see the **Getting Started** section.

If you are looking for a high performance streaming solution see the **Low Level API** section.

### Getting Started

The simplest method of converting between JSON and .NET objects is using the `JsonSerialization` API.

```csharp
class Item 
{
    public string Name;
    public ItemType Type;
}

class Player 
{
    public string Name;
    public int Health;
    public int2 Position;
    public Item[] Inventory;
}

var player = new Player 
{  
    Name = "Bob",  
    Health = 100,  
    Position = new int2(10, 20),  
    Inventory = new[]  
    {  
        new Item {Name = "Sword", Type = ItemType.Weapon},  
        new Item {Name = "Shield", Type = ItemType.Armor},  
        new Item {Name = "Health Potion", Type = ItemType.Consumable}  
    }  
};

var json = JsonSerialization.ToJson(player);

/*
OUTPUT:
{
    "Name": "Bob",
    "Health": 100,
    "Position": {
        "x": 10,
        "y": 20
    },
    "Inventory": [
        {
            "Name": "Sword",
            "Type": 0
        },
        {
            "Name": "Shield"
            "Type": 1,
        },
        {
            "Name": "Health Potion"
            "Type": 2
        }
    ]
}
*/

var deserializedPlayer = JsonSerialization.FromJson<Player>(json);
```

### Implementing Adapters for a Type

The serialization system can be extend through the use of adapters. An adapter lets you specify how a type should be serialized and deserialized.

```csharp
// e.g. We have a manager for items in our game. It handles saving and loading from a database.
// We instead want to write out just the `Id` of the item we have.

class ItemAdapter : IJsonAdapter<Item>  
{  
    void IJsonAdapter<Item>.Serialize(JsonStringBuffer writer, Item value) 
        => writer.Write(ItemManager.GetItemIdFromName(value.Name)); 
  
    Item IJsonAdapter<Item>.Deserialize(SerializedValueView view)  
        => ItemManager.CreateItemFromId(view.AsInt32());  
}

/*
OUTPUT:
{
    "Name": "Bob",
    "Health": 100,
    "Position": {
        "x": 10,
        "y": 20
    },
    "Inventory": [
        43,
        102,
        98
    ]
}
*/
```

_Note: Unity.Serialization also supports **contravariant** adapters. See `Unity.Serialization.Adapter.Contravariant.IJsonAdapter<T>`_

Adapters can be registered in one of two ways:

1) **Global Adapter** - This will be used by all calls to serialization and deserialization. Use this if you fully own the types and the adapter is stateless.
```csharp
JsonSerialization.AddGlobalAdapter(new ItemAdapter());
```

2) **User Defined Adapter** - This can be used on a per call basis. Use this if the adapter has state or the type needs to be handled differently depending on context.
```csharp
var parameters = new JsonSerializationParameters  
{  
    UserDefinedAdapters = new List<IJsonAdapter> { new ItemAdapter() }  
};

var json = JsonSerialization.ToJson(player, parameters);
```

### Implementing Migration for a Type

The serialization system supports migration and version through an API similar to the adapters.

In order to implement migration for a given type, an implementation of `IJsonMigration<T>`  must be provided.

```csharp
class PlayerMigration : IJsonMigration<Player> 
{
    int IJsonMigration<Player>.Version => 2;
    Player IJsonMigration<Player>.Migrate(JsonMigrationContext ctx) { ... }
}
```

This interface requires implementations for:

*   `Version`  - The current version for the type. This is used by both serialization to write the version, and by deserialization to determine if migration should take place.
*   `T Migrate(JsonMigrationContext ctx)`  - The entry point for the actual migration. This method is called **if and only if** the versions do not match. The serialization system passes a context object which can be used to help read data from the underlying stream.

The  `JsonMigrationContext`  gives the user access to:

*   `ctx.SerializedVersion`  - The serialized version reported by the stream. This can be compared against the existing version to determine how to migrate.
*   `ctx.SerializedObject`  - The view over the underlying stream. this can be used to directly access data from the stream.
*   `ctx.Read<T>(SerializedValueView view)`  - This method and its overloads can be used to unpack full structures from the underlying stream. The read method will invoke the correct adapters and migrations on nested types. This is the preferred API over the direct access.

E.g. Defining migration for a type.
```csharp
class PlayerMigration : IJsonMigration<Player> 
{
    int IJsonMigration<Player>.Version => 1;

    Player IJsonMigration<Player>.Migrate(JsonMigrationContext ctx)
    {
        var serializedVersion = ctx.SerializedVersion;
        var serializedObject = ctx.SerializedObject;

        // Construct and initialize and new player object from the stream. This will copy in all fields that match.
        var player = ctx.Read<Player>(serializedObject);

        if (serializedVersion == 0)
        {
            // In the original version 0 we had the position stored as 'x' and 'y' directly on the player.
            player.Position = new int2(serializedObject["x"].AsInt32(), serializedObject["y"].AsInt32())
        }
       
        return player;
    }
}
```

_Note: Migration supports **covariance** by default._

## Low Level API

The low level API can be used to stream very large input streams with no allocations. Currently we only support a low level API for deserialization. 

This API can be used to customize the deserialization itself. The views returned by the reader are also blittable and useable in `Unity.Jobs` and `Unity.Burst`. 

### Design

When it comes to deserializing Json data there are two main approaches:

#### DOM (Document Object Model)
This gives a very user friendly and easy to use API over deserialized data. It allows users to freely walk the data tree. On the other hand it requires that the entire deserialized object must live in memory which means we can't stream. This is what the high level API provides.

#### SAX (Simple API over XML) / Forward-Only Reader
This is a very performant way of deserializing. It gives us very low allocations, only the currently depth must remain in memory and on the stack. On the other hand this is a much harder to use API since it pushes more work on the user.

The low level API supports both approaches and allows mixing the two together.

###  API Usage

**Forward-Only Reading** - This example shows how to use the reader to walk over the stream. The reader itself takes a `SerializedObjectReaderConfiguration` structure which allows to define the internal buffer sizes.

E.g. This example will step through the stream and read all primitive values as a signed integral.
```csharp
using (var reader = new SerializedObjectReader(stream))  
{  
    NodeType node = reader.Step(out SerializedValueView current);  
  
    switch (node)   
    {  
        case NodeType.BeginObject:  
        case NodeType.EndObject:  
            break;  
        case NodeType.Primitive:  
            var value = current.AsInt64();  
            break;  
    }  
}
```

**Document Object Model Reading** - This example shows how to read en entire object into memory as a view and interpet the values.

E.g. This example will an object and unpack all fields.
```csharp
INPUT:
/*
{
    "a": 10,
    "b": "hello",
    "c": { "x": 0, "y": 0 }
}
*/
using (var reader = new SerializedObjectReader(stream))
{
    SerializedObjectView obj = reader.ReadObject();

    var a = obj["a"].AsInt64();
    var b = obj["b"].AsStringView();
        
    var position = obj["c"].AsObjectView();
    var x = position["x"].AsInt64();
    var y = position["y"].AsInt64();
}
```

**Mixed Reading** - This example shows step through a stream until a certain point and read an object scope.

E.g. Example of streaming through large arrays of objects and reading one object at a time.
```csharp
/*
INPUT:
[{
    "Id": "{GUID}",
    // ...
},
{
    "Id": "{GUID}",
    // ...
}]
*/
using (var reader = new SerializedObjectReader(stream))
{
    if (reader.Step() != NodeType.BeginArray) {
        // error
    }

    while (reader.ReadArrayElement(out var element)) 
    {
        var view = element.AsObjectView();
        SerializedStringView id = view["Id"].AsStringView();
        // ...
        reader.DiscardCompleted();
    }

    if (reader.Step() != NodeType.EndArray) {
        // error
    }
}
```

E.g. Example of streaming through large arrays of objects and reading 100 objects at a time in batch.
```csharp
using (var reader = new SerializedObjectReader(stream))
{
    if (reader.Step() != NodeType.BeginArray) {
        // error
    }

    var batch = stackalloc SerializedValueView[100];

    while (true)
    {
        // Try to read batches of 100 elements.
        var count = reader.ReadArrayElementBatch(batch, 100);

        if (count == 0) break;

        for (var i = 0; i < count; i++)
        {
            var view = batch[i].AsObjectView();
            SerializedStringView id = view["Id"].AsStringView();
            // ...
        }
        
        reader.DiscardCompleted();
    }

    if (reader.Step() != NodeType.EndArray) {
        // error
    }
}
```

# Binary
Documentation coming soon. 
