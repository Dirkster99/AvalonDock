namespace SettingsModel.Interfaces
{
    using System;
using System.Collections.Generic;

    /// <summary>
    /// Defines an interface to an object that implements an OptionsGroup/Options management engine.
    /// Thats an engine that keeps track of simple options like 'Reload files from last session (checkbox)',
    /// has means to define them, show their state when asked, and can store and retrive options in XML.
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        /// Adds a new <paramref name="optionName"/> option into an existing option group
        /// or creates a new option group (if <paramref name="nameOfOptionGroup"/> does not
        /// exist yet) with one option as requested per <paramref name="optionName"/> parameter.
        /// </summary>
        /// <param name="nameOfOptionGroup"></param>
        /// <param name="optionName"></param>
        /// <param name="type"></param>
        /// <param name="isOptional"></param>
        /// <param name="value"></param>
        IOptionsSchema AddOption(string nameOfOptionGroup, string optionName, Type type, bool isOptional, object value);

        /// <summary>
        /// Adds a new <paramref name="optionName"/> option into an existing option group
        /// and list or creates a new option group (if <paramref name="nameOfOptionGroup"/> does not
        /// exist yet) with a list option as requested per <paramref name="optionName"/> parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nameOfOptionGroup"></param>
        /// <param name="optionName"></param>
        /// <param name="type"></param>
        /// <param name="isOptional"></param>
        /// <param name="list"></param>
        IOptionsSchema AddListOption<T>(string nameOfOptionGroup, string optionName, Type type, bool isOptional, List<T> list);

        /// <summary>
        /// Gets the value of an option in a given <seealso cref="OptionGroup"/> or null
        /// if either option or <seealso cref="OptionGroup"/> does not exist.
        /// 
        /// Method returns false if option and <seealso cref="OptionGroup"/> are not known.
        /// </summary>
        /// <param name="nameOfOptionGroup"></param>
        /// <param name="optionName"></param>
        /// <param name="optValue">Indicates whether option and <seealso cref="OptionGroup"/> exist or not.</param>
        /// <returns></returns>
        bool GetOptionValue(string nameOfOptionGroup, string optionName, out object optValue);

        /// <summary>
        /// Gets the value of an option in a given <seealso cref="OptionGroup"/> or
        /// throws an exception if either option or <seealso cref="OptionGroup"/>
        /// does not exist.
        /// 
        /// Method the requested option value if option and <seealso cref="OptionGroup"/> are known.
        /// </summary>
        /// <param name="nameOfOptionGroup"></param>
        /// <param name="optionName"></param>
        /// <returns></returns>
        object GetOptionValue(string nameOfOptionGroup, string optionName);

        /// <summary>
        /// Gets the requested option and returns it as typed &lt;T> value.
        /// The method throws an exception if:
        ///  - requested option value is not stored as typed &lt;T> or
        ///  - the <seealso cref="OptionGroup"/> and option name does not exist.
        /// </summary>
        /// <param name="nameOfOptionGroup"></param>
        /// <param name="optionName"></param>
        /// <returns>current value of this option.</returns>
        T GetOptionValue<T>(string nameOfOptionGroup, string optionName);

        /// <summary>
        /// Set an option to a given value.
        /// </summary>
        /// <param name="nameOfOptionGroup"></param>
        /// <param name="optionName"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        bool SetOptionValue(string nameOfOptionGroup, string optionName, object newValue);

        /// <summary>
        /// Iterates through all <seealso cref="IOptionGroup"/> items stored in this object
        /// and gets each of these items at a time.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IOptionGroup> GetOptionGroups();

        /// <summary>
        /// Gets a set of option grouped in 1 object for simplified access.
        /// </summary>
        /// <param name="nameOfOptionGroup"></param>
        /// <returns></returns>
        IOptionGroup GetOptionGroup(string nameOfOptionGroup);

        /// <summary>
        /// Store option groups and their values in an XML file.
        /// </summary>
        /// <param name="fileName"></param>
        void WriteXML(string fileName);

        /// <summary>
        /// Write settings into an XML string.
        /// </summary>
        /// <returns></returns>
        string WriteXML();

        /// <summary>
        /// Read option groups and their values from an XML file.
        /// </summary>
        /// <param name="fileName"></param>
        void ReadXML(string fileName);

        /// <summary>
        /// Use this class to read XML from strings via <seealso cref="System.IO.StringReader"/>.
        /// </summary>
        /// <param name="reader"></param>
        void ReadXML(System.IO.TextReader reader);

        /// <summary>
        /// Resets the IsDirty flag to false to indicate that the current
        /// data was not changed/edited by a user request, yet. This is
        /// useful after defining a new options model and starting to work
        /// with it, as well, as after reading options from persistance...
        /// </summary>
        void SetUndirty();

        /// <summary>
        /// Gets whether any of the properties stored in any options group have changed or not.
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Remove a complete options group from the current model
        /// (this will remove multiple settings at once). Use with extreem care.
        /// Calling this methid is normally not required but the function is
        /// provided for completeness.
        /// </summary>
        /// <param name="nameOfOptionGroup"></param>
        /// <returns></returns>
        bool RemoveOptionsGroup(string nameOfOptionGroup);

        /// <summary>
        /// Remove an option from the current model
        /// (this will remove multiple values at once if the
        /// requested option referes to a list of option values).
        /// 
        /// Use with extreem care.
        /// Calling this method is normally not required but
        /// the function is provided for completeness.
        /// </summary>
        /// <param name="nameOfOptionGroup"></param>
        /// <param name="optionName"></param>
        /// <returns></returns>
        bool RemoveOption(string nameOfOptionGroup, string optionName);

        /// <summary>
        /// Removes all options from the current model.
        /// </summary>
        void RemoveAllOptions();
    }
}