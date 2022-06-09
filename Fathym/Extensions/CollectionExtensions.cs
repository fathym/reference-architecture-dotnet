using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using System.Threading.Tasks;
using System.Dynamic;
using Fathym;

namespace System.Collections
{
}

namespace System.Collections.Generic
{
    /// <summary>
    ///     Extension class which contains generic collection extensions in the System.Collections.Generic namespace.
    /// </summary>
    public static class CollectionExtensions
    {
        #region Add Item
        public static IEnumerable<T> AddItem<T>(this IEnumerable<T> values, T item)
        {
            if (values.IsNullOrEmpty())
                values = new List<T>();

            var list = values.ToList();

            list.Add(item);

            return list;
        }
        #endregion

        #region Each
        /// <summary>
        ///     Method will execute the action against every item in the collection.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="values">The collection of values.</param>
        /// <param name="action">The action to execute.</param>
        /// <example>
        ///     <code>
        ///         string[] values = new string[] { "1", "2", "3" };
        ///         
        ///         int total = 0; 
        ///         
        ///         values.Each(value => total += value);
        ///     </code>
        /// </example>
        public static void Each<T>(this IEnumerable<T> values, Action<T> action, bool parallel = false)
        {
            values.Each((value) => { action(value); return false; }, parallel);
        }

        /// <summary>
        ///     Method will execute the action against every item in the collection.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="values">The collection of values.</param>
        /// <param name="action">The action to execute.</param>
        /// <example>
        ///     <code>
        ///         string[] values = new string[] { "1", "2", "3" };
        ///         
        ///         int total = 0; 
        ///         
        ///         values.Each(value => total += value);
        ///     </code>
        /// </example>
        public static async Task Each<T>(this IEnumerable<T> values, Func<T, Task> action, bool parallel = false)
        {
            await values.Each(async (value) => { await action(value); return false; }, parallel);
        }

        /// <summary>
        ///     Method will execute the action against every item in the collection and will break once true is returned from one of the actions.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="values">The collection of values.</param>
        /// <param name="action">The action to execute.</param>
        /// <example>
        ///     <code>
        ///         string[] values = new string[] { "1", "2", "3" };
        ///         
        ///         int total = 0; 
        ///         
        ///         values.Each(value => total += value);
        ///     </code>
        /// </example>
        public static void Each<T>(this IEnumerable<T> values, Func<T, bool> action, bool parallel = false)
        {
            values.Each((t) =>
            {
                return Task.FromResult(action(t));
            }, parallel).Wait();
        }

        /// <summary>
        ///     Method will execute the action against every item in the collection and will break once true is returned from one of the actions.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="values">The collection of values.</param>
        /// <param name="action">The action to execute.</param>
        /// <example>
        ///     <code>
        ///         string[] values = new string[] { "1", "2", "3" };
        ///         
        ///         int total = 0; 
        ///         
        ///         values.Each(value => total += value);
        ///     </code>
        /// </example>
        public static async Task Each<T>(this IEnumerable<T> values, Func<T, Task<bool>> action, bool parallel = false)
        {
            if (values != null)
            {
                if (parallel)
                {
                    var valueTasks = values.Select(value =>
                    {
                        return action(value);
                    });

                    var successful = await Task.WhenAll(valueTasks);

                    //Parallel.ForEach(values, value => action(value));  //  TODO:  Implement Multi-Threaded break logic
                }
                else
                {
                    bool shouldBreak;

                    foreach (T value in values)
                    {
                        shouldBreak = await action(value);

                        if (shouldBreak)
                            break;
                    }
                }
            }
        }

        //public static async Task ForEachAsync<T>(this IEnumerable<T> values, Func<T, Task<bool>> action)
        //{
        //    Parallel.ForEach()
        //}
        #endregion
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();

            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
        #region Is Null Or Empty
        public static bool IsNullOrEmpty<TValue>(this IEnumerable<TValue> values)
        {
            return values == null || !values.Any();
        }
        #endregion

        #region Query String
        public static IDictionary<string, string> FromQueryString(this string query)
        {
            query = query.TrimStart('?');

            var paramSplits = query.Split('&', StringSplitOptions.RemoveEmptyEntries);

            return paramSplits.ToDictionary(ps => ps.Split('=')[0], ps => ps.Split('=')[1]);
        }

