namespace SettingsModel.Models.XML
{
	using SettingsModel.Interfaces;
	using SettingsModel.Models.XML.Converters;
	using System;
	using System.Data;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Security;

	/// <summary>
	/// Host API necessary to read and write settings model related data
	/// from and to XML file storage.
	/// </summary>
	internal class XMLLayer
	{
		#region fields
		public readonly static char[] ResvedOptionListCharacters = new char[] { '$', '{', '}' };
		#endregion fields

		/// <summary>
		/// Write current settings stored and manage in <paramref name="engine"/>
		/// into an XML file as hinted by <paramref name="fileName"/>.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="engine"></param>
		public void WriteXML(string fileName, IEngine engine)
		{
			try
			{
				using (DataSet dataSet = ConvertFromModelToDataSet(engine))
				{
					dataSet.WriteXml(fileName);
				}
			}
			catch (Exception exp)
			{
				Debug.WriteLine("Failed to read program options from persistence.", exp);
				throw;
			}
		}

		/// <summary>
		/// Write XML represantation of option
		/// settings into string and return it.
		/// </summary>
		/// <param name="engine"></param>
		/// <returns></returns>
		public string WriteXML(IEngine engine)
		{
			try
			{
				using (StringWriter writer = new StringWriter())
				{
					using (DataSet dataSet = ConvertFromModelToDataSet(engine))
					{
						dataSet.WriteXml(writer);
					}

					return writer.ToString();
				}
			}
			catch (Exception exp)
			{
				Debug.WriteLine("Failed to read program options from persistence.", exp);
				throw;
			}
		}

		/// <summary>
		/// Read current settings for storage and management in <paramref name="engine"/>
		/// from an XML file as hinted by <paramref name="fileName"/>.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="engine"></param>
		public void ReadXML(string fileName, IEngine engine)
		{
			try
			{
				if (System.IO.File.Exists(fileName) == false)
					return;

				// Do this conversion to generate an implecite schema
				// that can be used by the DataSet to associate each XML item
				// with correct table, row, and column ...
				using (DataSet dataSet = ConvertFromModelToDataSet(engine))
				{
					dataSet.ReadXml(fileName);

					// Transfer data from DataSet, DataTables into internal representation
					// Convert external format into internal format
					ConvertFromDataSetToModel(engine, dataSet);
				}
			}
			catch (Exception exp)
			{
				Debug.WriteLine("Failed to read program options from persistence.", exp);
				throw;
			}
		}

		/// <summary>
		/// Read current settings for storage and management in <paramref name="engine"/>
		/// from an XML <seealso cref="TextReader"/> object as hinted by <paramref name="reader"/>.
		/// Use this method to read XML from strings via <seealso cref="StringReader"/> class.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="engine"></param>
		public void ReadXML(TextReader reader, IEngine engine)
		{
			try
			{
				// Do this conversion to generate an implecite schema
				// that can be used by the DataSet to associate each XML item
				// with correct table, row, and column ...
				using (DataSet dataSet = ConvertFromModelToDataSet(engine))
				{
					// Do this conversion to generate an implecite schema
					// that can be used by the DataSet to associate each XML item
					// with correct table, row, and column ...
					dataSet.ReadXml(reader);

					// Transfer data from DataSet, DataTables into internal representation
					// Convert external format into internal format
					ConvertFromDataSetToModel(engine, dataSet);
				}
			}
			catch (Exception exp)
			{
				Debug.WriteLine("Failed to read program options from persistence.", exp);
				throw;
			}
		}

