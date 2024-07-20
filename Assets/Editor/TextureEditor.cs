using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(TextureGenerator))]
public class TextureEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TextureGenerator textureGenerator = (TextureGenerator)target;

        if (GUILayout.Button("Generate Texture"))
        {
            textureGenerator.GenerateTexture();
        }
    }
}
