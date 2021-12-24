using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ikenpack;

public class ImgImage{
    public Image Image => bitmap;
    private Bitmap bitmap;
    public int Width { get;}
    public int Height { get;}

    /// <summary>
    /// Parse a .img file to Image Object
    /// </summary>
    /// <param name="filePath">The path to the .img file</param>
    public ImgImage(string filePath) {

        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
            var len = stream.Length;
            using (var reader = new BinaryReader(stream)) {
                Width = reader.ReadInt32();
                Height = reader.ReadInt32();
                Bitmap bm = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
                var dstData = bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                unsafe {
                    byte* dstPtr = (byte*)dstData.Scan0;
                    int x = 0;
                    int y = 0;

                    byte num;
                    byte r;
                    byte g;
                    byte b;
                    byte a;
                    while (stream.Position != len && x < Width && y < Height) {
                        num = reader.ReadByte();
                        r = reader.ReadByte();
                        g = reader.ReadByte();
                        b = reader.ReadByte();
                        a = reader.ReadByte();
                        while (num > 0) {
                            dstPtr[0] = b;
                            dstPtr[1] = g;
                            dstPtr[2] = r;
                            dstPtr[3] = a;
                            dstPtr += 4;
                            x++;
                            if(x >= Width) {
                                x = 0; 
                                y++;
                            }
                            --num;
                        }
                    }
                }

                bm.UnlockBits(dstData);
                bitmap = bm;

                Console.WriteLine();
            }
        }
    }

    /// <summary>
    /// create .img object from Bitmap
    /// </summary>
    /// <param name="bm">The Bitmap that is </param>
    public ImgImage(Bitmap bm) {
        this.bitmap = bm;
        this.Height = bm.Height;
        this.Width = bm.Width;
    }

    /// <summary>
    /// save the object to a .img file
    /// </summary>
    /// <param name="path">Path of the file where the data is written to</param>
    public void Save(string path) {
        using (var stream = new FileStream(path, FileMode.Create)) {
            using (var writer = new BinaryWriter(stream)) {
                writer.Write(Width);
                writer.Write(Height);

                var srcData = bitmap.LockBits(new Rectangle(0, 0, Width, Height),ImageLockMode.ReadOnly, bitmap.PixelFormat);
                Color prev = Color.Transparent;
                Color col = Color.Transparent;
                unsafe {
                    byte* srcPtr = (byte*)srcData.Scan0;
                    byte num = 0;
                    for (int i = 0; i < Width * Height; ++i) {

                        col = Color.FromArgb(srcPtr[3], srcPtr[2], srcPtr[1], srcPtr[0]);
                        srcPtr += 4;
                        if (col != prev || num == byte.MaxValue) {
                            if (num > 0) {
                                writer.Write(num);
                                writer.Write(prev.R);
                                writer.Write(prev.G);
                                writer.Write(prev.B);
                                writer.Write(prev.A);
                            }
                            prev = col;
                            num = 1;
                        } else {
                            num++;
                        }
                    }
                    if (num > 0) {
                        writer.Write(num);
                        writer.Write(prev.R);
                        writer.Write(prev.G);
                        writer.Write(prev.B);
                        writer.Write(prev.A);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Get a reactangle from the Image
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public Image GetSprite(float x, float y, float width, float height) {
        return bitmap.CropImage(new Rectangle((int)Math.Round(x * Width), (int)Math.Round(y * Height), (int)Math.Round(width * Width), (int)Math.Round(height * Height)));
    }
}