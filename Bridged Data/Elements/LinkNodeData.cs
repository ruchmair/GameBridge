using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DaiMangou.BridgedData

{
    [Serializable]
    public class LinkNodeData: NodeData
    {

        public bool Loop;
        public int LoopValue;
        public RouteNodeData loopRoute;
        public int _iterationCount;

        public override void OnEnable()
        {
            type = GetType();
            base.OnEnable();
        }

        public override void ProcessData()
        {
            base.ProcessData();

         /*   if (Loop)
            {

                if(_iterationCount == LoopValue)
                {
                    loopRoute.RouteID = loopRoute.AutoSwitchValue;
                }
            }

   */
        }
    }
}