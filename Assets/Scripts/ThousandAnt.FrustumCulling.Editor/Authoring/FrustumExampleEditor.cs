using ThousandAnt.FrustumCulling.Example;
using UnityEditor;
using UnityEngine;

namespace ThousandAnt.FrustrumCulling.EditorTools {

    [CustomEditor(typeof(FrustumExample))]
    public class FrustumExampleEditor : Editor {

        SerializedProperty positionProperty;

        void OnEnable() {
            positionProperty = serializedObject.FindProperty("Position");
        }

        void OnSceneGUI() {
            serializedObject.Update();

            positionProperty.vector3Value = Handles.PositionHandle(
                positionProperty.vector3Value, 
                Quaternion.identity);

            Handles.FreeMoveHandle(
                positionProperty.vector3Value, 
                Quaternion.identity, 
                0.15f, 
                Vector3.one * 0.1f, 
                Handles.DotHandleCap);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
