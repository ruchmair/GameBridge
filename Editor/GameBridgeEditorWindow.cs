using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using DaiMangou.GameBridge;
using DaiMangou.Storyteller;
using DaiMangou.Storyteller.Elements;
using DaiMangou.BridgedData;
using System.IO;

using System.Globalization;
using ActionNodeData = DaiMangou.BridgedData.ActionNodeData;


namespace DaiMangou.GameBridgeEditor
{
    using Object = UnityEngine.Object;
    using Grid = DaiMangou.Storyteller.Grid;

    [Serializable]
    public class ComponentData
    {
        public Texture2D Icons;
        public string Description;
        public string Name;


        public ComponentData(Texture2D icon, string name, string description)
        {
            Icons = icon;
            Name = name;
            Description = description;

        }
    }

    [Serializable]
    public class PushSettings
    {
        public List<Rect> Areas = new List<Rect>();
        public bool PushSoundEffects = true;
        public bool PushVoiceOver = true;
        public bool PushStoryboardImage = false;

        public List<bool> pushOptions = new List<bool>();
        public List<string> optionNames = new List<string>();

        public bool OverrideAll;
        public bool UpdateText = true;
        public bool UpdateSoundEffects = true;
        public bool UpdateVoiceover = true;
        public bool UpdateCharacter = true;
        public bool UpdateEnvironment = true;
        public bool UpdateStoryboardImages = true;


        public PushSettings()
        {

            pushOptions.AddMany(
          OverrideAll,
          UpdateText,
          UpdateSoundEffects,
          UpdateVoiceover,
          UpdateCharacter,
          UpdateEnvironment,
          UpdateStoryboardImages);

            optionNames.AddMany(
          "Override All",
          "Text",
          "Sound Effects",
          "Voiceover",
          "Character",
          "Environment",
          "Storyboard ");


        }


    }

    [Serializable]
    public class GameBridgeEditorWindow : EditorWindow
    {


        /* void Create(string name, Dialoguer dialoguer)
         {
             var asset = ScriptableObject.CreateInstance<DialogueData>();
             ProjectWindowUtil.CreateAsset(asset, "Assets/" + name + ".asset");
             //  var newDialogueData = AssetDatabase.LoadAssetAtPath("Assets/" + name + ".asset", typeof(DialogueData)) as DialogueData;
             asset.name = name;

             dialoguer.dialogueData = asset;

             ProjectWindowUtil.ShowCreatedAsset(asset);

             AssetDatabase.SaveAssets();
             AssetDatabase.Refresh();
             Selection.activeObject = null;
         }*/

        private static string GetSelectedPathOrFallback()
        {
            var path = "Assets";

            foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) continue;
                path = Path.GetDirectoryName(path);
                break;
            }

