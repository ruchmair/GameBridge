using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DaiMangou.BridgedData
{
    public class ClickListener : MonoBehaviour
    {

        public int indexInList;
        public Dialoguer dialoguer;

        public void SwitchRoute()
        {
            var route = (RouteNodeData)dialoguer.dialogueData.ActiveCharacterDialogueSet[dialoguer.ActiveIndex];
            route.RouteID = indexInList;

        

            dialoguer.cashedRoute = route;



        }
    }

}