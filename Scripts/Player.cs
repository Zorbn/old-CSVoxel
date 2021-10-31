using Godot;

public class Player : KinematicBody
{
    private const float MouseSensitivity = 0.3f;
    private const float MovementSpeed = 10.0f;
    private const float Gravity = 20.0f;
    private const float JumpVelocity = 8.0f;

    private Spatial head;
    private Camera camera;
    private RayCast raycast;
    private MeshInstance blockOutline;

    private float camXRotation = 0.0f;

    private Vector3 velocity = new Vector3();

    private bool paused = false;

    [Signal]
    public delegate void PlaceBlock(Vector3 position, Global.BlockType blockType);

    [Signal]
    public delegate void BreakBlock(Vector3 position);

    public override void _Ready()
    {
        head = GetNode("Head") as Spatial;
        camera = head.GetNode("Camera") as Camera;
        raycast = camera.GetNode("RayCast") as RayCast;
        blockOutline = GetNode("BlockOutline") as MeshInstance;

        Input.SetMouseMode(Input.MouseMode.Captured);
    }

    public override void _Input(InputEvent ie)
    {
        if (Input.IsActionJustPressed("pause"))
        {
            paused = !paused;

            if (paused)
            {
                Input.SetMouseMode(Input.MouseMode.Visible);
            }
            else
            {
                Input.SetMouseMode(Input.MouseMode.Captured);
            }
        }

        if (paused) return;

        if (ie is InputEventMouseMotion mouseMotion)
        {
            head.RotateY(Mathf.Deg2Rad(-mouseMotion.Relative.x * MouseSensitivity));

            float deltaX = mouseMotion.Relative.y * MouseSensitivity;

            if (camXRotation + deltaX > -90.0f && camXRotation + deltaX < 90.0f)
            {
                camera.RotateX(Mathf.Deg2Rad(-deltaX));
                camXRotation += deltaX;
            }
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (paused) return;

        Basis basis = head.GlobalTransform.basis;
        Vector3 direction = new Vector3();

        if (Input.IsActionPressed("forward"))  direction -= basis.z;
        if (Input.IsActionPressed("backward")) direction += basis.z;
        if (Input.IsActionPressed("left"))     direction -= basis.x;
        if (Input.IsActionPressed("right"))    direction += basis.x;

        velocity.z = direction.z * MovementSpeed;
        velocity.x = direction.x * MovementSpeed;

        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.y = JumpVelocity;
        }

        velocity.y -= Gravity * delta;
        velocity = MoveAndSlide(velocity, Vector3.Up);

        if (raycast.IsColliding())
        {
            Vector3 normal = raycast.GetCollisionNormal();
            Vector3 position = raycast.GetCollisionPoint() - normal * 0.5f;

            float blockX = Mathf.Floor(position.x) + 0.5f;
            float blockY = Mathf.Floor(position.y) + 0.5f;
            float blockZ = Mathf.Floor(position.z) + 0.5f;
            Vector3 blockPos = new Vector3(blockX, blockY, blockZ) - Translation;

            blockOutline.Translation = blockPos;
            blockOutline.Visible = true;

            if (Input.IsActionJustPressed("break"))
            {
                EmitSignal("BreakBlock", position);
            }
            else if (Input.IsActionJustPressed("place"))
            {
                EmitSignal("PlaceBlock", position + normal, Global.BlockType.STONE);
            }
        }
        else
        {
            blockOutline.Visible = false;
        }
    }
}
