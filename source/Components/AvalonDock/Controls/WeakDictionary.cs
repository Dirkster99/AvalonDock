/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Collections.Generic;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Implements a dictionary class that uses weak references for keys
	/// while values are referenced with normal references.
	/// </summary>
	/// <typeparam name="K"></typeparam>
	/// <typeparam name="V"></typeparam>
	internal class WeakDictionary<K, V> where K : class
	{
		#region fields
		private List<WeakReference> _keys = new List<WeakReference>();
		private List<V> _values = new List<V>();
		#endregion fields

		#region Public Methods

		/// <summary>
		/// Get a value by its key index.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public V this[K key]
		{
			get
			{
				V valueToReturn;
				if (!GetValue(key, out valueToReturn))
					throw new ArgumentException();
				return valueToReturn;
			}
			set
			{
				SetValue(key, value);
			}
		}

		/// <summary>
		/// Gets whether a <paramref name="key"/> is included in the dictionary or not.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool ContainsKey(K key)
		{
			CollectGarbage();
			return -1 != _keys.FindIndex(k => k.GetValueOrDefault<K>() == key);
		}

		/// <summary>
		/// Set the <paramref name="value"/> for a given <paramref name="key"/>.
		/// Either
		/// - inserts both key and value pair if key was not present or
		/// - resets the value only if key was already present.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void SetValue(K key, V value)
		{
			CollectGarbage();
			int vIndex = _keys.FindIndex(k => k.GetValueOrDefault<K>() == key);
			if (vIndex > -1)
				_values[vIndex] = value;
			else
			{
				_values.Add(value);
				_keys.Add(new WeakReference(key));
			}
		}

		/// <summary>
		/// Get whether a key value pair exists and return its <paramref name="value"/> if so.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns>True if key exists in the collection, otherwise false.</returns>
		public bool GetValue(K key, out V value)
		{
			CollectGarbage();
			int vIndex = _keys.FindIndex(k => k.GetValueOrDefault<K>() == key);
			value = default(V);
			if (vIndex == -1)
				return false;
			value = _values[vIndex];
			return true;
		}

		#endregion Public Methods

		#region Private Methods

		/// <summary>
		/// Removes all entries where the key has already been garbage collected.
		/// </summary>
		private void CollectGarbage()
		{
			int vIndex = 0;

			do
			{
				vIndex = _keys.FindIndex(vIndex, k => !k.IsAlive);
				if (vIndex >= 0)
				{
					_keys.RemoveAt(vIndex);
					_values.RemoveAt(vIndex);
				}
			}
			while (vIndex >= 0);
		}

		#endregion Private Methods
	}
}
