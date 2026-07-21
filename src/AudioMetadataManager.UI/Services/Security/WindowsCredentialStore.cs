using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace AudioMetadataManager.UI.Services.Security;

/// <summary>
/// Guarda secretos como credenciales genéricas en el
/// Administrador de credenciales de Windows.
///
/// Los secretos quedan asociados al usuario de Windows actual
/// y no se almacenan dentro del repositorio del proyecto.
/// </summary>
public sealed class WindowsCredentialStore
    : ISecretStore
{
    private const uint GenericCredentialType = 1;

    private const uint LocalMachinePersistence = 2;

    private const int ErrorNotFound = 1168;

    /// <inheritdoc />
    public void SaveSecret(
        string targetName,
        string secret)
    {
        ValidateTargetName(
            targetName);

        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new ArgumentException(
                "El secreto no puede estar vacío.",
                nameof(secret));
        }

        byte[] secretBytes =
            Encoding.Unicode.GetBytes(
                secret);

        IntPtr secretPointer =
            IntPtr.Zero;

        try
        {
            secretPointer =
                Marshal.AllocHGlobal(
                    secretBytes.Length);

            Marshal.Copy(
                secretBytes,
                0,
                secretPointer,
                secretBytes.Length);

            NativeCredential credential =
                new()
                {
                    Type =
                        GenericCredentialType,

                    TargetName =
                        targetName.Trim(),

                    CredentialBlobSize =
                        (uint)secretBytes.Length,

                    CredentialBlob =
                        secretPointer,

                    Persist =
                        LocalMachinePersistence,

                    UserName =
                        Environment.UserName,

                    Comment =
                        "Audio Metadata Manager secure credential"
                };

            bool success =
                CredWrite(
                    ref credential,
                    0);

            if (!success)
            {
                throw CreateNativeException(
                    "No fue posible guardar la credencial.");
            }
        }
        finally
        {
            ClearManagedBytes(
                secretBytes);

            if (secretPointer != IntPtr.Zero)
            {
                ClearUnmanagedMemory(
                    secretPointer,
                    secretBytes.Length);

                Marshal.FreeHGlobal(
                    secretPointer);
            }
        }
    }

    /// <inheritdoc />
    public string? ReadSecret(
        string targetName)
    {
        ValidateTargetName(
            targetName);

        bool success =
            CredRead(
                targetName.Trim(),
                GenericCredentialType,
                0,
                out IntPtr credentialPointer);

        if (!success)
        {
            int errorCode =
                Marshal.GetLastWin32Error();

            if (errorCode == ErrorNotFound)
            {
                return null;
            }

            throw new Win32Exception(
                errorCode,
                "No fue posible leer la credencial.");
        }

        try
        {
            NativeCredential credential =
                Marshal.PtrToStructure<
                    NativeCredential>(
                        credentialPointer);

            if (credential.CredentialBlob ==
                    IntPtr.Zero ||
                credential.CredentialBlobSize == 0)
            {
                return null;
            }

            int byteCount =
                checked(
                    (int)credential.CredentialBlobSize);

            byte[] secretBytes =
                new byte[byteCount];

            try
            {
                Marshal.Copy(
                    credential.CredentialBlob,
                    secretBytes,
                    0,
                    byteCount);

                string secret =
                    Encoding.Unicode.GetString(
                        secretBytes);

                return string.IsNullOrWhiteSpace(
                        secret)
                            ? null
                            : secret;
            }
            finally
            {
                ClearManagedBytes(
                    secretBytes);
            }
        }
        finally
        {
            CredFree(
                credentialPointer);
        }
    }

    /// <inheritdoc />
    public bool DeleteSecret(
        string targetName)
    {
        ValidateTargetName(
            targetName);

        bool success =
            CredDelete(
                targetName.Trim(),
                GenericCredentialType,
                0);

        if (success)
        {
            return true;
        }

        int errorCode =
            Marshal.GetLastWin32Error();

        if (errorCode == ErrorNotFound)
        {
            return true;
        }

        throw new Win32Exception(
            errorCode,
            "No fue posible eliminar la credencial.");
    }

    /// <inheritdoc />
    public bool ContainsSecret(
        string targetName)
    {
        return !string.IsNullOrWhiteSpace(
            ReadSecret(
                targetName));
    }

    private static void ValidateTargetName(
        string targetName)
    {
        if (string.IsNullOrWhiteSpace(
                targetName))
        {
            throw new ArgumentException(
                "El identificador de la credencial no puede estar vacío.",
                nameof(targetName));
        }
    }

    private static Win32Exception
        CreateNativeException(
            string message)
    {
        int errorCode =
            Marshal.GetLastWin32Error();

        return new Win32Exception(
            errorCode,
            message);
    }

    private static void ClearManagedBytes(
        byte[] bytes)
    {
        if (bytes.Length > 0)
        {
            Array.Clear(
                bytes,
                0,
                bytes.Length);
        }
    }

    private static void ClearUnmanagedMemory(
        IntPtr pointer,
        int length)
    {
        for (int index = 0;
             index < length;
             index++)
        {
            Marshal.WriteByte(
                pointer,
                index,
                0);
        }
    }

    [StructLayout(
        LayoutKind.Sequential,
        CharSet = CharSet.Unicode)]
    private struct NativeCredential
    {
        public uint Flags;

        public uint Type;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string TargetName;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? Comment;

        public System.Runtime.InteropServices.ComTypes.FILETIME
            LastWritten;

        public uint CredentialBlobSize;

        public IntPtr CredentialBlob;

        public uint Persist;

        public uint AttributeCount;

        public IntPtr Attributes;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? TargetAlias;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string? UserName;
    }

    [DllImport(
        "Advapi32.dll",
        EntryPoint = "CredWriteW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredWrite(
        ref NativeCredential credential,
        uint flags);

    [DllImport(
        "Advapi32.dll",
        EntryPoint = "CredReadW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredRead(
        string targetName,
        uint type,
        uint flags,
        out IntPtr credentialPointer);

    [DllImport(
        "Advapi32.dll",
        EntryPoint = "CredDeleteW",
        CharSet = CharSet.Unicode,
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CredDelete(
        string targetName,
        uint type,
        uint flags);

    [DllImport(
        "Advapi32.dll",
        EntryPoint = "CredFree")]
    private static extern void CredFree(
        IntPtr credentialPointer);
}