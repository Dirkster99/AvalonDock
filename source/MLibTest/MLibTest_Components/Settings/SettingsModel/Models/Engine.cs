namespace SettingsModel
{
	using SettingsModel.Interfaces;
	using SettingsModel.Models.XML;
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Defines an interface to an object that implements an OptionsGroup/Options management engine.
	/// Thats an engine that keeps track of simple options like 'Reload files from last session (checkbox)',
	/// has means to define them, show their state when asked, and can store and retrive options in XML.
	/// </summary>
	internal class OptionsEngine : Interfaces.IEngine
	{
		#region fields
		private readonly Dictionary<string, OptionGroup> mOptionGroups = new Dictionary<string, OptionGroup>();

		private bool mIsDirty = false;
		#endregion fields

		#region constructor
		/// <summary>
		/// Class constructor
		/// </summary>
		public OptionsEngine()
		{
		}
		#endregion constructor

		#region properties
		/// <summary>
		/// Gets whether any of the properties stored in any options group have changed or not.
		/// </summary>
		public bool IsDirty
		{
			get
			{
				// Compute and return complex IsDirty property for whole structure if available
				if (mOptionGroups != null)
				{
					if (mOptionGroups.Count > 0)
					{
						bool isDirty = mIsDirty;

						foreach (var item in mOptionGroups)
							isDirty = isDirty | item.Value.IsDirty;

						return isDirty;
					}
				}

				// Return simple IsDirty property if nothing else's available
				return mIsDirty;
			}

			private set
			{
				if (mIsDirty != value)
					mIsDirty = value;

				if (mOptionGroups != null)
				{
					foreach (var item in mOptionGroups)
						item.Value.SetUndirty(value);
				}
			}
		}
		#endregion properties

		#region methods
		public override int GetHashCode()
		{
			return base.GetHashCode() | mOptionGroups.GetHashCode();
		}

		/// <summary>
		/// Determines whether the specified System.Object is equal to the current System.Object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>
		/// true if the specified System.Object is equal to the current System.Object;
		/// otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			var compObj = obj as IEngine;

			if (compObj == null)
				return false;

			string objXML = compObj.WriteXML();
			string thisXML = this.WriteXML();

			return string.Compare(objXML, thisXML, false) == 0;
		}

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
		public IOptionsSchema AddOption(string nameOfOptionGroup, string optionName, Type type, bool isOptional, object value)
		{
			string message;

			if (string.IsNullOrEmpty(message = CheckForValidName(nameOfOptionGroup)) == false)
				throw new Exception(message);

			if (string.IsNullOrEmpty(message = CheckForValidName(optionName)) == false)
				throw new Exception(message);

			OptionGroup opgroup;
			var option = mOptionGroups.TryGetValue(nameOfOptionGroup, out opgroup);

			// Create a new option group if this one is not present, yet
			if (opgroup == null)
			{
				opgroup = new OptionGroup(nameOfOptionGroup);
				mOptionGroups.Add(nameOfOptionGroup, opgroup);

				IsDirty = true;
			}

			return opgroup.AddOption(optionName, type, isOptional, value);
		}

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
		public IOptionsSchema AddListOption<T>(string nameOfOptionGroup,
							string optionName,
							Type type,
							bool isOptional, List<T> list)
		{
			OptionGroup opgroup;
			var option = mOptionGroups.TryGetValue(nameOfOptionGroup, out opgroup);

			if (opgroup == null) // Create a new option group if this one is not present, yet
			{
				opgroup = new OptionGroup(nameOfOptionGroup);
				mOptionGroups.Add(nameOfOptionGroup, opgroup);

				IsDirty = true;
			}

			return opgroup.List_CreateOption<T>(optionName, type, isOptional, list);
		}

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
		public bool GetOptionValue(string nameOfOptionGroup, string optionName, out object optValue)
		{
			optValue = null;

			var optionGroup = GetOptionGroup(nameOfOptionGroup);

			if (optionGroup == null)
				return false;

			return optionGroup.GetValue(optionName, out optValue);
		}

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
		public object GetOptionValue(string nameOfOptionGroup, string optionName)
		{
			object optValue;

			if (GetOptionValue(nameOfOptionGroup, optionName, out optValue) == false)
				throw new Exception(string.Format("The application option {0}-{1} cannot be located.",
												  nameOfOptionGroup, optionName));

			return optValue;
		}

		/// <summary>
		/// Gets the requested option and returns it as typed &lt;T> value.
		/// The method throws an exception if:
		///  - requested option value is not stored as typed &lt;T> or
		///  - the <seealso cref="OptionGroup"/> and option name does not exist.
		/// </summary>
		/// <param name="nameOfOptionGroup"></param>
		/// <param name="optionName"></param>
		/// <returns>current value of this option.</returns>
		public T GetOptionValue<T>(string nameOfOptionGroup, string optionName)
		{
			object optValue = GetOptionValue(nameOfOptionGroup, optionName);

			if ((optValue is T) == false)
				throw new Exception(string.Format("The requested option {0}-{1} is not of requested type <T>.",
												  nameOfOptionGroup, optionName));

			return (T)optValue;
		}

		/// <summary>
		/// Sets the value of a given option in this option table.
		/// </summary>
		/// <param name="nameOfOptionGroup"></param>
		/// <param name="optionName"></param>
		/// <param name="newOptValue"></param>
		/// <returns></returns>
		public bool SetOptionValue(string nameOfOptionGroup,
			string optionName, object newOptValue)
		{
			var optionGroup = GetOptionGroup(nameOfOptionGroup);

			if (optionGroup == null)
				return false;

			return optionGroup.SetValue(optionName, newOptValue);
		}

		/// <summary>
		/// Iterates through all <seealso cref="IOptionGroup"/> items stored in this object
		/// and gets each of these items at a time.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IOptionGroup> GetOptionGroups()
		{
			if (mOptionGroups != null)
			{
				foreach (var item in mOptionGroups)
					yield return item.Value;
			}
		}

		/// <summary>
		/// Gets a set of option grouped in 1 object for simplified access.
		/// </summary>
		/// <param name="nameOfOptionGroup"></param>
		/// <returns></returns>
		public IOptionGroup GetOptionGroup(string nameOfOptionGroup)
		{
			OptionGroup result = null;

			if (mOptionGroups.TryGetValue(nameOfOptionGroup, out result) == false)
				return null;

			return result;
		}

		/// <summary>
		/// Resets the IsDirty flag to false to indicate that the current
		/// data was not changed/edited by a user request, yet. This is
		/// useful after defining a new options model and starting to work
		/// with it, as well, as after reading options from persistance...
		/// </summary>
		public void SetUndirty()
		{
			if (mOptionGroups == null)
				return;

			// Reset all dirty states of option groups stored in this engine
			foreach (var item in this.mOptionGroups)
				item.Value.SetUndirty(false);

			IsDirty = false;
		}

		/// <summary>
		/// Remove a complete options group from the current model
		/// (this will remove multiple settings at once). Use with extreem care.
		/// Calling this methid is normally not required but the function is
		/// provided for completeness.
		/// </summary>
		/// <param name="nameOfOptionGroup"></param>
		/// <returns></returns>
		bool Interfaces.IEngine.RemoveOptionsGroup(string nameOfOptionGroup)
		{
			OptionGroup opgroup;
			if (mOptionGroups.TryGetValue(nameOfOptionGroup, out opgroup) == false)
				return false;

			mOptionGroups.Remove(nameOfOptionGroup);

			return true;
		}

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
		bool Interfaces.IEngine.RemoveOption(string nameOfOptionGroup, string optionName)
		{
			OptionGroup opgroup;
			if (mOptionGroups.TryGetValue(nameOfOptionGroup, out opgroup) == false)
				return false;

			return opgroup.RemoveOptionDefinition(optionName);
		}

		/// <summary>
		/// Removes all option groups and their options and values manged in this object.
		/// </summary>
		void Interfaces.IEngine.RemoveAllOptions()
		{
			mOptionGroups.Clear();
		}

		#region XML
		/// <summary>
		/// Stores option groups and their values in an XML file and sets the
		/// IsDirty flag to false.
		/// </summary>
		/// <param name="fileName"></param>
		void IEngine.WriteXML(string fileName)
		{
			var xmlWriter = new XMLLayer();
			xmlWriter.WriteXML(fileName, this);
			IsDirty = false;
		}

		/// <summary>
		/// Stores option groups and their values in a string containing XML
		/// formatted data and sets the IsDirty flag to false.
		/// </summary>
		/// <returns></returns>
		public string WriteXML()
		{
			var xmlWriter = new XMLLayer();
			string result = xmlWriter.WriteXML(this);
			IsDirty = false;

			return result;
		}

		/// <summary>
		/// Read option groups and their values from an XML file.
		/// </summary>
		/// <param name="fileName"></param>
		void IEngine.ReadXML(string fileName)
		{
			try
			{
				var xmlReader = new XMLLayer();
				xmlReader.ReadXML(fileName, this);
				IsDirty = false;
			}
			catch
			{
			}
		}

		/// <summary>
		/// Use this class to read XML from strings via <seealso cref="System.IO.StringReader"/>.
		/// </summary>
		/// <param name="reader"></param>
		void IEngine.ReadXML(System.IO.TextReader reader)
		{
			var xmlReader = new XMLLayer();
			xmlReader.ReadXML(reader, this);
			IsDirty = false;
		}
		#endregion XML

		/// <summary>
		/// Checks if the name contains reserved characters and
		/// returns an error message string if thats the case.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		private string CheckForValidName(string name)
		{
			if (string.IsNullOrEmpty(name) == true)
				return string.Format("The '{0}' name cannot be empty or null", name);

			foreach (var item in XMLLayer.ResvedOptionListCharacters)
			{
				if (name.Contains(string.Format("{0}", item)))
					return string.Format("The '{0}' name is not valid since it contains the '{1}' character" +
										 "\n(this character is reserved for internal usage only).", name, item);
			}

			return null;
		}
		#endregion methods
	}
}
