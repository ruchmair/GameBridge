using System;
using System.Collections.Generic;
using UnityEngine;


namespace DaiMangou.BridgedData
{

    /// <summary>
    /// this is a piece of data that has an ID vlue matching a node in the storyteller scene it is uses in parallel with the 
    /// </summary>
    [Serializable]
    public class ReflectedData : MonoBehaviour
    {
        /// <summary>
        /// this is the ID value used as a reference point for NodeData (Do not remove or edit)
        /// </summary>
        public string UID = "";
        /// <summary>
        /// This is the gameobject which this script is attached to
        /// </summary>
        public GameObject self;
        /// <summary>
        /// If the refelected data is generated under a gameboject with a Dialoguer Component then the gameobjects is set here
        /// </summary>
        public GameObject DialoguerGameObject;
        /// <summary>
        /// value is set by get set 
        /// </summary>
        public Dialoguer dialoguer;
        /// <summary>
        ///  If the refelected data is generated under a gameboject with a Dialoguer then the dialogue scriptreference is set here
        /// </summary>
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

        #region Character Specific 
        /// <summary>
        /// If the refelected data is generated under a gameboject with a Character Component then the gameobjects is set here
        /// </summary>
        public GameObject CharacterGameObject;
        /// <summary>
        /// value is set by get set
        /// </summary>
        public Character character;
        /// <summary>
        /// If the refelected data is generated under a gameboject with a Character Component then the dialogue scriptreference is set here
        /// </summary>
        public Character characterComponent
        {
            get
            {
                if (character == null)
                {
                    character = CharacterGameObject.GetComponent<Character>();
                }

                return character;
            }
            set
            {
                character = value;
            }
        }
        #endregion

        /// <summary>
        /// we keep a list of conditions under which certain function can be triggered
        /// </summary>
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
