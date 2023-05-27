using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Schema;

#nullable enable

namespace Serialization
{
  class SchemaSerializer<T>
  {
    private XmlSerializer _serializer;
    public SchemaSerializer()
    {
      _serializer = new XmlSerializer(typeof(T));
    }
    public T? Deserialize(System.IO.Stream stream)
    {
      return (T?)_serializer.Deserialize(stream);
    }

    internal T? Deserialize(string filename)
    {
      return Deserialize(System.IO.File.OpenRead(filename));
    }

    internal void Serialize(TextWriter writer, T obj)
    {
      _serializer.Serialize(writer, obj);
    }
    internal void Serialize(XmlWriter writer, T obj)
    {
      _serializer.Serialize(writer, obj);
    }

    internal void Serialize(Stream stream, T obj)
    {
      _serializer.Serialize(stream, obj);
    }
  }
}