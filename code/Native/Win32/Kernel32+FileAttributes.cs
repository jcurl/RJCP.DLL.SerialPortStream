// Copyright © Jason Curl 2012-2021
// Sources at https://github.com/jcurl/SerialPortStream
// Licensed under the Microsoft Public License (Ms-PL)

namespace RJCP.IO.Ports.Native.Win32
{
    using System;

    internal static partial class Kernel32
    {
        [Flags]
        public enum FileAttributes
        {
            /// <summary>
            /// A file that is read-only
            /// </summary>
            /// <remarks>
            /// Applications can read the file, but cannot write to it or delete it. This attribute is not honoured on
            /// directories. For more information, see "You cannot view or change the Read-only or the System attributes
            /// of folders in Windows Server 2003, in Windows XP, in Windows Vista or in Windows 7".
            /// </remarks>
            FILE_ATTRIBUTE_READONLY = 0x00000001,

            /// <summary>
            /// The file or directory is hidden.
            /// </summary>
            /// <remarks>It is not included in an ordinary directory listing.</remarks>
            FILE_ATTRIBUTE_HIDDEN = 0x00000002,

            /// <summary>
            /// A file or directory that the operating system uses a part of, or uses exclusively.
            /// </summary>
            FILE_ATTRIBUTE_SYSTEM = 0x00000004,

            /// <summary>
            /// The handle that identifies a directory.
            /// </summary>
            FILE_ATTRIBUTE_DIRECTORY = 0x00000010,

            /// <summary>
            /// A file or directory that is an archive file or directory.
            /// </summary>
            /// <remarks>Applications typically use this attribute to mark files for backup or removal.</remarks>
            FILE_ATTRIBUTE_ARCHIVE = 0x00000020,

            /// <summary>
            /// This value is reserved for system use.
            /// </summary>
            FILE_ATTRIBUTE_DEVICE = 0x00000040,

            /// <summary>
            /// A file that does not have other attributes set.
            /// </summary>
            /// <remarks>This attribute is valid only when used alone.</remarks>
            FILE_ATTRIBUTE_NORMAL = 0x00000080,

            /// <summary>
            /// A file that is being used for temporary storage.
            /// </summary>
            /// <remarks>
            /// File systems avoid writing data back to mass storage if sufficient cache memory is available, because
            /// typically, an application deletes a temporary file after the handle is closed. In that scenario, the
            /// system can entirely avoid writing the data. Otherwise, the data is written after the handle is closed.
            /// </remarks>
            FILE_ATTRIBUTE_TEMPORARY = 0x00000100,

            /// <summary>
            /// A file that is a sparse file.
            /// </summary>
            FILE_ATTRIBUTE_SPARSE_FILE = 0x00000200,

            /// <summary>
            /// A file or directory that has an associated reparse point, or a file that is a symbolic link.
            /// </summary>
            FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400,

            /// <summary>
            /// A file or directory that is compressed.
            /// </summary>
            /// <remarks>
            /// For a file, all of the data in the file is compressed. For a directory, compression is the default for
            /// newly created files and subdirectories.
            /// </remarks>
            FILE_ATTRIBUTE_COMPRESSED = 0x00000800,

            /// <summary>
            /// The data of a file is not available immediately.
            /// </summary>
            /// <remarks>
            /// This attribute indicates that the file data is physically moved to offline storage. This attribute is
            /// used by Remote Storage, which is the hierarchical storage management software. Applications should not
            /// arbitrarily change this attribute.
            /// </remarks>
            FILE_ATTRIBUTE_OFFLINE = 0x00001000,

            /// <summary>
            /// The file or directory is not to be indexed by the content indexing service.
            /// </summary>
            FILE_ATTRIBUTE_NOT_CONTENT_INDEXED = 0x00002000,

            /// <summary>
            /// A file or directory that is encrypted.
            /// </summary>
            /// <remarks>
            /// For a file, all data streams in the file are encrypted. For a directory, encryption is the default for
            /// newly created files and subdirectories.
            /// </remarks>
            FILE_ATTRIBUTE_ENCRYPTED = 0x00004000,

            /// <summary>
            /// This value is reserved for system use.
            /// </summary>
            FILE_ATTRIBUTE_VIRTUAL = 0x00010000,

            /// <summary>
            /// If you attempt to create multiple instances of a pipe with this flag, creation of the first instance
            /// succeeds, but creation of the next instance fails with ERROR_ACCESS_DENIED.
            /// </summary>
            /// <remarks>Windows 2000: This flag is not supported until Windows 2000 SP2 and Windows XP.</remarks>
            FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000,

