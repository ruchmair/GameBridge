using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DaiMangou.BridgedData

{
    [Serializable]
    public class ActionNodeData : NodeData
    {

        
        public string ActionName = "";
        public AudioClip SoundEffect = null;
        public Sprite StoryboardImage = null;
        public string Tag = "";


        public override void OnEnable()
        {
            type = GetType();
            useTime = true;
            base.OnEnable();
        
        }

        public override void ProcessData()
        {
            base.ProcessData();


        }


    }
}