		/// <summary>
		/// Builds a <seealso cref="DataSet"/> with <seealso cref="DataTable"/> items
		/// and <seealso cref="DataRow"/> items for the current schema of options and
		/// option values for data write XML to persistence.
		/// </summary>
		/// <param name="engine"></param>
		/// <returns></returns>
		private DataSet ConvertFromModelToDataSet(IEngine engine)
		{
			DataSet mDataSet = new DataSet();
			DataTable dataTable = null;
			AlternativeDataTypeHandler handle = new AlternativeDataTypeHandler();

			foreach (var optionsGroup in engine.GetOptionGroups())
			{
				// Create a new table (per Option group) and add a data row
				dataTable = CreateTable(optionsGroup);

				mDataSet.Tables.Add(dataTable);
				DataRow row = dataTable.NewRow();

				// Fill data row with data to be stored
				foreach (var optionDefinition in optionsGroup.GetOptionDefinitions())
				{
					var handler = handle.FindHandler(optionDefinition.TypeOfValue);

					if (optionDefinition.SchemaType == OptionSchemaType.SingleValue)
					{
						if (handler != null)
							row[optionDefinition.OptionName] = handler.Convert(optionDefinition.Value as SecureString);
						else
							row[optionDefinition.OptionName] = optionDefinition.Value;
					}
					else
					{
						// Create a unique id-able name for this list item and then create the list item table with that name
						var tableName = CreateListItemTableName(optionsGroup, optionDefinition);
						var listTable = CreateListTable(tableName, optionDefinition, handler, row);

						mDataSet.Tables.Add(listTable);

						////ForeignKeyConstraint custOrderFK = new ForeignKeyConstraint(optionsGroup.Name + "_" + optionDefinition.OptionName + "_FK",
						////                                                            listTable.Columns[optionDefinition.OptionName],
						////                                                            dataTable.Columns[optionDefinition.OptionName]);
						////custOrderFK.DeleteRule = Rule.None;
						////
						////// Cannot delete a customer value that has associated existing orders.
						////dataTable.Constraints.Add(custOrderFK);
					}
				}

				// Check if last data row has data and add it with table in collection, if so
				if (row.ItemArray.Count() > 0)
				{
					dataTable.Rows.Add(row);
				}
			}

			return mDataSet;
		}

		/// <summary>
		/// Method transfers data format from <seealso cref="DataSet"/> with <seealso cref="DataTable"/> items
		/// and <seealso cref="DataRow"/> items into the current schema of options
		/// and option values for data read from XML persistence into the in-memory model.
		/// Converts external XML format into internal format.
		/// </summary>
		/// <param name="engine"></param>
		/// <param name="dataSet"></param>
		private void ConvertFromDataSetToModel(IEngine engine, DataSet dataSet)
		{
			AlternativeDataTypeHandler handle = new AlternativeDataTypeHandler();

			foreach (DataTable table in dataSet.Tables)
			{
				string optionName;

				// Locate options group for this table
				IOptionGroup optionsGroup = FindOptionsGroup(engine, table.TableName, out optionName);

				// Found a table that refers to a list of values stored in an optiongroup/option
				if (string.IsNullOrEmpty(optionName) == false)
				{
					// Empty list is allowed but requires no processing
					// -> remove defaults if actual data is available
					if (table.Rows.Count > 0)
					{
						optionsGroup.List_Clear(optionName);

						for (int i = 0; i < table.Rows.Count; i++)
						{
							// assumption: Address first column in each row since list
							// table contains only 1 data column
							optionsGroup.SetValue(optionName, table.Rows[i].ItemArray[0]);
						}
					}
				}
				else
				{
					// This code requires at least a header row and 1 data row in order to work!
					if (table.Rows.Count >= 2)
					{
						for (int i = 0; i < table.Columns.Count; i++)
						{
							IOptionsSchema schema = null;

							schema = optionsGroup.GetOptionDefinition(table.Columns[i].ColumnName);

							var handler = handle.FindHandler(schema.TypeOfValue);

							if (handler != null)
							{
								object s = handler.ConvertBack(table.Rows[1].ItemArray[i] as string);

								optionsGroup.SetValue(table.Columns[i].ColumnName, s);
							}
							else
							{
								optionsGroup.SetValue(table.Columns[i].ColumnName, table.Rows[1].ItemArray[i]);
							}
						}
					}
				}
			}

			dataSet = null;
		}

		/// <summary>
		/// Creates a <seealso cref="DataTable"/>.
		/// and adds corresponding column definitions.
		/// </summary>
		/// <param name="tableSchema"></param>
		/// <returns></returns>
		private static DataTable CreateTable(IOptionGroup tableSchema)
		{
			DataTable dataTable = new DataTable(tableSchema.Name);

			foreach (var item in tableSchema.GetOptionDefinitions())
			{
				DataColumn col = new DataColumn(item.OptionName, (item.TypeOfValue == typeof(SecureString) ? typeof(string) : item.TypeOfValue));
				col.AllowDBNull = item.IsOptional;

				dataTable.Columns.Add(col);
			}

			return dataTable;
		}

