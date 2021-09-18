// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Win32
{
    using System;

    internal static partial class Kernel32
    {
        [Flags]
        public enum FileAccess
        {
            /// <summary>
            /// The caller can delete the object.
            /// </summary>
            DELETE = 0x10000,

            /// <summary>
            /// Read access to the owner, group and discretionary access control list (DACL) of the security descriptor,
            /// not including the information in the system access control list (SACL).
            /// </summary>
            READ_CONTROL = 0x20000,

            /// <summary>
            /// Write access to the DACL, to change information for the object.
            /// </summary>
            WRITE_DAC = 0x40000,

            /// <summary>
            /// The right to change the owner in the object's security descriptor.
            /// </summary>
            WRITE_OWNER = 0x80000,

            /// <summary>
            /// The caller can perform a wait operation on the object.
            /// </summary>
            /// <remarks>
            /// This enables a thread to wait until the object is in the signalled state. Some object types do not
            /// support this access right.
            /// </remarks>
            SYNCHRONIZE = 0x100000,

            /// <summary>
            /// Combines DELETE, READ_CONTROL, WRITE_DAC, and WRITE_OWNER access.
            /// </summary>
            STANDARD_RIGHTS_REQUIRED = 0xF0000,

            /// <summary>
            /// Includes READ_CONTROL, which is the right to read information in the file or directory object's security
            /// descriptor. This does not include the information in the SACL.
            /// </summary>
            STANDARD_RIGHTS_READ = READ_CONTROL,

            /// <summary>
            /// Same as STANDARD_RIGHTS_READ, READ_CONTROL.
            /// </summary>
            STANDARD_RIGHTS_WRITE = READ_CONTROL,

            /// <summary>
            /// Same as READ_CONTROL.
            /// </summary>
            STANDARD_RIGHTS_EXECUTE = READ_CONTROL,

            /// <summary>
            /// Combines DELETE, READ_CONTROL, WRITE_DAC, WRITE_OWNER and SYNCHRONIZE access.
            /// </summary>
            STANDARD_RIGHTS_ALL = 0x1F0000,

            /// <summary>
            /// Access System Security.
            /// </summary>
            /// <remarks>
            /// It is used to indicate access to a system access control list (SACL). This type of access requires the
            /// calling process to have the SE_SECURITY_NAME (Manage auditing and security log) privilege. If this flag
            /// is set in the access mask of an audit access ACE (successful or unsuccessful access), the SACL access
            /// will be audited.
            /// </remarks>
            ACCESS_SYSTEM_SECURITY = 0x01000000,

            /// <summary>
            /// Maximum Allowed.
            /// </summary>
            MAXIMUM_ALLOWED = 0x02000000,

            /// <summary>
            /// Read, write and execute access.
            /// </summary>
            GENERIC_ALL = 0x10000000,

            /// <summary>
            /// Execute access.
            /// </summary>
            GENERIC_EXECUTE = 0x20000000,

            /// <summary>
            /// Write access.
            /// </summary>
            GENERIC_WRITE = 0x40000000,

            /// <summary>
            /// Read access.
            /// </summary>
            GENERIC_READ = unchecked((int)0x80000000),

            /// <summary>
            /// The right to read the corresponding file data.
            /// </summary>
            FILE_READ_DATA = 0x0001,

            /// <summary>
            /// The right to list the contents of the directory.
            /// </summary>
            FILE_LIST_DIRECTORY = 0x0001,

            /// <summary>
            /// Read data from a pipe. Always has SYNCHRONIZE access.
            /// </summary>
            PIPE_ACCESS_INBOUND = 0x0001,

            /// <summary>
            /// the right to write data to the file.
            /// </summary>
            FILE_WRITE_DATA = 0x0002,

            /// <summary>
            /// The right to create a file in the directory.
            /// </summary>
            FILE_ADD_FILE = 0x0002,

            /// <summary>
            /// Write data to a pipe. Always has SYNCHRONIZE access.
            /// </summary>
            PIPE_ACCESS_OUTBOUND = 0x0002,

            /// <summary>
            /// Allow read/write access to the pipe. Always has SYNCHRONIZE access.
            /// </summary>
            PIPE_ACCESS_DUPLEX = 0x0003,

            /// <summary>
            /// For a file object, the right to append data to the file.
            /// </summary>
            /// <remarks>
            /// For local files, write operations will not overwrite existing data if this flag is specified without
            /// FILE_WRITE_DATA.
            /// </remarks>
            FILE_APPEND_DATA = 0x0004,

            /// <summary>
            /// The right to create a subdirectory.
            /// </summary>
            /// <remarks>
            /// For local directories, write operations will not overwrite existing data if this flag is specified
            /// without FILE_ADD_SUBDIRECTORY.
            /// </remarks>
            FILE_ADD_SUBDIRECTORY = 0x0004,

            /// <summary>
            /// The right to create a pipe.
            /// </summary>
            FILE_CREATE_PIPE_INSTANCE = 0x0004,

            /// <summary>
            /// The right to read extended file attributes.
            /// </summary>
            FILE_READ_EA = 0x0008,

            /// <summary>
            /// The right to write extended file attributes.
            /// </summary>
            FILE_WRITE_EA = 0x0010,

            /// <summary>
            /// For a native code file, the right to execute the file.
            /// </summary>
            /// <remarks>
            /// This access right given to scripts may cause the script to be executable, depending on the script
            /// interpreter.
            /// </remarks>
            FILE_EXECUTE = 0x0020,

            /// <summary>
            /// The right to traverse the directory.
            /// </summary>
            /// <remarks>
            /// By default, users are assigned the BYPASS_TRAVERSE_CHECKING privilege, which ignores the FILE_TRAVERSE
            /// access right. See the remarks in "File Security and Access Rights" for more information.
            /// </remarks>
            FILE_TRAVERSE = 0x0020,

            /// <summary>
            /// The right to delete a directory and all the files it contains, including read-only files.
            /// </summary>
            FILE_DELETE_CHILD = 0x0040,

            /// <summary>
            /// The right to read file attributes, for file, directory and pipe.
            /// </summary>
            FILE_READ_ATTRIBUTES = 0x0080,

            /// <summary>
            /// FThe right to write file attributes.
            /// </summary>
            FILE_WRITE_ATTRIBUTES = 0x0100,

            /// <summary>
            /// All possible access rights for a file.
            /// </summary>
            FILE_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x1FF,

            /// <summary>
            /// Provide generic read access to a file.
            /// </summary>
            FILE_GENERIC_READ = STANDARD_RIGHTS_READ | FILE_READ_DATA | FILE_READ_ATTRIBUTES | FILE_READ_EA | SYNCHRONIZE,

            /// <summary>
            /// Provide generic write access to a file.
            /// </summary>
            FILE_GENERIC_WRITE = STANDARD_RIGHTS_WRITE | FILE_WRITE_DATA | FILE_WRITE_ATTRIBUTES | FILE_WRITE_EA | FILE_APPEND_DATA | SYNCHRONIZE,

            /// <summary>
            /// Provide generic execute access to a file.
            /// </summary>
            FILE_GENERIC_EXECUTE = STANDARD_RIGHTS_EXECUTE | FILE_READ_ATTRIBUTES | FILE_EXECUTE | SYNCHRONIZE,

            SPECIFIC_RIGHTS_ALL = 0xFFFF,
        }
    }
}
