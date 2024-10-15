using System;

namespace MonoRivUI;

/// <summary>
/// Represents an attribute that indicates that the scene's content should be auto-loaded.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AutoLoadContentAttribute : Attribute
{
}
