using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace ThousandAnt.FrustumCulling.Transforms {
    public static class TransformUtils {

        /// <summary>
        /// Utility function to convert a Tranform handle collection to a matrix 4x4.
        /// </summary>
        public static NativeArray<float4x4> ToMatrixArray(this TransformHandle[] handles, Allocator allocator) {
            var array = new NativeArray<float4x4>(handles.Length, allocator);

            for (int i = 0; i < handles.Length; i++) {
                var handle = handles[i];
                array[i] = float4x4.TRS(handle.Position, quaternion.Euler(handle.Rotation), handle.Scale);
            }

            return array;
        }
    }
}
