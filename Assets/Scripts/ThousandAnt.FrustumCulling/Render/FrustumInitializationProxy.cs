using UnityEngine;

namespace ThousandAnt.FrustumCulling.Render {
    internal class FrustumInitializationProxy : MonoBehaviour {

        void OnEnable() {
            FrustumUtils.Initialize();
        }

        void Update() {
            FrustumUtils.SetFrustumPlanes(Camera.main);
        }

        void OnDisable() {
            FrustumUtils.Release();
        }
    }
}
