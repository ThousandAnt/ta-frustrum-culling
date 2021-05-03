using System.Collections.Generic;
using ThousandAnt.FrustumCulling.Transforms;
using ThousandAnt.FrustumCulling.Transforms.Authoring;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace ThousandAnt.FrustrumCulling.EditorTools {

    public class RenderBatchEditorWindow : EditorWindow {

        [MenuItem("Tools/Thousand Ant/Render Batch Window")]
        static void ShowWindow() {
            var window = EditorWindow.CreateWindow<RenderBatchEditorWindow>("Render Batch Window");
            window.minSize = new Vector2(400, 200);
            window.Show();
        }

        Transform parent;
        Mesh previewMesh;
        Material[] previewMats;
        List<TransformHandle> collection;

        void OnEnable() {
            collection = new List<TransformHandle>(100);
        }

        void OnGUI() {
            parent = (Transform)EditorGUILayout.ObjectField("Parent", parent, typeof(Transform), true);
            PreviewRenderData();
            CollectChildren();
        }

        void PreviewRenderData() {
            GUI.enabled = false;
            if (parent != null && parent.childCount > 0) {
                var filter = parent.GetComponentInChildren<MeshFilter>();
                var renderer = parent.GetComponentInChildren<MeshRenderer>();

                if (renderer != null && filter != null) {
                    if (renderer.sharedMaterials == null) {
                        previewMats = new Material[1] { renderer.sharedMaterial };
                    } else {
                        previewMats = new Material[renderer.sharedMaterials.Length];

                        for (int i = 0; i < previewMats.Length; i++) {
                            previewMats[i] = renderer.sharedMaterials[i];
                        }
                    }
                    previewMesh = filter.sharedMesh;
                }
            }

            EditorGUI.indentLevel += 2;
            previewMesh = (Mesh)EditorGUILayout.ObjectField("Preview Mesh", previewMesh, typeof(Mesh), true);

            // Debug.Log(previewMats == null);

            if (previewMats != null) {
                for (int i = 0; i < previewMats.Length; i++) {
                    previewMats[i] = (Material)EditorGUILayout.ObjectField($"Preview Mat {i + 1}", previewMats[i], typeof(Material), true);
                }
            }

            EditorGUI.indentLevel -= 2;
            GUI.enabled = true;
        }

        void CollectChildren() {
            if (parent == null) {
                return;
            }

            collection.Clear();

            // This will not get all cases, but will be enough for the demo.
            var meshRenderers = parent.GetComponentsInChildren<MeshRenderer>();
            var meshFilters = parent.GetComponentsInChildren<MeshFilter>();

            // Create the Scriptable Object which will store all positions
            Assert.AreEqual(meshRenderers.Length, meshFilters.Length, "Mismatched number of MeshFilters and MeshRenderers!");

            for (int i = 0; i < meshRenderers.Length; i++) {
                var transform = meshRenderers[i].transform;
                collection.Add(transform);
            }

            if (GUILayout.Button("Save Render Batch")) {
                var path = EditorUtility.SaveFilePanelInProject(
                    "Save Render Batch", "RenderBatch", "asset", "Enter a file name and click save");
                if (path.Length > 0) {
                    var renderBatch = ScriptableObject.CreateInstance<RenderBatch>();

                    if (meshRenderers[0].sharedMaterials != null) {
                        renderBatch.Materials = new Material[meshRenderers[0].sharedMaterials.Length];

                        for (int i = 0; i < meshRenderers[0].sharedMaterials.Length; i++) {
                            renderBatch.Materials[i] = meshRenderers[0].sharedMaterials[i];
                        }
                    } else {
                        renderBatch.Materials = new Material[] { meshRenderers[0].sharedMaterial };
                    }

                    renderBatch.Mesh = meshFilters[0].sharedMesh;
                    renderBatch.Transforms = collection.ToArray();
                    AssetDatabase.CreateAsset(renderBatch, path);
                }
            }
        }
    }
}
