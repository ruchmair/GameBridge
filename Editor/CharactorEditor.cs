using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using DaiMangou.BridgedData;
using UnityEngine;
using DaiMangou.GameBridge;
using DaiMangou.Storyteller;
using DaiMangou.Storyteller.Elements;
using UnityEngine.Events;


namespace DaiMangou.GameBridgeEditor
{
    using Grid = DaiMangou.Storyteller.Grid;

    [CustomEditor(typeof(Character))]
    public class CharactorEditor : Editor
    {


        #region variables
        [SerializeField]
        public string UID = "";
        [SerializeField]
        private NodeData matchingSelectedNodeData;
        private ReflectedData matchingReflectedData;
        private bool iconSet;
        private Rect ScreenRect = new Rect(0, 0, 0, 0);
        private Vector2 scrollView;
        private List<int> ConditionSpecificSpaceing = new List<int>();
        private List<Rect> _characterAreas = new List<Rect>();
        private Vector2 characterSelectionScrollView;
        //  private SerializedProperty eventTrigger;
        //private SerializedProperty moveNextTrigger;
        //private SerializedObject myNodeData;

        /// the matching selectedNodeDatas matching reflected data
        // private SerializedObject matchingNodeDataSerializedObject;
        /// <summary>
        /// the matching selectedNodeDatas matching reflected datas condition
        /// </summary>
       
        private List<string> characternames = new List<string>();

        #endregion

