# ikenpack
A tool to extract and repackage ikenfells img files

Developed by Yannik Höflich https://www.fiverr.com/yannikhoeflich?source=order_page_summary_seller_link
Uses code by Chevy Ray (The Ikenfell developer)
Uses code by davidtwilcox for the spritesheet https://github.com/davidtwilcox/SheetYourself
Instructed and payed by webfischi

This tool is created with Visual Studio 2022

# How to use
This is a commandline tool, so everything is done within cmd or the Windows Terminal, there is currently one bug we weren't able to fix. The game refuses to load solid colors, the workaround will be included in this section.

If you're unfamiliar with VS 2022 there is an .exe for download

This example shows how to extend the fonts:

## Step 1
Put atlas.img and atlas.dat into a save location
Create a copy of atlas.dat and name it atlasorg.dat

## Step 2
Click into the address bar and type cmd (Windows 10)
Right click in empty space "Open in Terminal" (Windows 11)

## Step 3
Export the whole atlas.img first, you'll need it later
```
ikenpack.exe -ei atlas.img atlasorg.png
```

## Step 4
Export all sprites
```
ikenpack.exe -e atlas.img atlas.bin atlas
```

## Step 5
Add or Edit files, for new files you can only use names that are not already used and you will only be able to load new letters

Pepackage the files (currently there are some unnecessary instructions)
```
ikenpack.exe -p atlas atlasorg.bin atlas.img atlas.bin Graphics
```

## Step 6
Now export the new atlas.img and merge atlasorg.png and atlas.png in Gimp or similar
```
ikenpack.exe -ei atlas.img atlas.png
```
Important notice: Only copy over the new/changed sprites into atlasorg.png, remove all other sprites, be careful that the new sprites are in the correct position

## Step 7
Merge the layers and overwrite atlasorg.png
Repackage the .png

```
ikenpack.exe -pi atlasorg.png atlas.img
```

## Step 8
Copy the atlas files into the game directory and keep close attention to the crash log located in %appdata%\ikenfell

Enjoy
