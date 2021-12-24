using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ikenpack;

public class Tileset {
    public string Name { get; set; }
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public int Cols { get; set; }
    public int Rows { get; set; }
    public Sprite[,] Sprites { get; set; }
    public bool[,] optimized { get; set; }
    public bool Parsed { get; set; } = false;

    /// <summary>
    /// Read a tileset from a file
    /// </summary>
    /// <param name="b"></param>
    /// <param name="atlas"></param>
    public Tileset(BinaryReader b, Atlas atlas) {
        LoadBinary(b, atlas);
    }

    public Tileset(string name, int tileWidth, int tileHeight, int cols, int rows, Sprite[,] sprites) {
        Name = name;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        Cols = cols;
        Rows = rows;
        Sprites = sprites;
    }

    public void LoadBinary(BinaryReader b, Atlas atlas) {
        Name = b.ReadString();
        TileWidth = b.ReadInt32();
        TileHeight = b.ReadInt32();
        Cols = b.ReadInt32();
        Rows = b.ReadInt32();

        Sprites = new Sprite[Cols, Rows];

        int n = b.ReadInt32();
        int id, x, y;
        for (int i = 0; i < n; ++i) {
            id = b.ReadInt32();
            x = id % Cols;
            y = id / Cols;
            Sprites[x, y] = atlas.GetSprite(Name + "_" + x + "_" + y);
        }

        if (b.ReadBoolean()) {
            optimized = new bool[Cols, Rows];
            for (y = 0; y < Rows; ++y)
                for (x = 0; x < Cols; ++x)
                    optimized[x, y] = b.ReadBoolean();
        }
    }


    internal void Save(BinaryWriter writer) {
        writer.Write(Name);              // string
        writer.Write(TileWidth);         // int
        writer.Write(TileHeight);        // int
        writer.Write(Cols);              // int
        writer.Write(Rows);              // int

        // for every sprite in the tileset, its "ID" is the index in the tilemap
        // eg.  0,  1,  2,  3,  4
        //      5,  6,  7,  8,  9
        //     10, 11, 12, 13, 14
        writer.Write(Sprites.Length);    // int
        for (int i = 0; i < Sprites.Length; i++)
            writer.Write(i);

        writer.Write(optimized != null);             // bool
        if (optimized != null)
            for (int y = 0; y < Rows; ++y)
                for (int x = 0; x < Cols; ++x)
                    writer.Write(optimized[x, y]);   // bool
    }

    /// <summary>
    /// Create directory and image files for every sprite
    /// </summary>
    /// <param name="directory">The directory for the sub directory</param>
    /// <param name="rawImg">Raw image where all the sprites are</param>
    public void Export(string directory, ImgImage rawImg) {
        directory = Path.Combine(directory, Name);
        if(!Directory.Exists(directory)) 
            Directory.CreateDirectory(directory);
        foreach (var sprite in Sprites) {
            if (sprite != null)
                sprite.Export(directory, rawImg);
        }
    }
    private void ExtendSprites(int dimension, int length) {
        Sprites = Sprites.Extend(dimension, length);
        if(optimized != null)
            optimized = optimized.Extend(dimension, length);
    }

    public void ExtendSpritesWidth(int addedAmount) => ExtendSprites(0, addedAmount);
    public void ExtendSpritesHeight(int addedAmount) => ExtendSprites(1, addedAmount);
}
