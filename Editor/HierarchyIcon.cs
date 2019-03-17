using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using DaiMangou.Storyteller;
using DaiMangou.BridgedData;

namespace DaiMangou.GameBridgeEditor
{
    [InitializeOnLoad]
    internal class HierarchyIcon
    {
  
        static List<int> markedObjects;

        static HierarchyIcon()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
        }

        static void HierarchyItemCB(int instanceID, Rect rect)
        {

            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (go != null && (go.GetComponent<Dialoguer>()))
                GUI.DrawTexture(rect.ToCenterLeft(16, 16,-30), ImageLibrary.chatIcon);



        }
    }
}
