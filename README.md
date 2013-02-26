VB6ImageCreator
===============

VB6 image creator. Converts all PNG images in a given source directory to 24 bit rgb BMP images.

Allows to set a transparent color, a transparency threshold and a background color which is used to render partly transparent pixels. GUI based.

But why?!
---------

I needed new toolbar icons for an old VB6 project. VB6 toolbars are supplied with icons by an `ImageList` control which unfortunately only allows 24 bit images. It is possible to define one color which is treated as fully transparent, though.

Because the icon pool I use here at work solely consists of 32 bit argb PNG images I came up with this little program to have a possibility to convert those existing images automatically.
