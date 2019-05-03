using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace DaiMangou.BridgedData
{



    /// <summary>
    ///  choice between typed or instant text
    /// </summary>
    public enum CharacterTextDisplayMode
    {
        Instant = 0,
        Typed = 1,
        Custom = 2

    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class CharacterDisplaySettings
    {
        public bool ShowGeneralSettings;
    }



    public class Character : MonoBehaviour
    {

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            if (moveNextButton)
                moveNextButton.onClick.AddListener(MoveNext);

            if (movePreviousButton)
                movePreviousButton.onClick.AddListener(MovePrevious);

            //  SetupStageCanvas();
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnEnable()
        {
            self = sceneData.Characters[targetChararacterIndex];

            if (self.IsPlayer)
                doRefresh += GenerateActiveDialogueSet;

        }

        /// <summary>
        /// 
        /// </summary>
        public void OnDisable()
        {
            if (self.IsPlayer) // if we dont do this , then all other characters will modify the ActiveCharacterDialogueSet -__-
                doRefresh -= GenerateActiveDialogueSet;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /*  IEnumerator Start()
          {



              while (true)
              {


                  yield return null;

              }
          }*/

        private void Start()
        {
            sceneData.ActiveCharacterDialogueSet = new List<NodeData>();
        }



        /// <summary>
        /// whne two or more character interact , the ActiveCharacterDialogueSetWill be generated
        /// </summary>
        public void GenerateActiveDialogueSet()
        {



            combi = self.NodeDataInMyChain.ToList();
            for (int i = 0; i < CommunicatingCharacters.Count; i++)
            {
                combi = combi.Concat(CommunicatingCharacters[i].NodeDataInMyChain).ToList();
            }


            //  combi= combi.OrderBy(t => t.StartTime).ToList();
            Characters = new List<CharacterNodeData>();

            foreach (var character in CommunicatingCharacters)
                Characters.Add(character);

            Characters.Add(self);

            //  countlist = new List<int>();
            #region here we will populate the  ActiveChracterDalogueSet at runtime, unlike the NodeDataInMyChain list, this list will have all nodes ordered correctly for execution at runtime
            // firstly make ActiveCharacterDialogueSet a new list
            sceneData.ActiveCharacterDialogueSet = new List<NodeData>();


            for (var i = 0; i < combi.Count; i++)
            {
                var data = combi[i];



                // each n=NodeData in the combi already has their has value set when the data was first pushed to the Dialoguers DialogueData scriptableObject so we just check the values
                if (!data.Pass)
                {

                    //   countlist.Add(i);
                    // we want to ignore all character and environment nodes
                    if (data.type != typeof(CharacterNodeData) && data.type != typeof(EnvironmentNodeData))
                    {
                        sceneData.ActiveCharacterDialogueSet.Add(data);
                    }
                }
            }



            // here we will begin the process of  setting times on all other nodes. Do not break this function.
            foreach (var character in Characters)
            {
                // we will start from the end of the nodeData list and assign start times and delay times in reverse
                for (int i = character.NodeDataInMyChain.Count - 1; i >= 0; i--)
                {
                    // if the nodeData is not set to pass then lets process it
                    if (!character.NodeDataInMyChain[i].Pass)
                    {
                        if (character.NodeDataInMyChain[i].type == typeof(RouteNodeData))
                        {
                            var route = (RouteNodeData)character.NodeDataInMyChain[i];

                            if (!route.OverrideStartTime)
                                route.StartTime = route.DataIconnectedTo[route.RouteID].StartTime - 0.0001f;
                        }
                        if (character.NodeDataInMyChain[i].type == typeof(LinkNodeData))
                        {
                            var link = (LinkNodeData)character.NodeDataInMyChain[i];

                            if (!link.OverrideStartTime)
                                link.StartTime = link.DataIconnectedTo[0].StartTime - 0.0001f;

                        }
                        // end nodes are connected to nothing , so we must look at the entire list again and find the node data with the largest start time in the current chain and use that value as the 
                        // an nodes start time
                        if (character.NodeDataInMyChain[i].type == typeof(EndNodeData))
                        {
                            var end = (EndNodeData)character.NodeDataInMyChain[i];

                            var templist = new List<NodeData>();
                            foreach (var n in character.NodeDataInMyChain)
                            {
                                if (!n.Pass)
                                {
                                    templist.Add(n);
                                }
                            }
                            end.StartTime = templist.Max(s => s.StartTime);
                        }
                    }
                }
            }

            sceneData.ActiveCharacterDialogueSet = sceneData.ActiveCharacterDialogueSet.OrderBy(t => t.StartTime).ToList();

            // now set the new Active index based on the cachedUID

            if (!CachedUID.Equals(""))
                ActiveIndex = sceneData.ActiveCharacterDialogueSet.FindIndex(i => i.UID == CachedUID);

            #endregion

        }


        public void CleanUp()
        {
            if (CommunicatingCharacters.Count == 0 && Characters.Count > 0)
            {
                Characters = new List<CharacterNodeData>();
                sceneData.ActiveCharacterDialogueSet = new List<NodeData>();
                ActiveIndex = 0; // // this resets the conversation. this must be changed
                CachedUID = "";
                communicatingCharacterCount = CommunicatingCharacters.Count;
            }

            if (CommunicatingCharacters.Count > 0 && Characters.Count > 0)
            {
                if (CommunicatingCharacters.Contains(sceneData.ActiveCharacterDialogueSet[ActiveIndex].CallingNodeData))
                    CachedUID = sceneData.ActiveCharacterDialogueSet[ActiveIndex].UID;
                else
                {
                    sceneData.ActiveCharacterDialogueSet = new List<NodeData>();
                    CommunicatingCharacters = new List<CharacterNodeData>();
                    ActiveIndex = 0; // this resets the conversation. this must be changed
                    CachedUID = "";
                }

                GenerateActiveDialogueSet();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        void FixedUpdate()
        {
           /* if (Characters.Count == 0 && dialogueData.ActiveCharacterDialogueSet.Count > 0)
            {
                Debug.Log("ran");
                dialogueData.ActiveCharacterDialogueSet = new List<NodeData>();
                return;
            }*/


            if (!self.IsPlayer) return;

            if (CommunicatingCharacters.Count != communicatingCharacterCount)
            {
                GenerateActiveDialogueSet();
                communicatingCharacterCount = CommunicatingCharacters.Count;

            }

            if (sceneData.ActiveCharacterDialogueSet.Count == 0) return;

            if (BridgeData.ActiveEvents > 0) return;

            ActiveNodeData = sceneData.ActiveCharacterDialogueSet[ActiveIndex];
            // we call processData on the nodedata that is at ActiveInxex in the ActiveCharacterDialogueSet
            ActiveNodeData.ProcessData();

            // once there are no more routes processing events then we triger a refrsh
            if (BridgeData.ActiveEvents == 0)
            {
                doRefresh();
                BridgeData.ActiveEvents = -1;
                return;
            }

            if (TargetReflectedData == null || CachedActiveIndex != ActiveIndex)
            {
                // needs to be optimized to not have to search thle list this way
                var tlist = ReflectedDataSet.ToList();
                if (communicatingCharacterCount > 0)
                {
                    for (int q = 0; q < CommunicatingCharacterGameobject.Count; q++)
                    {
                        tlist = tlist.Concat(CommunicatingCharacterGameobject[q].GetComponent<Character>().ReflectedDataSet).ToList();
                    }
                    /* foreach(var character in CommunicatingCharacterGameobject)
                      {

                          TargetReflectedData = character.GetComponent<Character>().ReflectedDataSet.Find(r => r.UID == ActiveNodeData.UID);
                      }*/
                    TargetReflectedData = tlist.Find(r => r.UID == ActiveNodeData.UID);
                }
                //  TargetReflectedData = ReflectedDataSet.Find(r => r.UID == ActiveNodeData.UID);

                CachedActiveIndex = ActiveIndex;
            }

            NameUI.text = ActiveNodeData.CharacterName;


            // this wont run until we place routes in the ActiveCharacterDialogueSet. we wil do this by cheking each nodes connection at the end of generating the ActiveCharacterDialogueSet
            // and checkng if and connected nodes are routes, if they are , we just put them infront... of course after matching back the ID to make sure it is the correct node 
            if (ActiveNodeData.type == typeof(RouteNodeData))
            {
                var route = (RouteNodeData)ActiveNodeData;

                // here we want to jump forward in the list by adding 1 to Active index if the RouteNodeData at ActiveIndex is from a character other than a player
                // you can increase or reduce the flexibility of the system here 
                if (!route.IsPlayer)
                {
                    MoveNext();

                }
                else
                {
                    DisplayedTextUI.text = "";

                    for (var i = 0; i < RouteButtons.Count; i++)
                    {
                        // set clicklistener indexInList value here

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
                foreach (var condition in TargetReflectedData.Conditions)
                {
                    condition.ProcessConditionData();

                }

                if (route.IsPlayer)
                {
                    if (moveNextButton.gameObject.activeInHierarchy)
                    {
                        moveNextButton.gameObject.SetActive(false);
                        movePreviousButton.gameObject.SetActive(false);
                    }
                }

                if (CachedRoute != null)
                {
                    foreach (var button in RouteButtons)
                        button.gameObject.SetActive(false);


                    ActiveIndex = sceneData.ActiveCharacterDialogueSet.IndexOf(CachedRoute) + 1;
                    CachedRoute = null;
                    moveNextButton.gameObject.SetActive(true);
                    movePreviousButton.gameObject.SetActive(true);
                    TargetReflectedData.Conditions.All(inv => inv.Invoked = false);

                }

                return;

            }
            else if (ActiveNodeData.type == typeof(LinkNodeData))
            {
                var link = (LinkNodeData)ActiveNodeData;

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
                    var idOfNodedLoopedTo = link.DataIconnectedTo[0].DataIconnectedTo[0].UID;
                    var loopedToNodeInTheActiveDialogueSet = sceneData.ActiveCharacterDialogueSet.Find(rd => rd.UID == idOfNodedLoopedTo);

                    ActiveIndex = sceneData.ActiveCharacterDialogueSet.IndexOf(loopedToNodeInTheActiveDialogueSet);

                }
                foreach (var condition in TargetReflectedData.Conditions)
                {
                    condition.ProcessConditionData();

                }

            }
            else if (ActiveNodeData.type == typeof(EndNodeData))
            {
                var end = (EndNodeData)ActiveNodeData;

                if (!end.IsPlayer)
                {
                    MoveNext();
                }
                foreach (var condition in TargetReflectedData.Conditions)
                {
                    condition.ProcessConditionData();

                }
                // do something . i dont know yet

            }
            else if (ActiveNodeData.type == typeof(ActionNodeData))
            {
                var action = (ActionNodeData)ActiveNodeData;
                DisplayedTextUI.text = action.Text;

                if (action.SoundEffect != null)
                    SoundEffectAudioSource.clip = action.SoundEffect;

                foreach (var condition in TargetReflectedData.Conditions)
                {
                    condition.ProcessConditionData();

                }
                // 

            }
            else
            {
                var dialogue = (DialogueNodeData)ActiveNodeData;

                if (textDisplayMode == CharacterTextDisplayMode.Typed)
                    if (TextLength < ActiveNodeData.Text.Length)
                        TypeText();

                if (textDisplayMode == CharacterTextDisplayMode.Instant)
                    DisplayedTextUI.text = dialogue.Text;

                if (dialogue.VoicedDialogue != null)
                    VoiceAudioSource.clip = dialogue.VoicedDialogue;

                if (dialogue.SoundEffect != null)
                    SoundEffectAudioSource.clip = dialogue.SoundEffect;

                // TEST
                foreach (var condition in TargetReflectedData.Conditions)
                {
                    condition.ProcessConditionData();
                }
            }



        }



        /// <summary>
        /// 
        /// </summary>
        void TypeText()
        {
            if (TypingAudioCip != null)
                TypingAudioSource.clip = TypingAudioCip;

            if (Timer == 0)
                Timer = Time.time;

            if (Time.time - Timer >= Delay)
            {
                Timer = Time.time;
                for (int i = 0; i < TypingSpeed; i++)
                {
                    TypingAudioSource.Play();
                    TextLength += 1;
                    DisplayedTextUI.text = ActiveNodeData.Text.Substring(0, Math.Min(ActiveNodeData.Text.Length, TextLength));
                }
            }



        }

        /// <summary>
        /// move to next event in chain of events
        /// </summary>
        public void MoveNext()
        {
            // make sureto set invoked back to false to false so that the events can be invoked again if we move back to it
            TargetReflectedData.Conditions.All(inv => inv.Invoked = false);

            // we also deactivate the buttons BEFORE 
            foreach (var button in RouteButtons)
                button.gameObject.SetActive(false);



            if (ActiveIndex + 1 < sceneData.ActiveCharacterDialogueSet.Count)
            {
                // reset out text length so that typed text can type itself
                TextLength = 0;
                ActiveIndex += 1;
            }
        }


        /// <summary>
        /// move to previous evnt in chaing of events
        /// </summary>
        public void MovePrevious()
        {
            // make sureto set invoked back to false to false so that the events can be invoked again if we move back to it

            TargetReflectedData.Conditions.All(inv => inv.Invoked = false);

            foreach (var button in RouteButtons)
                button.gameObject.SetActive(false);


            if (ActiveIndex > 0)
            {
                // reset out text length so that typed text can type itself
                TextLength = 0;
                ActiveIndex -= 1;
            }
        }

        public void ContinueOnRoute()
        {
            // make sureto set invoked back to false to false so that the events can be invoked again if we move back to it
            TargetReflectedData.Conditions.All(inv => inv.Invoked = false);

            var route = (RouteNodeData)sceneData.ActiveCharacterDialogueSet[ActiveIndex];
            CachedRoute = route;
            //route.ProcessData();
        }
        public void GoToRoute(int routeID)
        {
            // make sureto set invoked back to false to false so that the events can be invoked again if we move back to it
            TargetReflectedData.Conditions.All(inv => inv.Invoked = false);

            var route = (RouteNodeData)sceneData.ActiveCharacterDialogueSet[ActiveIndex];
            route.RouteID = routeID;
            CachedRoute = route;
        }

        #region variables 
        //  public UnityEvent m_MyEvent;

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public CharacterDisplaySettings CharacterDisplaySettings;

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        [UnityEngine.Serialization.FormerlySerializedAs("dialogueData")]
        public SceneData sceneData;

        /// <summary>
        /// the value here is set to -1 to start , so that we can select a scene value which will then pass its scene id to this scene id value which will then be serialized
        /// </summary>
        // [HideInInspector]
        // public int SceneID = -1;

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public delegate void RefreshCharacterDialogue();

        /// <summary>
        /// used to re-generate the ActiveCharacterDialogueSet
        /// </summary>
        [HideInInspector]
        public static RefreshCharacterDialogue doRefresh;

        /// <summary>
        /// static int flag used to check how many charactrs are speaking at once, if the number inceases the aUI for dialogue will be added
        /// </summary>
        [HideInInspector]
        public static int ActiveDialogues = 0;


        /// <summary>
        /// this is the value of the data in ActiveNodeData that is currently being processed
        /// </summary>
       // [HideInInspector]
        public int ActiveIndex;

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        private int CachedActiveIndex = -1;

        //   public List<UnityEvent> EventTrigger = new List<UnityEvent>();

        // a special set of data with an ID value matching that of each NodeData in the FullCharacterDataSet
        //  [HideInInspector]
        public List<CharacterNodeData> Characters = new List<CharacterNodeData>();

        /// <summary>
        /// 
        /// </summary>
       //[HideInInspector]
        public List<ReflectedData> ReflectedDataSet = new List<ReflectedData>();
        /// <summary>
        /// 
        /// </summary>
        public List<ReflectedData> TempReflectedDataSet = new List<ReflectedData>();
        [HideInInspector]
        public GameObject ReflectedDataParent;

        /// <summary>
        /// 
        /// </summary>
       // [HideInInspector]
        public ReflectedData TargetReflectedData;

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public RouteNodeData CachedRoute;

        /// <summary>
        /// 
        /// </summary>
        [NonSerialized]
        public NodeData ActiveNodeData;



        public Text NameUI;
        public Text DisplayedTextUI;
        public Button moveNextButton;
        public Button movePreviousButton;
        public List<Button> RouteButtons = new List<Button>();


        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public CharacterTextDisplayMode textDisplayMode = CharacterTextDisplayMode.Instant;

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public int TypingSpeed = 2;

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public float Delay = 0.001f;

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public int TextLength = 0;

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public float Timer = 0;

        /// <summary>
        /// 
        /// </summary>
       // [HideInInspector]
        public AudioSource TypingAudioSource;
        [HideInInspector]
        public AudioClip TypingAudioCip = null;
        //  [HideInInspector]
        public AudioSource VoiceAudioSource;
        // [HideInInspector]
        public AudioSource SoundEffectAudioSource;
        #endregion

        public List<CharacterNodeData> CommunicatingCharacters = new List<CharacterNodeData>();
        public List<GameObject> CommunicatingCharacterGameobject = new List<GameObject>();
        private int communicatingCharacterCount = 0;
        public CharacterNodeData self;

        public int targetChararacterIndex;
        public List<NodeData> combi = new List<NodeData>();

        public string CachedUID = "";
    }



}