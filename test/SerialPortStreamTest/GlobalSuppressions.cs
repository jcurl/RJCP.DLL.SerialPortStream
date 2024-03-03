// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0056:Use index operator", Justification = ".NET Framework 4.0 compatibility")]
[assembly: SuppressMessage("Style", "IDE0074:Use compound assignment", Justification = ".NET Framework 4.0 compatibility")]
[assembly: SuppressMessage("Style", "IDE0230:Use UTF-8 string literal", Justification = "Testing byte streams, not UTF8 strings")]
[assembly: SuppressMessage("Performance", "CA1835:Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'", Justification = "Specific API Test")]
