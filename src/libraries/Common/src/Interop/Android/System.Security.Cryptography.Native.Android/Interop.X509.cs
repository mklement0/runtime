// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

internal static partial class Interop
{
    internal static partial class AndroidCrypto
    {
        [DllImport(Libraries.CryptoNative, EntryPoint = "AndroidCryptoNative_X509Decode")]
        internal static extern SafeX509Handle X509Decode(ref byte buf, int len);

        [DllImport(Libraries.CryptoNative, EntryPoint = "AndroidCryptoNative_X509Encode")]
        private static extern int X509Encode(SafeX509Handle x, [Out] byte[]? buf, int len);
        internal static byte[] X509Encode(SafeX509Handle x)
        {
            return GetDynamicBuffer((ptr, buf, i) => X509Encode(ptr, buf, i), x);
        }

        [DllImport(Libraries.CryptoNative, EntryPoint = "AndroidCryptoNative_X509DecodeCollection")]
        private static extern int X509DecodeCollection(ref byte buf, int bufLen, IntPtr[]? ptrs, ref int handlesLen);
        internal static SafeX509Handle[] X509DecodeCollection(ReadOnlySpan<byte> data)
        {
            const int INSUFFICIENT_BUFFER = -1;
            const int SUCCESS = 1;

            ref byte buf = ref MemoryMarshal.GetReference(data);
            int size = 0;
            int ret = X509DecodeCollection(ref buf, data.Length, null, ref size);
            if (ret == SUCCESS && size == 0)
                return Array.Empty<SafeX509Handle>();

            if (ret != INSUFFICIENT_BUFFER)
                throw new CryptographicException();

            IntPtr[] ptrs = new IntPtr[size];
            ret = X509DecodeCollection(ref buf, data.Length, ptrs, ref size);
            if (ret != SUCCESS)
                throw new CryptographicException();

            SafeX509Handle[] handles = new SafeX509Handle[ptrs.Length];
            for (var i = 0; i < handles.Length; i++)
            {
                handles[i] = new SafeX509Handle(ptrs[i]);
            }

            return handles;
        }

        [DllImport(Libraries.CryptoNative, EntryPoint = "AndroidCryptoNative_X509GetContentType")]
        private static extern X509ContentType X509GetContentType(ref byte buf, int len);
        internal static X509ContentType X509GetContentType(ReadOnlySpan<byte> data)
        {
            return X509GetContentType(ref MemoryMarshal.GetReference(data), data.Length);
        }

        internal enum PAL_KeyAlgorithm
        {
            DSA,
            EC,
            RSA,
        }

        [DllImport(Libraries.CryptoNative, EntryPoint = "AndroidCryptoNative_X509PublicKey")]
        internal static extern IntPtr X509GetPublicKey(SafeX509Handle x, PAL_KeyAlgorithm algorithm);
    }
}

namespace System.Security.Cryptography.X509Certificates
{
    internal sealed class SafeX509Handle : Interop.JObjectLifetime.SafeJObjectHandle
    {
        public SafeX509Handle()
        {
        }

        internal SafeX509Handle(IntPtr ptr)
            : base(ptr)
        {
        }
    }
}
