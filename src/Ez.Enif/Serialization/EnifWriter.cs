using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Linq;

namespace Ez.Enif.Serialization
{
    public sealed class EnifWriter : IDisposable
    {
        private readonly Queue<(string Name, object Value)> _queue;

        public EnifWriter(Session root)
        {
            Root = root;
            Version = GetType().Assembly.GetName().Version;
            _queue = new();
            Converters = new List<EnifConverter>(new EnifConverter[]
            {
                new PrimitiveConverter(),
                new ReferenceConverter(),
                new ArrayConverter(),
                new EnifTypeConverter(),
                new ValueConverter(),
            });
        }

        public Session Root { get; }
        public Version Version { get; }
        public ICollection<EnifConverter> Converters { get; }

        public void Dispose()
        {
            
        }

        public void Write(string name, object obj)
        {
            _queue.Enqueue((name, obj));
        }

        public void Write(Session session, string name, params object[] values)
        {

            Write(session, name, (IEnumerable<object>)values);
            //var type = value.GetType();

            //if (type.IsPrimitive || type.IsAssignableTo(typeof(string)))
            //    session.AddValues(name new[] { (IConvertible)value });
            //else
            //    InternalWrite(session, name, value);
        }

        public void Write(Session session, string name, IEnumerable<object> values)
        {
            var primitives = values.All((a) =>
            {
                var type = a.GetType();
                return type.IsPrimitive || type.IsAssignableTo(typeof(string));
            });

            if (primitives)
                session.AddValues(name, values.Cast<IConvertible>());
            else
                InternalWrite(session, name, values);
        }

        private void InternalWrite(Session session, string name, IEnumerable<object> values)
        {
            foreach (var converter in Converters)
            {
                if (converter.CanConvert(session, name, values.First()))
                {
                    converter.Write(this, session, name, values);
                    return;
                }
            }
        }

        public void Flush()
        {
            if (!Root.TryGetValue("ENIF", out _))
            {
                var enifSession = Root.CreateSessionPath("ENIF");
                enifSession.AddValues("version", new IConvertible[] { Version.Major, Version.Minor, Version.Build });
            }

            while (_queue.TryDequeue(out var temp))
            {
                Write(Root, temp.Name, temp.Value);
            }
        }
    }
}
