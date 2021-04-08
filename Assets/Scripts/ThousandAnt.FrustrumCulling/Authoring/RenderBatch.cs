using UnityEngine;

namespace ThousandAnt.Authoring {
    /// <summary>
    /// Transform Handle stores only the position and euler rotation. 
    /// Scale is assumed to be a uniform value.
    /// </summary>
    [CreateAssetMenu(menuName = "Thousand Ant/RenderBatch", fileName = "RenderBatch")]
    public class RenderBatch : ScriptableObject {

        public Material Material;
        public Mesh Mesh;

        public TransformHandle[] Transforms;
    }
}