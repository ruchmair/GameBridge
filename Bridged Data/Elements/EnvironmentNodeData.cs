using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DaiMangou.BridgedData
{
    [Serializable]
    public class EnvironmentNodeData : NodeData
    {
        public string EnvironmentLocation = "";

        public override void OnEnable()
        {
            type = GetType();
            base.OnEnable();
        }
    }
}

