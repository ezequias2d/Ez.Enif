using System;
using Xunit;

namespace Ez.Enif.Tests
{
    public class EnifTests
    {
        [Fact]
        public void BasicEnifFileTest()
        {
            var enifFile = @"[ENIF]
version=0.1.0
name=ENIF Test
number=123.123
str='string  test'
a=2
b=1
c=.b/.a
";
        }

        [Fact]
        public void SessionTest()
        {
            var sessionTest =
@"[ENIF]
version=0.1.0

[session1]
value='temp'

[session1.subsession1]
property=1
";
            var root = Read(sessionTest);

            Assert.True(root.ContainsKey("ENIF"));
            var enif = root["ENIF"];

            Assert.True(root.ContainsKey("session1"));
            var session1 = root["session1"];

            Assert.True(session1.ContainsKey("subsession1"));
            var subsession1 = session1["subsession1"];

            Assert.True(enif.Properties.ContainsKey("version"));
            var version = enif.Properties["version"];
            Assert.Equal("0.1.0", version.ToString());

            Assert.True(session1.Properties.ContainsKey("value"));
            var value = session1.Properties["value"];
            Assert.Equal("temp", value.ToString());

            Assert.True(subsession1.Properties.ContainsKey("property"));
            var property = subsession1.Properties["property"];
            Assert.Equal("1", property.ToString());
        }

        private Session Read(string enifString)
        {
            var context = new EnifManager();
            return context.Read(enifString);
        }
    }
}
