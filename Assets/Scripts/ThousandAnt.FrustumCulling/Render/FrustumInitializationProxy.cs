using UnityEngine;

namespace ThousandAnt.FrustumCulling.Render {
    internal class FrustumInitializationProxy : MonoBehaviour {

        void OnEnable() {
            FrustumUtility.Initialize();
        }

        void Update() {
            FrustumUtility.SetFrustumPlanes(Camera.main);
        }

        void OnDisable() {
            FrustumUtility.Release();
        }
    }
}
