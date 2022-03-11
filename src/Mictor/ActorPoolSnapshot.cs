using System.Collections;
using System.Collections.Generic;

namespace Mictor
{
    /// <summary>
    /// A state snapshot of a collection of actors
    /// </summary>
    public class ActorPoolSnapshot : IReadOnlyDictionary<string, ActorSnapshot>
    {
        private readonly IReadOnlyDictionary<string, ActorSnapshot> _dictionary;

        /// <summary>
        /// Creates a new <see cref="ActorPoolSnapshot"/>
        /// </summary>
        public ActorPoolSnapshot(IReadOnlyDictionary<string, ActorSnapshot> dictionary)
        {
            _dictionary = dictionary;
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, ActorSnapshot>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public int Count => _dictionary.Count;

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out ActorSnapshot value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public ActorSnapshot this[string key] => _dictionary[key];

        /// <inheritdoc />
        public IEnumerable<string> Keys => _dictionary.Keys;

        /// <inheritdoc />
        public IEnumerable<ActorSnapshot> Values => _dictionary.Values;
    }
}