            /// <summary>
            /// The file data is requested, but it should continue to be located in remote storage. It should not be
            /// transported back to local storage. This flag is for use by remote storage systems.
            /// </summary>
            FILE_FLAG_OPEN_NO_RECALL = 0x00100000,

            /// <summary>
            /// Normal reparse point processing will not occur.
            /// </summary>
            /// <remarks>
            /// CreateFile will attempt to open the reparse point. When a file is opened, a file handle is returned,
            /// whether or not the filter that controls the reparse point is operational. This flag cannot be used with
            /// the CREATE_ALWAYS flag. If the file is not a reparse point, then this flag is ignored.
            /// </remarks>
            FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000,

            /// <summary>
            /// Access will occur according to POSIX rules.
            /// </summary>
            /// <remarks>
            /// This includes allowing multiple files with names, differing only in case, for file systems that support
            /// that naming. Use care when using this option, because files created with this flag may not be accessible
            /// by applications that are written for MS-DOS or 16-bit Windows.
            /// </remarks>
            FILE_FLAG_POSIX_SEMANTICS = 0x01000000,

            /// <summary>
            /// The file is being opened or created for a backup or restore operation.
            /// </summary>
            /// <remarks>
            /// The system ensures that the calling process overrides file security checks when the process has
            /// SE_BACKUP_NAME and SE_RESTORE_NAME privileges. For more information, see "Changing Privileges in a
            /// Token".
            /// </remarks>
            FILE_FLAG_BACKUP_SEMANTICS = 0x02000000,

            /// <summary>
            /// The file is to be deleted immediately after all of its handles are closed, which includes the specified
            /// handle and any other open or duplicated handles.
            /// </summary>
            /// <remarks>
            /// If there are existing open handles to a file, the call fails unless they were all opened with the
            /// FILE_SHARE_DELETE share mode. Subsequent open requests for the file fail, unless the FILE_SHARE_DELETE
            /// share mode is specified.
            /// </remarks>
            FILE_FLAG_DELETE_ON_CLOSE = 0x04000000,

            /// <summary>
            /// Access is intended to be sequential from beginning to end.
            /// </summary>
            /// <remarks>
            /// The system can use this as a hint to optimize file caching. This flag should not be used if read-behind
            /// (that is, reverse scans) will be used. This flag has no effect if the file system does not support
            /// cached I/O and FILE_FLAG_NO_BUFFERING. For more information, see the Caching Behaviour section of
            /// CreateFile().
            /// </remarks>
            FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000,

            /// <summary>
            /// Access is intended to be random.
            /// </summary>
            /// <remarks>
            /// The system can use this as a hint to optimize file caching. This flag has no effect if the file system
            /// does not support cached I/O and FILE_FLAG_NO_BUFFERING. For more information, see the Caching Behaviour
            /// section of CreateFile().
            /// </remarks>
            FILE_FLAG_RANDOM_ACCESS = 0x10000000,

            /// <summary>
            /// The file or device is being opened with no system caching for data reads and writes. This flag does not
            /// affect hard disk caching or memory mapped files.
            /// </summary>
            /// <remarks>
            /// There are strict requirements for successfully working with files opened with CreateFile using the
            /// FILE_FLAG_NO_BUFFERING flag, for details see "File Buffering".
            /// </remarks>
            FILE_FLAG_NO_BUFFERING = 0x20000000,

            /// <summary>
            /// The file or device is being opened or created for asynchronous I/O.
            /// </summary>
            /// <remarks>
            /// When subsequent I/O operations are completed on this handle, the event specified in the OVERLAPPED
            /// structure will be set to the signalled state. If this flag is specified, the file can be used for
            /// simultaneous read and write operations. If this flag is not specified, then I/O operations are
            /// serialized, even if the calls to the read and write functions specify an OVERLAPPED structure. For
            /// information about considerations when using a file handle created with this flag, see the Synchronous
            /// and Asynchronous I/O Handles section of CreateFile().
            /// </remarks>
            FILE_FLAG_OVERLAPPED = 0x40000000,

            /// <summary>
            /// Write operations will not go through any intermediate cache, they will go directly to disk.
            /// </summary>
            /// <remarks>For additional information, see the Caching Behaviour section of CreateFile().</remarks>
            FILE_FLAG_WRITE_THROUGH = unchecked((int)0x80000000),
        }
    }
}
