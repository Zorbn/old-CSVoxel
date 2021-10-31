using System.Collections.Generic;
using Godot;

public static class Global
{
    public static Vector3 Dimensions = new Vector3(16, 64, 16);
    public static Vector2 TextureAtlasSize = new Vector2(3, 2);

    public enum TextureSides
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT,
        FRONT,
        BACK,
    }

    public enum BlockType
    {
        AIR,
        DIRT,
        GRASS,
        STONE,
    }

    public struct BlockInfo
    {
        public BlockInfo(Dictionary<TextureSides, Vector2> sideTextures, bool isSolid)
        {
            this.sideTextures = sideTextures;
            this.isSolid = isSolid;
        }

        public Dictionary<TextureSides, Vector2> sideTextures;
        public bool isSolid;
    }

    public static Dictionary<BlockType, BlockInfo> BlockTypes = new Dictionary<BlockType, BlockInfo>() {
        {
            BlockType.AIR,
            new BlockInfo(
                new Dictionary<TextureSides, Vector2>() {
                    { TextureSides.TOP,    new Vector2(2, 1) },
                    { TextureSides.BOTTOM, new Vector2(2, 1) },
                    { TextureSides.LEFT,   new Vector2(2, 1) },
                    { TextureSides.RIGHT,  new Vector2(2, 1) },
                    { TextureSides.FRONT,   new Vector2(2, 1) },
                    { TextureSides.BACK,   new Vector2(2, 1) },
                },
                false
            )
        },
        {
            BlockType.DIRT,
            new BlockInfo(
                new Dictionary<TextureSides, Vector2>() {
                    { TextureSides.TOP,    new Vector2(2, 0) },
                    { TextureSides.BOTTOM, new Vector2(2, 0) },
                    { TextureSides.LEFT,   new Vector2(2, 0) },
                    { TextureSides.RIGHT,  new Vector2(2, 0) },
                    { TextureSides.FRONT,   new Vector2(2, 0) },
                    { TextureSides.BACK,   new Vector2(2, 0) },
                },
                true
            )
        },
        {
            BlockType.GRASS,
            new BlockInfo(
                new Dictionary<TextureSides, Vector2>() {
                    { TextureSides.TOP,    new Vector2(0, 0) },
                    { TextureSides.BOTTOM, new Vector2(2, 0) },
                    { TextureSides.LEFT,   new Vector2(1, 0) },
                    { TextureSides.RIGHT,  new Vector2(1, 0) },
                    { TextureSides.FRONT,   new Vector2(1, 0) },
                    { TextureSides.BACK,   new Vector2(1, 0) },
                },
                true
            )
        },
        {
            BlockType.STONE,
            new BlockInfo(
                new Dictionary<TextureSides, Vector2>() {
                    { TextureSides.TOP,    new Vector2(0, 1) },
                    { TextureSides.BOTTOM, new Vector2(0, 1) },
                    { TextureSides.LEFT,   new Vector2(0, 1) },
                    { TextureSides.RIGHT,  new Vector2(0, 1) },
                    { TextureSides.FRONT,   new Vector2(0, 1) },
                    { TextureSides.BACK,   new Vector2(0, 1) },
                },
                true
            )
        },
    };
}
