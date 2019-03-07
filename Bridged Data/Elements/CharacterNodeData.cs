using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DaiMangou.BridgedData
{
    [Serializable]
    public class CharacterNodeData:NodeData
    {
        public List<NodeData> NodeDataInMyChain = new List<NodeData>();
        public Texture2D CharacterImage;

        public override void OnEnable()
        {
            type = GetType();
            base.OnEnable();
        }
    }
}
