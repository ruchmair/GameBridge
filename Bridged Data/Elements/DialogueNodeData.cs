using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;


namespace DaiMangou.BridgedData
{
    /// <summary>
    /// representation of Dialogue Node data
    /// </summary>
    [Serializable]
    public class DialogueNodeData : NodeData
    {
        /// <summary>
        /// This is the voiceover clip copied from the Dialogue Node
        /// </summary>
        public AudioClip VoicedDialogue = null;
        /// <summary>
        /// Soun effect copied from the dialogue node
        /// </summary>
        public AudioClip SoundEffect = null;
        /// <summary>
        /// Storyboard image copied from the Dialogue Node
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