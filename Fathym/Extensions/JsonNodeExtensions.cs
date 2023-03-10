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
    {
        var jo = JObject.Parse(node.ToJSON());

        var flat = jo.Flatten(false);

        //flatDraft = flatDraft.Where(kvp => kvp.Value != null).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return new Dictionary<string, object>(flat);
    }


    public static T Unflatten<T>(this IDictionary<string, object> flat)
    {
        var jo = flat.Unflatten();

        var joStr = jo.ToString();

        return joStr.FromJSON<T>();
    }

}
