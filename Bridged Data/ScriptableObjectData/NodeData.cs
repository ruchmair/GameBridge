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
    ///  this is the base class data for the data of the nodes in your story.
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
      //  [HideInInspector]
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
        /// <summary>
        /// the data which is connected to this node data
        /// </summary>
        public List<NodeData> DataConnectedToMe = new List<NodeData>();
        // [HideInInspector]
        /// <summary>
        /// the node data which this data is connected to
        /// </summary>
        public List<NodeData> DataIconnectedTo = new List<NodeData>();
        /// <summary>
        /// The type of node data
        /// </summary>
        [HideInInspector]
        public Type type;
        /// <summary>
        /// the duration of the node "playback"
        /// </summary>
        [HideInInspector]
        public float Duration;
        /// <summary>
        /// This is the delay from the playback of the last nodeData behind this one
        /// </summary>
        [HideInInspector]
        public float Delay;
        /// <summary>
        /// The actual delay time calculated dynamically (not yet setup)
        /// </summary>
        [HideInInspector]
        public float RealtimeDelay;
        /// <summary>
        /// The real time in which the 
        /// </summary>
        // [HideInInspector]
        public float StartTime;
        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public float DurationSum;
        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public bool useTime;
        /// <summary>
        /// a flag to check if the Data is a descendant of a character that is designated as the player
        /// </summary>
        public bool IsPlayer;
        /// <summary>
        /// flas to check if node data that typically does not use timing is forced to use a start time , e.g RouteNodes
        /// </summary>
        public bool OverrideStartTime;



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

        /// <summary>
        ///  here we generate a new chain of events based on choices made in the story
        /// </summary>
        public void Aggregate()
        {
            // look at each node data that we have connected to and tell it to aggrigate 
            for (var i = 0; i < DataIconnectedTo.Count; i++)
            {
                var dataIConnectedTo = DataIconnectedTo[i];
                // if this is a route , then 
                if (type == typeof(RouteNodeData))
                {

                    var route = (RouteNodeData)this;
                    if (Pass)
                    {
                        //we assign its pass value of what we are connected to , to true of pass is true
                        dataIConnectedTo.Pass = true;
                    }
                    else
                    {
                        // this means that the data we conneced to at i weill have a pass value equal to false if the route id does not match
                        dataIConnectedTo.Pass = i != route.RouteID;
                    }
                }
                else
                {
                    // set the pass value of what we are connected to , to be our pass value
                    dataIConnectedTo.Pass = Pass;
                }
                dataIConnectedTo.Aggregate();


            }


        }

      /// <summary>
      /// Overridden in inheriting classes and usd to process specidfic datasets
      /// </summary>
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


        // No longer Used
       /* public void AddMeToThisList(List<NodeData> nodeDataList)
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

        }*/

    }
}
