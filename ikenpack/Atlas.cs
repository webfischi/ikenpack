
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ikenpack;

public class Atlas {
        public Sprite[] Sprites { get; private set; }
        public Tileset[] Tilesets { get; }
        public string Name { get; }
        public float WhitePixelX { get; private set; }
        public float WhitePixelY { get; private set; }


    /// <summary>
    /// Parses a .bin file to an Atlas object
    /// </summary>
    /// <param name="path">The path to the .bin file</param>
    public Atlas(string path) {
        var sprites = new List<Sprite>();
        var tilesets = new List<Tileset>();
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
            var len = stream.Length;
            using (var reader = new BinaryReader(stream)) {
                //read general informations from file
                Name = reader.ReadString();
                WhitePixelX = reader.ReadSingle();
                WhitePixelY = reader.ReadSingle();

                // read all sprites
                int n = reader.ReadInt32();
                for (int i = 0; i < n; ++i) {
                    var sprite = new Sprite(reader);
                    sprites.Add(sprite);
                }
                this.Sprites = sprites.ToArray();

                // read all tilesets
                n = reader.ReadInt32();
                for (int i = 0; i < n; ++i) {
                    var tileset = new Tileset(reader, this);
                    tilesets.Add(tileset);
                }
                this.Tilesets = tilesets.ToArray();
            }
        }
    }

    /// <summary>
    /// Saves the atlas with all sprites and tilesets to a .bin file
    /// </summary>
    /// <param name="path">The path to the .bin file that gets created</param>
    internal void Save(string path) {
        using (var stream = new FileStream(path, FileMode.Create)) {
            using (var writer = new BinaryWriter(stream)) {
                // write general informations to file
                writer.Write(Name);
                writer.Write(WhitePixelX);
                writer.Write(WhitePixelY);

                // write all sprites to the file
                writer.Write(Sprites.Length);
                foreach (var sprite in Sprites) {
                    sprite.Save(writer);
                }

                // write all tilesets to the file
                writer.Write(Tilesets.Length);
                foreach (var tileset in Tilesets) {
                    tileset.Save(writer);
                }
            }
        }
    }

    /// <summary>
    /// get a sprite from the Sprite list by the sprite name
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Sprite GetSprite(string id) {
        return Sprites.FirstOrDefault(x => x.Name == id);
    }

    /// <summary>
    /// create files for every sprite and a directory for every tileset
    /// </summary>
    /// <param name="directory">the directory where all sprite files and tileset directories are written into</param>
    /// <param name="rawImg"></param>
    public void Export(string directory, ImgImage rawImg) {
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        Console.WriteLine("export tilesets . . .");
        Console.Write($"0 / {Tilesets.Length}");
        // creates directories for every tileset
        // and export the sprites into them
        for (int i = 0; i < Tilesets.Length; i++) {
            Tileset? tileset = this.Tilesets[i];
            tileset.Export(directory, rawImg);
            Console.Write($"\r{i + 1} / {Tilesets.Length}");
        }
        Console.WriteLine();

        Console.WriteLine("export left sprites . . .");

        Console.Write($"0");
        long found = 0;
        // export all sprites that are left (not in any tileset) to a file directly in the directory
        foreach (var sprite in this.Sprites.Where(x => !this.Tilesets.Any(y => y.Sprites.Cast<Sprite>().Any(z => z == x)))) {
            sprite.Export(directory, rawImg);
            found++;
            Console.Write($"\r{found}");
        }
        Console.WriteLine($"\r{found}");
    }

    // Get all Files in a directory and all subdirectories
    private IEnumerable<string> GetAllFilesRecursively(string directory) {
        foreach (var file in Directory.EnumerateFiles(directory))
            yield return file;

        foreach (var dir in Directory.GetDirectories(directory))
            foreach (var file in GetAllFilesRecursively(dir))
                yield return file;
    }

    /// <summary>
    /// Update the Atlas to fit all changed files in the directory, also creates a new sprite sheet where the new tiles are added
    /// </summary>
    /// <param name="sourceDirectory">Directory where the sprites and tilesets are saved</param>
    /// <param name="width">The width for the sprite sheet</param>
    /// <param name="height">The height for the sprite sheet</param>
    /// <returns>the new sprite sheet</returns>
    public Bitmap Update(string sourceDirectory, int width, int height) {
        Bitmap sheet = new Bitmap(width, height, PixelFormat.Format32bppArgb); // the new sprite sheet, content is added later
        var foundSprites = new List<Sprite>(); // all sprites that are found
        var newSprites = new List<(Sprite Sprite, Bitmap Image)>(); // new sprites, that are not in the current Atlas


        Console.WriteLine("create sprites with tileset . . .");
        long found = 0;
        Console.Write($"{found}");
        foreach (var dir in Directory.EnumerateDirectories(sourceDirectory)) {
            string name = Path.GetFileName(dir);
            var tileSet = this.Tilesets.FirstOrDefault(x => x.Name == name); // find matching tileset
            if (tileSet == null) // currently no new spritesheets can be adden so it is skipped
                continue;
            

            foreach(var file in Directory.EnumerateFiles(dir)) {
                var spriteName = Path.GetFileNameWithoutExtension(file);
                var bm = new Bitmap(file);
                var spritePos = tileSet.Sprites.FindPosition(x => x != null &&x.Name == spriteName); // find the position of the Sprite in the 2D array from the tileset
                Sprite sprite;
                if (spritePos == (-1, -1)) { // of no sprite is found

                    sprite = new Sprite(spriteName, bm.Width, bm.Height, 0, 0, 0, 0, bm.Width, bm.Height, 0, 0, bm.Width); // create new sprite

                    // get sprite position by the file name
                    (int X, int Y) newSpritePos = (X: 0, Y: 0);
                    var splittedName = spriteName.Split('_');
                    if (splittedName.Length < 3)
                        continue;

                    if (!int.TryParse(splittedName[^2], out newSpritePos.X) || !int.TryParse(splittedName[^1], out newSpritePos.Y))
                        continue;

                    // extend the Sprite Array length in both dimension, if its nessesery
                    while (newSpritePos.X >= tileSet.Cols) {
                        tileSet.Cols++;
                        tileSet.ExtendSpritesWidth(1);
                    }

                    while (newSpritePos.Y >= tileSet.Rows) {
                        tileSet.Rows++;
                        tileSet.ExtendSpritesHeight(1);
                    }
                    tileSet.Sprites[newSpritePos.X, newSpritePos.Y] = sprite;
                    this.Sprites = this.Sprites.Add(sprite).ToArray();
                    newSprites.Add((sprite,bm));
                } else {
                    sprite = tileSet.Sprites[spritePos.x, spritePos.y];
                    sheet.Draw(bm, (int)(sprite.StartX * width), (int)(sprite.StartY * height)); // draw the sprite in the new sprite sheet
                }
                foundSprites.Add(sprite);
            }

            found++;
            Console.Write($"\r{found}");
        }

        Console.WriteLine();
        Console.WriteLine("create sprites without tileset . . .");
        Console.Write($"{found}");
        foreach (var file in Directory.EnumerateFiles(sourceDirectory)) {
            var name = Path.GetFileNameWithoutExtension(file);
            var bm = new Bitmap(file);
            // find sprites the current Atlas
            var sprite = this.Sprites.FirstOrDefault(x => x.Name == name);
            if (sprite == null) { // if no sprite is found
                sprite = new Sprite(name, bm.Width, bm.Height, 0, 0, 0, 0, bm.Width, bm.Height, 0, 0, bm.Width);

                this.Sprites = this.Sprites.Add(sprite).ToArray();
                newSprites.Add((sprite,bm));
            }
            if(!Tilesets.Any(x => x.Name == name)) // some tilesets are also defined as one sprite, they should't get drawn in the sprite sheet, because they are already there
                sheet.Draw(bm, (int)(sprite.StartX * width), (int)(sprite.StartY * height)); // draw sprite in sprite sheet
            foundSprites.Add(sprite);
            found++;
            Console.Write($"\r{found}");
        }
        Console.WriteLine();

        // get biggest transparent area possible that ends in the bottom right
        var freePos = sheet.FreeArea();
        freePos.X += 10;
        freePos.Y += 10;

        int startX = freePos.X;
        int startY = freePos.Y;

        // draw all sprites that are new in the free area
        int maxHeight = 0;
        foreach (var c in newSprites) {
            var sprite = c.Sprite;
            if (freePos.X + sprite.Width >= width) {
                freePos.Y += maxHeight;
                maxHeight = 0;
                freePos.X = startX;
            }

            sprite.StartX = (float)freePos.X / width;
            sprite.StartY = (float)freePos.Y / height;
            sprite.EndX = (freePos.X + sprite.Width) / width;
            sprite.EndY = (freePos.Y + sprite.Height) / height;

            sheet.Draw(c.Image, freePos.X, freePos.Y);

            freePos.X += (int)sprite.Width;
            if (maxHeight < sprite.Height)
                maxHeight = (int)sprite.Height;
        }
        Console.WriteLine();

        Console.WriteLine("remove removed sprites");
        Sprites = Sprites.Where(x => foundSprites.Any(y => x.Name == y.Name)).ToArray();
        return sheet;
    }
}