		/// <summary>
		/// Creates a <seealso cref="DataTable"/> object with 1 column and n rows
		/// that represnts the values of a list of values.
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="columnSchema"></param>
		/// <param name="handler"></param>
		/// <param name="masterRow"></param>
		/// <returns></returns>
		private static DataTable CreateListTable(string tableName,
												 IOptionsSchema columnSchema,
												 IAlternativeDataTypeHandler handler,
												 DataRow masterRow)
		{
			DataTable dataTable = new DataTable(tableName);

			DataColumn col = new DataColumn(columnSchema.OptionName,
											(columnSchema.TypeOfValue == typeof(SecureString) ? typeof(string) :
																								columnSchema.TypeOfValue));

			col.AllowDBNull = columnSchema.IsOptional;
			dataTable.Columns.Add(col);

			////var list = columnSchema.Value as IEnumerable<object>;

			bool IsFirstRow = true;

			foreach (var item in columnSchema.List_GetListOfValues())
			{
				if (IsFirstRow == true)
				{
					IsFirstRow = false;

					// Initialize row for caller this must be non-null to make it storeable
					if (handler != null)
						masterRow[columnSchema.OptionName] = handler.Convert(item as SecureString);
					else
						masterRow[columnSchema.OptionName] = item;
				}


				var row = dataTable.NewRow();

				if (handler != null)
					row[columnSchema.OptionName] = handler.Convert(item as SecureString);
				else
					row[columnSchema.OptionName] = item;

				dataTable.Rows.Add(row);
			}

			return dataTable;
		}

		/// <summary>
		/// Attempts to map a table name into an OptionsGroup which is return as
		/// <see cref="IOptionGroup"/> interface.
		/// 
		/// This includes lists of values that are stored inside
		/// an option in an optiongroup (optionName is set in this case).
		/// </summary>
		/// <param name="engine"></param>
		/// <param name="tableName"></param>
		/// <param name="optionName"></param>
		/// <returns></returns>
		private IOptionGroup FindOptionsGroup(IEngine engine,
											  string tableName,
											  out string optionName)
		{
			string groupName = string.Empty;
			optionName = string.Empty;

			// Locate options group for this table
			IOptionGroup optionsGroup = engine.GetOptionGroup(tableName);

			// Table may not have an options group if it is a list of values belonging to an options
			// Search for options group that has this field name as reference to this list
			if (optionsGroup == null)
			{
				if (TryResolveOptionsGroupAndOptionName(tableName, out groupName, out optionName) == true)
					throw new Exception(string.Format("Unknown item detected. Cannot resolve {0}.", tableName));

				optionsGroup = engine.GetOptionGroup(groupName);
			}

			if (optionsGroup == null)
				throw new Exception(string.Format("Cannot resolve table name: {0}", tableName));

			return optionsGroup;
		}

		/// <summary>
		/// Creates a unique id-able name for list items that are indirectly attached to OptionsGroups
		/// but are physically stored in their own table (to enable storage of multiple values in XML).
		/// </summary>
		/// <param name="optionsGroup"></param>
		/// <param name="optionSchema"></param>
		/// <returns></returns>
		private string CreateListItemTableName(IOptionGroup optionsGroup, IOptionsSchema optionSchema)
		{
			return "${" + optionsGroup.Name + "}${" + optionSchema.OptionName + "}";
		}

		/// <summary>
		/// Options list are coded in XML with special characters. This method is used
		/// to resolve the coding based on the given parameter.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="groupName"></param>
		/// <param name="optionName"></param>
		/// <returns></returns>
		private bool TryResolveOptionsGroupAndOptionName(string name, out string groupName, out string optionName)
		{
			groupName = string.Empty;
			optionName = string.Empty;
			string[] items = name.Split(ResvedOptionListCharacters);

			foreach (var item in items)
			{
				if (string.IsNullOrEmpty(item) == true)
					continue;

				if (string.IsNullOrEmpty(groupName) == true)
				{
					groupName = item;
					continue;
				}
				else
				{
					// We resolved more parts of the equation than required
					// (information if not formatted correctly)
					if (string.IsNullOrEmpty(optionName) == false)
						throw new NotSupportedException(name);

					optionName = item;
					continue;
				}
			}

			// We could resolve none or not all parts of the equation
			if (string.IsNullOrEmpty(optionName) == true || string.IsNullOrEmpty(groupName) == true)
				throw new NotSupportedException(name);

			return false;
		}
	}
}
