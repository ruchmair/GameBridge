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
    /// <summary>
    /// 
    /// </summary>
    public enum TextDisplayMode
    {
        Instant = 0,
        Typed = 1,
        Custom = 2

    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class DisplaySettings
    {
        public bool ShowGeneralSettings;
    }

    public class Dialoguer : MonoBehaviour
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

            doRefresh += Refresh;

        }

        /// <summary>
        /// 
        /// </summary>
        public void OnDisable()
        {
            doRefresh -= Refresh;
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


                yield return null;

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        void Refresh()
        {
            GenerateActiveDialogueSet();
        }

        /// <summary>
        /// 
        /// </summary>
        void GenerateActiveDialogueSet()
        {


            //  countlist = new List<int>();
            #region here we will populate the  ActiveChracterDalogueSet at runtime, unlike the FullCharacterDialogueSet list, this list will have all nodes ordered correctly for execution at runtime
            // firstly make ActiveCharacterDialogueSet a new list
            dialogueData.ActiveCharacterDialogueSet = new List<NodeData>();
            for (var i = 0; i < dialogueData.FullCharacterDialogueSet.Count; i++)
            {
                var data = dialogueData.FullCharacterDialogueSet[i];

                // each n=NodeData in the FullCharacterDialogueSet already has their has value set when the data was first pushed to the Dialoguers DialogueData scriptableObject so we just check the values
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
                            // RIGHT AFTER we do another for loop, this loop breaks the first loop and will continue it one this loop completes
                            // in this loop we are going to execute a function which will check what data is connected to  and will tell if to run the function AddMeToThisList();
                            // if the nodedata it is conned to is not set to pass. this function is executed in NodeData so it will ad itself t to the list, this node must also NOT be an 
                            // ActionNodeData or a DoialogueNodeData since those are already going to be adde to the list in the above function^
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

                            route.StartTime = route.DataIconnectedTo[route.RouteID].StartTime;
                        }
                        if (character.NodeDataInMyChain[i].type == typeof(LinkNodeData))
                        {
                            var link = (LinkNodeData)character.NodeDataInMyChain[i];
                            link.StartTime = link.DataIconnectedTo[0].StartTime;

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

            dialogueData.ActiveCharacterDialogueSet = dialogueData.ActiveCharacterDialogueSet.OrderBy(t => t.StartTime).ToList();
            #endregion

        }

        /// <summary>
        /// 
        /// </summary>
        void FixedUpdate()
        {
            if (dialogueData.ActiveCharacterDialogueSet.Count == 0) return;

            if (ActiveEvents > 0) return;

            ActiveNodeData = dialogueData.ActiveCharacterDialogueSet[ActiveIndex];
            // we call processData on the nodedata that is at ActiveInxex in the ActiveCharacterDialogueSet
            ActiveNodeData.ProcessData();

            // once there are no more routes processing events then we triger a refrsh
            if (ActiveEvents == 0)
            {
                doRefresh();
                ActiveEvents = -1;
                return;
            }

            if (TargetReflectedData == null || CachedActiveIndex != ActiveIndex)
            {
                // needs to be optimized to not have to search thle list this way
                TargetReflectedData = ReflectedDataSet.Find(r => r.UID == ActiveNodeData.UID);
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


                    ActiveIndex = dialogueData.ActiveCharacterDialogueSet.IndexOf(CachedRoute) + 1;
                    CachedRoute = null;
                    moveNextButton.gameObject.SetActive(true);
                    movePreviousButton.gameObject.SetActive(true);

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
                    var loopedToNodeInTheActiveDialogueSet = dialogueData.ActiveCharacterDialogueSet.Find(rd => rd.UID == idOfNodedLoopedTo);

                    ActiveIndex = dialogueData.ActiveCharacterDialogueSet.IndexOf(loopedToNodeInTheActiveDialogueSet);

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

                if (textDisplayMode == TextDisplayMode.Typed)
                    if (TextLength < ActiveNodeData.Text.Length)
                        TypeText();

                if (textDisplayMode == TextDisplayMode.Instant)
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
            if (TypingAudioCip != null) ;
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
            TargetReflectedData.Conditions.All(inv => inv.Invoked == false);

            // we also deactivate the buttons BEFORE 
            foreach (var button in RouteButtons)
                button.gameObject.SetActive(false);



            if (ActiveIndex + 1 < dialogueData.ActiveCharacterDialogueSet.Count)
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
            TargetReflectedData.Conditions.All(inv => inv.Invoked == false);

            foreach (var button in RouteButtons)
                button.gameObject.SetActive(false);


            if (ActiveIndex > 0)
            {
                // reset out text length so that typed text can type itself
                TextLength = 0;
                ActiveIndex -= 1;
            }
        }

        private void SetupStageCanvas()
        {
            /*  #region setup main canvas
              var newStageCanvas = new GameObject("Stage Canvas");
              newStageCanvas.AddComponent<Canvas>();

              newStageCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

              newStageCanvas.AddComponent<CanvasScaler>();
              var canvasScaler = newStageCanvas.GetComponent<CanvasScaler>();
              canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

              newStageCanvas.AddComponent<GraphicRaycaster>();
              newStageCanvas.transform.position = Vector3.zero;
              #endregion

              #region Dialogue Panel
              var newDialoguePanel = new GameObject("Dialogue Panel");
              newDialoguePanel.AddComponent<CanvasRenderer>();

              newDialoguePanel.transform.SetParent(newStageCanvas.transform);
              newDialoguePanel.transform.localPosition = new Vector3(0,10,0);

              newDialoguePanel.AddComponent<Image>();
              var newDialoguePanelImage = newDialoguePanel.GetComponent<Image>();
              newDialoguePanelImage.sprite = Resources.Load<Sprite>("DefaultBackground");
             // newDialoguePanelImage.material = new Material(Shader.Find("Sprites/Default"));
              newDialoguePanelImage.type = Image.Type.Sliced;
              newDialoguePanelImage.fillCenter = true;
              newDialoguePanelImage.color = new Color(60, 60, 60, 0.35f);

              var paneldata = newDialoguePanel.GetComponent<RectTransform>();
              paneldata.anchorMin = new Vector2(0, 0);
              paneldata.anchorMax = new Vector2(1, 0);
              paneldata.pivot = new Vector2(0.5f, 0);
              paneldata.sizeDelta = new Vector2(-40, 230);

              #endregion

              #region Name And Dialogue UI
              var nameAndDialoguDisplay = new GameObject("Name and Dialogue Display");
              nameAndDialoguDisplay.transform.SetParent(newDialoguePanel.transform);
              nameAndDialoguDisplay.AddComponent<VerticalLayoutGroup>();
              var verticalLayoutGroup = nameAndDialoguDisplay.GetComponent<VerticalLayoutGroup>();
              verticalLayoutGroup.spacing = 0;
              verticalLayoutGroup.childAlignment = TextAnchor.UpperLeft;
              verticalLayoutGroup.childControlWidth = true;
              verticalLayoutGroup.childControlHeight = false;
              verticalLayoutGroup.childForceExpandHeight = true;
              verticalLayoutGroup.childForceExpandWidth = false;
              nameAndDialoguDisplay.transform.localPosition = new Vector3(0,115,0);



              var DialoguePanelTransform = nameAndDialoguDisplay.GetComponent<RectTransform>();
              DialoguePanelTransform.anchorMin = new Vector2(0, 1);
              DialoguePanelTransform.anchorMax = new Vector2(1, 1);
              DialoguePanelTransform.pivot = new Vector2(0.5f, 1);
              DialoguePanelTransform.sizeDelta = new Vector2(0, 100);
              #region Name 

              var nameUI = new GameObject("Name");
             nameUI.transform.SetParent(nameAndDialoguDisplay.transform);          
              nameUI.AddComponent<CanvasRenderer>();
              nameUI.AddComponent<Text>();
              var nameUIText = nameUI.GetComponent<Text>();
              nameUIText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
              nameUIText.fontStyle = FontStyle.Bold;
              nameUIText.fontSize = 16;
              nameUIText.supportRichText = true;

              var nameUITransform = nameUI.GetComponent<RectTransform>();
              nameUITransform.pivot = new Vector2(0.5f, 0.5f);
              nameUITransform.sizeDelta = new Vector2(0, 16);

              #endregion

              #region Dialogue 
              var dialogueUI = new GameObject("Dioalogue");
              dialogueUI.transform.SetParent(nameAndDialoguDisplay.transform);
              var dialogueUITransform = dialogueUI.GetComponent<RectTransform>();

              dialogueUI.AddComponent<CanvasRenderer>();
              dialogueUI.AddComponent<Text>();
              var dialogueUIText = dialogueUI.GetComponent<Text>();
              dialogueUIText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
              dialogueUIText.fontSize = 14;
              dialogueUIText.supportRichText = true;


              dialogueUITransform.pivot = new Vector2(0.5f, 0.5f);
              dialogueUITransform.sizeDelta = new Vector2(0, 80);

              #endregion



              #endregion


              #region Route Buttons

              var routeButtonsUI = new GameObject("Route Buttons");
              routeButtonsUI.transform.SetParent(newDialoguePanel.transform);
              routeButtonsUI.AddComponent<GridLayoutGroup>();
              var gridLayout = routeButtonsUI.GetComponent<GridLayoutGroup>();
              routeButtonsUI.transform.localPosition = new Vector3(0,100,0);

              gridLayout.cellSize = new Vector2(186,29);
              gridLayout.spacing = new Vector2(86,0);
              gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
              gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
              gridLayout.childAlignment = TextAnchor.UpperLeft;
              gridLayout.constraint = GridLayoutGroup.Constraint.Flexible;

              var routeButtonsUITransform = routeButtonsUI.GetComponent<RectTransform>();
              routeButtonsUITransform.anchorMin = new Vector2(0, 1);
              routeButtonsUITransform.anchorMax = new Vector2(1, 1);
              routeButtonsUITransform.pivot = new Vector2(0.5f, 0.5f);
              routeButtonsUITransform.sizeDelta = new Vector2(0, 32);

              #region buttons
              for (var i = 0; i < tempcount; i++)
              {
                  var buttonsUI = new GameObject("Buttons"+i);
                  buttonsUI.transform.SetParent(routeButtonsUI.transform);
                  buttonsUI.AddComponent<CanvasRenderer>();
                  buttonsUI.AddComponent<Image>();
                  buttonsUI.AddComponent<Button>();
                  buttonsUI.AddComponent<ClickListener>();
                  var buttonImage = buttonsUI.GetComponent<Image>();
                  buttonImage.sprite = Resources.Load<Sprite>("DefaultBackground");
                  // newDialoguePanelImage.material = new Material(Shader.Find("Sprites/Default"));
                  buttonImage.type = Image.Type.Sliced;
                  buttonImage.fillCenter = true;
                  buttonImage.color = new Color(60, 60, 60, 0.35f);

                  var button = buttonsUI.GetComponent<Button>();
                  button.transition = Selectable.Transition.ColorTint;
                  button.targetGraphic = buttonImage;
                  var colblock = new ColorBlock();
                  colblock.normalColor = new Color(255, 255, 255);
                  colblock.normalColor = new Color(245, 245, 245);
                  colblock.normalColor = new Color(200, 200, 200);
                  colblock.disabledColor = new Color(200, 200, 200);
                  colblock.fadeDuration = 0.1f;

                  button.colors = colblock;
                  var buttonTransform = button.GetComponent<RectTransform>();
                  buttonTransform.anchorMin = new Vector2(0, 1);
                  buttonTransform.anchorMax = new Vector2(0, 1);
                  buttonTransform.pivot = new Vector2(0.5f, 0.5f);
                  buttonTransform.sizeDelta = new Vector2(0, 29);


                  var buttonClickListener = buttonsUI.GetComponent<ClickListener>();
                  buttonClickListener.dialoguer = this; // temp
                  button.onClick.AddListener(buttonClickListener.SwitchRoute);


                  var buttonsText = new GameObject("ButtonText");
                  buttonsText.transform.SetParent(buttonsUI.transform);
                  buttonsText.AddComponent<Text>();
                  var buttonTextObject = buttonsText.GetComponent<Text>();
                  buttonTextObject.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                  buttonTextObject.text = "Buton" + i;

                  var buttonsTextTransform = buttonsText.GetComponent<RectTransform>();
                  buttonsTextTransform.anchorMin = new Vector2(0, 0);
                  buttonsTextTransform.anchorMax = new Vector2(1, 1);
                  buttonsTextTransform.pivot = new Vector2(0.5f, 0.5f);
                  buttonsTextTransform.sizeDelta = new Vector2(0, 0);

                  buttonsUI.layer = 5;
                  buttonsText.layer = 5;

              }

              #endregion

              #endregion



              newStageCanvas.layer = 5;
              newDialoguePanel.layer = 5;
              nameAndDialoguDisplay.layer = 5;
              nameUI.layer = 5;
              dialogueUI.layer = 5;
              routeButtonsUI.layer = 5;
              */
        }

        #region variables 
        //  public UnityEvent m_MyEvent;

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public DisplaySettings DialoguerDisplaySettings;

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public DialogueData dialogueData;

        /// <summary>
        /// the value here is set to -1 to start , so that we can select a scene value which will then pass its scene id to this scene id value which will then be serialized
        /// </summary>
        [HideInInspector]
        public int SceneID = -1;

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public delegate void DoRefresh();

        /// <summary>
        /// used to re-generate the ActiveCharacterDialogueSet
        /// </summary>
        [HideInInspector]
        public static DoRefresh doRefresh;

        /// <summary>
        ///  flag to check if any nodes are still processing data
        /// </summary>
        [HideInInspector]
        public static int ActiveEvents = -1;

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
        [HideInInspector]
        public List<CharacterNodeData> Characters = new List<CharacterNodeData>();

        /// <summary>
        /// 
        /// </summary>
       //[HideInInspector]
        public List<ReflectedData> ReflectedDataSet = new List<ReflectedData>();
        public List<ReflectedData> TempReflectedDataSet = new List<ReflectedData>();
        [HideInInspector]
        public GameObject ReflectedDataParent;

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        private ReflectedData TargetReflectedData;

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
        public TextDisplayMode textDisplayMode = TextDisplayMode.Instant;

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

        // private int  tempcount = 4;
        //  private Font DefaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        //  private Material DefaultMaterial = new Material(Shader.Find("Sprites/Default"));
    }
}