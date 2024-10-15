using System;

namespace MonoRivUI;

/// <summary>
/// Represents an attribute that indicates that the class should be auto-initialized.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AutoInitializeAttribute : Attribute
{
}
