using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace DaiMangou.BridgedData
{
    [Serializable]
    public class SceneData : ScriptableObject
    {
        public int SceneID = -1;
       // [HideInInspector]
        public List<NodeData> FullCharacterDialogueSet = new List<NodeData>();
       // [HideInInspector] 
        public List<NodeData> ActiveCharacterDialogueSet = new List<NodeData>();// move to dialoguer
                                                                                //  private Texture2D image;
        public List<CharacterNodeData> Characters = new List<CharacterNodeData>();

        public void OnEnable()
        {
#if UNITY_EDITOR
            DaiMangou.Storyteller.IconManager.SetIcon(this, DaiMangou.Storyteller.IconManager.DaiMangouIcons.SceneIcon);

#endif
        }
    }
}
