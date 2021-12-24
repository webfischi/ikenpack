using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ikenpack;

public static class Extensions {
    /// <summary>
    /// Get a parts from a Bitmap
    /// </summary>
    /// <param name="source">The big Bitmap</param>
    /// <param name="section">The area that gets croped</param>
    /// <returns>The parts of the Bitmap that is selected</returns>
    public static Bitmap CropImage(this Bitmap source, Rectangle section) {
        try {
            var bitmap = new Bitmap(section.Width, section.Height);
            using var g = Graphics.FromImage(bitmap);
            g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
            return bitmap;
        } catch {
            Console.WriteLine("");
        }
        return null;
    }

    /// <summary>
    /// Draw an image to the bitmap
    /// </summary>
    /// <param name="source">The big Bitmap</param>
    /// <param name="image">the bitmap that is drawn</param>
    /// <param name="x">start X position</param>
    /// <param name="y">start Y position</param>
    public static void Draw(this Image source, Image image, int x, int y) {
        using var g = Graphics.FromImage(source);
        g.DrawImage(image, x, y);
    }

    /// <summary>
    /// Find the position in an 2D array
    /// </summary>
    /// <typeparam name="T">Array type</typeparam>
    /// <param name="source">Source array</param>
    /// <param name="predicate">A function that test all elements for the condition </param>
    /// <returns>The first found position of the element</returns>
    public static (int x, int y) FindPosition<T>(this T[,] source, Predicate<T> predicate) {
        for (int x = 0; x < source.GetLength(0); x++) {
            for (int y = 0; y < source.GetLength(1); y++) {
                if(predicate(source[x, y]))
                    return (x, y);
            }
        }
        return (-1, -1);
    }

    /// <summary>
    /// Extends a 2D array by a length in one of both dimensions
    /// </summary>
    /// <typeparam name="T">The source array Type</typeparam>
    /// <param name="source">array that gets extended</param>
    /// <param name="dimension">the dimension in which the array gets extended</param>
    /// <param name="addedLength">The length by that the dimension gets extended</param>
    /// <returns></returns>
    public static T[,] Extend<T>(this T[,] source, int dimension, int addedLength) {
        T[,] newArray = dimension == 0
            ? (new T[source.GetLength(0) + addedLength, source.GetLength(1)])
            : (new T[source.GetLength(0) , source.GetLength(1) + addedLength]);

        for (int i = 0; i < source.GetLength(0); i++) {
            for (int j = 0; j < source.GetLength(1); j++) {
                newArray[i, j] = source[i, j];
            }
        }

        return newArray;
    }

    /// <summary>
    /// Adds one element in the end
    /// </summary>
    /// <typeparam name="T">The source Enumerable type</typeparam>
    /// <param name="source">The source Enumerable</param>
    /// <param name="addedItem">The item that gets added at the end</param>
    /// <returns>A new Enumerable with the added element</returns>
    public static IEnumerable<T> Add<T>(this IEnumerable<T> source, T addedItem) {
        foreach (var item in source) {
            yield return item;
        }
        yield return addedItem;
    }

    /// <summary>
    /// Find the biggets free area (all alpha values are 0) from the bottom left that is possible
    /// </summary>
    /// <param name="bitmap">the source Bitmap </param>
    /// <returns>The top left position of the free area</returns>
    public static (int X, int Y) FreeArea(this Bitmap bitmap) {

        var srcData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        int width = bitmap.Width;
        int height = bitmap.Height;

        int maxX = 0;
        int minY = height;

        int maxV = 0; // biggest area volume that is found

        unsafe {
            byte* dstPtr = (byte*)srcData.Scan0;

            for (int y = height - 1; y >= 0; y--) {
                for (int x = width - 1; x > maxX; x--) {
                    int index = y * srcData.Stride + x * 4;
                    byte a = dstPtr[index + 3]; // get alpha value
                    if(a != 0) {
                        int v = (height - y - 1) * (width - x - 1); // calculate area of the free rectangle
                        if (v > maxV) {
                            maxV = v;
                            minY = y;
                            maxX = x;
                        }
                        break;
                    }
                }
            }
        }

        bitmap.UnlockBits(srcData);

        maxX++;
        return (maxX, minY);
    }
}
