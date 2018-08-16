namespace SettingsModel.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An option group is a logical group of options
    /// (setting values or preferences) of an application.
    /// </summary>
    public interface IOptionGroup
    {
        /// <summary>
        /// Gets the name of this options group. This name is used to
        /// cluster related options around a technically relevant name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Retrieves the schema of each option in this optiongroup.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IOptionsSchema> GetOptionDefinitions();

        /// <summary>
        /// Retrieves the schema of an option in this optiongroup.
        /// </summary>
        /// <returns></returns>
        IOptionsSchema GetOptionDefinition(string optionName);

        /// <summary>
        /// Gets the value of an option in a given <seealso cref="OptionGroup"/> or null
        /// if either option or <seealso cref="OptionGroup"/> does not exist.
        /// 
        /// Method returns false if option or <seealso cref="OptionGroup"/> are not known.
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="optValue">Indicates whether option and <seealso cref="OptionGroup"/> exist or not.</param>
        /// <returns></returns>
        bool GetValue(string optionName, out object optValue);

        /// <summary>
        /// Gets the value of an option in a given <seealso cref="OptionGroup"/> or
        /// throws an exception if either option or <seealso cref="OptionGroup"/>
        /// does not exist.
        /// 
        /// Method the requested option value if option and <seealso cref="OptionGroup"/> are known.
        /// </summary>
        /// <param name="optionName"></param>
        /// <returns></returns>
        object GetValue(string optionName);

        /// <summary>
        /// Gets the requested option and returns it as typed &lt;T> value.
        /// The method throws an exception if:
        ///  - requested option value is not stored as typed &lt;T> or
        ///  - the <seealso cref="OptionGroup"/> and option name does not exist.
        /// </summary>
        /// <param name="optionName"></param>
        /// <returns>current value of this option.</returns>
        T GetValue<T>(string optionName);

        /// <summary>
        /// Sets the value of a given option in this option table.
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        bool SetValue(string optionName, object newValue);

        /// <summary>
        /// Add a list item in a list schema
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="optionName"></param>
        /// <param name="value"></param>
        /// <returns>
        /// Returns true if item was succesfully added or false
        /// if schema is not a list schema.
        /// </returns>
        bool List_AddValue(string optionName, string keyName, object value);

        /// <summary>
        /// Clear all items contained in a list.
        /// </summary>
        /// <param name="optionName"></param>
        /// <returns></returns>
        bool List_Clear(string optionName);

        /// <summary>
        /// Create a new option that can hold a list of items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optionName"></param>
        /// <param name="type"></param>
        /// <param name="isOptional"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        IOptionsSchema List_CreateOption<T>(string optionName, Type type, bool isOptional, List<T> list);

        /// <summary>
        /// Gets a list of current values if this schema descripes a List.
        /// Return a single value schema as a list of 1 item.
        /// </summary>
        /// <param name="optionName"></param>
        /// <returns></returns>
        IEnumerable<object> List_GetListOfValues(string optionName);

        /// <summary>
        /// Gets a list of current keys and values if this schema
        /// descripes a List.
        /// 
        /// Return a single value schema as a list of 1 item.
        /// </summary>
        /// <param name="optionName"></param>
        /// <returns></returns>
        IEnumerable<KeyValuePair<object, object>> List_GetListOfKeyValues(string optionName);

        /// <summary>
        /// Resets the IsDirty flag to false to indicate that the current
        /// data was not changed/edited by a user request, yet. This is
        /// useful after defining a new options model and starting to work
        /// with it, as well, as after reading options from persistance...
        /// </summary>
        /// <param name="isDirty"></param>
        void SetUndirty(bool isDirty);
    }
}