
using LoadAA;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(Image))]
[CanEditMultipleObjects]
public class ImageInspector : Editor
{
    Image _image;
    SerializedProperty _spriteRef;
    SerializedProperty _imageType;
    SerializedProperty _useSpriteMesh;
    SerializedProperty _preserveAspect;
    SerializedProperty _fillCenter;
    SerializedProperty _pixelsPerUnitMultiplier;
    SerializedProperty _fillMethod;
    SerializedProperty _fillOrigin;
    SerializedProperty _fillAmount;
    SerializedProperty _fillClockwise;
    
    protected void OnEnable()
    {
        _image = (Image)target;
        
        _spriteRef = serializedObject.FindProperty("spriteRef");
        _useSpriteMesh = serializedObject.FindProperty("useSpriteMesh");
        _preserveAspect = serializedObject.FindProperty("preserveAspect");
        _fillCenter = serializedObject.FindProperty("fillCenter");
        _pixelsPerUnitMultiplier = serializedObject.FindProperty("pixelsPerUnitMultiplier");
        _fillMethod = serializedObject.FindProperty("fillMethod");
        _fillOrigin = serializedObject.FindProperty("fillOrigin");
        _fillAmount = serializedObject.FindProperty("fillAmount");
        _fillClockwise = serializedObject.FindProperty("fillClockwise");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.PropertyField(_spriteRef);
        
        _image.imageType = (UnityEngine.UI.Image.Type)EditorGUILayout.EnumPopup("图像类型", _image.imageType);
 
        if (_image.imageType == UnityEngine.UI.Image.Type.Simple)
        {
            EditorGUILayout.PropertyField(_useSpriteMesh);
            EditorGUILayout.PropertyField(_preserveAspect);
        }
        if (_image.imageType == UnityEngine.UI.Image.Type.Sliced || _image.imageType == UnityEngine.UI.Image.Type.Tiled)
        {
            EditorGUILayout.PropertyField(_fillCenter);
            EditorGUILayout.PropertyField(_pixelsPerUnitMultiplier);
        }
        if (_image.imageType == UnityEngine.UI.Image.Type.Filled)
        {
            EditorGUILayout.PropertyField(_fillMethod);
            EditorGUILayout.PropertyField(_fillOrigin);
            EditorGUILayout.PropertyField(_fillAmount);
            EditorGUILayout.PropertyField(_fillClockwise);
            EditorGUILayout.PropertyField(_preserveAspect);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}