            return path;
        }

        [MenuItem("Assets/Create/Game Bridge/SceneData")]
        public static void CreateSceneData()
        {
            var asset = ScriptableObject.CreateInstance<SceneData>();
            const string name = "New SceneData";
            ProjectWindowUtil.CreateAsset(asset, name + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateSceneData(string name, Dialoguer dialoguer)
        {
            if (!File.Exists(Application.dataPath + "/" + name + ".asset"))
            {
                var asset = ScriptableObject.CreateInstance<SceneData>();
                var path0 = GetSelectedPathOrFallback();
                var path = AssetDatabase.GetAssetPath(Selection.activeObject);
                var finalPath = path.Equals("") ? path0 : path;
                var i = 0;
                while (File.Exists(finalPath + "/" + name + ".asset"))
                {
                    i++;
                    name = name + i;
                }

                AssetDatabase.CreateAsset(asset, finalPath + "/" + name + ".asset");
                var newDialogueData =
                    AssetDatabase.LoadAssetAtPath(finalPath + "/" + name + ".asset", typeof(SceneData)) as SceneData;

                dialoguer.sceneData = newDialogueData;

                ProjectWindowUtil.ShowCreatedAsset(newDialogueData);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                var data = AssetDatabase.LoadAssetAtPath("Assets/" + name + ".asset", typeof(SceneData)) as SceneData;
                foreach (var d in data.FullCharacterDialogueSet)
                {
                    Object.DestroyImmediate(d, true);
                }
                data.ActiveCharacterDialogueSet = new List<NodeData>();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

        }

 

        /// <summary>
        /// UNUSED , would completely empty a dialogue data dataset
        /// </summary>
        /// <param name="dialoguer"></param>
      /*  private static void ClearDialogueData(Dialoguer dialoguer)
        {
            var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(dialoguer.dialogueData), typeof(DialogueData)) as DialogueData;
            foreach (var d in data.FullCharacterDialogueSet)
            {
                Object.DestroyImmediate(d, true);
            }
            data.ActiveCharacterDialogueSet = new List<NodeData>();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }*/

        public void OnEnable()
        {

            titleContent.text = "Bridge";
            minSize = new Vector2(275, 400);
            titleContent.image = ImageLibrary.GameBridgeIconSmall;
            M5_64_F1H = EditorPrefs.GetBool(i8.M5_64_F1H);


            //  Debug.Log(typeof(GameBridgeEditor).AssemblyQualifiedName);

            _componentData = new List<ComponentData>();
            _componentData.AddMany(
                new ComponentData(ImageLibrary.GBCharacterImage, "Character", "takes your character story into scene"),
                new ComponentData(ImageLibrary.GBDialogueImage, "Dialoguer", "Processes and handles dialogue display"));
        }

        private void CheckSelection()
        {
            if (CurrentStory.ActiveStory == null)
            {
                if (Selection.activeObject)
                    if (Selection.activeObject.GetType() == typeof(Story))
                    {
                        story = (Story)Selection.activeObject;
                    }
            }
            else
            {
                story = CurrentStory.ActiveStory;
            }

            if (!Selection.activeObject)
            {
                EditID = 0;
            }
            else
            {
                if (Selection.activeObject.GetType() == typeof(SceneData))
                {
                    SelectedSceneData = (SceneData)Selection.activeObject;
                    EditID = 1;
                }
                else
                {
                    EditID = 2;
                }
            }


        }

        public void OnGUI()
        {
            ScreenRect.size = position.size;

            Graphics.DrawTexture(ScreenRect, Textures.DuskLighter);
            GUI.skin = Theme.GameBridgeSkin;
            CheckSelection();


            if (story == null)
            {
                EditID = 0;
            }

            var windowArea = ScreenRect;
            var headerArea = windowArea.ToUpperLeft(0, 50);

            if (EditID != 0)
            {

                Graphics.DrawTexture(headerArea, Textures.DuskLight);
                Graphics.DrawTexture(headerArea.ToLowerLeft(0, 2), Textures.Blue);
            }

            if (!M5_64_F1H)
            {
                GUI.Label(ScreenRect.ToCenter(500, 20), "Please download the latest version of storyteller");
                return;

            }

            switch (EditID)
            {
                case 0:

                    //GUI.color = new Color(1, 1, 1,Mathf.Lerp(0,1,));
                    GUI.DrawTexture(ScreenRect.ToCenter(100, 100), ImageLibrary.GameBridgeIcon);

                    break;

                case 1:
                    #region Scene Data Setup interface

                    if (SelectedSceneData.SceneID != -1)
                    {
                        GUI.Label(headerArea, "Push Data To Game Scene", Theme.GameBridgeSkin.customStyles[0]);

                    }

                    if (SelectedSceneData.SceneID == -1)
                    {
                        #region we will prompt to uses to slect a scene , onve the scen is selected, we pass the scenes sceneid value to the dialogue scene id value.

                        GUI.Label(headerArea.ToCenter(0, 30), story.name, Theme.GameBridgeSkin.customStyles[0]);

                        var newarea = windowArea.AddRect(0, 50, 0, -10);
                        Grid.BeginDynaicGuiGrid(newarea.AddRect(0, 20), scenesAreas, 20, 50, 65, 75, story.Scenes.Count);

                        scrollView = GUI.BeginScrollView(new Rect(0, 0, newarea.width, newarea.height), scrollView,
                            newarea.AddRect(0, 0, 0, Grid.AreaHeight));

                        var activeClipArea = new Rect(0, scrollView.y, windowArea.width, windowArea.height);

                        #region we display all the scenes that can be uses and when the uses selects one, we pass hat scenes sceneId to the selectedDialiguers sceneId
                        foreach (var area in scenesAreas.Select((v, i) => new { value = v, index = i }))
                            if (activeClipArea.Contains(area.value.TopLeft()) || activeClipArea.Contains(area.value.BottomRight()))
                            {

                                GUI.DrawTexture(area.value, ImageLibrary.sceneIcon);



                                var textArea = area.value.PlaceUnder(0, 20);

                                GUI.Label(textArea, story.Scenes[area.index].SceneName);

                                if (ClickEvent.Click(3, area.value))
                                    SelectedSceneData.SceneID = area.index;


                            }
                        #endregion

                        GUI.EndScrollView();

                        Grid.EndDynaicGuiGrid();
                        #endregion
                    }

                    else
                    {
                        var activeDataPushArea = headerArea.PlaceUnder(0, ScreenRect.height - headerArea.height);


                        #region Push Data Settings

                        #region Overview Export Settings
                        var generalSettingsHeaderArea = activeDataPushArea.ToUpperLeft(0, 20);
                        GUI.DrawTexture(generalSettingsHeaderArea, Textures.DuskLight);
                        GUI.Label(generalSettingsHeaderArea, "General Settings");

                        var pushSoundEfectToggleArea = generalSettingsHeaderArea.PlaceUnder(0, 0, 0, 10);
                        pushSettings.PushSoundEffects = GUI.Toggle(pushSoundEfectToggleArea, pushSettings.PushSoundEffects, "Sound Effects");

                        var pushVoiceOverToggleArea = pushSoundEfectToggleArea.PlaceUnder(0, 0, 0, 10);
                        pushSettings.PushVoiceOver = GUI.Toggle(pushVoiceOverToggleArea, pushSettings.PushVoiceOver, "Voice Over");

                        var pushStoryboardImageToggleArea = pushVoiceOverToggleArea.PlaceUnder(0, 0, 0, 10);
                        pushSettings.PushStoryboardImage = GUI.Toggle(pushStoryboardImageToggleArea, pushSettings.PushStoryboardImage, "Storyboard Images");


                        #endregion

                        #region Push Data Update Setings

                        var dataUpdateSettingsAreaHeader = pushStoryboardImageToggleArea.PlaceUnder(0, 20);
                        GUI.DrawTexture(dataUpdateSettingsAreaHeader, Textures.DuskLight);
                        GUI.Label(dataUpdateSettingsAreaHeader, "Pushed Data Update Settings");

                        var dataUpdateSettingsArea = dataUpdateSettingsAreaHeader.PlaceUnder(0, 200);
                        GUI.DrawTexture(dataUpdateSettingsArea.PlaceUnder(0, 1), Textures.Blue);

                        Grid.BeginDynaicGuiGrid(dataUpdateSettingsArea, pushSettings.Areas, 5, 10, 100, 20, pushSettings.pushOptions.Count);

                        for (var i = 0; i < pushSettings.pushOptions.Count; i++)
                        {
                            var updateSettingArea = pushSettings.Areas[i];
                            pushSettings.pushOptions[i] = GUI.Toggle(updateSettingArea, pushSettings.pushOptions[i], pushSettings.optionNames[i]);
                        }

                        Grid.EndDynaicGuiGrid();

                        #endregion

                        #endregion


                        #region here we allow the uses to push their selected scenes data over into the DiaogueData asset. how this works is further broken down in this region


                        var pushDataButtonArea = activeDataPushArea.ToCenterBottom(50, 20);

                        #region begin pushing storyteller data over into dialogue Data Asset
                        if (GUI.Button(pushDataButtonArea, "Push"))
                        {
                            if (SelectedSceneData == null)
                            {
                                Debug.Log("please assign a Scene Data Object before pushing data");
                                return;
                            }

                            #region find each character in the scene and aggregate the nodes in its chain. Rechaining 
                            foreach (var ch in story.Scenes[SelectedSceneData.SceneID].Characters)
                            {
                                // this is not a critical step. it simply ensures that he data being pushed is not flawed. this is the same as refreshing the timeline
                                ch.AggregateNodesInChain();

                            }
                            #endregion

                            #region we create a new list which we will all nodes except Abstract and Media nodes in
                            var sortedList = new List<StoryElement>();
                            #endregion

                            #region iterating through all the nodes in the current storyteller scene
                            foreach (var el in story.Scenes[SelectedSceneData.SceneID].NodeElements)
                            {
                                #region we only want Character nodes, Link nodes, Route nodes, Dialogue nodes and Action Nodes Added to the list
                                if (el.GetType() != typeof(MediaNode) || el.GetType() != typeof(AbstractNode))
                                {
                                    #region prevent nodes that are not connected to a character from being added to the list
                                    if (el.CallingNode != null)
                                        sortedList.Add(el);
                                    #endregion

                                }
                                #endregion

                            }
                            #endregion

                            // increate the size of the list of node dataset to be the same as the sorted list
                            SelectedSceneData.FullCharacterDialogueSet.Resize(sortedList.Count);




                            SelectedSceneData.Characters = new List<CharacterNodeData>();

                            // loop through the sorted list
                            for (var i = 0; i < sortedList.Count; i++)
                            {



                                #region here we lookat each sorted list StoryElement and we pass their relevant data over into NodeData scriptable object data which had similar properties and names

                                if (sortedList[i].GetType() == typeof(CharacterNode))
                                {
                                    var character = (CharacterNode)sortedList[i];
                                    SelectedSceneData.FullCharacterDialogueSet[i] = ScriptableObject.CreateInstance(typeof(CharacterNodeData)) as NodeData;
                                    AssetDatabase.AddObjectToAsset(SelectedSceneData.FullCharacterDialogueSet[i], SelectedSceneData);
                                    var imagePath = Application.dataPath;
                                    // File.WriteAllBytes(imagePath+"/" + character.CharacterBios[character.BioID].CharacterName+ ".png", character.CharacterBios[character.BioID].CharacterImage.EncodeToPNG());
                                    // var image = AssetDatabase.LoadAssetAtPath("Assets/" + character.CharacterBios[character.BioID].CharacterName+".png", typeof(Texture2D));
                                    /// AssetDatabase.AddObjectToAsset(image, SelectedDialogueData);
                                    // AssetDatabase.Refresh();
                                    SelectedSceneData.FullCharacterDialogueSet[i].hideFlags = HideFlags.HideInHierarchy;
                                    var data = (CharacterNodeData)SelectedSceneData.FullCharacterDialogueSet[i];
                                    data.UID = character.UID;
                                    data.OverrideStartTime = character.OverrideStartTime;
                                    data.name = data.Name = character.CharacterBios[character.BioID].CharacterName;
                                    data.CharacterName = character.CharacterBios[character.BioID].CharacterName;
                                    SelectedSceneData.Characters.Add(data);

                                }

                                if (sortedList[i].GetType() == typeof(EnvironmentNode))
                                {
                                    var environment = (EnvironmentNode)sortedList[i];
                                    SelectedSceneData.FullCharacterDialogueSet[i] = ScriptableObject.CreateInstance(typeof(EnvironmentNodeData)) as NodeData;
                                    AssetDatabase.AddObjectToAsset(SelectedSceneData.FullCharacterDialogueSet[i], SelectedSceneData);
                                    SelectedSceneData.FullCharacterDialogueSet[i].hideFlags = HideFlags.HideInHierarchy;
                                    var data = (EnvironmentNodeData)SelectedSceneData.FullCharacterDialogueSet[i];
                                    data.UID = environment.UID;
                                    data.OverrideStartTime = environment.OverrideStartTime;
                                    data.name = data.Name = environment.Name;
                                    data.CharacterName = environment.CallingNode.Name;

                                }

                                if (sortedList[i].GetType() == typeof(RouteNode))
                                {
                                    var route = (RouteNode)sortedList[i];
                                    SelectedSceneData.FullCharacterDialogueSet[i] = ScriptableObject.CreateInstance(typeof(RouteNodeData)) as NodeData;
                                    AssetDatabase.AddObjectToAsset(SelectedSceneData.FullCharacterDialogueSet[i], SelectedSceneData);
                                    SelectedSceneData.FullCharacterDialogueSet[i].hideFlags = HideFlags.HideInHierarchy;
                                    var data = (RouteNodeData)SelectedSceneData.FullCharacterDialogueSet[i];
                                    data.UID = route.UID;
                                    data.OverrideStartTime = route.OverrideStartTime;
                                    data.DurationSum = route.NodeDurationSum;
                                    data.AutoSwitchValue = route.AutoSwitchValue;
                                    data.Pass = route.Pass;
                                    data.RouteID = route.RouteId;
                                    data.name = data.Name = route.Name;
                                    data.CharacterName = route.CallingNode.CharacterBios[route.CallingNode.BioID].CharacterName;
                                    if (route.Environment)
                                        data.EnvironmentName = route.Environment.Name;



                                }

                                if (sortedList[i].GetType() == typeof(LinkNode))
                                {
                                    var link = (LinkNode)sortedList[i];
                                    SelectedSceneData.FullCharacterDialogueSet[i] = ScriptableObject.CreateInstance(typeof(LinkNodeData)) as NodeData;
                                    AssetDatabase.AddObjectToAsset(SelectedSceneData.FullCharacterDialogueSet[i], SelectedSceneData);
                                    SelectedSceneData.FullCharacterDialogueSet[i].hideFlags = HideFlags.HideInHierarchy;
                                    var data = (LinkNodeData)SelectedSceneData.FullCharacterDialogueSet[i];
                                    data.UID = link.UID;
                                    data.OverrideStartTime = link.OverrideStartTime;
                                    data.LoopValue = link.LoopValue;
                                    data.name = data.Name = link.Name;
                                    data.Pass = link.Pass;
                                    data.Loop = link.Loop;
                                    data.CharacterName = link.CallingNode.CharacterBios[link.CallingNode.BioID].CharacterName;
                                    if (link.Environment)
                                        data.EnvironmentName = link.Environment.Name;

                                }

                                if (sortedList[i].GetType() == typeof(EndNode))
                                {
                                    var end = (EndNode)sortedList[i];
                                    SelectedSceneData.FullCharacterDialogueSet[i] = ScriptableObject.CreateInstance(typeof(EndNodeData)) as NodeData;
                                    AssetDatabase.AddObjectToAsset(SelectedSceneData.FullCharacterDialogueSet[i], SelectedSceneData);
                                    SelectedSceneData.FullCharacterDialogueSet[i].hideFlags = HideFlags.HideInHierarchy;
                                    var data = (EndNodeData)SelectedSceneData.FullCharacterDialogueSet[i];
                                    data.UID = end.UID;
                                    data.OverrideStartTime = end.OverrideStartTime;
                                    data.Pass = end.Pass;
                                    data.name = data.Name = end.Name;
                                    data.CharacterName = end.CallingNode.CharacterBios[end.CallingNode.BioID].CharacterName;
                                    if (end.Environment)
                                        data.EnvironmentName = end.Environment.Name;

                                }

                                if (sortedList[i].GetType() == typeof(ActionNode))
                                {
                                    var action = (ActionNode)sortedList[i];
                                    SelectedSceneData.FullCharacterDialogueSet[i] = ScriptableObject.CreateInstance(typeof(ActionNodeData)) as NodeData;
                                    AssetDatabase.AddObjectToAsset(SelectedSceneData.FullCharacterDialogueSet[i], SelectedSceneData);
                                    SelectedSceneData.FullCharacterDialogueSet[i].hideFlags = HideFlags.HideInHierarchy;
                                    var data = (ActionNodeData)SelectedSceneData.FullCharacterDialogueSet[i];
                                    data.UID = action.UID;
                                    data.OverrideStartTime = action.OverrideStartTime;
                                    data.Delay = action.DelayTimeInSeconds;
                                    data.Duration = action.TimeInSeconds;
                                    data.StartTime = action.StartingTime;
                                    data.DurationSum = action.NodeDurationSum;

                                    if (action.StoryboardImage)
                                        data.StoryboardImage = Sprite.Create(action.StoryboardImage, new Rect(0, 0, action.StoryboardImage.width, action.StoryboardImage.height), new Vector2(action.StoryboardImage.width / 2, action.StoryboardImage.height / 2));

                                    if (pushSettings.PushSoundEffects)
                                        data.SoundEffect = action.SoundEffect;

                                    data.Pass = action.Pass;
                                    data.ActionName = action.ActionName;
                                    data.name = data.Name = action.Name;
                                    data.Text = action.Text;
                                    data.Tag = action.Tag;
                                    data.CharacterName = action.CallingNode.CharacterBios[action.CallingNode.BioID].CharacterName;

                                    if (action.Environment)
                                        data.EnvironmentName = action.Environment.Name;
                                    //  data.EnvironmentLocation = action.Environment.EnvironmentInfo.Location;

                                }

                                if (sortedList[i].GetType() == typeof(DialogueNode))
                                {
                                    var dialogue = (DialogueNode)sortedList[i];
                                    SelectedSceneData.FullCharacterDialogueSet[i] = ScriptableObject.CreateInstance(typeof(DialogueNodeData)) as NodeData;
                                    AssetDatabase.AddObjectToAsset(SelectedSceneData.FullCharacterDialogueSet[i], SelectedSceneData);
                                    SelectedSceneData.FullCharacterDialogueSet[i].hideFlags = HideFlags.HideInHierarchy;
                                    var data = (DialogueNodeData)SelectedSceneData.FullCharacterDialogueSet[i];
                                    data.UID = dialogue.UID;
                                    data.OverrideStartTime = dialogue.OverrideStartTime;
                                    data.Delay = dialogue.DelayTimeInSeconds;
                                    data.Duration = dialogue.TimeInSeconds;
                                    data.StartTime = dialogue.StartingTime;
                                    data.DurationSum = dialogue.NodeDurationSum;

                                    if (dialogue.StoryboardImage)
                                        data.StoryboardImage = Sprite.Create(dialogue.StoryboardImage, new Rect(0, 0, dialogue.StoryboardImage.width, dialogue.StoryboardImage.height), new Vector2(dialogue.StoryboardImage.width / 2, dialogue.StoryboardImage.height / 2));

                                    if (pushSettings.PushSoundEffects)
                                        data.SoundEffect = dialogue.SoundEffect;

                                    if (pushSettings.UpdateVoiceover)
                                        data.VoicedDialogue = dialogue.VoiceRecording;

                                    //  if (pushSettings.PushStoryboardImage)
                                    //  data.StoryboardImage = dialogue.StoryboardImage;

                                    data.Pass = dialogue.Pass;
                                    data.name = data.Name = dialogue.Name;
                                    data.Text = dialogue.Text;
                                    data.Tag = dialogue.Tag;

                                    data.CharacterName = dialogue.CallingNode.CharacterBios[dialogue.CallingNode.BioID].CharacterName;
                                    if (dialogue.Environment)
                                        data.EnvironmentName = dialogue.Environment.Name;
                                }

                                #endregion



                            }

                            #region we do the loop yet again , this time to pass in specific data to nodes that have properties that take in node data values like character nodes Nodes in its hain, link nodes linked nodes and route nodes routed nodes
                            // the process is quite efficient a node data in the sortedDataList and fullChracterDialogueSet have data with UID values that match.
                            for (var i = 0; i < sortedList.Count; i++)
                            {
                                if (sortedList[i].GetType() == typeof(CharacterNode))
                                {
                                    var character = (CharacterNode)sortedList[i];
                                    var data = (CharacterNodeData)SelectedSceneData.FullCharacterDialogueSet[i];
                                    // selectedDialoguer.Characters.Add(data);

                                    for (var n = 0; n < sortedList.Count; n++)
                                    {
                                        var node = sortedList[n];
                                        if (node.CallingNode == character) //&& node != character
                                        {
                                            // we add to the NodeDataInMyChain  node data list, the node data in the NodeDataInMyChain that have matching UIDs to the 
                                            // nodes in the  sorted list  that have character as their calling node .
                                            // this could be done in another way.
                                            data.NodeDataInMyChain.Add(SelectedSceneData.FullCharacterDialogueSet.Find(c => c.UID == node.UID));
                                        }
                                    }
                                    //   data.NodeDataInMyChain = data.NodeDataInMyChain.OrderBy(m => m.StartTime).ToList();
                                    data.NodeDataInMyChain.All(a => a.CallingNodeData = data);


                                }

                                if (sortedList[i].GetType() == typeof(RouteNode))
                                {
                                    var route = (RouteNode)sortedList[i];
                                    var data = (RouteNodeData)SelectedSceneData.FullCharacterDialogueSet[i];

                                    if (route.LinkedRout != null)
                                    {
                                        var idOfLinkRoute = route.LinkedRout.UID;
                                        data.LinkedRoute = SelectedSceneData.FullCharacterDialogueSet.Find(id => id.UID == idOfLinkRoute) as RouteNodeData;

                                        data.LinkedRoute.RoutesLinkedToMe.Add(data);
                                    }
                                }

                                if (sortedList[i].GetType() != typeof(LinkNode)) continue;
                                {
                                    var link = (LinkNode)sortedList[i];
                                    var data = (LinkNodeData)SelectedSceneData.FullCharacterDialogueSet[i];


                                    if (link.LoopRoute == null) continue;
                                    var idOfLinkedLink = link.LoopRoute.UID;
                                    data.loopRoute = SelectedSceneData.FullCharacterDialogueSet.Find(id => id.UID == idOfLinkedLink) as RouteNodeData;
                                }
                            }
                            #endregion

                            #region now we use our matching UID's again to find out which nodes were connected in the SortedData list and we connect the nodes with matching UIDs in the FullCharacterDataList with the same UID's
                            for (var i = 0; i < SelectedSceneData.FullCharacterDialogueSet.Count; i++)
                            {
                                var data = SelectedSceneData.FullCharacterDialogueSet[i];
                                var matchingStoryElement = CurrentStory.ActiveStory.Scenes[SelectedSceneData.SceneID].NodeElements.Find(id => id.UID == data.UID);
                                data.DataIconnectedTo.Resize(matchingStoryElement.NodesIMadeConnectionsTo.Count);
                                data.DataConnectedToMe.Resize(matchingStoryElement.NodesThatMadeAConnectionToMe.Count);

                                // assign all the node data that the fullcharacterdataset element at i connected to
                                for (int d = 0; d < data.DataIconnectedTo.Count; d++)
                                {
                                    //   var iConnectedTo = matchingStoryElement.NodesIMadeConnectionsTo[d];
                                    //   if (iConnectedTo.GetType() == typeof(DialogueNode)|| iConnectedTo.GetType() == typeof(ActionNode) || iConnectedTo.GetType() == typeof(RouteNode)|| iConnectedTo.GetType() == typeof(LinkNode))

                                    data.DataIconnectedTo[d] = SelectedSceneData.FullCharacterDialogueSet.Find(v => v.UID == matchingStoryElement.NodesIMadeConnectionsTo[d].UID);

                                }
                                // assign all the node data that are connected to the fullcharacterdataset element at i
                                for (var d = 0; d < data.DataConnectedToMe.Count; d++)
                                {
                                    data.DataConnectedToMe[d] = SelectedSceneData.FullCharacterDialogueSet.Find(v => v.UID == matchingStoryElement.NodesThatMadeAConnectionToMe[d].UID);

                                }
                            }
                            #endregion

                            // lastly we order the list of NodeData by their startStime value, this is  necessary for when we populate the  ActiveCharacterDialogueSet
                            SelectedSceneData.FullCharacterDialogueSet = SelectedSceneData.FullCharacterDialogueSet.OrderBy(r => r.StartTime).ToList();


                        }
                        #endregion
                        #endregion
                    }

                    #endregion
                    break;
                case 2:

                    #region Component addition  

                    GUI.Label(headerArea.ToCenter(0, 30), story.name, Theme.GameBridgeSkin.customStyles[0]);

                    Grid.BeginDynaicGuiGrid(headerArea.PlaceUnder(0, Screen.height - headerArea.height, 0, 30), 1, 2, _componentAreas, 0, 130, 0, 70);

                    for (var a = 0; a < _componentAreas.Count; a++)
                    {
                        var area = _componentAreas[a];

                        GUI.DrawTexture(area, Textures.DuskLight);
                        if (InfoBlock.Click(1, area, _componentData[a].Icons, SnapPosition.TopMiddle, Color.clear, InfoBlock.HoverEvent.None, Theme.GameBridgeSkin.customStyles[0], Theme.GameBridgeSkin.customStyles[1], _componentData[a].Name, _componentData[a].Description, 0, 0, 180, 60))
                            AttachComponen(a);
                    }
                    
                    Grid.EndDynaicGuiGrid();


                    #endregion

                    break;
                default:
                    break;
            }

        }

        public void OnInspectorUpdate()
        {
            Repaint();

        }

        void AttachComponen(int a)
        {
            switch (a)
            {
                case 0:
                    _Selection().AddComponent<Character>();
                    break;
                case 1:
                    _Selection().AddComponent<Dialoguer>();
                    break;

            }
        }

        private int SelectionUI(int i, int Offset)
        {
            // var headerBarArea = ScreenRect()

            return i + 1;

        }

        protected void OverrideAll()
        {
            if (EditorUtility.DisplayDialog("Replace Data?",
"Are you sure you want to override the Current Dialoguers NodeData ?", "yes", "cancel"))
            {

            }
        }

        protected void UpdateText()
        {

        }

        protected void UpdateSoundEffects()
        {

        }

        protected void UpdateVoiceover()
        {

        }

        protected void UpdateChatacter()
        {

        }

        protected void UpdateEnvironment()
        {

        }

        protected void UpdateStoryboardImage()
        {

        }

        protected void UpdateNoeData()
        {

        }



        #region variables

        public SceneData SelectedSceneData;
        internal bool M5_64_F1H = false;
        private int EditID;
        public List<Rect> scenesAreas = new List<Rect>();
        private List<Rect> _componentAreas = new List<Rect>();

        private Vector2 scrollView;
      
        [SerializeField]
        private Story story;

        [SerializeField]
        private List<ComponentData> _componentData = new List<ComponentData>();

        private Rect ScreenRect = new Rect();

        public GameObject _Selection()
        {
            try
            {
                return Selection.activeGameObject;
            }
            catch
            {
                return null;
            }
        }

        public PushSettings pushSettings = new PushSettings();
        #endregion


    }

}