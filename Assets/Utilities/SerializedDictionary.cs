using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
public class SerializableDictionary {}
[Serializable]
public class SerializableDictionary<TKey, TValue> : SerializableDictionary, ISerializationCallbackReceiver, IDictionary<TKey, TValue> {
    [SerializeField]
    private List<SerializableKeyValuePair> list = new List<SerializableKeyValuePair>();
    [Serializable]
    public struct SerializableKeyValuePair {
        public TKey Key;
        public TValue Value;
        public SerializableKeyValuePair (TKey key, TValue value) {
            Key   = key;
            Value = value;
        }
        public void SetValue (TValue value) {
            Value = value;
        }
    }
    Dictionary<TKey, uint> KeyPositions => _keyPositions.Value;
    Lazy<Dictionary<TKey, uint>> _keyPositions;
    public SerializableDictionary() => _keyPositions = new Lazy<Dictionary<TKey, uint>>(MakeKeyPositions);
    Dictionary<TKey, uint> MakeKeyPositions () {
        var numEntries = list.Count;
        var result = new Dictionary<TKey, uint>( numEntries );
        for (int i = 0; i < numEntries; i++)
            result[list[i].Key] = (uint) i;
        return result;
    }
    public void OnBeforeSerialize() {}
    public void OnAfterDeserialize() =>  _keyPositions = new Lazy<Dictionary<TKey, uint>>(MakeKeyPositions);
 
#region IDictionary<TKey, TValue>
    public TValue this[TKey key] {
        get => list[(int) KeyPositions[key]].Value;
        set {
            if (KeyPositions.TryGetValue(key, out uint index))
                list[(int) index].SetValue(value);
            else {
                KeyPositions[key] = (uint) list.Count;
                list.Add( new SerializableKeyValuePair( key, value ) );
            }
        }
    }
    public ICollection<TKey>   Keys   => list.Select( tuple => tuple.Key ).ToArray();
    public ICollection<TValue> Values => list.Select( tuple => tuple.Value ).ToArray();
    public void Add(TKey key, TValue value) {
        if (KeyPositions.ContainsKey(key))
            throw new ArgumentException("An element with the same key already exists in the dictionary.");
        else {
            KeyPositions[key] = (uint) list.Count;
            list.Add( new SerializableKeyValuePair( key, value ) );
        }
    }
    public bool ContainsKey(TKey key) => KeyPositions.ContainsKey(key);
    public bool Remove (TKey key) {
        if (KeyPositions.TryGetValue(key, out uint index)) {
            var kp = KeyPositions;
            kp.Remove(key);
            var numEntries = list.Count;
            list.RemoveAt( (int) index );
            return true;
        }
        else return false;
    }
    public bool TryGetValue (TKey key, out TValue value) {
        if (KeyPositions.TryGetValue(key, out uint index)) {
            value = list[(int) index].Value;
            return true;
        }
        else {
            value = default;
            return false;
        }
    }
#endregion
#region ICollection <KeyValuePair<TKey, TValue>>
    public int  Count      => list.Count;
    public bool IsReadOnly => false;
    public void Add (KeyValuePair<TKey, TValue> kvp) => Add( kvp.Key, kvp.Value );
    public void Clear    ()                               => list.Clear();
    public bool Contains (KeyValuePair<TKey, TValue> kvp) => KeyPositions.ContainsKey(kvp.Key);
    public void CopyTo (KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
        var numKeys = list.Count;
        if (array.Length - arrayIndex < numKeys)
            throw new ArgumentException("arrayIndex");
        for (int i = 0; i < numKeys; i++, arrayIndex++) {
            var entry = list[i];
            array[arrayIndex] = new KeyValuePair<TKey, TValue>( entry.Key, entry.Value );
        }
    }
    public bool Remove(KeyValuePair<TKey, TValue> kvp) => Remove(kvp.Key);
#endregion
#region IEnumerable <KeyValuePair<TKey, TValue>>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator () {
        return list.Select(ToKeyValuePair).GetEnumerator();
 
        KeyValuePair<TKey, TValue> ToKeyValuePair (SerializableKeyValuePair skvp) {
            return new KeyValuePair<TKey, TValue>( skvp.Key, skvp.Value );
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
#endregion
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SerializableDictionary), true)]
public class SerializableDictionaryPropertyDrawer : PropertyDrawer {
    private ReorderableList list;
    private Func<Rect> VisibleRect;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if (list == null) {
            var listProp = property.FindPropertyRelative("list");
            list = new ReorderableList(property.serializedObject, listProp, true, false, true, true);
            list.drawElementCallback = DrawListItems;
        }
        var firstLine = position;
        firstLine.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(firstLine, property, label);
        if (property.isExpanded) {
            position.y += firstLine.height;
            if (VisibleRect == null) {
                var tyGUIClip = System.Type.GetType("UnityEngine.GUIClip,UnityEngine");
                if (tyGUIClip != null) {
                    var piVisibleRect = tyGUIClip.GetProperty("visibleRect", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (piVisibleRect != null) {
                        var getMethod = piVisibleRect.GetGetMethod(true) ?? piVisibleRect.GetGetMethod(false);
                        VisibleRect = (Func<Rect>)Delegate.CreateDelegate(typeof(Func<Rect>), getMethod);
                    }
                }
            }
            var vRect = VisibleRect();
            vRect.y -= position.y;
            if (elementIndex == null) elementIndex = new GUIContent();
            list.DoList(position, vRect);
        }
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (property.isExpanded) {
            var listProp = property.FindPropertyRelative("list");
            if (listProp.arraySize < 2)
                return EditorGUIUtility.singleLineHeight + 52f;
            else
                return EditorGUIUtility.singleLineHeight + 23f * listProp.arraySize + 29;
        }
        else return EditorGUIUtility.singleLineHeight;
    }
    static GUIContent[] PairElementLabels => s_pairElementLabels ??= new[] {new GUIContent("Key"), new GUIContent ("=>")};
    static GUIContent[] s_pairElementLabels;
    static GUIContent elementIndex;
    void DrawListItems(Rect rect, int index, bool isActive, bool isFocused) {
        SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
        var keyProp   = element.FindPropertyRelative("Key");
        var valueProp = element.FindPropertyRelative("Value");
        elementIndex.text = $"Element {index}";
        EditorGUI.BeginProperty(rect, elementIndex, element);
            var prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 75;
            var rect0 = rect;
            var halfWidth = rect0.width / 2f;
            rect0.width = halfWidth;
            rect0.y += 1f;
            rect0.height -= 2f;
            EditorGUIUtility.labelWidth = 40;
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rect0, keyProp);
            rect0.x += halfWidth + 4f;
            EditorGUI.PropertyField(rect0, valueProp);
            EditorGUIUtility.labelWidth = prevLabelWidth;
        EditorGUI.EndProperty();
    }
}
#endif
