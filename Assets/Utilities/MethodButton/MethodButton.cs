using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public struct MethodButton {
    public string displayName;
	public string methodName;
	public MethodButton(string methodName, string displayName = "Button") {
		this.displayName = displayName;
		this.methodName = methodName;
	}
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(MethodButton))]
public class MethodButtonDrawer : PropertyDrawer {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		var displayName = property.FindPropertyRelative("displayName").stringValue;
		var methodName = property.FindPropertyRelative("methodName").stringValue;
		if (GUI.Button(position, displayName)) {
            InvokeMethod(property, methodName);
        }
    }
	void InvokeMethod(SerializedProperty property, string name) {
        Object target = property.serializedObject.targetObject;
        target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)?.Invoke(target, null);  
    }
}
#endif
