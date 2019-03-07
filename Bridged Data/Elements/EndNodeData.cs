using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DaiMangou.BridgedData

{
    [Serializable]
    public class EndNodeData : NodeData
    {



        public override void OnEnable()
        {
            type = GetType();
            base.OnEnable();
        }

        public override void ProcessData()
        {
            base.ProcessData();


        }
    }
}
