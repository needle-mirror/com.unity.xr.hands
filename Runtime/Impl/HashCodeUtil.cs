namespace UnityEngine.XR.Hands
{
    static class HashCodeUtil
    {
        internal static int ReferenceHash(object obj)
            => obj != null ? System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj) : 0;

        internal static int Combine(int hash1, int hash2)
        {
            unchecked
            {
                return hash1 * 486187739 + hash2;
            }
        }

        internal static int Combine(int hash1, int hash2, int hash3)
            => Combine(Combine(hash1, hash2), hash3);
        internal static int Combine(int hash1, int hash2, int hash3, int hash4)
            => Combine(Combine(hash1, hash2, hash3), hash4);
        internal static int Combine(int hash1, int hash2, int hash3, int hash4, int hash5)
            => Combine(Combine(hash1, hash2, hash3, hash4), hash5);
        internal static int Combine(int hash1, int hash2, int hash3, int hash4, int hash5, int hash6)
            => Combine(Combine(hash1, hash2, hash3, hash4, hash5), hash6);
        internal static int Combine(int hash1, int hash2, int hash3, int hash4, int hash5, int hash6, int hash7)
            => Combine(Combine(hash1, hash2, hash3, hash4, hash5, hash6), hash7);
        internal static int Combine(int hash1, int hash2, int hash3, int hash4, int hash5, int hash6, int hash7, int hash8)
            => Combine(Combine(hash1, hash2, hash3, hash4, hash5, hash6, hash7), hash8);
        internal static int Combine(int hash1, int hash2, int hash3, int hash4, int hash5, int hash6, int hash7, int hash8, int hash9)
            => Combine(Combine(hash1, hash2, hash3, hash4, hash5, hash6, hash7, hash8), hash9);
        internal static int Combine(int hash1, int hash2, int hash3, int hash4, int hash5, int hash6, int hash7, int hash8, int hash9, int hash10)
            => Combine(Combine(hash1, hash2, hash3, hash4, hash5, hash6, hash7, hash8, hash9), hash10);
        internal static int Combine(int hash1, int hash2, int hash3, int hash4, int hash5, int hash6, int hash7, int hash8, int hash9, int hash10, int hash11)
            => Combine(Combine(hash1, hash2, hash3, hash4, hash5, hash6, hash7, hash8, hash9, hash10), hash11);
        internal static int Combine(int hash1, int hash2, int hash3, int hash4, int hash5, int hash6, int hash7, int hash8, int hash9, int hash10, int hash11, int hash12)
            => Combine(Combine(hash1, hash2, hash3, hash4, hash5, hash6, hash7, hash8, hash9, hash10, hash11), hash12);
    }
}
