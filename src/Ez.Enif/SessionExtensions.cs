using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ez.Enif
{
    public static class SessionExtensions
    {
        public static Session CreateSessionPath(this Session session, string name)
        {
            name = Regex.Replace(name, @"\s+", "");
            var temp = name.Split('.', System.StringSplitOptions.RemoveEmptyEntries);

            foreach(var t in temp)
            {
                if (session.TryCreate(t, out var s))
                    session = s;
                else
                    session = session[t];
            }

            return session;
        }

        public static void AddValues(this Session session, string name, IEnumerable<IConvertible> values)
        {
            name = Regex.Replace(name, @"\s+", "");
            var temp = name.Split('.', System.StringSplitOptions.RemoveEmptyEntries);

            for(var i = 0; i < temp.Length - 1; i++)
            {
                var t = temp[i];
                if (session.TryCreate(t, out var s))
                    session = s;
                else
                    session = session[t];
            }

            var property = new Property(values);
            var propertyName = temp[temp.Length - 1];
            session.Properties.Add(propertyName, property);
        }
    }
}
