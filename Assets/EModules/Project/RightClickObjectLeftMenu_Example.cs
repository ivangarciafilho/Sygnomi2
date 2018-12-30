//////// Custom Tree-IGenericMenu Example ////////
/*
    To create a hotkey you can use the following special characters: % (ctrl on Windows, cmd on macOS), # (shift).
    To create a menu with hotkey g and no key modifiers pressed use "MySubItem/MyMenuItem _g".
    Hot keys work only in the Hierarchy Window and do not overlap hot keys in other windows.
*/

#if UNITY_EDITOR

using System.Linq;
using System.Collections.Generic;
using EModules.EModulesInternal;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


/////////////////////////////////////////////////////MENU ITEM TEMPLATE///////////////////////////////////////////////////////////////////////////////
/*
 
    class MyMenu : ProjectExtensions.IGenericMenu
    {
        public string Name { get { return "MySubItem/MyMenuItem %k"; } }
        public int PositionInMenu { get { return 0; } }

        public bool IsEnable(string clickedObjectPath, string clickedObjectGUID, int instanceId, bool isFolder, bool isMainAsset) { return true; }
        public bool NeedExcludeFromMenu(string clickedObjectPath, string clickedObjectGUID, int instanceId, bool isFolder, bool isMainAsset) { return false; } 

        public void OnClick(string[] affectedObjectsPathArray, string[] affectedObjectsGUIDArray, int[] affectedObjectsInstanceId, bool[] affectedObjectIsFolder, bool[] isMainAsset)
        {
            throw new System.NotImplementedException();
        }
    }

*/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


namespace Project_Examples
{



    #region ITEM 1000-1001 - ExpandSelecdedObject/CollapseSelecdedObject

    class MyMenu_ExpandSelecdedObject : ProjectExtensions.IGenericMenu
    {
        public bool IsEnable(string clickedObjectPath, string clickedObjectGUID, int instanceId, bool isFolder, bool isMainAsset) { return true; }
        public bool NeedExcludeFromMenu(string clickedObjectPath, string clickedObjectGUID, int instanceId, bool isFolder, bool isMainAsset) { return false; }

        public int PositionInMenu { get { return 1000; } }
        public string Name { get { return "Expand Selection"; } }


        public void OnClick(string[] affectedObjectsPathArray, string[] affectedObjectsGUIDArray, int[] affectedObjectsInstanceId, bool[] affectedObjectIsFolder, bool[] isMainAsset)
        {
            for (int i = 0 ; i < affectedObjectsInstanceId.Length ; i++)
            {
                if (!affectedObjectIsFolder[i]) continue;
                ProjectExtensions.Utility.SetExpandedRecursiveInProjectWindow( affectedObjectsInstanceId[i], true );
            }
        }
    }


    class MyMenu_CollapseSelecdedObject : ProjectExtensions.IGenericMenu
    {
        public bool IsEnable(string clickedObjectPath, string clickedObjectGUID, int instanceId, bool isFolder, bool isMainAsset) { return true; }
        public bool NeedExcludeFromMenu(string clickedObjectPath, string clickedObjectGUID, int instanceId, bool isFolder, bool isMainAsset) { return false; }

        public int PositionInMenu { get { return 1001; } }
        public string Name { get { return "Collapse Selection"; } }


        public void OnClick(string[] affectedObjectsPathArray, string[] affectedObjectsGUIDArray, int[] affectedObjectsInstanceId, bool[] affectedObjectIsFolder, bool[] isMainAsset)
        {
            for (int i = 0 ; i < affectedObjectsInstanceId.Length ; i++)
            {
                if (!affectedObjectIsFolder[i]) continue;
                ProjectExtensions.Utility.SetExpandedRecursiveInProjectWindow( affectedObjectsInstanceId[i], false );
            }
        }

    }

    #endregion




}//namespace

#endif