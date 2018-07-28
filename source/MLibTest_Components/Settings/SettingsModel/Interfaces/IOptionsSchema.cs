namespace SettingsModel.Interfaces
{
    using SettingsModel.Models;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines available schema information for 1 option.
    /// </summary>
    public interface IOptionsSchema
    {
        /// <summary>
        /// Gets the type of schema (list or single value)
        /// </summary>
        OptionSchemaType SchemaType { get; }

        /// <summary>
        /// Gets the name of an option.
        /// </summary>
        string OptionName { get; }

        /// <summary>
        /// Gets the type of the option being defined here.
        /// </summary>
        Type TypeOfValue { get; }

        /// <summary>
        /// Gets whether this options is optional or required.
        /// This is important when perisiting data and when reading
        /// data from persistance.
        /// </summary>
        bool IsOptional { get; }

        /// <summary>
        /// Gets the value of this option.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Gets/sets the default value of this option.
        /// </summary>
        object DefaultValue { get; }

        #region methods
        /// <summary>
        /// Removes the value with the specified key
        /// from the internal dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully found and removed; otherwise, false.
        /// This method returns false if key is not found in
        /// the System.Collections.Generic.Dictionary&lt;TKey,TValue>.
        /// 
        /// Exceptions:
        ///   System.ArgumentNullException:
        ///     key is null.
        /// </returns>
        bool List_Remove(string key);

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
        bool List_TryGetValue(string key, out object value);

        /// <summary>
        /// Sets the value of a given option in this option object.
        /// </summary>
        /// <returns>true if data actually changed (for dirty state tracking).
        /// Otherwise, false if requested value was already present.</returns>
        /// <param name="newValue"></param>
        bool SetValue(object newValue);

        /// <summary>
        /// Add a list item in a list schema
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns>
        /// Returns true if item was succesfully added or false
        /// if schema is not a list schema.
        /// </returns>
        bool List_AddValue(string name, object value);

        /// <summary>
        /// Clear all items contained in a list.
        /// </summary>
        /// <returns></returns>
        bool List_Clear();

        /// <summary>
        /// Gets a list of current values if this schema descripes a List.
        /// Return a single value schema as a list of 1 item.
        /// </summary>
        /// <returns></returns>
        IEnumerable<object> List_GetListOfValues();

        /// <summary>
        /// Gets a list of current keys and values if this schema
        /// descripes a List.
        /// 
        /// Return a single value schema as a list of 1 item.
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValuePair<object, object>> List_GetListOfKeyValues();
        #endregion methods
    }
}
