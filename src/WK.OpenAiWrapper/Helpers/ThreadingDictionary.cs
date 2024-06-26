using System.Collections.Concurrent;

namespace WK.OpenAiWrapper.Helpers;

/// <summary>
/// Class ThreadingDictionary.
/// </summary>
/// <typeparam name="TK">The type of the tk.</typeparam>
/// <typeparam name="TV">The type of the tv.</typeparam>
/// <seealso cref="System.Collections.Concurrent.ConcurrentDictionary{TK, TV}" />/>
public class ThreadingDictionary<TK, TV> : ConcurrentDictionary<TK, TV>, IDisposable
{
    /// <summary>
    /// Gets or sets the try counter.
    /// </summary>
    /// <value>The try counter.</value>
    public int TryCounter { get; set; } = 100;

    /// <summary>
    /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
    /// </summary>
    /// <param name="key">The object to use as the key of the element to add.</param>
    /// <param name="value">The object to use as the value of the element to add.</param>
    /// <param name="replace">Replace if key already exists</param>
    /// <exception cref="AccessViolationException">No access for adding {nameof(key)}</exception>
    /// <exception cref="System.AccessViolationException">Thrown if the key is not in the dictionary.</exception>
    public virtual void Add(TK key, TV value, bool replace = false)
    {
        if(ContainsKey(key))
            if(replace)
                RemoveKey(key);
            else
                return;
        int i = 0;

        while(i++ < TryCounter)
            if(TryAdd(key, value))
                break;

        if(!ContainsKey(key)) throw new AccessViolationException($"No access for adding {nameof(key)} to the ConcurrentDictionary.");
    }

    /// <summary>
    /// Adds the range.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <param name="replace">if set to <c>true</c> [replace].</param>
    public virtual void AddRange(IEnumerable<KeyValuePair<TK, TV>> collection, bool replace = false)
    {
        foreach(KeyValuePair<TK, TV> keyValuePair in collection) Add(keyValuePair.Key, keyValuePair.Value);
    }

    /// <summary>
    /// Removes the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <exception cref="AccessViolationException">No access for removing {nameof(key)}</exception>
    /// <exception cref="System.AccessViolationException">Thrown if the key is not in the dictionary.</exception>
    public virtual void RemoveKey(TK key)
    {
        if(!ContainsKey(key)) return;
        TV? value = default;
        int i = 0;

        while(i++ < TryCounter)
            if(TryRemove(key, out value))
                break;
        if(value is IDisposable) ((IDisposable) value).Dispose();

        if(ContainsKey(key)) throw new AccessViolationException($"No access for removing {nameof(key)} from the ConcurrentDictionary.");
    }

    /// <summary>
    /// Removes the value.
    /// </summary>
    /// <param name="value">The value.</param>
    public virtual void RemoveValues(TV value)
    {
        if(this.All(pair => !pair.Value.Equals(value))) return;
        foreach (var pair in this.Where(p => p.Value.Equals(value)))
        {
            RemoveKey(pair.Key);
        }
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>V.</returns>
    /// <exception cref="AccessViolationException">No access for getting value {key?.ToString() ?? nameof(key)}</exception>
    /// <exception cref="System.AccessViolationException">Thrown if the value is null.</exception>
    public virtual TV? GetValue(TK key)
    {
        TV? value = default;

        if(!ContainsKey(key)) return value;
        int i = 0;
        bool accessGranted = false;

        while(i++ < TryCounter)
        {
            if(TryGetValue(key, out value))
            {
                accessGranted = true;
                break;
            }
        }

        if(!accessGranted) throw new AccessViolationException($"No access for getting value {key?.ToString() ?? nameof(key)} from the ConcurrentDictionary.");

        return value;
    }

    #region Implementation of IDisposable

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        foreach(IDisposable value in Values.OfType<IDisposable>()) value.Dispose();
        Clear();
    }

    #endregion
}