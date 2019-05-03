using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaiMangou.BridgedData;
public class PlayerTrigger : MonoBehaviour
{


    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "NPC")
        {
            var npcTrigger = other.GetComponent<NPCTrigger>();

            if (!npcTrigger.added)
            {
                var characterComponent = gameObject.GetComponent<Character>();
                characterComponent.CommunicatingCharacters.Add(other.GetComponent<NPCTrigger>().character.self);
                characterComponent.CommunicatingCharacterGameobject.Add(other.GetComponent<NPCTrigger>().character.gameObject);
                npcTrigger.added = true;
                characterComponent.GenerateActiveDialogueSet();
            }
            //   Debug.Log(other.GetComponent<NPCTrigger>().character.CharacterName);
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "NPC")
        {
            var npcTrigger = other.GetComponent<NPCTrigger>();
            if (npcTrigger.added)
            {
                var characterComponent = gameObject.GetComponent<Character>();
                characterComponent.CommunicatingCharacters.Remove(other.GetComponent<NPCTrigger>().character.self);
                characterComponent.CommunicatingCharacterGameobject.Remove(other.GetComponent<NPCTrigger>().character.gameObject);
                npcTrigger.added = false;
                characterComponent.CleanUp();
                //gameObject.GetComponent<Character>().dialogueData.ActiveCharacterDialogueSet = new List<NodeData>();
                //gameObject.GetComponent<Character>().GenerateActiveDialogueSet();
            }
        }
    }
}