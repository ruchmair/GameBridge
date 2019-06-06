using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DaiMangou.BridgedData
{
    /// <summary>
    /// A representation of Action node data 
    /// </summary>
    [Serializable]
    public class ActionNodeData : NodeData
    {

        /// <summary>
        /// 
        /// </summary>
        public string ActionName = ""; // not yet uese
        /// <summary>
        /// The Sound effect copied for mthe Action Node
        /// </summary>
        public AudioClip SoundEffect = null;
        /// <summary>
        /// Storyboard image copied from the Action Ndoe
        /// </summary>
        public Sprite StoryboardImage = null;
        /// <summary>
        /// your string tag 
        /// </summary>
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
