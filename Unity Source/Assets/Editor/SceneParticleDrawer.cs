using UnityEditor;
using UnityEngine;

namespace BounceBack
{
    [CustomPropertyDrawer(typeof(SceneParticleAttribute))]
    internal sealed class SceneParticleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var current = (property.objectReferenceValue as ParticleSystem)?.gameObject;
            bool allowSceneObjects = !EditorUtility.IsPersistent(property.serializedObject.targetObject);
            EditorGUI.BeginProperty(rect, label, property);
            EditorGUI.BeginChangeCheck();
            var selected = (GameObject)EditorGUI.ObjectField(rect, label, current, typeof(GameObject), allowSceneObjects);
            if (EditorGUI.EndChangeCheck())
            {
                var system = selected != null ? selected.GetComponentInChildren<ParticleSystem>(true) : null;
                if (selected == null || system != null)
                    property.objectReferenceValue = system;
                else
                    Debug.LogWarning("Choose a GameObject with a Particle System on it or one of its children.", selected);
            }
            EditorGUI.EndProperty();
        }
    }
}
