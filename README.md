# MapleManager

_Your extensible maple data manager._


Supports:
- deMSwZ-style WZ processing, ignoring WZ package key
- WZ package key bruteforcing for WZ files that use a version number as package key (all official ones)
- Unpacking WZ files to IMG files
- On-the-fly WZ encryption detection (GMS, SEA and no-op)
- Image `_inlink` and `_outlink`s
- Image decompression
- A4R4G4B4, RGB565, ARGB32, DXT3, DXT5 images
- Tiled images (MagLevel > 0)
- External scripts written in C#
- Animation of nodes (eg. attack node of a mob)
- Exporting animations to GIF
- NPC scripts text processing (formatting) and explanations

Does not support:
- Saving .img files back to disk (TODO)
- Sounds
- ASCII Property nodes
