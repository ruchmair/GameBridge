using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DaiMangou.BridgedData
{
    public class ClickListener : MonoBehaviour
    {

        public int indexInList;
        [UnityEngine.Serialization.FormerlySerializedAs("dialoguer")]
        public Dialoguer dialoguerComponent;
        public Character characterComponent;

        public void SwitchRoute()
        {
            if (dialoguerComponent)
            {
                var route = (RouteNodeData)dialoguerComponent.sceneData.ActiveCharacterDialogueSet[dialoguerComponent.ActiveIndex];
                route.RouteID = indexInList;
                dialoguerComponent.CachedRoute = route;
            }

            if (characterComponent)
            {
                var route = (RouteNodeData)characterComponent.sceneData.ActiveCharacterDialogueSet[characterComponent.ActiveIndex];
                route.RouteID = indexInList;
                characterComponent.CachedRoute = route;
            }


        }
    }

}