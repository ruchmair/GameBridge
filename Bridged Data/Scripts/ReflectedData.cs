using System;
using System.Collections.Generic;
using UnityEngine;


namespace DaiMangou.BridgedData
{

    /// <summary>
    /// this is a piece of datawho has an ID vlue matching a node in the storyteller scene it is uses in parallel with the 
    /// </summary>
    [Serializable]
    public class ReflectedData : MonoBehaviour
    {
       // public int Id = 0;
        public string UID = "";
        public GameObject self;
        public GameObject DialoguerGameObject;
        public Dialoguer dialoguer;
        public Dialoguer dialoguerComponent
        {
            get
            {
                if (dialoguer == null)
                {
                    dialoguer = DialoguerGameObject.GetComponent<Dialoguer>();
                }

                return dialoguer;
            }
            set
            {
                dialoguer = value;
            }
        }

        public List<Condition> Conditions = new List<Condition>();


        private void Awake()
        {
          /*  if(Conditions.Count == 0)
            {
                var newCondition = new GameObject(gameObject.name + "Condition " + Conditions.Count);
                newCondition.AddComponent<Condition>();
                var _condition = newCondition.GetComponent<Condition>();
                _condition.DialoguerGameObject = DialoguerGameObject;
                _condition.self = newCondition;
                newCondition.transform.SetParent(transform);
                // newCondition.hideFlags = HideFlags.HideInHierarchy;
                Conditions.Add(newCondition.GetComponent<Condition>());

            }*/
            //   self = gameObject;
        }

    }
}
