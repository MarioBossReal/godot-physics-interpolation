using Godot;
using System;

public class InterpolationConfig
{
    public Node3D Node { get; private set; }
    public Transform3D PreviousTransform { get; set; }
    public Transform3D CurrentTransform { get; set; }

    public bool InterpolatePosition { get; set; }
    public bool InterpolateRotation { get; set; }
    public bool DisabledNextFrame { get; set; }
    public bool DiscardNonInterpolatedProperties { get; set; }

    public InterpolationConfig(Node3D node, bool interpolatePosition = true, bool interpolateRotation = true, bool discardNonInterpolatedProperties = false)
    {
        Node = node;
        InterpolatePosition = interpolatePosition;
        InterpolateRotation = interpolateRotation;
        DiscardNonInterpolatedProperties = discardNonInterpolatedProperties;
        DisabledNextFrame = false;
    }

    public void DisableNextFrame()
    {
        DisabledNextFrame = true;
    }
}
