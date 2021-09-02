using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Ez.Enif
{
    public class Session : IReadOnlyDictionary<string, Session>
    {
        private readonly IDictionary<string, Session> _sessions;

        public Session()
        {
            _sessions = new Dictionary<string, Session>();
            Properties = new Dictionary<string, Property>();
        }

        public Session this[string key] => _sessions[key];

        public IDictionary<string, Property> Properties { get; }

        public IEnumerable<string> Keys => _sessions.Keys;

        public IEnumerable<Session> Values => _sessions.Values;

        public int Count => _sessions.Count;

        public bool ContainsKey(string key) => _sessions.ContainsKey(key);

        public bool TryCreate(string name, [MaybeNullWhen(false)] out Session session)
        {
            session = null;
            if (name.Contains('.'))
                return false;

            if (_sessions.ContainsKey(name))
                return false;

            _sessions[name] = session = new();
            return true;
        }

        public bool TryGetValue(string name, [MaybeNullWhen(false)] out Session value) => _sessions.TryGetValue(name, out value);

        public bool Remove(string name) => _sessions.Remove(name);

        public IEnumerator<KeyValuePair<string, Session>> GetEnumerator() => _sessions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
