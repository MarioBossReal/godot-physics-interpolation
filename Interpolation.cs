using Godot;
using System;
using System.Collections.Generic;

public partial class Interpolation : Node3D
{
    private static Interpolation Instance { get; set; }

    private Dictionary<Node3D, InterpolationConfig> _interpolatedObjects;

    public override void _Ready()
    {
        Instance ??= this;
        _interpolatedObjects = new();
        ProcessPhysicsPriority = 1000;
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

        var config = Get(node);

        if (config == null)
        {
            config = new InterpolationConfig(node, interpolatePosition, interpolateRotation, discardNonInterpolatedProperties);
            Instance._interpolatedObjects.Add(node, config);
            node.TreeExited += () => Remove(node);
        }
        else
        {
            config.InterpolatePosition = interpolatePosition;
            config.InterpolateRotation = interpolateRotation;
            config.DiscardNonInterpolatedProperties = discardNonInterpolatedProperties;
        }

        config.PreviousTransform = parent.GlobalTransform;
        config.CurrentTransform = parent.GlobalTransform;
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
            var config = pair.Value;
            var node = config.Node;

            if (node == null)
                continue;

            var parent = config.Node.GetParent<Node3D>();

            if (parent == null)
                continue;

            config.PreviousTransform = config.CurrentTransform;
            config.CurrentTransform = parent.GlobalTransform;
        }
    }

    public override void _Process(double delta)
    {
        double fraction = Engine.GetPhysicsInterpolationFraction();

        foreach (var pair in _interpolatedObjects)
        {
            var config = pair.Value;
            var node = config.Node;

            if (node == null)
                continue;

            var parent = config.Node.GetParent<Node3D>();

            if (parent == null)
                continue;

            var newTransform = node.GlobalTransform;

            if (config.DisabledNextFrame)
            {
                if (config.InterpolatePosition || !config.DiscardNonInterpolatedProperties)
                {
                    newTransform.Origin = parent.GlobalPosition;
                }

                if (config.InterpolateRotation || !config.DiscardNonInterpolatedProperties)
                {
                    newTransform.Basis = parent.GlobalTransform.Basis;
                }

                config.CurrentTransform = newTransform;
                config.PreviousTransform = newTransform;
                config.DisabledNextFrame = false;

                continue;
            }

            if (config.InterpolatePosition)
            {
                newTransform.Origin = config.PreviousTransform.Origin.Lerp(config.CurrentTransform.Origin, (float)fraction);
            }
            else if (!config.DiscardNonInterpolatedProperties)
            {
                newTransform.Origin = parent.GlobalPosition;
            }

            if (config.InterpolateRotation)
            {
                newTransform.Basis = config.PreviousTransform.Basis.Slerp(config.CurrentTransform.Basis, (float)fraction);
            }
            else if (!config.DiscardNonInterpolatedProperties)
            {
                newTransform.Basis = parent.GlobalTransform.Basis;
            }

            node.GlobalTransform = newTransform;
        }
    }
}
