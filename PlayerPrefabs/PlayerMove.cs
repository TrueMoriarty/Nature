using Godot;

public partial class PlayerMove : CharacterBody3D
{
    [Export]
    private int _speed = 10;
    [Export]
	private float _jumpVelocity = 4.5f;
    [Export]
    private float _rotationSpeed = 1.5f;
    [Export]
    private int _acceleration = 2;

    private const float _rayLenght = 1000;

    // To avoid creating/deleting variables every frame
    private Vector3 _playerDirection = Vector3.Zero;
    private Basis targetBasis = new Basis();

    // Camera for raycast
    private Camera3D camera;

    private CollisionShape3D head;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    private float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _Ready()
    {
        camera = GetNode<Node3D>("../MotionCamera").GetNode<Camera3D>("Camera3D");
        head = GetNode<CollisionShape3D>("HeadShape");
    }

    public override void _PhysicsProcess(double delta)
	{
        Move(delta);
        LookAtCursor();
    }

    private void Move(double delta)
    {
        Vector2 moveDir = GetInput();

        _playerDirection = new Vector3(moveDir.X, 0, moveDir.Y).Normalized();

        if (!IsOnFloor())
            _playerDirection.Y -= gravity * (float)delta;

        // Normalize the direction vector to prevent faster diagonal movement
        if (_playerDirection != Vector3.Zero)
        {
            Velocity = Velocity.Lerp(_playerDirection * _speed, (float)delta * _acceleration);

            MoveAndSlide();

            // Calculate the target rotation
            Transform3D transform = GlobalTransform;
            targetBasis = new Basis(Vector3.Up, Mathf.Atan2(_playerDirection.X, _playerDirection.Z));

            // Smoothly rotate towards the target direction using Slerp
            transform.Basis = transform.Basis.Slerp(targetBasis, (float)(_rotationSpeed * delta));
            GlobalTransform = transform;
        }
        else
        {
            // Stop movement if there's no input
            Velocity = Vector3.Zero;
        }
    }

    private Vector2 GetInput()
    {
        Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
        return inputDir;
    }

    private void LookAtCursor()
    {
        // Get the cursor position in 2D on the screen
        Vector2 cursorPosition = GetViewport().GetMousePosition();

        // Create a ray through the cursor in 3D space
        Vector3 from = camera.ProjectRayOrigin(cursorPosition);
        Vector3 to = from + camera.ProjectRayNormal(cursorPosition) * _rayLenght;

        // Manually create a RayCast, check for intersections with the surface
        var spaceState = GetWorld3D().DirectSpaceState;
        var parameters = PhysicsRayQueryParameters3D.Create(from, to);
        var result = spaceState.IntersectRay(parameters);

        if (result.Count > 0)
        {
            // If the ray intersects something, we get the intersection point
            Vector3 cursorWorldPosition = (Vector3)result["position"];

            Vector3 lookAtPosition = new Vector3(cursorWorldPosition.X, GlobalTransform.Origin.Y, cursorWorldPosition.Z);

            // Rotate the character to face the point on the plane
            head.LookAt(lookAtPosition, Vector3.Up);
        }
    }
}
