// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to this project.
// Project-level suppressions either have no target or are given a specific target and scoped to a namespace, type,
// member, etc.
//
// To add a suppression to this file, right-click the message in the Code Analysis results, point to "Suppress Message",
// and click "In Suppression File". You do not need to add suppressions to this file manually.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Justification = "Native Methods", Scope = "namespaceanddescendants", Target = "~N:RJCP.IO.Ports.Native.Win32")]
[assembly: SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Native Methods", Scope = "namespaceanddescendants", Target = "~N:RJCP.IO.Ports.Native.Win32")]
[assembly: SuppressMessage("Style", "IDE0074:Use compound assignment", Justification = ".NET Core only feature")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "NativeMethods", Scope = "namespaceanddescendants", Target = "~N:RJCP.IO.Ports.Native.Unix")]
