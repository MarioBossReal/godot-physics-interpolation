using Godot;
using System.Collections.Generic;

public partial class Interpolation : Node3D
{
    private static Interpolation Instance { get; set; }

    private Dictionary<Node3D, InterpolationConfig> _interpolatedObjects;

    public override void _Ready()
    {
        Instance ??= this;
        _interpolatedObjects = new();
        ProcessPhysicsPriority = -1000;
        ProcessPriority = 1000;
    }

    /// <summary>
    /// Creates an InterpolationConfig for the given node with it's parent as the target object and adds it to the interpolation pool.
    /// 
    /// </summary>
    /// <param name="node">The node to interpolate.</param>
    /// <param name="interpolatePosition">When true, the given node's position will be smoothly interpolated to that of its parent's.</param>
    /// <param name="interpolateRotation">When true, the given node's rotation will be smoothly interpolated to that of its parent's.</param>
    /// <param name="discardNonInterpolatedProperties">When true, the given node will not inherit any properties which are not set to interpolate. Otherwise non-interpolated properties will be inherited as normal.</param>
    /// <returns>Returns either a newly created InterpolationConfig, or one which already exists for the given node.</returns>
    public static InterpolationConfig Add(Node3D node, bool interpolatePosition = true, bool interpolateRotation = true, bool discardNonInterpolatedProperties = false)
    {
        Node3D parent = node.GetParent<Node3D>();

        if (parent == null)
            return null;

        InterpolationConfig config;

        config = Get(node);

        if (config == null)
        {
            config = new InterpolationConfig(node, interpolatePosition, interpolateRotation, discardNonInterpolatedProperties);
            Instance._interpolatedObjects.Add(node, config);
        }
        else
        {
            config.InterpolatePosition = interpolatePosition;
            config.InterpolateRotation = interpolateRotation;
            config.DiscardNonInterpolatedProperties = discardNonInterpolatedProperties;
        }

        config.InterpolatedPosition = parent.GlobalPosition;
        config.InterpolatedQuaternion = parent.Quaternion.Normalized();
        config.FrameStartPosition = parent.GlobalPosition;
        config.FrameStartQuaternion = parent.Quaternion.Normalized();

        node.TopLevel = true;
        return config;
    }

    /// <summary>
    /// Remove a node from the interpolation pool.
    /// </summary>
    /// <param name="node"></param>
    public static void Remove(Node3D node)
    {
        if (Instance._interpolatedObjects.Remove(node))
        {
            node.TopLevel = false;
        }
    }

    /// <summary>
    /// Get a node's InterpolationConfig from the interpolation pool if it has one.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static InterpolationConfig Get(Node3D node)
    {
        Instance._interpolatedObjects.TryGetValue(node, out var config);
        return config;
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (var pair in _interpolatedObjects)
        {
            InterpolationConfig config = pair.Value;
            Node3D node = config.Node;

            if (node == null)
                continue;

            Node3D parent = config.Node.GetParent<Node3D>();

            if (parent == null)
                continue;

            config.FrameStartPosition = parent.GlobalPosition;
            config.FrameStartQuaternion = parent.Quaternion.Normalized();
        }
    }

    public override void _Process(double delta)
    {
        double fraction = Engine.GetPhysicsInterpolationFraction();

        foreach (var pair in _interpolatedObjects)
        {
            InterpolationConfig config = pair.Value;
            Node3D node = config.Node;

            if (node == null)
                continue;

            Node3D parent = config.Node.GetParent<Node3D>();

            if (parent == null)
                continue;

            if (config.DisableNextFrame)
            {
                if (config.InterpolatePosition || !config.DiscardNonInterpolatedProperties)
                {
                    node.GlobalPosition = parent.GlobalPosition;
                    config.FrameStartPosition = parent.GlobalPosition;
                    config.InterpolatedPosition = parent.GlobalPosition;
                }

                if (config.InterpolateRotation || !config.DiscardNonInterpolatedProperties)
                {
                    node.GlobalRotation = parent.GlobalRotation;
                    config.InterpolatedQuaternion = parent.Quaternion.Normalized();
                    config.FrameStartQuaternion = parent.Quaternion.Normalized();
                }

                config.DisableNextFrame = false;
                continue;
            }

            if (config.InterpolatePosition)
            {
                config.InterpolatedPosition = config.FrameStartPosition.Lerp(parent.GlobalPosition, (float)fraction);
                node.GlobalPosition = config.InterpolatedPosition;
            }
            else if (!config.DiscardNonInterpolatedProperties)
            {
                node.GlobalPosition = parent.GlobalPosition;
                config.InterpolatedPosition = parent.GlobalPosition;
                config.FrameStartPosition = parent.GlobalPosition;
            }

            if (config.InterpolateRotation)
            {
                config.InterpolatedQuaternion = config.FrameStartQuaternion.Slerp(parent.Quaternion, (float)fraction).Normalized();
                node.Quaternion = config.InterpolatedQuaternion;
            }
            else if (!config.DiscardNonInterpolatedProperties)
            {
                node.GlobalRotation = parent.GlobalRotation;
                config.InterpolatedQuaternion = parent.Quaternion.Normalized();
                config.FrameStartQuaternion = parent.Quaternion.Normalized();
            }
        }
    }
}
