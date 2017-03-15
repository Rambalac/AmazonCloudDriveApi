namespace Azi.Amazon.CloudDrive
{
    using System.Collections.Generic;

    /// <summary>
    /// Key to Value
    /// </summary>
    /// <typeparam name="TKey">Kye type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public class KeyToValue<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue>[] dictionaries;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyToValue{TKey, TValue}"/> class.
        /// Create new KeyToStore
        /// </summary>
        /// <param name="dictionaries">Dictionaries for key to value</param>
        public KeyToValue(params IDictionary<TKey, TValue>[] dictionaries)
        {
            this.dictionaries = dictionaries;
        }

        /// <summary>
        /// Returns value by key or null if missing
        /// </summary>
        /// <param name="key">Key to value</param>
        public TValue this[TKey key]
        {
            get
            {
                foreach (var dic in dictionaries)
                {
                    if (dic.TryGetValue(key, out var value))
                    {
                        return value;
                    }
                }

                return default(TValue);
            }
        }
    }
}
