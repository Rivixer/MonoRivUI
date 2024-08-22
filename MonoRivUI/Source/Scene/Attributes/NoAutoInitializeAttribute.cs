using System;

namespace MonoRivUI;

/// <summary>
/// Represents an attribute that indicates that the class should not be auto-initialized.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class NoAutoInitializeAttribute : Attribute
{
}
