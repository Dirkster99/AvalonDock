namespace SettingsModel.Models
{
    using SettingsModel.Interfaces;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Determines the type of schema of a schema object <seealso cref="IOptionsSchema"/>
    /// </summary>
    public enum OptionSchemaType
    {
        /// <summary>
        /// The schema object represents a simple value (bool, int etc).
        /// </summary>
        SingleValue = 0,

        /// <summary>
        /// The schema object represents a list of simple values (bool values, int values etc).
        /// </summary>
        List = 1
    }

    /// <summary>
    /// Defines available schema information (or the model) for 1 option.
    /// </summary>
    internal class OptionsSchema : IOptionsSchema
    {
        #region fields
        private Dictionary<object, object> mValues;
        #endregion fields
        
        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="typeOfOptionValue"></param>
        /// <param name="isOptional"></param>
        /// <param name="defaultValue"></param>
        /// <param name="schemaType"></param>
        public OptionsSchema(string optionName,
                             Type typeOfOptionValue,
                             bool isOptional,
                             object defaultValue,
                             OptionSchemaType schemaType = OptionSchemaType.SingleValue)
        {
            SchemaType = schemaType;
            OptionName = optionName;
            TypeOfValue = typeOfOptionValue;
            IsOptional = isOptional;

            switch (schemaType)
            {
                case OptionSchemaType.SingleValue:
                case OptionSchemaType.List:
                    // Value and default value is same at construction time
                    Value = DefaultValue = defaultValue;
                    break;

                default:
                    throw new NotSupportedException(schemaType.ToString());
            }
        }

        public static OptionsSchema CreateOptionsSchema<T>(string optionName,
                                                            Type typeOfOptionValue,
                                                            bool isOptional,
                                                            List<T> values)
        {
            var retSchema = new OptionsSchema(optionName, typeOfOptionValue, isOptional, null, OptionSchemaType.List);

            retSchema.InitializeValueList<T>(values != null ? values : new List<T>());

            return retSchema;
        }

        private void InitializeValueList<T>(List<T> values)
        {
            if (mValues != null)
                mValues.Clear();
            else
                mValues = new Dictionary<object, object>();

            foreach (var item in values)
                mValues.Add(item, item);
        }
        #endregion constructors

        #region properties

        public OptionSchemaType SchemaType { get; private set; }

        /// <summary>
        /// Gets/sets the name of an option.
        /// </summary>
        public string OptionName { get; private set; }

        /// <summary>
        /// Gets/sets the type of the option being defined here.
        /// </summary>
        public Type TypeOfValue { get; private set; }

        /// <summary>
        /// Gets/sets whether this options is optional or required.
        /// This is important when perisiting data and when reading
        /// data from persistance.
        /// </summary>
        public bool IsOptional { get; private set; }

        /// <summary>
        /// Gets/sets the value of this option.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Gets/sets the default value of this option.
        /// </summary>
        public object DefaultValue { get; private set; }
        #endregion properties

        #region methods
        /// <summary>
        /// Removes the value with the specified key
        /// from the internal dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully found and removed; otherwise, false.
        /// This method returns false if key is not found in the System.Collections.Generic.Dictionary&lt;TKey,TValue>.
        /// 
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     key is null.
        /// </returns>
        public bool List_Remove(string key)
        {
            if (this.mValues != null)
                return this.mValues.Remove(key);

            return false;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">
        ///     The key of the value to get.
        /// </param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the specified
        ///     key, if the key is found; otherwise, the default value for the type of the
        ///     value parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        ///     true if the System.Collections.Generic.Dictionary&lt;TKey,TValue> contains an
        ///     element with the specified key; otherwise, false.
        ///
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     key is null.
        /// </returns>
        public bool List_TryGetValue(string key, out object value)
        {
            value = null;

            if (this.mValues != null)
                return this.mValues.TryGetValue(key, out value);
                
            return false;
        }

        /// <summary>
        /// Sets the value of a given option in this option object.
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns>true if data actually changed (for dirty state tracking).
        /// Otherwise, false if requested value was already present.</returns>
        public bool SetValue(object newValue)
        {
            if (SchemaType == OptionSchemaType.List)
            {
                object checkValue;
                if (this.mValues.TryGetValue(newValue, out checkValue) == true)
                    return false;

                this.mValues.Add(newValue, newValue);
                return true;
            }

            if (Value != newValue)
            {
                Value = newValue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a list item in a list schema
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name"></param>
        /// <returns>
        /// Returns true if item was succesfully added or false
        /// if schema is not a list schema.
        /// </returns>
        public bool List_AddValue(string name, object value)
        {
            if (SchemaType == OptionSchemaType.List)
            {
                // Remove key if item exists and re-add below
                object checkValue;
                if (this.mValues.TryGetValue(name, out checkValue) == true)
                    this.mValues.Remove(name);

                this.mValues.Add(name, value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clear all items contained in a list.
        /// </summary>
        /// <returns></returns>
        public bool List_Clear()
        {
            if (SchemaType == OptionSchemaType.List)
            {
                this.mValues.Clear();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a list of current values if this schema descripes a List.
        /// Return a single value schema as a list of 1 item.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object> List_GetListOfValues()
        {
            if (SchemaType == OptionSchemaType.List)
            {
                foreach (var item in mValues)
                {
                    yield return item.Value as object;
                }

                yield break; // End of list reached
            }

            // return a single value as a list of 1 item
            yield return Value;
        }

        /// <summary>
        /// Gets a list of current keys and values if this schema
        /// descripes a List.
        /// 
        /// Return a single value schema as a list of 1 item.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<object, object>> List_GetListOfKeyValues()
        {
            if (SchemaType == OptionSchemaType.List)
            {
                foreach (var item in mValues)
                {
                    yield return item;
                }

                yield break; // End of list reached
            }
        }
        #endregion methods
    }
}
