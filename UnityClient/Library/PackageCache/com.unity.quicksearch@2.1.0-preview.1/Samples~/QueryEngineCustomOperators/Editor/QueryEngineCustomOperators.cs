// -----------------------------------------------------------------------------
//
// Custom QueryEngine example. This example shows how to customize the query
// engine for your specific needs. It shows how to add new operators and new
// type parsers.
//
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using UnityEngine;
using Unity.QuickSearch;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

/// <summary>
/// CustomObject type use to show how to use a custom QueryEngine.
/// </summary>
public class MyCustomObjectType
{
    /// <summary>Id</summary>
    public int Id { get; set; }
    /// <summary>Position</summary>
    public Vector2 Position { get; set; }

    /// <summary>
    ///  Create a new instance of MyCustomObjectType
    /// </summary>
    public MyCustomObjectType()
    {
        Id = 0;
        Position = Vector2.zero;
    }

    /// <summary>
    ///  Get string representation of MyCustomObjectType
    /// </summary>
    /// <returns>Returns a string representation of MyCustomObjectType</returns>
    public override string ToString()
    {
        return $"({Id}, ({Position.x}, {Position.y}))";
    }
}

/// <summary>
/// Sample QueryEngine able to perform simple query on MyObjectType
/// </summary>
public class QueryEngineCustomOperators
{
    QueryEngine<MyCustomObjectType> m_QueryEngine;
    List<MyCustomObjectType> m_Data;

    /// <summary>
    /// Create a query engine to showcase how custom operators work.
    /// </summary>
    public QueryEngineCustomOperators()
    {
        GenerateData(1000);

        // Setup the query engine
        m_QueryEngine = new QueryEngine<MyCustomObjectType>();
        // Id supports all operators
        m_QueryEngine.AddFilter("id", myObj => myObj.Id);

        // Setup what data will be matched against search words
        m_QueryEngine.SetSearchDataCallback(myObj => new[] { myObj.Id.ToString() });

        // Extend the set of operators and type parsers

        // Modulo operator will work on all filters that are integers
        const string moduloOp = "%";
        m_QueryEngine.AddOperator(moduloOp);
        m_QueryEngine.AddOperatorHandler(moduloOp, (int ev, int fv) => ev % fv == 0);

        // List operator will work on al filters that have an integer as left hand side and
        // a list of integers as right hand side
        var listOp = "?";
        m_QueryEngine.AddOperator(listOp);
        m_QueryEngine.AddOperatorHandler(listOp, (int ev, List<int> values) => values.Contains(ev));

        // To correctly parse this new type (because it is not supported by default), we add a type parser
        m_QueryEngine.AddTypeParser(s =>
        {
            var tokens = s.Split(',');
            if (tokens.Length == 0)
                return new ParseResult<List<int>>(false, null);

            var numberList = new List<int>(tokens.Length);
            foreach (var token in tokens)
            {
                if (TryConvertValue(token, out int number))
                {
                    numberList.Add(number);
                }
                else
                    return new ParseResult<List<int>>(false, null);
            }

            return new ParseResult<List<int>>(true, numberList);
        });

        // We an also extend existing operators

        // Position supports =, !=, <, >, <=, >=
        m_QueryEngine.AddFilter("p", myObj => myObj.Position, new[] { "=", "!=", "<", ">", "<=", ">=" });

        // Extend the =, !=, <, >, <=, >= operators to support comparing Vector2's magnitude
        m_QueryEngine.AddOperatorHandler("=", (Vector2 ev, Vector2 fv) => Math.Abs(ev.magnitude - fv.magnitude) < float.Epsilon);
        m_QueryEngine.AddOperatorHandler("!=", (Vector2 ev, Vector2 fv) => Math.Abs(ev.magnitude - fv.magnitude) > float.Epsilon);
        m_QueryEngine.AddOperatorHandler("<", (Vector2 ev, Vector2 fv) => ev.magnitude < fv.magnitude);
        m_QueryEngine.AddOperatorHandler(">", (Vector2 ev, Vector2 fv) => ev.magnitude > fv.magnitude);
        m_QueryEngine.AddOperatorHandler("<=", (Vector2 ev, Vector2 fv) => ev.magnitude <= fv.magnitude);
        m_QueryEngine.AddOperatorHandler(">=", (Vector2 ev, Vector2 fv) => ev.magnitude >= fv.magnitude);

        // Add a new type parser for Vector2
        m_QueryEngine.AddTypeParser(s =>
        {
            if (!s.StartsWith("[") || !s.EndsWith("]"))
                return new ParseResult<Vector2>(false, Vector2.zero);

            var trimmed = s.Trim('[', ']');
            var vectorTokens = trimmed.Split(',');
            var vectorValues = vectorTokens.Select(token => float.Parse(token, CultureInfo.InvariantCulture.NumberFormat)).ToList();
            Assert.AreEqual(vectorValues.Count, 2);
            var vector = new Vector2(vectorValues[0], vectorValues[1]);
            return new ParseResult<Vector2>(true, vector);
        });

        // If you don't want to add a multitude of operator handlers, you can define
        // a generic filter handler that will handle all existing operators on a filter
        m_QueryEngine.AddFilter<string>("is", IsFilterResolver);
    }

    static bool IsFilterResolver(MyCustomObjectType data, string op, string keyword)
    {
        if (op != ":")
            return false;

        if (keyword.Equals("high-priority"))
            return data.Id > 900 && data.Position.magnitude > 85.0f;

        return false;
    }
    /// <summary>
    /// Test function that runs the Query engine on a list of queries.
    /// </summary>
    public void TestFiltering()
    {
        void PrintFilteredData(IEnumerable<MyCustomObjectType> data)
        {
            foreach (var obj in data)
            {
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, obj.ToString());
            }
        }

        // Find objects that have an even id (id%2==0)
        var filteredData = FilterData("id%2");
        PrintFilteredData(filteredData);

        // Find objects that have an id that is contained in a specified list
        filteredData = FilterData("id?2,8,19,42");
        PrintFilteredData(filteredData);

        // Find objects that have a position vector smaller than a specified vector
        filteredData = FilterData("p<[50,50]");
        PrintFilteredData(filteredData);

        // Find objects that are defined as high-priority
        filteredData = FilterData("is:high-priority");
        PrintFilteredData(filteredData);
    }
    /// <summary>
    /// Filter data according to a specific inputQuery.
    /// </summary>
    /// <param name="inputQuery">Text query</param>
    /// <returns>Yields a list of matching MyObjectType.</returns>
    public IEnumerable<MyCustomObjectType> FilterData(string inputQuery)
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

            return new List<MyCustomObjectType>();
        }

        var filteredData = query.Apply(m_Data);
        return filteredData;
    }

    void GenerateData(int size)
    {
        m_Data = new List<MyCustomObjectType>(size);
        for (var i = 0; i < size; ++i)
        {
            var posX = Random.Range(0, 100);
            var posY = Random.Range(0, 100);
            var newObj = new MyCustomObjectType { Id = i, Position = new Vector2(posX, posY) };
            m_Data.Add(newObj);
        }
    }

    static bool TryConvertValue<T>(string value, out T convertedValue)
    {
        var type = typeof(T);
        var converter = TypeDescriptor.GetConverter(type);
        if (converter.IsValid(value))
        {
            convertedValue = (T)converter.ConvertFromString(null, CultureInfo.InvariantCulture, value);
            return true;
        }

        convertedValue = default;
        return false;
    }
}
