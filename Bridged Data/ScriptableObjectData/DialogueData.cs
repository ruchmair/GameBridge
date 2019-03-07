using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace DaiMangou.BridgedData
{
    [Serializable]
    public class DialogueData : ScriptableObject
    {
  
       // [HideInInspector]
        public List<NodeData> FullCharacterDialogueSet = new List<NodeData>();
       // [HideInInspector]
        public List<NodeData> ActiveCharacterDialogueSet = new List<NodeData>();
      //  private Texture2D image;

        public void OnEnable()
        {
#if UNITY_EDITOR
            DaiMangou.Storyteller.IconManager.SetIcon(this, DaiMangou.Storyteller.IconManager.DaiMangouIcons.DialogueDataIcon);

#endif
        }
    }
}
