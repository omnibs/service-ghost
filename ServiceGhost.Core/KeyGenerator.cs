namespace ServiceGhost.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Gera chave de cache para um método
    /// </summary>
    /// <remarks>
    /// Baseado no código disponível em: http://www.avantprime.com/blog/19/generating-unique-keys-based-on-a-method-or-class
    /// </remarks>
    public static class KeyGenerator
    {
        private static readonly Dictionary<Tuple<Type, MethodBase, String>, string> Keys =
            new Dictionary<Tuple<Type, MethodBase, String>, string>();

        private readonly static object SyncObject = new object();
        private const string SaltSeparator = "_";

        public static string ExpressionKey<T>(Expression<T> expression)
        {
            var call = expression.Body as MethodCallExpression;
            if (call == null)
            {
                throw new ArgumentException("Not a method call");
            }

            return string.Concat(GetMethodKey(call.Method.DeclaringType, call.Method), "_", MethodValue(call, expression.Parameters));
        }

        /// <summary>
        /// Retorna chave com os parâmetros do método atual
        /// </summary>
        /// <param name="call">expressão que define o método</param>
        private static string MethodValue(MethodCallExpression call, ReadOnlyCollection<ParameterExpression> parameters)
        {
            var sb = new StringBuilder();

            foreach (Expression argument in call.Arguments)
            {
                var lambda = Expression.Lambda(argument, parameters);
                var d = lambda.Compile();
                var value = d.DynamicInvoke(new object[1]);
                sb.Append(value).Append("_");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets a method level unique key for the calling
        /// method with an optional salt randomizer
        /// </summary>
        /// <param name="salt">
        /// The salt randomizer
        /// </param>
        public static string GetMethodKey(string salt = null)
        {
            var stackTrace = new StackTrace();
            var callingMethod = stackTrace.GetFrame(1).GetMethod();
            var @class = callingMethod.DeclaringType;
            return GetMethodKey(@class, callingMethod, salt);
        }

        /// <summary>
        /// Gets a method level unique key for the specified
        /// method and the class with an optional salt randomizer
        /// </summary>
        /// <param name="class">
        /// Classe que contém o método
        /// </param>
        /// <param name="method">
        /// Método analizado
        /// </param>
        /// <param name="salt">
        /// The salt.
        /// </param>
        public static string GetMethodKey(Type @class, MethodBase method, string salt = null)
        {
            lock (SyncObject)
            {
                var key = new Tuple<Type, MethodBase, string>(@class, method, salt ?? "");
                if (Keys.ContainsKey(key))
                {
                    return Keys[key];
                }

                var parameters =
                    string.Join
                        (
                            SaltSeparator,
                            (method is MethodInfo) ? (method as MethodInfo).ReturnType.ToString() : string.Empty,
                            method.GetParameters().Select(
                                x => string.Concat(x.Name, x.ParameterType.AssemblyQualifiedName)).ToArray(),
                            method.IsGenericMethod ? method.GetGenericArguments() : new object[] { string.Empty }
                        );

                var value = string.Format(
                    "{0}-{1}-{2}-{3}-{4}", @class.AssemblyQualifiedName, method.Name, parameters, SaltSeparator, salt);
                Keys.Add(key, value);

                return value;
            }
        }

        /// <summary>
        /// Gets a class level unique key with an optional randomizer
        /// </summary>
        /// <typeparam name="T">
        /// Tipo da classe
        /// </typeparam>
        /// <param name="salt">
        /// The salt.
        /// </param>
        /// <returns>
        /// Chave para a classe
        /// </returns>
        public static string GetClassKey<T>(object salt)
            where T : class
        {
            lock (SyncObject)
            {
                var key = new Tuple<Type, MethodBase, string>(typeof(T), null, salt.ToString());
                if (Keys.ContainsKey(key))
                {
                    return Keys[key];
                }

                var genericParameterValues = typeof(T).IsGenericType
                                                 ? typeof(T).GetGenericParameterConstraints().Select(
                                                     x => x.AssemblyQualifiedName).ToArray()
                                                 : new[] { string.Empty };

                var genericParameters = string.Join(
                    SaltSeparator,
                    genericParameterValues);

                var value = string.Format("{0}-{1}{2}{3}", typeof(T).AssemblyQualifiedName, genericParameters, SaltSeparator, salt);
                Keys.Add(key, value);

                return value;
            }
        }
    }
}
