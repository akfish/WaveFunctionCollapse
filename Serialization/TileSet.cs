using System;
using System.IO;
using System.Xml.Serialization;
using Schema;

namespace Serialization {
  class TileSetSerializer {
    private XmlSerializer _serializer;
    public TileSetSerializer() {
      _serializer = new XmlSerializer(typeof(TileSet));
    }
    public TileSet Deserialize(System.IO.Stream stream) {
      return (TileSet)_serializer.Deserialize(stream);
    }

    internal TileSet Deserialize(string filename)
    {
      return Deserialize(System.IO.File.OpenRead(filename));
    }

    internal void Serialize(TextWriter writer, TileSet tileSet)
    {
      _serializer.Serialize(writer, tileSet);
    }
  }
}