namespace SettingsModel.Models.XML.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Security;

    /// <summary>
    /// Holds a collection of <seealso cref="IAlternativeDataTypeHandler"/> alternative datatype
    /// handlers to handle datatypes that are not supported through equivalent conversion
    /// in alternative datatypes.
    /// </summary>
    internal class AlternativeDataTypeHandler
    {
        #region fields
        private readonly Dictionary<Type, IAlternativeDataTypeHandler> converters = null;
        #endregion fields

        public AlternativeDataTypeHandler()
        {
            converters = new Dictionary<Type, IAlternativeDataTypeHandler>();

            converters.Add(typeof(SecureString), new SecureStringHandler());
        }

        /// <summary>
        /// Finds an alternative datatype handler to handle datatypes that are not
        /// supported through equivalent conversion in alternative datatypes.
        /// </summary>
        /// <param name="typeOfDataType2Handle"></param>
        /// <returns></returns>
        public IAlternativeDataTypeHandler FindHandler(Type typeOfDataType2Handle)
        {
            IAlternativeDataTypeHandler ret = null;

            try
            {
                converters.TryGetValue(typeOfDataType2Handle, out ret);
            }
            catch
            {
            }

            return ret;
        }
    }
}
