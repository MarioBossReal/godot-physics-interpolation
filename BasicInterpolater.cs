using Godot;
using System;

public partial class BasicInterpolater : Node3D
{
    [Export]
    private bool InterpolatePosition { get; set; }

    [Export]
    private bool InterpolateRotation { get; set; }

    [Export]
    private bool DiscardNonInterpolatedProperties { get; set; }

    public override void _Ready()
    {
        Interpolation.Add(this, InterpolatePosition, InterpolateRotation, DiscardNonInterpolatedProperties);
    }
}
