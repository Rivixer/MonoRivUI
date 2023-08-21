// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "StyleCop.CSharp.OrderingRules",
    "SA1200:Using directives should be placed correctly",
    Justification = "We prefer to put using directives at the beginning of the file.")]

[assembly: SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1633:File should have header",
    Justification = "Temporary solution to supress the warning.")]