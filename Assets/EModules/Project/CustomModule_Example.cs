//////// Custom Tree-IGenericMenu Example ////////
/*
        To add your own module, inherit the slot class (ProjectExtensions.CustomModule_Slot1 / 2 / 3) anywhere in your code.
*/

#if UNITY_EDITOR

using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


/////////////////////////////////////////////////////MENU ITEM TEMPLATE///////////////////////////////////////////////////////////////////////////////
/*
    class MyModule : ProjectExtensions.CustomModule_Slot1
    {
        public override string NameOfModule { get { return "MyModule"; } }
    
        // In this method, you can display information and buttons
        public override void Draw(Rect drawRect, string assetPath, string assetGuid, int instanceId, bool isFolder, bool isMainAsset)
        {
            // You can invoke different built-in methods for changing variables
            //        if (GUI.Button(drawRect,"string")) SHOW_StringInput(...
            //        if (GUI.Button(drawRect,"int")) SHOW_IntInput(...
            //        if (GUI.Button(drawRect,"dropdown")) SHOW_DropDownMenu(...
        }
    
        // ToString(...) method is used for the search box
        public override string ToString(string assetPath, string assetGuid, int instanceId, bool isFolder, bool isMainAsset)
        {
            return null;
        }
    }
*/
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


namespace Project_Examples
{



  #region MODULE 1 - Material Shader
  class CustomModule_Example_MaterialShader : ProjectExtensions.CustomModule_Slot1
  {
    private Type materialType = typeof(Material);

    public override string NameOfModule { get { return "Material Shader"; } }

    public override string ToString(string assetPath, string assetGuid, int instanceId, bool isFolder, bool isMainAsset)
    {
      if (isFolder || InternalEditorUtility.GetTypeWithoutLoadingObject( instanceId ) != materialType) return ""; //to avoid slow downing search
      var mat =  EditorUtility.InstanceIDToObject(instanceId) as Material;
      if (!mat) return "";
      return mat.shader ? mat.shader.name : "";
    }//ToString


    public override void Draw(Rect drawRect, string assetPath, string assetGuid, int instanceId, bool isFolder, bool isMainAsset)
    {
      if (!isMainAsset || isFolder) return;

      var mat =  EditorUtility.InstanceIDToObject(instanceId) as Material;
      if (!mat || !mat.shader) return;

      if (GUI.Button( drawRect, mat.shader.name ))
      {

        var affectedArray = ProjectExtensions.Utility.GetAffectsGameObjects( instanceId ).Select(s=>EditorUtility.InstanceIDToObject(s) as Material).Where(m=>m).ToArray();

        var callback = ScriptableObject.CreateInstance<FakeCallbak>();
        callback.affectedArray = affectedArray;
        var  mc = new MenuCommand( callback, 0 );

#pragma warning disable
        UnityEditorInternal.InternalEditorUtility.SetupShaderMenu( mat );
#pragma warning restore

        EditorUtility.DisplayPopupMenu( drawRect, "CONTEXT/ShaderPopup", mc );
      }
    }//Draw


    class FakeCallbak : ScriptableObject
    {
      internal Material[] affectedArray = null;
      private void OnSelectedShaderPopup(string command, Shader shader)
      {
        foreach (var o in affectedArray)
        {
          Undo.RecordObject( o, "Change shader" );
          o.shader = shader;
          EditorUtility.SetDirty( o );
        }
      }
    }//FakeCallbak

  }
#endregion // MODULE 1 - Material Shader



}

#endif