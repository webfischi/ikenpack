using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ikenpack;

public class Sprite {

    /// <summary>
    /// Load sprite from file with binary reader
    /// </summary>
    /// <param name="reader"></param>
    public Sprite( BinaryReader reader ) {
        LoadBinary( reader );
    }

    public Sprite(string name, float width, float height, float startX, float startY, float endX, float endY, float trimWidth, float trimHeight, float offsetX, float offsetY, float charW) {
        Name = name;
        Width = width;
        Height = height;
        StartX = startX;
        StartY = startY;
        EndX = endX;
        EndY = endY;
        TrimWidth = trimWidth;
        TrimHeight = trimHeight;
        OffsetX = offsetX;
        OffsetY = offsetY;
        CharW = charW;
    }

    public void LoadBinary(BinaryReader b) {
        Name = b.ReadString();
        Width = b.ReadSingle();
        Height = b.ReadSingle();
        StartX = b.ReadSingle();
        StartY = b.ReadSingle();
        EndX = b.ReadSingle();
        EndY = b.ReadSingle();
        TrimWidth = b.ReadSingle();
        TrimHeight = b.ReadSingle();
        OffsetX = b.ReadSingle();
        OffsetY = b.ReadSingle();
        CharW = b.ReadSingle();
    }

    /// <summary>
    /// write the sprite to a file with binary writer
    /// </summary>
    /// <param name="writer"></param>
    internal void Save(BinaryWriter writer) {
        writer.Write(Name);          // string
        writer.Write(Width);         // float
        writer.Write(Height);        // float
        writer.Write(StartX);          // float
        writer.Write(StartY);          // float
        writer.Write(EndX);          // float
        writer.Write(EndY);          // float
        writer.Write(TrimWidth);     // float
        writer.Write(TrimHeight);    // float
        writer.Write(OffsetX);      // float
        writer.Write(OffsetY);      // float
        writer.Write(CharW);         // float
    }

    public string Name { get; set; }
    public float Width { get; private set; }
    public float Height { get; private set; }
    public float StartX { get; set; }
    public float StartY { get; set; }
    public float EndX { get; set; }
    public float EndY { get; set; }
    public float TrimWidth { get; private set; }
    public float TrimHeight { get; private set; }
    public float OffsetX { get; private set; }
    public float OffsetY { get; private set; }
    public float CharW { get; private set; }

    /// <summary>
    /// Export the Sprite to a single png file
    /// </summary>
    /// <param name="directory">the directory where the file gets created</param>
    /// <param name="rawImg">The whole image from which the sprite image comes from</param>
    public void Export(string directory, ImgImage rawImg) {
        rawImg.GetSprite(StartX, StartY, EndX - StartX, EndY - StartY).Save(Path.Combine(directory, $"{Name}.png"), ImageFormat.Png);
    }
}
