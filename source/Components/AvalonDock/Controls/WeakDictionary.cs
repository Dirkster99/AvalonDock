using System;
using System.Collections.Generic;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the weak dictionary.
	/// </summary>
	/// <typeparam name="K">The type of k.</typeparam>
	/// <typeparam name="V">The type of v.</typeparam>
	internal class WeakDictionary<K, V>
		where K : class
	{
		private List<WeakReference> _keys = new List<WeakReference>();
		private List<V> _values = new List<V>();

		/// <summary>
		/// Gets or sets the value associated with the specified index.
		/// </summary>
		/// <param name="key">The key.</param>
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
		/// Contains key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>true if the collection contains the specified item; otherwise, false.</returns>
		public bool ContainsKey(K key)
		{
			CollectGarbage();
			return -1 != _keys.FindIndex(k => k.GetValueOrDefault<K>() == key);
		}

		/// <summary>
		/// Sets the value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public void SetValue(K key, V value)
		{
			CollectGarbage();
			int vIndex = _keys.FindIndex(k => k.GetValueOrDefault<K>() == key);
			if (vIndex > -1)
			{
				_values[vIndex] = value;
			}
			else
			{
				_values.Add(value);
				_keys.Add(new WeakReference(key));
			}
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns>true if the operation for get value succeeds; otherwise, false.</returns>
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
	}
}