        public void OnEnable()
        {

            #region set the icon
            if (!iconSet)
            {
                var selectedCharacter = (Character)target;
                IconManager.SetIcon(selectedCharacter, IconManager.DaiMangouIcons.ChatIcon);
                iconSet = true;


            }
            #endregion

            ScreenRect.size = new Vector2(Screen.width, Screen.height);

        }

        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        public override void OnInspectorGUI()
        {
            var selectedCharacter = (Character)target;





            serializedObject.Update();



            #region Character cover image
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(ImageLibrary.GBCharacterImage, EditorStyles.inspectorDefaultMargins);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            #endregion
            Separator();

            GUILayout.Space(5);

            DrawDefaultInspector();

            GUILayout.Space(5);

            Separator();
            selectedCharacter.sceneData = (SceneData)EditorGUILayout.ObjectField(selectedCharacter.sceneData, typeof(SceneData), false);

            if (selectedCharacter.sceneData == null) return;

            if(characternames.Count == 0)
            {
                foreach (var character in selectedCharacter.sceneData.Characters)
                    characternames.Add(character.CharacterName);
            }

          selectedCharacter.targetChararacterIndex = EditorGUILayout.Popup(selectedCharacter.targetChararacterIndex, characternames.ToArray());

            selectedCharacter.self = selectedCharacter.sceneData.Characters[selectedCharacter.targetChararacterIndex];
              if (GUILayout.Button("Setup/ Re-setup Character"))
             {
                
                    var targetCharacter = selectedCharacter.sceneData.Characters[selectedCharacter.targetChararacterIndex];

                        var sortedList = new List<NodeData>();
                        foreach (var characterNodeChainData in targetCharacter.NodeDataInMyChain)
                        {
                            sortedList.Add(characterNodeChainData);
                        }


                        #region rename the selectedgameObject with Character  script on it
                        selectedCharacter.gameObject.name = targetCharacter.CharacterName;
                        #endregion

                      //  var tempNodeDataList = new List<StoryElement>();

                        // now we update or crreate the Reflected data gameobject if necessary
                        if (selectedCharacter.ReflectedDataSet.Count == 0)
                        {
                            selectedCharacter.ReflectedDataSet.Resize(sortedList.Count);

                            selectedCharacter.ReflectedDataParent = new GameObject("Reflected Data");
                            selectedCharacter.ReflectedDataParent.transform.SetParent(selectedCharacter.transform);
                            selectedCharacter.ReflectedDataParent.transform.localPosition = Vector3.zero;
                            selectedCharacter.ReflectedDataParent.hideFlags = HideFlags.HideInHierarchy;


                            var AudioManager = new GameObject("Audio Manager");
                            AudioManager.transform.SetParent(selectedCharacter.transform);
                            AudioManager.transform.localPosition = Vector3.zero;

                            var TypingAudioManager = new GameObject("Typing");
                            TypingAudioManager.transform.SetParent(AudioManager.transform);
                            TypingAudioManager.transform.localPosition = Vector3.zero;
                            TypingAudioManager.AddComponent<AudioSource>();
                            selectedCharacter.TypingAudioSource = TypingAudioManager.GetComponent<AudioSource>();

                            var VoiceAudioManager = new GameObject("Voice");
                            VoiceAudioManager.transform.SetParent(AudioManager.transform);
                            VoiceAudioManager.transform.localPosition = Vector3.zero;
                            VoiceAudioManager.AddComponent<AudioSource>();
                            selectedCharacter.VoiceAudioSource = VoiceAudioManager.GetComponent<AudioSource>();

                            var SoundEffectsAudioManager = new GameObject("Sound Effects");
                            SoundEffectsAudioManager.transform.SetParent(AudioManager.transform);
                            SoundEffectsAudioManager.transform.localPosition = Vector3.zero;
                            SoundEffectsAudioManager.AddComponent<AudioSource>();
                            selectedCharacter.SoundEffectAudioSource = SoundEffectsAudioManager.GetComponent<AudioSource>();

                        }
                        else
                        {
                            // we cache the current set of reflected data in a temporary list and then empry and resize the reflecteddataset list
                            selectedCharacter.TempReflectedDataSet = new List<ReflectedData>(); ;
                            foreach (var capturedData in selectedCharacter.ReflectedDataSet)
                                selectedCharacter.TempReflectedDataSet.Add(capturedData);

                            selectedCharacter.ReflectedDataSet = new List<ReflectedData>();
                            selectedCharacter.ReflectedDataSet.Resize(sortedList.Count);

                        }

                        // loop through the sorted list
                        for (var i = 0; i < sortedList.Count; i++)
                        {

                            #region create a new instance of ReflectedData as a gameObject and then assign the sortedList value at i to the reflected data ID
                            var newReflectedDatagameObject = new GameObject(sortedList[i].Name + "Reflected");
                            newReflectedDatagameObject.transform.SetParent(selectedCharacter.ReflectedDataParent.transform);
                            newReflectedDatagameObject.AddComponent<ReflectedData>();
                            var theReflectedDataComponent = newReflectedDatagameObject.GetComponent<ReflectedData>();
                            theReflectedDataComponent.CharacterGameObject = selectedCharacter.gameObject;
                            theReflectedDataComponent.character = selectedCharacter;
                            theReflectedDataComponent.self = newReflectedDatagameObject;
                            // we already resized the ReflectedDataSet list to be the same size as the SortedList so we dont use .Add
                            selectedCharacter.ReflectedDataSet[i] = theReflectedDataComponent;
                            // it is VERY important that the UIDs match.
                            selectedCharacter.ReflectedDataSet[i].UID = sortedList[i].UID;
                            #endregion

                            #region Add the first conditin
                            var newCondition = new GameObject(newReflectedDatagameObject.name + "Condition " + theReflectedDataComponent.Conditions.Count);
                            newCondition.AddComponent<Condition>();
                            var _condition = newCondition.GetComponent<Condition>();
                            _condition.CharacterGameObject = selectedCharacter.gameObject;
                            _condition.character = selectedCharacter;
                            _condition.Self = newCondition;
                            newCondition.transform.SetParent(newReflectedDatagameObject.transform);
                            // newCondition.hideFlags = HideFlags.HideInHierarchy;
                            theReflectedDataComponent.Conditions.Add(newCondition.GetComponent<Condition>());
                            #endregion

                            #region here we begin checking to see if any UID values we have for reflected data in the temp reflected data. if so , we destroy their conditions and replace the m with the conditions in the TempReflectedDataSet
                            if (selectedCharacter.TempReflectedDataSet.Count != 0)
                            {
                                foreach (var tempData in selectedCharacter.TempReflectedDataSet)
                                {
                                    //we can use ReflectedDataSet[i] because the sorted list count and ReflectedDataSet ount are the same
                                    var data = selectedCharacter.ReflectedDataSet[i];
                                    if (sortedList[i].UID == tempData.UID)
                                    {
                                        if (tempData.UID == data.UID)
                                        {
                                            data.CharacterGameObject = tempData.CharacterGameObject;
                                            data.character = tempData.character;
                                            data.characterComponent = tempData.characterComponent;


                                            for (var c = 0; c < data.Conditions.Count; c++)
                                            {
                                                var conditionToDelete = data.Conditions[c];
                                                DestroyImmediate(conditionToDelete.Self);
                                                data.Conditions.RemoveAt(c);
                                            }

                                            // finally we move the condition from TempReflectedDataSet[i] conditions to the datas condition list
                                            foreach (var condition in tempData.Conditions)
                                            {
                                                condition.Self.transform.SetParent(data.self.transform);
                                                data.Conditions.Add(condition);
                                            }


                                        }
                                    }
                                }


                            }
                            #endregion

                        }

                        // now destroy all the data in TempReflectedDataSet
                        foreach (var item in selectedCharacter.TempReflectedDataSet)
                        {
                            DestroyImmediate(item.self);
                        }
                        selectedCharacter.TempReflectedDataSet.RemoveAll(n => n == null);






                    
                


            }


            #region General Settings

            selectedCharacter.CharacterDisplaySettings.ShowGeneralSettings = EditorGUILayout.Foldout(selectedCharacter.CharacterDisplaySettings.ShowGeneralSettings, "General Settings");

            if (selectedCharacter.CharacterDisplaySettings.ShowGeneralSettings)
            {

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Text Display Mode");
                selectedCharacter.textDisplayMode = (CharacterTextDisplayMode)EditorGUILayout.EnumPopup(selectedCharacter.textDisplayMode);
                GUILayout.EndHorizontal();

                switch (selectedCharacter.textDisplayMode)
                {
                    case CharacterTextDisplayMode.Instant:

                        break;
                    case CharacterTextDisplayMode.Typed:
                        GUILayout.BeginHorizontal();
                        // GUILayout.FlexibleSpace();
                        GUILayout.Label("Typing Speed");
                        selectedCharacter.TypingSpeed = EditorGUILayout.IntField(selectedCharacter.TypingSpeed, GUILayout.Height(15), GUILayout.Width(150));
                        //  GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();


                        GUILayout.BeginHorizontal();
                        // GUILayout.FlexibleSpace();
                        GUILayout.Label("Delay");
                        selectedCharacter.Delay = EditorGUILayout.FloatField(selectedCharacter.Delay, GUILayout.Height(15), GUILayout.Width(150));
                        //  GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        //GUILayout.FlexibleSpace();
                        GUILayout.Label("Typing Sound");
                        selectedCharacter.TypingAudioCip = (AudioClip)EditorGUILayout.ObjectField(selectedCharacter.TypingAudioCip, typeof(AudioClip), false, GUILayout.Height(15), GUILayout.Width(150));
                        // GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();



                        break;
                    case CharacterTextDisplayMode.Custom:

                        break;
                }
            }
            GUILayout.Space(5);

            #endregion


            Separator();

            #region if there is no ActiveStory
            if (CurrentStory.ActiveStory == null)
                return;
            #endregion

            #region if no scenes are in the project
            if (CurrentStory.ActiveStory.Scenes.Count == 0)
            {
                return;
            }
            #endregion

            #region if scene id == default -1
            if (selectedCharacter.sceneData.SceneID == -1)
            {
                return;
            }
            #endregion
            var scene = CurrentStory.ActiveStory.Scenes[selectedCharacter.sceneData.SceneID];

            #region if no nodes are in the scene
            if (scene.NodeElements.Count == 0)
            {
                return;
            }
            #endregion

            var selectedNode = scene.NodeElements.Last();

           
            #region check if we made a selection of a diferent node
            if (UID != selectedNode.UID)
            {

                matchingSelectedNodeData = selectedCharacter.sceneData.Characters[selectedCharacter.targetChararacterIndex].NodeDataInMyChain.Find(n => n.UID == selectedNode.UID) as NodeData;
                matchingReflectedData = selectedCharacter.ReflectedDataSet.Find(r => r.UID == selectedNode.UID);

                UID = selectedNode.UID;


                //  matchingNodeDataSerializedObject = null;
            }
            #endregion

            #region if there is no matchingSelectedNodeData
            if (matchingSelectedNodeData == null)
            {
                return;
            }
            #endregion



            #region IMGUI


            #region Draw Selected Node name

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(matchingSelectedNodeData.CharacterName, Theme.GameBridgeSkin.customStyles[5]);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(matchingSelectedNodeData.Name, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            #endregion

            GUILayout.Space(5);

            // Separator2();

            GUILayout.Space(5);


            GUILayout.Space(5);

            #region node specific data
            // we check if the matchingSelectedNodeData is a charactrNodeData, if it is , we show the option for setting the IsPlayer value
            if (matchingSelectedNodeData.type == typeof(CharacterNodeData))
            {
                var character = (CharacterNodeData)matchingSelectedNodeData;

                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();
                var state = character.IsPlayer ? "Is Player,Turn Off Player ?" : "Is Not Player,Turn On Player ?";
                if (GUILayout.Button(state, GUILayout.Height(15)))
                {
                    character.IsPlayer = !character.IsPlayer;
                    foreach (var dataset in character.NodeDataInMyChain)
                        dataset.IsPlayer = character.IsPlayer;

                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(5);


            }


            if (matchingSelectedNodeData.type == typeof(EnvironmentNodeData))
            {

            }

            if (matchingSelectedNodeData.type == typeof(ActionNodeData))
            {
                var action = (ActionNodeData)matchingSelectedNodeData;

                action.Text = EditorGUILayout.TextArea(action.Text, EditorStyles.textArea, GUILayout.Height(80));

                GUILayout.Space(5);

                action.SoundEffect = (AudioClip)EditorGUILayout.ObjectField("Sound Effect", action.SoundEffect, typeof(AudioClip), false);

                EditorGUILayout.LabelField("Start Time", action.StartTime.ToString());
                EditorGUILayout.LabelField("Duration", action.Duration.ToString());
                EditorGUILayout.LabelField("Delay", action.Delay.ToString());
                // EditorGUILayout.LabelField("Realtime Delay", action.RealtimeDelay.ToString());
                DrawConditionCreator(selectedCharacter);
            }

            if (matchingSelectedNodeData.type == typeof(DialogueNodeData))
            {
                var dialogue = (DialogueNodeData)matchingSelectedNodeData;

                dialogue.Text = EditorGUILayout.TextArea(dialogue.Text, EditorStyles.textArea, GUILayout.Height(80));

                GUILayout.Space(5);
                dialogue.VoicedDialogue = (AudioClip)EditorGUILayout.ObjectField("Voice clip", dialogue.VoicedDialogue, typeof(AudioClip), false);
                GUILayout.Space(5);

                // GUILayout.Box(AssetPreview.GetAssetPreview(dialogue.VoicedDialogue),EditorStyles.inspectorDefaultMargins);


                GUILayout.Space(5);
                dialogue.SoundEffect = (AudioClip)EditorGUILayout.ObjectField("Sound Effect", dialogue.SoundEffect, typeof(AudioClip), false);

                EditorGUILayout.LabelField("Start Time", dialogue.StartTime.ToString());
                EditorGUILayout.LabelField("Duration", dialogue.Duration.ToString());
                EditorGUILayout.LabelField("Delay", dialogue.Delay.ToString());
                // EditorGUILayout.LabelField("Realtime Delay", dialogue.RealtimeDelay.ToString());
                DrawConditionCreator(selectedCharacter);

            }

            if (matchingSelectedNodeData.type == typeof(RouteNodeData))
            {
                var route = (RouteNodeData)matchingSelectedNodeData;

                var useAlternativeRouteTitleState = route.UseAlternativeRouteTitles ? "AlternativeRoute Title is On" : "Alternative Route Title Is Off";
                if (GUILayout.Button(useAlternativeRouteTitleState, GUILayout.Height(15)))
                    route.UseAlternativeRouteTitles = !route.UseAlternativeRouteTitles;
                GUILayout.Space(2);


                if (route.UseAlternativeRouteTitles)
                {
                    if (route.AlternativeRouteTitles.Count != route.DataIconnectedTo.Count)
                        route.AlternativeRouteTitles.Resize(route.DataIconnectedTo.Count);

                    for (var i = 0; i < route.DataIconnectedTo.Count; i++)
                    {
                        route.AlternativeRouteTitles[i] = EditorGUILayout.DelayedTextField(route.AlternativeRouteTitles[i], GUILayout.Height(15));
                        GUILayout.Space(2);
                    }
                }
                DrawConditionCreator(selectedCharacter);

            }

            if (matchingSelectedNodeData.type == typeof(LinkNodeData))
            {
                var link = (LinkNodeData)matchingSelectedNodeData;

                DrawConditionCreator(selectedCharacter);

            }

            if (matchingSelectedNodeData.type == typeof(EndNodeData))
            {
                var end = (EndNodeData)matchingSelectedNodeData;

                DrawConditionCreator(selectedCharacter);

            }



            #endregion

            GUILayout.Space(10);

            #endregion



            //  matchingNodeDataSerializedObject.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();





        }

        private void DrawConditionCreator(Character character)
        {


            if (matchingReflectedData == null) return;

            if (ConditionSpecificSpaceing.Count != matchingReflectedData.Conditions.Count)
                ConditionSpecificSpaceing.Resize(matchingReflectedData.Conditions.Count);


            for (var c = 0; c < matchingReflectedData.Conditions.Count; c++)
            {
                var condition = matchingReflectedData.Conditions[c];

                var conditionSerializedObject = new SerializedObject(condition);
                conditionSerializedObject.Update();

                var area = EditorGUILayout.GetControlRect();
                // GUI.DrawTexture(area.AddRect(0,0,0,60), Textures.Gray);

                #region Backround UI
                var eventCount = matchingReflectedData.Conditions[c].targetEvent.GetPersistentEventCount();
                var countSpacing = eventCount < 2 ? 0 : ((eventCount - 1) * 43);
                /* var conditionBodyArea = area.ToUpperLeft(0, areaHeight + countSpacing);*/
                var conditionBodyArea = area.ToUpperLeft(0, 100 + ConditionSpecificSpaceing[c] + countSpacing);
                GUI.Box(conditionBodyArea, "", Theme.GameBridgeSkin.customStyles[2]);
                GUI.DrawTexture(area.ToUpperLeft(0, 3, 0, 15), Textures.DuskLighter);
                var conditionBodyFooter = conditionBodyArea.PlaceUnder(0, 5);
                GUI.Box(conditionBodyFooter, "", Theme.GameBridgeSkin.customStyles[3]);
                var buttonArea = conditionBodyFooter.ToLowerRight(55, 14, 0, 14);
                GUI.Box(buttonArea, "", Theme.GameBridgeSkin.customStyles[4]);

                var addConditionButtonArea = buttonArea.ToCenterLeft(8, 8, 10);
                if (ClickEvent.Click(4, addConditionButtonArea, ImageLibrary.addConditionIcon))
                {
                    var newCondition = new GameObject(matchingSelectedNodeData.name + "Condition " + matchingReflectedData.Conditions.Count);
                    newCondition.AddComponent<Condition>();//.self = newCondition;
                    var _condition = newCondition.GetComponent<Condition>();
                    _condition.CharacterGameObject = character.gameObject;
                    _condition.Self = newCondition;
                    newCondition.transform.SetParent(matchingReflectedData.transform);
                    // newCondition.hideFlags = HideFlags.HideInHierarchy;
                    //   matchingReflectedData.Conditions.Add(newCondition.GetComponent<Condition>());
                    matchingReflectedData.Conditions.Insert(c + 1, newCondition.GetComponent<Condition>());

                    ConditionSpecificSpaceing.Add(0);
                }

                var deleteConditionButtonArea = buttonArea.ToCenterRight(8, 8, -10);
                if (c != 0)
                    if (ClickEvent.Click(4, deleteConditionButtonArea, ImageLibrary.deleteConditionIcon))
                    {
                        DestroyImmediate(matchingReflectedData.Conditions[c].gameObject);
                        matchingReflectedData.Conditions.RemoveAt(c);
                        return;

                    }
                #endregion



                var autoStartInfo = condition.AutoStart ? "Auto Start Is On" : "Auto Start Is Off";
                if (GUILayout.Button(autoStartInfo, GUILayout.Height(15)))
                    condition.AutoStart = !condition.AutoStart;

                /*  if (GUILayout.Button("get condtion methods", GUILayout.Height(15)))
                  {
                      condition.processMethods();
                  }*/

                // starting out 4 pixels must be added to the layout height
                ConditionSpecificSpaceing[c] = 19;
                if (matchingSelectedNodeData.useTime)
                {
                    var useTimeInfo = condition.UseTime ? "Use Time Is On" : "Use Time Is Off";
                    if (GUILayout.Button(useTimeInfo, GUILayout.Height(15)))
                        condition.UseTime = !condition.UseTime;

                    // from here only 3 must be added to evey height value
                    ConditionSpecificSpaceing[c] = 172;
                }

                #region  

                // all the content inside here 2 added the y
                GUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();
                GUILayout.Label("If", GUILayout.Height(15));
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();



                //  condition.ComponentIndex = EditorGUILayout.Popup(condition.ComponentIndex,condition.Components, GUILayout.Height(15));


                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                condition.TargetGameObject = (GameObject)EditorGUILayout.ObjectField(condition.TargetGameObject, typeof(GameObject), true, GUILayout.Height(15));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (condition.cachedTargetObject != condition.TargetGameObject)
                {
                    condition.SetComponent(0);
                    condition.SetMethod(0);
                    condition.cachedTargetObject = condition.TargetGameObject;
                }


                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Box(ImageLibrary.DownFlowArrow, EditorStyles.label, GUILayout.Width(15), GUILayout.Height(15));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                var componentName = !condition.Components.Any() ? "None" : condition.Components[condition.ComponentIndex].GetType().ToString();
                var disabledComponents = condition.TargetGameObject == null;
                EditorGUI.BeginDisabledGroup(disabledComponents);


                if (GUILayout.Button(componentName, EditorStyles.popup, GUILayout.MinWidth(100), GUILayout.Height(15)))
                {

                    condition.GetGameObjectComponents();
                    var menu = new GenericMenu();
                    for (var i = 0; i < condition.Components.Length; i++)
                    {
                        menu.AddItem(new GUIContent(condition.Components[i].GetType().ToString()), condition.ComponentIndex.Equals(i), condition.SetComponent, i);
                    }
                    menu.ShowAsContext();
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Box(ImageLibrary.DownFlowArrow, EditorStyles.label, GUILayout.Width(15), GUILayout.Height(15));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                var methodName = !condition.serializedMethods.Any() ? "None" : condition.serializedMethods[condition.MethodIndex].methodName;
                var disabledMethods = condition.TargetGameObject == null;
                EditorGUI.BeginDisabledGroup(disabledMethods);


                if (GUILayout.Button(methodName, EditorStyles.popup, GUILayout.MinWidth(100), GUILayout.Height(15)))
                {

                    condition.GetComponentMethods();
                    var menu = new GenericMenu();
                    for (var i = 0; i < condition.serializedMethods.Length; i++)
                    {
                        menu.AddItem(new GUIContent(condition.serializedMethods[i].methodInfo.Name), condition.MethodIndex.Equals(i), condition.SetMethod, i);
                    }
                    menu.ShowAsContext();
                }
                EditorGUI.EndDisabledGroup();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Box(ImageLibrary.DownEqualSign, EditorStyles.label, GUILayout.Width(15), GUILayout.Height(15));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                var buttonState = condition.ObjectiveBool ? "True" : "False";
                if (GUILayout.Button(buttonState, GUILayout.Height(15)))
                    condition.ObjectiveBool = !condition.ObjectiveBool;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Box(ImageLibrary.DownFlowArrow, EditorStyles.label, GUILayout.Width(15), GUILayout.Height(15));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();



                if (matchingSelectedNodeData.useTime)
                    ConditionSpecificSpaceing[c] = 192;
                else
                    ConditionSpecificSpaceing[c] = 174;

                #endregion






                if (matchingSelectedNodeData.type != typeof(EnvironmentNodeData) && matchingSelectedNodeData.type != typeof(CharacterNodeData))
                {

                    EditorGUILayout.PropertyField(conditionSerializedObject.FindProperty("targetEvent"), new GUIContent(condition.name));

                    conditionSerializedObject.ApplyModifiedProperties();
                }

                GUILayout.Space(40);

            }

            // final spacing
            GUILayout.Space(5);





        }

        void Separator()
        {
            var area = GUILayoutUtility.GetLastRect().AddRect(-15, 0);

            GUI.DrawTexture(area.ToLowerLeft(Screen.width, 1), Textures.DuskLightest);

        }

        void Separator2()
        {
            var area = GUILayoutUtility.GetLastRect().AddRect(-15, 0);

            GUI.DrawTexture(area.ToLowerLeft(Screen.width * 70, 1), Textures.DuskLight);

        }

    }

}