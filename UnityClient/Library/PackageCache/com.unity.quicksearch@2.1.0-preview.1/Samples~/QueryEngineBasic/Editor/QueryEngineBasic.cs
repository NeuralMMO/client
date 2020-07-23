// -----------------------------------------------------------------------------
//
// Basic QueryEngine example. This example should cover most of your needs for
// searching a data set. If you need more customization, you can look at the other
// QueryEngine examples.
//
// -----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Unity.QuickSearch;
using UnityEngine.Assertions;

/// <summary>
/// Example type
/// </summary>
public class MyObjectType
{
    /// <summary>Id</summary>
    public int Id { get; set; }
    /// <summary>Name</summary>
    public string Name { get; set; }
    /// <summary>Position</summary>
    public Vector2 Position { get; set; }
    /// <summary>Active</summary>
    public bool Active { get; set; }

    /// <summary>
    /// Create a new object
    /// </summary>
    public MyObjectType()
    {
        Id = 0;
        Name = "";
        Position = Vector2.zero;
        Active = false;
    }

    /// <summary>
    /// Returns string representation.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"({Id}, {Name}, ({Position.x}, {Position.y}), {Active})";
    }
}

/// <summary>
/// Sample QueryEngine able to perform simple query on MyObjectType
/// </summary>
public class QueryEngineBasic
{
    QueryEngine<MyObjectType> m_QueryEngine;
    List<MyObjectType> m_Data;

    /// <summary>
    /// Create a sample QueryEngine able to perform simple query on MyObjectType
    /// </summary>
    public QueryEngineBasic()
    {
        GenerateData(1000);

        // Setup the query engine
        m_QueryEngine = new QueryEngine<MyObjectType>();
        // Id supports all operators
        m_QueryEngine.AddFilter("id", myObj => myObj.Id);
        // Name supports only contains (:), equal (=) and not equal (!=)
        m_QueryEngine.AddFilter("n", myObj => myObj.Name, new []{":", "=", "!="});
        // Active supports only equal and not equal
        m_QueryEngine.AddFilter("a", myObj => myObj.Active, new []{"=", "!="});
        // The magnitude support equal, not equal, lesser, greater, lesser or equal and greater or equal.
        m_QueryEngine.AddFilter("m", myObj => myObj.Position.magnitude, new []{"=", "!=", "<", ">", "<=", ">="});

        // Add a function "dist" to filter item that are at a certain distance from a position or another item
        m_QueryEngine.AddFilter("dist", HandleDistFilter, HandlerDistParameter, new[] { "=", "!=", "<", ">", "<=", ">=" });

        // Setup what data will be matched against search words
        m_QueryEngine.SetSearchDataCallback(myObj => new []{myObj.Id.ToString(), myObj.Name});
    }

    /// <summary>
    /// Test function that runs the Query engine on a list of queries.
    /// </summary>
    public void TestFiltering()
    {
        void PrintFilteredData(IEnumerable<MyObjectType> data)
        {
            foreach (var obj in data)
            {
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, obj.ToString());
            }
        }

        // Find objects that have an id > 100 and are active
        var filteredData = FilterData("id>100 a=true");
        PrintFilteredData(filteredData);

        // Find objects that are not active or have a name that contains Camera
        filteredData = FilterData("a=false or n:Camera");
        PrintFilteredData(filteredData);

        // Find objects that are near "Mesh 28" and active, or contains 42 in their id or name
        filteredData = FilterData("(dist(Mesh 28)<5 a=true) or 42");
        PrintFilteredData(filteredData);
    }

    /// <summary>
    /// Filter data according to a specific inputQuery.
    /// </summary>
    /// <param name="inputQuery">Text query</param>
    /// <returns>Yields a list of matching MyObjectType.</returns>
    public IEnumerable<MyObjectType> FilterData(string inputQuery)
    {
        // Parse the query string into a query operation
        var query = m_QueryEngine.Parse(inputQuery);

        // If the query is not valid, print all errors and return an empty data set
        if (!query.valid)
        {
            foreach (var queryError in query.errors)
            {
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, $"Error parsing input at {queryError.index}: {queryError.reason}");
            }

            return new List<MyObjectType>();
        }

        var filteredData = query.Apply(m_Data);
        return filteredData;
    }

    static float HandleDistFilter(MyObjectType myObj, Vector2 param)
    {
        var vec = myObj.Position - param;
        return vec.magnitude;
    }

    Vector2 HandlerDistParameter(string filterValue)
    {
        // If the user specified a vector
        if (filterValue.StartsWith("[") && filterValue.EndsWith("]"))
        {
            filterValue = filterValue.Trim('[', ']');
            var vectorTokens = filterValue.Split(',');
            var vectorValues = vectorTokens.Select(token => float.Parse(token, CultureInfo.InvariantCulture.NumberFormat)).ToList();
            Assert.AreEqual(vectorValues.Count, 2);
            return new Vector2(vectorValues[0], vectorValues[1]);
        }

        // Treat the value as the name of an object
        var myObj = m_Data.Find(obj => obj.Name == filterValue);
        Assert.IsNotNull(myObj);
        return myObj.Position;
    }

    void GenerateData(int size)
    {
        m_Data = new List<MyObjectType>(size);
        for (var i = 0; i < size; ++i)
        {
            var posX = Random.Range(0, 100);
            var posY = Random.Range(0, 100);
            string name;
            switch (i % 3)
            {
                case 0: name = $"Material {i}";
                    break;
                case 1: name = $"Mesh {i}";
                    break;
                case 3: name = $"Camera {i}";
                    break;
                default: name = $"Object {i}";
                    break;
            }
            var newObj = new MyObjectType { Id = i, Name = name, Position = new Vector2(posX, posY), Active = i % 2 == 0 };
            m_Data.Add(newObj);
        }
    }
}
