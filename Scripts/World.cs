using Godot;

public class World : Spatial
{
    private PackedScene chunkScene = ResourceLoader.Load("res://Scenes/Chunk.tscn") as PackedScene;
    private int loadRadius = 5;
    private Spatial chunks;
    private KinematicBody player;

    Thread loadThread = new Thread();

    public override void _Ready()
    {
        chunks = GetNode("Chunks") as Spatial;
        player = GetNode("Player") as KinematicBody;

        for (int x = 0; x < loadRadius; x++)
        {
            for (int z = 0; z < loadRadius; z++)
            {
                Chunk chunk = chunkScene.Instance() as Chunk;
                chunk.SetChunkPosition(new Vector2(x, z));
                chunks.AddChild(chunk);
            }
        }

        loadThread.Start(this, nameof(_LoadThreadProcess));

        player.Connect("PlaceBlock", this, "_OnPlayerPlaceBlock");
        player.Connect("BreakBlock", this, "_OnPlayerBreakBlock");
    }

    private void _LoadThreadProcess(object _threadData)
    {
        while (true)
        {
            foreach (Chunk chunk in chunks.GetChildren())
            {
                float chunkX = chunk.ChunkPosition.x;
                float chunkZ = chunk.ChunkPosition.y;

                float playerX = Mathf.Floor(player.Translation.x / Global.Dimensions.x);
                float playerZ = Mathf.Floor(player.Translation.z / Global.Dimensions.z);

                float newX = Mathf.PosMod(chunkX - playerX + loadRadius / 2, loadRadius) + 
                    playerX - loadRadius / 2;
                float newZ = Mathf.PosMod(chunkZ - playerZ + loadRadius / 2, loadRadius) + 
                    playerZ - loadRadius / 2;

                if (newX != chunkX || newZ != chunkZ)
                {
                    chunk.SetChunkPosition(new Vector2((int)newX, (int)newZ));
                    chunk.Generate();
                    chunk.Update();
                }
            }
        }
    }

    private Chunk GetChunk(Vector2 chunkPos)
    {
        foreach (Chunk chunk in chunks.GetChildren())
        {
            if (chunk.ChunkPosition == chunkPos)
            {
                return chunk;
            }
        }

        return null;
    }

    private void _OnPlayerPlaceBlock(Vector3 position, Global.BlockType blockType)
    {
        float chunkX = (int)(Mathf.Floor(position.x / Global.Dimensions.x));
        float chunkZ = (int)(Mathf.Floor(position.z / Global.Dimensions.z));

        int blockX = (int)Mathf.PosMod(Mathf.Floor(position.x), Global.Dimensions.x);
        int blockY = (int)Mathf.PosMod(Mathf.Floor(position.y), Global.Dimensions.y);
        int blockZ = (int)Mathf.PosMod(Mathf.Floor(position.z), Global.Dimensions.z);

        Chunk chunk = GetChunk(new Vector2(chunkX, chunkZ));

        if (chunk != null)
        {
            chunk.blocks[blockX, blockY, blockZ] = blockType;
            chunk.Update();
        }
    }

    private void _OnPlayerBreakBlock(Vector3 position)
    {
        _OnPlayerPlaceBlock(position, Global.BlockType.AIR);
    }
}
