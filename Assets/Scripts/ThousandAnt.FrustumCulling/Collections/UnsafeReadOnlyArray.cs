using Unity.Collections.LowLevel.Unsafe;

namespace ThousandAnt.FrustumCulling.Collections {

    public unsafe struct UnsafeReadonlyArray<T> where T : unmanaged {

        public readonly int Length;

        [NativeDisableUnsafePtrRestriction]
        internal T* Ptr;

        public UnsafeReadonlyArray(T* ptr, int length) {
            Ptr = ptr;
            Length = length;
        }

        public T this[int i] {
            get { return *(Ptr + i); }
        }
    }
}
