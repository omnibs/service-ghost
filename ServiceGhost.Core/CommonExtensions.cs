﻿namespace ServiceGhost.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;

    using FakeItEasy.Core;

    /// <summary>
    /// Provides extension methods for the common uses.
    /// </summary>
    internal static class CommonExtensions
    {
        /// <summary>
        /// Replaces the format item in a specified System.String with the text equivalent
        /// of the value of a corresponding System.Object instance in a specified array using
        /// invariant culture as <see cref="IFormatProvider" />.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arguments">An <see cref="Object" /> array containing zero or more objects to format.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatInvariant(this string format, params object[] arguments)
        {
            return string.Format(CultureInfo.InvariantCulture, format, arguments);
        }

        /// <summary>
        /// Gets an enumerable of tuples where the first value of each tuple is a value
        /// from the first collection and the second value of each tuple is the value at the same position
        /// from the second collection.
        /// </summary>
        /// <typeparam name="TFirst">The type of values in the first collection.</typeparam>
        /// <typeparam name="TSecond">The type of values in the second collection.</typeparam>
        /// <param name="firstCollection">The first of the collections to combine.</param>
        /// <param name="secondCollection">The second of the collections to combine.</param>
        /// <returns>An enumerable of tuples.</returns>
        public static IEnumerable<Tuple<TFirst, TSecond>> Zip<TFirst, TSecond>(this IEnumerable<TFirst> firstCollection, IEnumerable<TSecond> secondCollection)
        {
            return new ZipEnumerable<TFirst, TSecond>(firstCollection, secondCollection);
        }

        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T Copy<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        internal static string GetCurrentStack()
        {
            var stackTrace = new StackTrace();

            // ReSharper disable once AssignNullToNotNullAttribute
            var stack = string.Join(
                "\r\n",
                stackTrace.GetFrames().Skip(1)
                    .Reverse()
                    .Select(x => x.GetMethod().ReflectedType + "." + x.GetMethod().Name)
                    .SkipWhile(
                        x =>
                            x.StartsWith("Microsoft", StringComparison.Ordinal)
                            || x.StartsWith("System", StringComparison.Ordinal))
                            .TakeWhile(x => !x.StartsWith("ServiceGhost.Core", StringComparison.Ordinal)));

            return stack;
        }

        internal static IEnumerable<object> GetOutputArgumentsForCall(IFakeObjectCall call)
        {
            return
                from valueAndParameterInfo in call.Method.GetParameters().Zip(call.Arguments.AsEnumerable())
                where valueAndParameterInfo.Item1.ParameterType.IsByRef
                select valueAndParameterInfo.Item2;
        }


        /// <summary>
        /// Joins the collection to a string.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="items">The items to join.</param>
        /// <param name="stringConverter">A function that converts from an item to a string value.</param>
        /// <param name="separator">Separator to insert between each item.</param>
        /// <returns>A string representation of the collection.</returns>
        public static string ToCollectionString<T>(this IEnumerable<T> items, Func<T, string> stringConverter, string separator)
        {
            var result = new StringBuilder();

            foreach (var item in items)
            {
                if (result.Length > 0)
                {
                    result.Append(separator);
                }

                result.Append(stringConverter(item));
            }

            return result.ToString();
        }

        /// <summary>
        /// Gets a dictionary containing the first element from the sequence that has a key specified by the key selector.
        /// </summary>
        /// <typeparam name="T">The type of items in the sequence.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns>A dictionary.</returns>
        public static IDictionary<TKey, T> FirstFromEachKey<T, TKey>(this IEnumerable<T> sequence, Func<T, TKey> keySelector)
        {
            return sequence.ToLookup(keySelector).ToDictionary(x => x.Key, x => x.First());
        }

        public static StringBuilder AppendIndented(this StringBuilder builder, string indentString, string value)
        {
            var lines = value == null ? new string[] { } : value.Split('\n');

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                builder.Append(indentString).Append(line);

                if (i != lines.Length - 1)
                {
                    builder.Append('\n');
                }
            }

            return builder;
        }

        public static string GetGenericArgumentsCSharp(this MethodBase method)
        {
            var genericArguments = method.GetGenericArguments();
            if (genericArguments.Length == 0)
            {
                return string.Empty;
            }

            return string.Concat("<", string.Join(", ", genericArguments.Select(type => type.FullNameCSharp()).ToArray()), ">");
        }

        public static string FullNameCSharp(this Type type)
        {

            if (!type.IsGenericType)
            {
                return type.FullName;
            }

            var partName = type.FullName.Split('`')[0];
            var genericArgNames = type.GetGenericArguments().Select(arg => arg.FullNameCSharp()).ToArray();
            return string.Format(CultureInfo.InvariantCulture, "{0}<{1}>", partName, string.Join(", ", genericArgNames));
        }

        private class ZipEnumerable<TFirst, TSecond>
           : IEnumerable<Tuple<TFirst, TSecond>>
        {
            private IEnumerable<TFirst> firstCollection;
            private IEnumerable<TSecond> secondCollection;

            public ZipEnumerable(IEnumerable<TFirst> firstCollection, IEnumerable<TSecond> secondCollection)
            {
                this.firstCollection = firstCollection;
                this.secondCollection = secondCollection;
            }

            public IEnumerator<Tuple<TFirst, TSecond>> GetEnumerator()
            {
                return new ZipEnumerator(this.firstCollection, this.secondCollection);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            private class ZipEnumerator
                : IEnumerator<Tuple<TFirst, TSecond>>
            {
                private IEnumerable<TFirst> firstCollection;
                private IEnumerable<TSecond> secondCollection;
                private IEnumerator<TFirst> firsts;
                private IEnumerator<TSecond> seconds;

                public ZipEnumerator(IEnumerable<TFirst> firstCollection, IEnumerable<TSecond> secondCollection)
                {
                    this.firstCollection = firstCollection;
                    this.secondCollection = secondCollection;
                    this.Reset();
                }

                public Tuple<TFirst, TSecond> Current
                {
                    get { return new Tuple<TFirst, TSecond>(this.firsts.Current, this.seconds.Current); }
                }

                object System.Collections.IEnumerator.Current
                {
                    get { return this.Current; }
                }

                public void Dispose()
                {
                    this.firsts.Dispose();
                    this.seconds.Dispose();
                    GC.SuppressFinalize(this);
                }

                public bool MoveNext()
                {
                    var firstsCanMove = this.firsts.MoveNext();
                    var secondsCanMove = this.seconds.MoveNext();

                    return firstsCanMove && secondsCanMove;
                }

                public void Reset()
                {
                    this.firsts = this.firstCollection.GetEnumerator();
                    this.seconds = this.secondCollection.GetEnumerator();
                }
            }
        }
    }
}
