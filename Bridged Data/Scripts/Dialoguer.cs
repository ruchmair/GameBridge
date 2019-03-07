using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DaiMangou.BridgedData
{
    public enum TextDisplayMode
    {
        Instant =0,
        Typed=1,
        Custom =2

    }

    public class Dialoguer : MonoBehaviour
    {
        //  public UnityEvent m_MyEvent;

        public DialogueData dialogueData;
        // the value here is set to -1 to start , so that we can select a scene value which will then pass its scene id to this scene id value which will then be serialized
        public int sceneID = -1;

        //   private List<int> countlist = new List<int>();
        //   public Event ev = new Event();
        //  public EventTrigger evtr;

        /// <summary>
        /// 
        /// </summary>
        public delegate void DoRefresh();

        /// <summary>
        /// used to re-generate the ActiveCharacterDialogueSet
        /// </summary>
        public static DoRefresh doRefresh;

        /// <summary>
        ///  flag to check if any nodes are still processing data
        /// </summary>
        public static int ActiveEvents = -1;
        /// <summary>
        /// static int flag used to check how many charactrs are speaking at once, if the number inceases the aUI for dialogue will be added
        /// </summary>

        public static int ActiveDialogues = 0;

        // this is the value of the data in ActiveNodeData that is currently being processed
        public int ActiveIndex;
        private int cashedActiveIndex = -1;
        //   public List<UnityEvent> EventTrigger = new List<UnityEvent>();

        // a special set of data wach with an ID value matching that of each NodeData in the FullCharacterDataSet
        public List<CharacterNodeData> Characters = new List<CharacterNodeData>();
        public List<ReflectedData> reflectedDataSet = new List<ReflectedData>();

        private ReflectedData targetReflectedData;

        public RouteNodeData cashedRoute;



        int ival = 0;

        public Text Name;
        public Text text;
        public Button moveNextButton;
        public Button movePreviousButton;
        public List<Button> RouteButtons = new List<Button>();


        public TextDisplayMode textDisplayMode = TextDisplayMode.Instant;
        public float delay;


        private void Awake()
        {
            if (moveNextButton)
                moveNextButton.onClick.AddListener(() => MoveNext());

            if (movePreviousButton)
                movePreviousButton.onClick.AddListener(() => MovePrevious());


        }


        public void OnEnable()
        {

            doRefresh += StartRefresh;

        }
        public void OnDisable()
        {
            doRefresh -= StartRefresh;
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartRefresh()
        {
            StartCoroutine(Refresh());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator Start()
        {
            doRefresh();

            while (true)
            {

                yield return StartCoroutine(CustomUpdate());
                //  if()
                // {
                //      yield return  StartCoroutine();
                //  }


                yield return null;

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator Refresh()
        {
            yield return StartCoroutine(GenerateActiveDialogueSet());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator GenerateActiveDialogueSet()
        {


            //  countlist = new List<int>();
            #region here we will populate the  ActiveChracterDalogueSet at runtime, unlike the FullCharacterDialogueSet list, this list will have all nodes ordered correctly for execution at runtime
            // firstly make ActiveCharacterDialogueSet a new list
            dialogueData.ActiveCharacterDialogueSet = new List<NodeData>();
            for (int i = 0; i < dialogueData.FullCharacterDialogueSet.Count; i++)
            {
                var data = dialogueData.FullCharacterDialogueSet[i];

                // each n=NodeData in the FullCharacterDialogueSet already has their pas value set when the data was first pushed to the Dialoguers DialogueData scriptableObject so we just check the values
                if (!data.Pass)
                {
                    //   countlist.Add(i);
                    // we want to ignore all character and environment nodes
                    if (data.type != typeof(CharacterNodeData) && data.type != typeof(EnvironmentNodeData))
                    {
                        //since the ActionNodeData and the DialogeNodeData are odered in the FullCharacterDialogueSet by time , using the for loop gives us acces to each in order
                        if (data.type == typeof(ActionNodeData) || data.type == typeof(DialogueNodeData))
                        {
                            // this gives us the first nodedata added to the ActiveCharacterDialogueSet with its actionNodeData or DialogueNodeData added to it
                            dialogueData.ActiveCharacterDialogueSet.Add(data);
                            // RIGHT AFTER we do another for loop, this loop breaks the first loop and will continur it one this loop completes
                            // in this loop we are going to execute a function which will check what data is connected to  and will tell if to run the function AddMeToThisList();
                            // if the nodedata it is conned to is not set to pass. this function is executed in NodeData so it will ad itself t to the list, this node must also NOT be an 
                            // ActionNodeData or a DoialogueNodeData since those are already goig to be adde to the list in the above function^
                            foreach (var node in data.DataIconnectedTo)
                            {
                                if (node.type != typeof(ActionNodeData) && node.type != typeof(DialogueNodeData))
                                    if (!node.Pass)
                                    {
                                        node.AddMeToThisList(dialogueData.ActiveCharacterDialogueSet);

                                    }
                            }
                        }

                    }
                }
            }

            foreach (var character in Characters)
            {
                for (int i = character.NodeDataInMyChain.Count - 1; i >= 0; i--)
                {
                    if (!character.NodeDataInMyChain[i].Pass)
                    {
                        if (character.NodeDataInMyChain[i].type == typeof(RouteNodeData))
                        {
                            var route = (RouteNodeData)character.NodeDataInMyChain[i];

                            route.StartTime = route.DataIconnectedTo[route.RouteID].StartTime;
                        }
                        if(character.NodeDataInMyChain[i].type == typeof(LinkNodeData))
                        {
                            var link = (LinkNodeData)character.NodeDataInMyChain[i];
                            link.StartTime = link.DataIconnectedTo[0].StartTime;

                        }
                        if(character.NodeDataInMyChain[i].type == typeof(EndNodeData))
                        {
                            var end = (EndNodeData)character.NodeDataInMyChain[i];

                            List<NodeData> templist = new List<NodeData>();
                            foreach(var n in character.NodeDataInMyChain)
                            {
                                if(!n.Pass)
                                {
                                    templist.Add(n);
                                }
                            }
                            end.StartTime = templist.Max(s => s.StartTime);
                        }
                    }
                }
            }

            
            dialogueData.ActiveCharacterDialogueSet = dialogueData.ActiveCharacterDialogueSet.OrderBy(t => t.StartTime).ToList();
            #endregion

            yield return null;
        }


        IEnumerator CustomUpdate()
        {
            ival++;

            yield return null;
        }

        void Update()
        {

            if (dialogueData.ActiveCharacterDialogueSet.Count == 0) return;


            var data = dialogueData.ActiveCharacterDialogueSet[ActiveIndex];
            // we call processData on the nodedata that is at ActiveInxex in the ActiveCharacterDialogueSet
            data.ProcessData();

            // once there are no more routes processing events then we triger a refrsh
            if (ActiveEvents == 0)
            {
                doRefresh();
                MovePrevious();
            }

            if (targetReflectedData == null || cashedActiveIndex != ActiveIndex)
            {
                targetReflectedData = reflectedDataSet.Find(r => r.Id == data.DataID);
                cashedActiveIndex = ActiveIndex;
            }

            Name.text = data.CharacterName;


            // this wont run until we place routes in the ActiveCharacterDialogueSet. we wil do this by cheking each nodes connection at the end of generating the ActiveCharacterDialogueSet
            // and checkng if and connected nodes are routes, if they are , we just put them infront... of course after matching back the ID to make sure it is the correct node 
            if (data.type == typeof(RouteNodeData))
            {
                var route = (RouteNodeData)data;

                // here we want to jump forward in the list by adding 1 to Active index if the RouteNodeData at ActiveIndex is from a character other than a player
                // you can increase or reduce the flexibility of the system here 
                if (!route.IsPlayer)
                {
                    MoveNext();

                }
                else
                {
                    text.text = "";

                    for (int i = 0; i < RouteButtons.Count; i++)
                    {


                        if (route.DataIconnectedTo.Count >= i + 1)
                        {
                            var textchild = RouteButtons[i].transform.GetChild(0).GetComponent<Text>();


                            if (!RouteButtons[i].gameObject.activeInHierarchy)
                                RouteButtons[i].gameObject.SetActive(true);

                            if (route.UseAlternativeRouteTitles)
                                textchild.text = route.AlternativeRouteTitles[i];
                            else
                                textchild.text = route.DataIconnectedTo[i].Text;
                        }
                    }


                }
                foreach (var condition in targetReflectedData.Conditions)
                {
                    condition.ProcessConditionData();

                }

                if(route.IsPlayer)
                {
                    if (moveNextButton.gameObject.activeInHierarchy)
                    {
                        moveNextButton.gameObject.SetActive(false);
                        movePreviousButton.gameObject.SetActive(false);
                    }
                }

                if (cashedRoute != null)
                {
                    foreach (var button in RouteButtons)
                        button.gameObject.SetActive(false);


                    ActiveIndex = dialogueData.ActiveCharacterDialogueSet.IndexOf(cashedRoute) + 1;
                    cashedRoute = null;
                    moveNextButton.gameObject.SetActive(true);
                    movePreviousButton.gameObject.SetActive(true);

                }

                return;

            }

            else if (data.type == typeof(LinkNodeData))
            {
                var link = (LinkNodeData)data;

                // here we want to jump forward in the list by adding 1 to Active index if the RouteNodeData at ActiveIndex is from a character other than a player
                // you can increase or reduce the flexibility of the system here 
                //  if (!link.IsPlayer)
                //      ActiveIndex += 1;
                // return;
                if (!link.Loop)
                {
                    MoveNext();
                }
                else // this will be check in the sime loop scene
                {
                    var idOfNodedLoopedTo = link.DataIconnectedTo[0].DataIconnectedTo[0].DataID;
                    var loopedToNodeInTheActiveDialogueSet = dialogueData.ActiveCharacterDialogueSet.Find(rd => rd.DataID == idOfNodedLoopedTo);

                    ActiveIndex = dialogueData.ActiveCharacterDialogueSet.IndexOf(loopedToNodeInTheActiveDialogueSet);

                }
                foreach (var condition in targetReflectedData.Conditions)
                {
                    condition.ProcessConditionData();

                }

            }
            else if (data.type == typeof(EndNodeData))
            {
                var end = (EndNodeData)data;

                if(!end.IsPlayer)
                {
                    MoveNext();
                }
                foreach (var condition in targetReflectedData.Conditions)
                {
                    condition.ProcessConditionData();

                }
                // do something . i dont know yet

            }
            else if (data.type == typeof(ActionNodeData))
            {
                var action = (ActionNodeData)data;
                text.text = action.Text;
                foreach (var condition in targetReflectedData.Conditions)
                {
                    condition.ProcessConditionData();

                }
                // 

            }
            else
            {
                var dialogue = (DialogueNodeData)data;
                text.text = dialogue.Text;
                // TEST
                foreach (var condition in targetReflectedData.Conditions)
                {
                    condition.ProcessConditionData();
                }
            }

        }


        /// <summary>
        /// move to next event in chain of events
        /// </summary>
       public void MoveNext()
        {
            if(ActiveIndex +1 < dialogueData.ActiveCharacterDialogueSet.Count)
            ActiveIndex += 1;
        }
        /// <summary>
        /// move to previous evnt in chaing of events
        /// </summary>
      public  void MovePrevious()
        {
            if(ActiveIndex>0)
            ActiveIndex -= 1;
        }



    }
}