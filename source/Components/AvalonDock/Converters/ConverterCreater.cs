using System;
using System.Collections.Generic;

namespace AvalonDock.Converters
{
    internal class ConverterCreater
    {
        #region Private fields

        private static readonly Dictionary<Type, object> ConverterMap = new Dictionary<Type, object>();

        #endregion Private fields

        #region Public methods

        public static T Get<T>() where T : new()
        {
            if (!ConverterMap.ContainsKey(typeof(T)))
            {
                ConverterMap.Add(typeof(T), new T());
            }
            return (T)ConverterMap[typeof(T)];
        }

        #endregion Public methods
    }
}