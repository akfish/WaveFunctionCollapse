using System.Linq;
#nullable enable

namespace Schema {
  public partial class TileSet {
    public Subset? FindSubsetByName(string name) {
      return Subsets.FirstOrDefault(s => s.name == name);
    }
  }

  public partial class Subset {
    public Tile? FindTileByName(string name) {
      return Items.FirstOrDefault(t => t.name == name);
    }

    public bool Contains(string name) {
      return FindTileByName(name) != null;
    }
  }
}