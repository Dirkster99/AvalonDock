namespace SettingsModel
{
    using SettingsModel.Interfaces;
    using SettingsModel.Models;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An <seealso cref="OptionGroup"/> is a set of options that is either grouped
    /// physically or logically into any specific group. Using these groups or sets
    /// of options allows application to configure and manage different sets of options
    /// while using a similar backend system.
    /// 
    /// Using this technique means, the set of available application options is dynamic
    /// and can be configured in dependence of current run-time conditions.
    /// </summary>
    internal class OptionGroup : IOptionGroup
    {
        #region constructors
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="name"></param>
        public OptionGroup(string name)
        {
            Name = name;
            OptionDefinitions = new Dictionary<string, OptionsSchema>();
            IsDirty = false;
        }
        #endregion constructors

        #region properties
        /// <summary>
        /// Gets the name of the <seealso cref="OptionGroup"/>.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets this <seealso cref="OptionGroup"/> has unsaved changes or not.
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        /// Gets a dictionary of otpions defined for this <seealso cref="OptionGroup"/>.
        /// </summary>
        private Dictionary<string, OptionsSchema> OptionDefinitions { get; set; }
        #endregion properties

        #region methods
        /// <summary>
        /// Retrieves the schema of each option in this optiongroup.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IOptionsSchema> GetOptionDefinitions()
        {
            foreach (var item in OptionDefinitions)
            {
                yield return item.Value;
            }
        }

        /// <summary>
        /// Retrieves the schema of an option in this optiongroup
        /// or null if <param name="optionName"/> cannot be resolved.
        /// </summary>
        /// <returns></returns>
        public IOptionsSchema GetOptionDefinition(string optionName)
        {
            OptionsSchema ret;

            if (OptionDefinitions.TryGetValue(optionName, out ret) == true)
                return ret;

            return null;
        }

        /// <summary>
        /// Gets the value of a given option in this option table.
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="outresult"></param>
        /// <returns></returns>
        public bool GetValue(string optionName, out object outresult)
        {
            outresult = null;
            OptionsSchema result;

            if (OptionDefinitions.TryGetValue(optionName, out result) == false)
                return false;

            outresult = result.Value;
            return true;
        }

        /// <summary>
        /// Gets the value of an option in a given <seealso cref="OptionGroup"/> or
        /// throws an exception if either option or <seealso cref="OptionGroup"/>
        /// does not exist.
        /// 
        /// Method the requested option value if option and <seealso cref="OptionGroup"/> are known.
        /// </summary>
        /// <param name="optionName"></param>
        /// <returns></returns>
        public object GetValue(string optionName)
        {
            object optValue;

            if (GetValue(optionName, out optValue) == false)
                throw new Exception(string.Format("The application option {0}-{1} cannot be located.",
                    this.Name, optionName));

            return optValue;
        }

        /// <summary>
        /// Gets the requested option and returns it as typed &lt;T> value.
        /// The method throws an exception if:
        ///  - requested option value is not stored as typed &lt;T> or
        ///  - the <seealso cref="OptionGroup"/> and option name does not exist.
        /// </summary>
        /// <param name="optionName"></param>
        /// <returns>current value of this option.</returns>
        public T GetValue<T>(string optionName)
        {
            object optValue = GetValue(optionName);

            if ((optValue is T) == false)
                throw new Exception(string.Format("The requested option {0}-{1} is not of requested type <T>.",
                this.Name, optionName));

            return (T)optValue;
        }

        /// <summary>
        /// Sets the value of a given option in this option table.
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public bool SetValue(string optionName, object newValue)
        {
            OptionsSchema result;

            if (OptionDefinitions.TryGetValue(optionName, out result) == false)
                return false;

            if (result.TypeOfValue != newValue.GetType())
                throw new Exception(string.Format("Expected Type:'{0}' of '{1}-{2}' was not supplied '{3}'",
                                    result.TypeOfValue, this.Name, result.OptionName,
                                    newValue.GetType()));

            bool bdirty = result.SetValue(newValue);

            IsDirty = (IsDirty | bdirty);

            return bdirty;
        }

        #region List related methods
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
        public bool List_AddValue(string optionName, string keyName, object value)
        {
            var schema = GetOptionDefinition(optionName);

            if (schema == null)
                return false;

            return schema.List_AddValue(keyName, value);
        }

        /// <summary>
        /// Clear all items contained in a list.
        /// </summary>
        /// <param name="optionName"></param>
        /// <returns></returns>
        public bool List_Clear(string optionName)
        {
            OptionsSchema result;

            if (OptionDefinitions.TryGetValue(optionName, out result) == false)
                return false;

            result.List_Clear();

            return true;
        }

        /// <summary>
        /// Create a new option that can hold a list of items.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="optionName"></param>
        /// <param name="type"></param>
        /// <param name="isOptional"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public IOptionsSchema List_CreateOption<T>(string optionName, Type type, bool isOptional, List<T> list)
        {
            var schema = OptionsSchema.CreateOptionsSchema<T>(optionName, type, isOptional, list);
            OptionDefinitions.Add(optionName, schema);
            IsDirty = true;

            return schema;
        }

        /// <summary>
        /// Gets a list of current values if this schema descripes a List.
        /// Return a single value schema as a list of 1 item.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object> List_GetListOfValues(string optionName)
        {
            var schema = GetOptionDefinition(optionName);

            if (schema == null)
                return null;

            return schema.List_GetListOfValues();
        }

        /// <summary>
        /// Gets a list of current keys and values if this schema
        /// descripes a List.
        /// 
        /// Return a single value schema as a list of 1 item.
        /// </summary>
        /// <param name="optionName"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<object, object>> List_GetListOfKeyValues(string optionName)
        {
            var schema = GetOptionDefinition(optionName);

            if (schema == null)
                return null;

            return schema.List_GetListOfKeyValues();
        }
        #endregion List related methods

        /// <summary>
        /// Resets the IsDirty flag to false to indicate that the current
        /// data was not changed/edited by a user request, yet. This is
        /// useful after defining a new options model and starting to work
        /// with it, as well, as after reading options from persistance...
        /// </summary>
        /// <param name="isDirty"></param>
        public void SetUndirty(bool isDirty)
        {
            IsDirty = isDirty;
        }

        /// <summary>
        /// Add another option and its definition (schema) into this option group.
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="type"></param>
        /// <param name="IsOptional"></param>
        /// <param name="value"></param>
        internal IOptionsSchema AddOption(string optionName, Type type, bool IsOptional, object value)
        {
            var schema = new OptionsSchema(optionName, type, IsOptional, value);
            OptionDefinitions.Add(optionName, schema);
            IsDirty = true;

            return schema;
        }

        /// <summary>
        /// Removes the option and value with the specified key
        /// (option name) from the available list of options.
        /// </summary>
        /// <param name="optionName"></param>
        /// <returns></returns>
        internal bool RemoveOptionDefinition(string optionName)
        {
            return OptionDefinitions.Remove(optionName);
        }
        #endregion methods
    }
}