        public static string ToQueryString<TKey, TValue>(this IDictionary<TKey, TValue> values, bool urlEncode = true)
        {
            var pairs = new List<string>();

            values.Each(value =>
            {
                var val = value.Value.ToString();

                if (urlEncode)
                    val = val.URLEncode();

                pairs.Add($"{value.Key}={val}");
            });

            return string.Join("&", pairs);
        }
        #endregion

        #region Page
        public static Pageable<TValue> Page<TValue>(this IEnumerable<TValue> values, int page, int pageSize)
        {
            var items = new List<TValue> { };

            var totalRecords = 0;

            if (!values.IsNullOrEmpty())
            {
                var startRow = (page * pageSize) - pageSize;

                Parallel.Invoke(
                    () => items.AddRange(values.Skip(startRow).Take(pageSize)),
                    () => totalRecords = values.Count()
                );
            }

            return new Pageable<TValue>()
            {
                Items = items,
                TotalRecords = totalRecords
            };
        }
        #endregion

        #region Randomize
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> values, int take = 0)
        {
            IEnumerable<T> randomValues = values.OrderBy(v => Guid.NewGuid());

            if (take > 0)
                randomValues = randomValues.Take(take);

            return randomValues.ToArray();
        }
        #endregion

        #region Remove Item
        public static IEnumerable<T> RemoveItem<T>(this IEnumerable<T> values, T item)
        {
            if (values.IsNullOrEmpty())
                values = new List<T>();

            var list = values.ToList();

            list.Remove(item);

            return list;
        }
        #endregion

        #region When All
        public static void WhenAll(this IEnumerable<Task> source)
        {
            Task.WaitAll(source.ToArray());
        }

        public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> source)
        {
            return Task.WhenAll(source);
        }
        #endregion
    }

    public static class DictionaryExtensions
    {
        #region Merge
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> toMerge, bool overwrite)
        {
            toMerge.Each(
                (kvp) =>
                {
                    if (!dictionary.ContainsKey(kvp.Key) || overwrite)
                        dictionary[kvp.Key] = kvp.Value;
                });

            return dictionary;
        }
        #endregion

        #region To Dynamic
        public static dynamic ToDynamic<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return ToDynamic(dictionary, (key) => key.ToString(), (value) => value);
        }

        public static dynamic ToDynamic<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<TKey, string> getPropertyName, Func<TValue, object> getPropertyValue)
        {
            return new DynamicDictionary(dictionary.ToDictionary((kvp) => getPropertyName(kvp.Key), (kvp) => getPropertyValue(kvp.Value)));
        }
        #endregion

        #region To Dictionary Safe
        public static Dictionary<TKey, TElement> ToDictionarySafe<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector)
        {
            return source?.GroupBy(keySelector)?.ToDictionary(g => g.Key, g => elementSelector(g.First()));
        }
        #endregion
    }
}

namespace System.Dynamic
{
    public class DynamicDictionary : DynamicObject
    {
        #region Fields
        protected readonly IDictionary<string, object> properties;
        #endregion

        #region Constructors
        public DynamicDictionary(IDictionary<string, object> properties)
        {
            if (properties.IsNullOrEmpty())
                throw new ArgumentNullException("properties", "A properties collection must be provided.");

            this.properties = properties;
        }
        #endregion

        #region API Methods
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return properties.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            bool retrieved;

            if (properties.ContainsKey(binder.Name))
            {
                result = properties[binder.Name];

                retrieved = true;
            }
            else
                retrieved = base.TryGetMember(binder, out result);

            return retrieved;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            properties[binder.Name] = value;

            return properties.ContainsKey(binder.Name);
        }
        #endregion
    }
}

namespace System.Collections.Specialized
{
    public static class SpecializedExtensions
    {
        /// <summary>
        /// Greates query string using the name value collection
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static string ToQueryString(this NameValueCollection collection, bool urlEncode = true)
        {
            return string.Join("&",
                from k in collection.AllKeys
                select string.Join("&",
                    from v in collection.GetValues(k)
                    select $"{k}={(urlEncode ? v.URLEncode() : v)}")
                );
        }
    }
}
