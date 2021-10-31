using Godot;
using System.Collections.Generic;

[Tool]
public class Chunk : StaticBody
{
    public Vector2 ChunkPosition = new Vector2();

    private Vector3[] vertices = {
        new Vector3(0, 0, 0), // 0
        new Vector3(1, 0, 0), // 1
        new Vector3(0, 1, 0), // 2
        new Vector3(1, 1, 0), // 3
        new Vector3(0, 0, 1), // 4
        new Vector3(1, 0, 1), // 5
        new Vector3(0, 1, 1), // 6
        new Vector3(1, 1, 1), // 7
    };

    private int[] top = { 2, 3, 7, 6 };
    private int[] bottom = { 0, 4, 5, 1 };
    private int[] left = { 6, 4, 0, 2 };
    private int[] right = { 3, 1, 5, 7 };
    private int[] front = { 7, 5, 4, 6 };
    private int[] back = { 2, 0, 1, 3 };

    private SurfaceTool st = new SurfaceTool();
    private ArrayMesh mesh = null;
    private MeshInstance meshInstance = null;

    public Global.BlockType[,,] blocks;

    private SpatialMaterial material = ResourceLoader.Load("res://Textures/atlasMaterial.tres") as SpatialMaterial;

    private OpenSimplexNoise noise = new OpenSimplexNoise();

    public override void _Ready()
    {
        material.AlbedoTexture.Flags = 2; // Turn off filter to remove blur!

        Generate();
        Update();
    }

    public void Generate()
    {
        blocks = new Global.BlockType[
            (int)Global.Dimensions.x, 
            (int)Global.Dimensions.y, 
            (int)Global.Dimensions.z
        ];

        for (int x = 0; x < Global.Dimensions.x; x++)
        {
            for (int y = 0; y < Global.Dimensions.y; y++)
            {
                for (int z = 0; z < Global.Dimensions.z; z++)
                {
                    Vector2 globalPos = ChunkPosition * 
                        new Vector2(Global.Dimensions.x, Global.Dimensions.z) + 
                        new Vector2(x, z);

                    float height = (int)((noise.GetNoise2dv(globalPos) + 1) / 2.0f * Global.Dimensions.y);

                    Global.BlockType type = Global.BlockType.AIR;

                    if (y < height / 2)
                    {
                        type = Global.BlockType.STONE;
                    }
                    else if (y < height)
                    {
                        type = Global.BlockType.DIRT;
                    }
                    else if (y == height)
                    {
                        type = Global.BlockType.GRASS;
                    }

                    blocks[x, y, z] = type;
                }
            }
        }
    }

    public void Update()
    {
        // Unload
        if (meshInstance != null)
        {
            meshInstance.CallDeferred("queue_free");
            meshInstance = null;
        }

        mesh = new ArrayMesh();
        meshInstance = new MeshInstance();
        st.Begin(Mesh.PrimitiveType.Triangles);

        for (int x = 0; x < Global.Dimensions.x; x++)
        {
            for (int y = 0; y < Global.Dimensions.y; y++)
            {
                for (int z = 0; z < Global.Dimensions.z; z++)
                {
                    CreateBlock(x, y, z);
                }
            }
        }

        st.GenerateNormals(false);
        st.SetMaterial(material);
        st.Commit(mesh);
        meshInstance.Mesh = mesh;

        AddChild(meshInstance);
        meshInstance.CreateTrimeshCollision();

        Visible = true;
    }

    private bool CheckTransparent(int x, int y, int z)
    {
        if (x >= 0 && x < Global.Dimensions.x &&
            y >= 0 && y < Global.Dimensions.y &&
            z >= 0 && z < Global.Dimensions.z)
        {
            return !Global.BlockTypes[blocks[x, y, z]].isSolid;
        }

        return true;
    }

    private void CreateBlock(int x, int y, int z)
    {
        Global.BlockType block = blocks[x, y, z];
        Dictionary<Global.TextureSides, Vector2> sideTextures = Global.BlockTypes[block].sideTextures;

        if (block == Global.BlockType.AIR) return;

        if (CheckTransparent(x, y + 1, z)) CreateFace(top,    x, y, z, sideTextures[Global.TextureSides.TOP]);
        if (CheckTransparent(x, y - 1, z)) CreateFace(bottom, x, y, z, sideTextures[Global.TextureSides.BOTTOM]);
        if (CheckTransparent(x - 1, y, z)) CreateFace(left,   x, y, z, sideTextures[Global.TextureSides.LEFT]);
        if (CheckTransparent(x + 1, y, z)) CreateFace(right,  x, y, z, sideTextures[Global.TextureSides.RIGHT]);
        if (CheckTransparent(x, y, z - 1)) CreateFace(back,   x, y, z, sideTextures[Global.TextureSides.BACK]);
        if (CheckTransparent(x, y, z + 1)) CreateFace(front,  x, y, z, sideTextures[Global.TextureSides.FRONT]);
    }

    private void CreateFace(int[] i, int x, int y, int z, Vector2 textureAtlasOffset)
    {
        Vector3 offset = new Vector3(x, y, z);
        Vector3 a = vertices[i[0]] + offset;
        Vector3 b = vertices[i[1]] + offset;
        Vector3 c = vertices[i[2]] + offset;
        Vector3 d = vertices[i[3]] + offset;

        Vector2 uvOffset = textureAtlasOffset / Global.TextureAtlasSize;
        float height = 1.0f / Global.TextureAtlasSize.y;
        float width = 1.0f / Global.TextureAtlasSize.x;

        Vector2 uvA = uvOffset + new Vector2(0,     0);
        Vector2 uvB = uvOffset + new Vector2(0,     height);
        Vector2 uvC = uvOffset + new Vector2(width, height);
        Vector2 uvD = uvOffset + new Vector2(width, 0);

        st.AddTriangleFan(new Vector3[] { a, b, c }, new Vector2[] {uvA, uvB, uvC});
        st.AddTriangleFan(new Vector3[] { a, c, d }, new Vector2[] {uvA, uvC, uvD});
    }

    public void SetChunkPosition(Vector2 position)
    {
        ChunkPosition = position;
        Translation = new Vector3(position.x, 0, position.y) * Global.Dimensions;

        Visible = false;
    }
}
