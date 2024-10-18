using Godot;

public partial class Camera : Node3D
{
    [Export]
    private float cameraFollowSpeed = 4.0f;

    private CharacterBody3D player;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        player = GetNode<CharacterBody3D>("../Player");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
        if (player == null)
            return;

        Position = Position.Lerp(player.Position, (float)delta * cameraFollowSpeed);
    }
}
