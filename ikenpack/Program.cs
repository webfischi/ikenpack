using ikenpack;

using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

class Program {

    public static void Main(string[] args) {

        try {
            string t = args[0];

            if (t == "-h") {
                Console.WriteLine("-h                                                                                                         This help page");
                Console.WriteLine("-e [source .img file] [source .bin file] [destination directory]                                           Extract the sprites and tilesets to the directory");
                Console.WriteLine("-i [source directory] [destination directory]                                                              Extract sprites from all files in the source directory to the destination directory");
                Console.WriteLine();
                Console.WriteLine("-p [source directory] [original bin file] [destination .img file] [destination .bin file] [atlas name]     Packs all images from the source directory.");
                Console.WriteLine("                                                                                                           The [atlas name] is the name of the atlas, that is defined in the .bin file");
                Console.WriteLine();
                Console.WriteLine("-ei [source .img file] [destination PNG file]                                                              Converts the .img file to a .png file");
                Console.WriteLine("-pi [source PNG file] [destination .img file]                                                              Converts the .png file to a .img file");
                Console.WriteLine();
                Console.WriteLine("-d [source .img file] [source .bin file]                                                                   Get informations about the files");
            } else if (t == "-e") {
                string imgFile = args[1];
                string binFile = args[2];
                string destinationDirectory = args[3];
                Console.WriteLine($"parsing {imgFile}");
                var img = new ImgImage(imgFile);
                Console.WriteLine($"parsing {binFile}");
                var atlas = new Atlas(binFile);
                Console.WriteLine($"exporting . . .");
                atlas.Export(destinationDirectory, img);
            } else if (t == "-i") {
                string sourceDirectory = args[1];
                string destinationDirectory = args[2];
                foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*.img")) {
                    var binFile = Path.ChangeExtension(file, "bin");
                    if (!File.Exists(binFile)) {
                        Console.WriteLine($"{file} has no matching .bin file: \"{binFile}\" is missing");
                        continue;
                    }
                    var destinationDir = Path.Combine(destinationDirectory, Path.GetFileNameWithoutExtension(file));
                    Console.WriteLine($"extracting ${Path.GetFileNameWithoutExtension(file)}");
                    Main(new string[] { "-e", file, binFile, destinationDir });
                }
            } else if (t == "-p") {
                string sourceDirectory = args[1];
                string oldBinFile = args[2];
                string destinationImgFile = args[3];
                string destinationBinFile = args[4];
                string atlasName = args[5];

                Console.WriteLine($"parsing {oldBinFile}");
                var atlas = new Atlas(oldBinFile);

                Console.WriteLine("Creating atlas and bitmap");
                Bitmap bitmap = atlas.Update(sourceDirectory, 4096, 4096);
                var img = new ImgImage(bitmap);
                Console.WriteLine("converting to .bin file and write in file");
                atlas.Save(destinationBinFile);
                Console.WriteLine("converting to .img and write in file");
                img.Save(destinationImgFile);
            } else if (t == "-ei") {
                string imgFile = args[1];
                string destFile = args[2];
                Console.WriteLine("parsing .img file");
                var img = new ImgImage(imgFile);
                Console.WriteLine("creating image file");
                img.Image.Save(destFile, ImageFormat.Png);
            } else if (t == "-pi") {
                string pngFile = args[1];
                string destFile = args[2];
                Console.WriteLine("reading image file");
                var img = new ImgImage(new Bitmap(pngFile));
                Console.WriteLine("converting to .img and write in file");
                img.Save(destFile);
            } else if (t == "-d") {
                string imgFile = args[1];
                string binFile = args[2];
                Console.WriteLine($"parsing {imgFile}");
                var img = new ImgImage(imgFile);
                Console.WriteLine($"parsing {binFile}");
                var atlas = new Atlas(binFile);
                int spritesWithoutTilesset = atlas.Sprites.Where(x => !atlas.Tilesets.Any(y => y.Sprites.Cast<Sprite>().Any(z => z == x))).Count();

                Console.WriteLine("\n\n");
                Console.WriteLine($"full image width: {img.Width}");
                Console.WriteLine($"full image height: {img.Height}");
                Console.WriteLine();
                Console.WriteLine($"atlas name: {atlas.Name}");
                Console.WriteLine($"sprite amount: {atlas.Sprites.Length}");
                Console.WriteLine($"tileset amount: {atlas.Tilesets.Length}");
                Console.WriteLine($"sprites without tileset amount: {spritesWithoutTilesset}");
            } else if (t == "-a") {
                if (!Debugger.IsAttached) {
                    Console.WriteLine("-a is only avalable for debug");
                    return;
                }
                string imgFile = args[1];
                string binFile = args[2];
                string tempName2 = args[3];
                string tempName1 = Path.GetFileNameWithoutExtension(imgFile);
                Console.WriteLine("deleting old directory 1");
                if (Directory.Exists(tempName1))
                    Directory.Delete(tempName1, true);

                Console.WriteLine("deleting old directory 2");
                if (Directory.Exists(tempName2))
                    Directory.Delete(tempName2, true);

                Console.WriteLine();
                Main(new string[] { "-e", imgFile, binFile, tempName1});
                Console.WriteLine();
                Main(new string[] { "-p", tempName1, binFile, $"{tempName2}.img", $"{tempName2}.bin", "Graphics" });
                Console.WriteLine();
                Main(new string[] { "-e", $"{tempName2}.img", $"{tempName2}.bin", tempName2 });
                Console.WriteLine();
                Main(new string[] { "-ei", imgFile, $"{tempName1}.png" });
                Console.WriteLine();
                Main(new string[] { "-ei", $"{tempName2}.img", $"{tempName2}.png" });
                Console.WriteLine();
            } else {
                throw new ArgumentException();
            }
        } catch (Exception ex) {
            Console.WriteLine($"Invalid syntax: {ex.GetType()}");
            Main(new string[] { "-h" });
        }
    }
}