using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using JsonFlatten;

public static class JsonNodeExtensions
{
    public static Dictionary<string, object> Flatten<T>(this T node)
        where T : class
    {
        var jo = node != null ? JObject.Parse(node.ToJSON()) : null;

        var flat = jo?.Flatten(false);

        return flat != null ? new Dictionary<string, object>(flat) : null;
    }


    public static T Unflatten<T>(this IDictionary<string, object> flat)
        where T : class
    {
        var jo = flat?.Unflatten();

        var joStr = jo?.ToString();

        return joStr?.FromJSON<T>();
    }

}
