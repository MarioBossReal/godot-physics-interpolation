# Godot 4.1 Physics Interpolation

A simple, flexible C# solution for interpolating transforms in Godot 4.1.

Installation:

1. Clone repo into your Godot C# project. Build your project.<br>
2. Go to Project/Project Settings/Autoload and add Interpolation.cs. Make sure it is enabled.<br>
3. Disable physics jitter fix in your project settings.<br>
4. (Reccommended) Disable vsync in your project settings.<br>

Usage:

The asset works by using <code>Engine.GetPhysicsInterpolationFraction()</code> to interpolate a node's position and rotation to that of its parent.

To enable interpolation on a node, simply call the static method <code>Interpolation.Add(node)</code> from anywhere you like.<br>

The method returns an <code>InterpolationConfig</code> object which can be used to further control how the node gets interpolated.<br>

By default, both position and rotation will be interpolated. You can change this by overriding the default parameters when adding a node:
<code>Interpolator.Add(Node3D node, bool interpolatePosition = true, bool interpolateRotation = true, bool discardNonInterpolatedProperties = false)</code>

<code>InterpolationConfig.DiscardNonInterpolatedProperties</code> will determine if the non-interpolated properties of a node will inherit those of its parent.

You can set <code>InterpolationConfig.DisableNextFrame = true</code> to disable interpolation for one frame. It will be automatically set to false at the end of the frame.

You can call the static method <code>Interpolation.Get(Node3D node)</code> to retrieve its <code>InterpolationConfig</code> object should it have one.

You can remove interpolation from a node by calling the static method <code>Interpolation.Remove(Node3D node)</code>

A basic example script <code>BasicInterpolator.cs</code> has been provided for demonstration. You can attach it directly to a node (such as the MeshInstance3D of a rigidbody) to test if interpolation is working, and see how different settings affect it. You can delete this file if you don't need it.

Obviously, you should not interpolate the position of any physics nodes directly. The point of interpolation is to separate the visual representation of an entity with its physical representation. You can and should also use this asset to interpolate your Camera's position. You could also use it to interpolate it's rotation, but you probably don't want to. For responsive mouse movement, set <code>Input.UseAccumulatedInput</code> to <code>false</code> and update your camera's rotation directly from <code>_Input()</code> or <code>_UnhandledInput()</code>. Or, if your camera is a child of another node which (smoothly) controls its rotation/position, just disable <code>InterpolationConfig.InterpolateRotation</code> and make sure <code>InterpolationConfig.DiscardNonInterpolatedProperties</code> is also set to <code>false</code>
