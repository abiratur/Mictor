using System.Collections;
using System.Collections.Generic;

namespace Mictor
{
    public class ActorPoolSnapshot : IReadOnlyDictionary<string, ActorSnapshot>
    {
        private readonly IReadOnlyDictionary<string, ActorSnapshot> _dictionary;

        public ActorPoolSnapshot(IReadOnlyDictionary<string, ActorSnapshot> dictionary)
        {
            _dictionary = dictionary;
        }

        public IEnumerator<KeyValuePair<string, ActorSnapshot>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _dictionary.Count;

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool TryGetValue(string key, out ActorSnapshot value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public ActorSnapshot this[string key] => _dictionary[key];

        public IEnumerable<string> Keys => _dictionary.Keys;

        public IEnumerable<ActorSnapshot> Values => _dictionary.Values;
    }
}