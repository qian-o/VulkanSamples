using System.Numerics;
using Graphics.Core;

namespace Graphics.Vulkan;

/// <summary>
/// This data structure is used in GPU memory during acceleration structure build.
/// This struct definition is useful if generating instance data on the CPU first
/// then uploading to the GPU. But apps are also free to generate instance descriptions
/// directly into GPU memory from compute shaders for instance, following the same
/// layout.
/// </summary>
public class AccelStructInstance
{
    /// <summary>
    /// the bottom-level acceleration structure that is being instanced.
    /// </summary>
    public BottomLevelAS? BottomLevel { get; set; }

    /// <summary>
    /// An arbitrary 24-bit value that can be accessed via InstanceID() in shader.
    /// </summary>
    public uint InstanceID { get; set; }

    /// <summary>
    /// Per-instance contribution to add into shader table indexing to select the hit
    /// group to use. It is the offset of the instance inside the sahder-binding-table.
    /// </summary>
    public uint InstanceContributionToHitGroupIndex { get; set; }

    /// <summary>
    /// An 8-bit mask assigned to the instance, which can be used to include/reject groups
    /// of instances on a per-ray basis.
    /// </summary>
    public byte InstanceMask { get; set; }

    /// <summary>
    /// The options for the instance.
    /// </summary>
    public AccelStructInstanceOptions Options { get; set; }

    /// <summary>
    /// A 4x4 transform matrix in row major layout representing the instance-to-world
    /// transformation.
    /// </summary>
    public Matrix4x4 Transform4x4 { get; set; } = Matrix4x4.Identity;
}
