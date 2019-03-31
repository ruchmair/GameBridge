using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DaiMangou.BridgedData
{
    //[Serializable]
    //public class EvTrigger : UnityEvent<UnityEngine.Object> { }

    /// <summary>
    ///  this is the base class data for the data od the nodes in your story.
    /// </summary>
    [Serializable]
    public class NodeData : ScriptableObject//, ISerializationCallbackReceiver
    {

        /*  public void OnBeforeSerialize()
  {
  }
  public void OnAfterDeserialize()
  {
  }*/

        /// <summary>
        ///  the name of the node data
        /// </summary>
        public string Name = "";
        /// <summary>
        /// 
        /// </summary>
        public string Text = "";
        /// <summary>
        /// a specific ID number that matches the ID number of the node it represents in the storyteller
        /// </summary>
        [HideInInspector]
        //public int DataID;
        public string UID = "";
        /// <summary>
        /// the  character who this node data belongs to
        /// </summary>
        //[HideInInspector]
        public string CharacterName = "";
        [UnityEngine.Serialization.FormerlySerializedAs("MyCharacter")]
        public CharacterNodeData CallingNodeData;
        /// <summary>
        /// the environment in which this node data belongs
        /// </summary>
        [HideInInspector]
        public string EnvironmentName = "";
        //   [HideInInspector]
        public bool Pass;
        // [HideInInspector]
        public List<NodeData> DataConnectedToMe = new List<NodeData>();
        // [HideInInspector]
        public List<NodeData> DataIconnectedTo = new List<NodeData>();
        [HideInInspector]
        public Type type;
        [HideInInspector]
        public float Duration;
        [HideInInspector]
        public float Delay;
        [HideInInspector]
        public float RealtimeDelay;
        // [HideInInspector]
        public float StartTime;
        [HideInInspector]
        public float DurationSum;
        [HideInInspector]
        public bool useTime;

        public bool IsPlayer;

        #region EditorSpecific variables

        [HideInInspector]
        public bool ExpandFoldout;
        #endregion

        public virtual void OnEnable()
        {
            /* if (Conditions.Count == 0)
             {
                 var newCondition = CreateInstance(typeof(Condition)) as Condition;
                 newCondition.hideFlags = HideFlags.DontSave;
                 newCondition.name = name;
                 Conditions.Add(newCondition);
                 //     ExecutingEvent.Add(new UnityEvent());
                 //    Conditions.Last().targetEvent = ExecutingEvent.Last();
             }*/
        }


        public void Aggregate()
        {
            for (var i = 0; i < DataIconnectedTo.Count; i++)
            {
                var DataConnectedTo = DataIconnectedTo[i];
                if (type == typeof(RouteNodeData))
                {

                    var route = (RouteNodeData)this;
                    if (Pass)
                    {
                        DataConnectedTo.Pass = true;
                    }
                    else
                    {
                        DataConnectedTo.Pass = i != route.RouteID;
                    }
                }
                else
                {
                    DataConnectedTo.Pass = Pass;
                }
                DataConnectedTo.Aggregate();


            }


        }

        public virtual void ProcessData()
        {
            // no longer necessary
            /*foreach (var data in DataIconnectedTo)
            {
                if (!data.Pass)
                {
                    if (data.type == typeof(RouteNodeData) || data.type == typeof(LinkNodeData) || data.type == typeof(EndNodeData))
                    {
                        data.ProcessData();
                    }
                }
            }*/
        }



        public void AddMeToThisList(List<NodeData> nodeDataList)
        {



            nodeDataList.Add(this);
            foreach (var data in DataIconnectedTo)
            {
                if (data.Pass) continue;
                if (data.type != typeof(DialogueNodeData) && data.type != typeof(ActionNodeData))
                {
                    data.AddMeToThisList(nodeDataList);

                }
            }

        }

    }
}
