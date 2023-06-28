using Godot;
using System;

public class InterpolationConfig
{
    public Node3D Node { get; private set; }
    public Vector3 InterpolatedPosition { get; set; }
    public Quaternion InterpolatedQuaternion { get; set; }
    public Vector3 FrameStartPosition { get; set; }
    public Quaternion FrameStartQuaternion { get; set; }

    public bool InterpolatePosition { get; set; }
    public bool InterpolateRotation { get; set; }
    public bool DisableNextFrame { get; set; }
    public bool DiscardNonInterpolatedProperties { get; set; }

    public InterpolationConfig(Node3D node, bool interpolatePosition = true, bool interpolateRotation = true, bool discardNonInterpolatedProperties = false)
    {
        Node = node;
        InterpolatePosition = interpolatePosition;
        InterpolateRotation = interpolateRotation;
        DiscardNonInterpolatedProperties = discardNonInterpolatedProperties;
    }
}
