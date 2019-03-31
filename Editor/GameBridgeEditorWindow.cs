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

        [MenuItem("Assets/Create/Game Bridge/DialogueData")]
        public static void CreateDialogueData()
        {
            var asset = ScriptableObject.CreateInstance<DialogueData>();
            const string name = "New DialogueData";
            ProjectWindowUtil.CreateAsset(asset, name + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateDialogueData(string name, Dialoguer dialoguer)
        {
            if (!File.Exists(Application.dataPath + "/" + name + ".asset"))
            {
                var asset = ScriptableObject.CreateInstance<DialogueData>();
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
                    AssetDatabase.LoadAssetAtPath(finalPath + "/" + name + ".asset", typeof(DialogueData)) as DialogueData;

                dialoguer.dialogueData = newDialogueData;

                ProjectWindowUtil.ShowCreatedAsset(newDialogueData);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                var data = AssetDatabase.LoadAssetAtPath("Assets/" + name + ".asset", typeof(DialogueData)) as DialogueData;
                foreach (var d in data.FullCharacterDialogueSet)
                {
                    Object.DestroyImmediate(d, true);
                }
                data.ActiveCharacterDialogueSet = new List<NodeData>();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

        }

        private static void ClearDialogueData(Dialoguer dialoguer)
        {
            var data = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(dialoguer.dialogueData), typeof(DialogueData)) as DialogueData;
            foreach (var d in data.FullCharacterDialogueSet)
            {
                Object.DestroyImmediate(d, true);
            }
            data.ActiveCharacterDialogueSet = new List<NodeData>();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

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

            if (!_Selection())
            {
                EditID = 0;
            }
            else
            {

                if (_Selection().GetComponent<Character>() != null)
                {
                    SelectedCharacterData = _Selection().GetComponent<Character>();
                    EditID = 1;
                }
                else if (_Selection().GetComponent<Dialoguer>() != null)
                {
                    SelectedDialoguer = _Selection().GetComponent<Dialoguer>();
                    EditID = 2;
                }
                else
                {
                    EditID = 3;
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
                    #region Character
                    if (SelectedCharacterData.sceneID != -1)
                    {
                        var returnToScenSelectionArea = headerArea.ToCenterLeft(50, 30, 20);
                        GUI.Label(returnToScenSelectionArea, "Scenes", Theme.GameBridgeSkin.customStyles[0]);
                        if (ClickEvent.Click(1, returnToScenSelectionArea))
                            SelectedCharacterData.sceneID = -1;

                    }

                    if (SelectedCharacterData.sceneID == -1)
                    {
                        GUI.Label(headerArea.ToCenter(0, 30), story.name, Theme.GameBridgeSkin.customStyles[0]);

                        var newarea = windowArea.AddRect(0, 50, 0, -10);
                        Grid.BeginDynaicGuiGrid(newarea.AddRect(0, 20), scenesAreas, 20, 50, 65, 75, story.Scenes.Count);

                        scrollView = GUI.BeginScrollView(new Rect(0, 0, newarea.width, newarea.height), scrollView,
                            newarea.AddRect(0, 0, 0, Grid.AreaHeight));

                        var activeClipArea = new Rect(0, scrollView.y, windowArea.width, windowArea.height);
                        foreach (var area in scenesAreas.Select((v, i) => new { value = v, index = i }))
                            if (activeClipArea.Contains(area.value.TopLeft()) || activeClipArea.Contains(area.value.BottomRight()))
                            {

                                GUI.DrawTexture(area.value, ImageLibrary.sceneIcon);



                                var textArea = area.value.PlaceUnder(0, 20);

                                GUI.Label(textArea, story.Scenes[area.index].SceneName);

                                if (ClickEvent.Click(3, area.value))
                                    SelectedCharacterData.sceneID = area.index;


                            }

                        GUI.EndScrollView();

                        Grid.EndDynaicGuiGrid();

                    }
                    else
                    {

                        if (SelectedCharacterData.sceneID > story.Scenes.Count - 1)
                            SelectedCharacterData.sceneID -= 1;

                        var CharacterSelectionArea = new Rect(0, headerArea.height, Screen.width, Screen.height);
                        Grid.BeginDynaicGuiGrid(CharacterSelectionArea, _characterAreas, 50, 50, 60, 60, story.Scenes[SelectedCharacterData.sceneID].Characters.Count);
                        /*
                        for (var i = 0; i < _characterAreas.Count; i++)
                        {
                            var area = _characterAreas[i];
                            var character = story.Scenes[SelectedCharacterData.sceneID].Characters[i];

                            if (InfoBlock.Click(1, area,character.CharacterBios[character.BioID].CharacterImage  , SnapPosition.TopMiddle,Color.clear,InfoBlock.HoverEvent.None,Theme.GameBridgeSkin.customStyles[0],GUIStyle.none,character.Name))
                            {
                                character.AggregateForBridge();
                                SelectedCharacterData.characterImage = character.CharacterBios[character.BioID].CharacterImage;
                                SelectedCharacterData.age = character.CharacterBios[character.BioID].Age;
                                SelectedCharacterData.motivation = character.CharacterBios[character.BioID].Motivation;
                                SelectedCharacterData.backstory = character.CharacterBios[character.BioID].Backstory;
                                SelectedCharacterData.physicalAppearance = character.CharacterBios[character.BioID].PhysicalAppearance;
//SelectedCharacterData.birthDay = character.CharacterBio.BirthDay;
                              //  SelectedCharacterData.birthMonth = character.CharacterBio.BirthMonth;
                               // SelectedCharacterData.birthYear = character.CharacterBio.BirthYear;
                                SelectedCharacterData.name = SelectedCharacterData.characterName = character.Name;

                                switch (character.CharacterBios[character.BioID].CharacterSex)
                                {
                                    case Sex.Male:
                                        SelectedCharacterData.characterSex = Character.sex.Male;
                                        break;
                                    case Sex.Female:
                                        SelectedCharacterData.characterSex = Character.sex.Female;
                                        break;
                                    case Sex.Neither:
                                        SelectedCharacterData.characterSex = Character.sex.Neither;
                                        break;
                                    case Sex.Unknown:
                                        SelectedCharacterData.characterSex = Character.sex.Unknown;
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                                switch (character.CharacterBios[character.BioID].CharacterType)
                                {
                                    case CharacterType.Background:
                                        SelectedCharacterData.characterType = Character.CharacterType.Background;
                                        break;
                                    case CharacterType.Main:
                                        SelectedCharacterData.characterType = Character.CharacterType.Main;
                                        break;
                                    case CharacterType.Supporting:
                                        SelectedCharacterData.characterType = Character.CharacterType.Supporting;
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                                SelectedCharacterData.height = character.CharacterBios[character.BioID].Height;


                              //  SelectedCharacterData.voiceSamples = character.CharacterBios[character.BioID].VoiceSamples;

                                SelectedCharacterData.characterDataset.Resize(story.Scenes[SelectedCharacterData.sceneID].Characters[i].BridgeNodeChain.Count());


                                foreach (var node in story.Scenes[SelectedCharacterData.sceneID].Characters[i].BridgeNodeChain.Select((v, ind) => new { value = v, index = ind }))
                                {
                                    var type = node.value.GetType();

                                    if (type == typeof(DialogueNode))
                                    {
                                        var dialogue = (DialogueNode)node.value;
                                        SelectedCharacterData.characterDataset[node.index].delay = node.value.DelayTimeInSeconds;
                                        SelectedCharacterData.characterDataset[node.index].duration = node.value.TimeInSeconds;
                                        SelectedCharacterData.characterDataset[node.index].startTime = node.value.StartingTime;
                                        if (dialogue.StoryboardImage)
                                            SelectedCharacterData.characterDataset[i].storyboardImage = Sprite.Create(dialogue.StoryboardImage, new Rect(0, 0, dialogue.StoryboardImage.width, dialogue.StoryboardImage.height), new Vector2(dialogue.StoryboardImage.width / 2, dialogue.StoryboardImage.height / 2));

                                        if (dialogue.SoundEffect)
                                            SelectedCharacterData.characterDataset[node.index].soundEffect = dialogue.SoundEffect;

                                        SelectedCharacterData.characterDataset[node.index].textDialogue = node.value.Text;

                                    }
                                    else if (type == typeof(ActionNode))
                                    {
                                        var action = (ActionNode)node.value;
                                        SelectedCharacterData.characterDataset[node.index].delay = node.value.DelayTimeInSeconds;
                                        SelectedCharacterData.characterDataset[node.index].duration = node.value.TimeInSeconds;
                                        SelectedCharacterData.characterDataset[node.index].startTime = node.value.StartingTime;
                                        if (action.StoryboardImage)
                                            SelectedCharacterData.characterDataset[node.index].storyboardImage = Sprite.Create(action.StoryboardImage, new Rect(0, 0, action.StoryboardImage.width, action.StoryboardImage.height), new Vector2(action.StoryboardImage.width / 2, action.StoryboardImage.height / 2));

                                        if (action.SoundEffect)
                                            SelectedCharacterData.characterDataset[node.index].soundEffect = action.SoundEffect;

                                        SelectedCharacterData.characterDataset[node.index].actionName = action.ActionName;
                                        SelectedCharacterData.characterDataset[node.index].textAction = node.value.Text;
                                    }
                                }






                            }
                        }
                        */
                        Grid.EndDynaicGuiGrid();



                    }
                    #endregion

                    break;
                case 2:

                    #region Dialoguer interface

                    if (SelectedDialoguer.SceneID != -1)
                    {
                        GUI.Label(headerArea, "Push Data To Game Scene", Theme.GameBridgeSkin.customStyles[0]);

                    }

                    if (SelectedDialoguer.SceneID == -1)
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
                                    SelectedDialoguer.SceneID = area.index;


                            }
                        #endregion

                        GUI.EndScrollView();

                        Grid.EndDynaicGuiGrid();
                        #endregion
                    }

                    else
                    {
                        var activeDataPushArea = headerArea.PlaceUnder(0, ScreenRect.height - headerArea.height);


                        #region if the dialogue scene id is not zero. meaning that the users has selected a scene. then we show a ui which they can use to go back to selct a new sce. this mean seting the dialogue sceneID to -1

                        #endregion


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



                        #region here we allow the uses to push their selected scenes data over into the game bridge. how this works is further broken down in this region


                        var pushDataButtonArea = activeDataPushArea.ToCenterBottom(50, 20);

                        #region begin pushing storyteller data over into dialoguer
                        if (GUI.Button(pushDataButtonArea, "Push"))
                        {
                            if (SelectedDialoguer.dialogueData == null)
                            {
                                Debug.Log("please assign a Dialogue Data Object before pushing data");
                                return;
                            }

                            #region rename the selectedgameObject with Dialoguer script on it
                            _Selection().name = story.Scenes[SelectedDialoguer.SceneID].SceneName + " Dialoguer";
                            #endregion

                            #region find each character in the scene and aggregate the nodes in its chain. Rechaining 
                            foreach (var ch in story.Scenes[SelectedDialoguer.SceneID].Characters)
                            {
                                ch.AggregateNodesInChain();

                            }
                            #endregion

                            #region we create a new list which we will all nodes except Abstract and Media nodes in
                            var sortedList = new List<StoryElement>();
                            #endregion

                            #region iterating through all the nodes in the current storyteller scene
                            foreach (var el in story.Scenes[SelectedDialoguer.SceneID].NodeElements)
                            {
                                #region we only want Route nodes, Dialogue nodes and Action Nodes Added to the list
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
                            SelectedDialoguer.dialogueData.FullCharacterDialogueSet.Resize(sortedList.Count);
                            // also resize the RefelectedData list size 

                            //var updtereflectedData = 

                            if (SelectedDialoguer.ReflectedDataSet.Count == 0)
                            {
                                SelectedDialoguer.ReflectedDataSet.Resize(sortedList.Count);

                                SelectedDialoguer.ReflectedDataParent = new GameObject("Reflected Data");
                                SelectedDialoguer.ReflectedDataParent.transform.SetParent(SelectedDialoguer.transform);
                                SelectedDialoguer.ReflectedDataParent.transform.localPosition = Vector3.zero;
                                SelectedDialoguer.ReflectedDataParent.hideFlags = HideFlags.HideInHierarchy;


                                var AudioManager = new GameObject("Audio Manager");
                                AudioManager.transform.SetParent(SelectedDialoguer.transform);
                                AudioManager.transform.localPosition = Vector3.zero;

                                var TypingAudioManager = new GameObject("Typing");
                                TypingAudioManager.transform.SetParent(AudioManager.transform);
                                TypingAudioManager.transform.localPosition = Vector3.zero;
                                TypingAudioManager.AddComponent<AudioSource>();
                                SelectedDialoguer.TypingAudioSource = TypingAudioManager.GetComponent<AudioSource>();

                                var VoiceAudioManager = new GameObject("Voice");
                                VoiceAudioManager.transform.SetParent(AudioManager.transform);
                                VoiceAudioManager.transform.localPosition = Vector3.zero;
                                VoiceAudioManager.AddComponent<AudioSource>();
                                SelectedDialoguer.VoiceAudioSource = VoiceAudioManager.GetComponent<AudioSource>();

                                var SoundEffectsAudioManager = new GameObject("Sound Effects");
                                SoundEffectsAudioManager.transform.SetParent(AudioManager.transform);
                                SoundEffectsAudioManager.transform.localPosition = Vector3.zero;
                                SoundEffectsAudioManager.AddComponent<AudioSource>();
                                SelectedDialoguer.SoundEffectAudioSource = SoundEffectsAudioManager.GetComponent<AudioSource>();

                            }
                            else
                            {
                                SelectedDialoguer.TempReflectedDataSet = new List<ReflectedData>(); ;
                                foreach (var capturedData in SelectedDialoguer.ReflectedDataSet)
                                    SelectedDialoguer.TempReflectedDataSet.Add(capturedData);

                                SelectedDialoguer.ReflectedDataSet = new List<ReflectedData>();
                                SelectedDialoguer.ReflectedDataSet.Resize(sortedList.Count);

                            }



                            // loop through the sorted list
                            for (var i = 0; i < sortedList.Count; i++)
                            {
                                // do a assignment of a new block of character data to the dialogue set at i so to avoid a null reference exception when we fetch data during pushing data to the scene
                                // this is highly unlikely to be called
                                if (sortedList[i].CallingNode == null)
                                {
                                    Debug.LogWarning("please ensure that you have a character node starting each node chain");
                                    return;
                                }

                                #region create a new instance of ReflectedData as a gameObject and then assign the sortedList value at i (NodeElement id) to the reflected data ID
                                var newReflectedDatagameObject = new GameObject(sortedList[i].Name + "Reflected");
                                newReflectedDatagameObject.transform.SetParent(SelectedDialoguer.ReflectedDataParent.transform);
                                newReflectedDatagameObject.AddComponent<ReflectedData>();
                                var theReflectedDataComponent = newReflectedDatagameObject.GetComponent<ReflectedData>();
                                theReflectedDataComponent.DialoguerGameObject = SelectedDialoguer.gameObject;
                                theReflectedDataComponent.dialoguer = SelectedDialoguer;
                                theReflectedDataComponent.self = newReflectedDatagameObject;
                                SelectedDialoguer.ReflectedDataSet[i] = theReflectedDataComponent;
                                //  SelectedDialoguer.ReflectedDataSet[i].Id = sortedList[i].Id;
                                SelectedDialoguer.ReflectedDataSet[i].UID = sortedList[i].UID;
                                //    newReflectedDatagameObject.hideFlags = HideFlags.HideInHierarchy;
                                #endregion

                                #region Add the first conditin
                                var newCondition = new GameObject(newReflectedDatagameObject.name + "Condition " + theReflectedDataComponent.Conditions.Count);
                                newCondition.AddComponent<Condition>();
                                var _condition = newCondition.GetComponent<Condition>();
                                _condition.DialoguerGameObject = SelectedDialoguer.gameObject;
                                _condition.dialoguer = SelectedDialoguer;
                                _condition.Self = newCondition;
                                newCondition.transform.SetParent(newReflectedDatagameObject.transform);
                                // newCondition.hideFlags = HideFlags.HideInHierarchy;
                                theReflectedDataComponent.Conditions.Add(newCondition.GetComponent<Condition>());
                                #endregion




                                #region here we lookat each sorted list StoryElement and we pass their relevant data over into NodeData scriptable object data which had similar properties and names

                                if (sortedList[i].GetType() == typeof(CharacterNode))
                                {
                                    var character = (CharacterNode)sortedList[i];
                                    SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i] = ScriptableObject.CreateInstance(typeof(CharacterNodeData)) as NodeData;
                                    AssetDatabase.AddObjectToAsset(SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i], SelectedDialoguer.dialogueData);
                                    var imagePath = Application.dataPath;
                                    // File.WriteAllBytes(imagePath+"/" + character.CharacterBios[character.BioID].CharacterName+ ".png", character.CharacterBios[character.BioID].CharacterImage.EncodeToPNG());
                                    // var image = AssetDatabase.LoadAssetAtPath("Assets/" + character.CharacterBios[character.BioID].CharacterName+".png", typeof(Texture2D));
                                    /// AssetDatabase.AddObjectToAsset(image, SelectedDialoguer.dialogueData);
                                    // AssetDatabase.Refresh();
                                    SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i].hideFlags = HideFlags.HideInHierarchy;
                                    var data = (CharacterNodeData)SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i];
                                    //   data.DataID = character.Id;
                                    data.UID = character.UID;
                                    data.name = data.Name = character.CharacterBios[character.BioID].CharacterName;
                                    data.CharacterName = character.CharacterBios[character.BioID].CharacterName;

                                }

                                if (sortedList[i].GetType() == typeof(EnvironmentNode))
                                {
                                    var environment = (EnvironmentNode)sortedList[i];
                                    SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i] = ScriptableObject.CreateInstance(typeof(EnvironmentNodeData)) as NodeData;
                                    AssetDatabase.AddObjectToAsset(SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i], SelectedDialoguer.dialogueData);
                                    SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i].hideFlags = HideFlags.HideInHierarchy;
                                    var data = (EnvironmentNodeData)SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i];

                                    // data.DataID = environment.Id;
                                    data.UID = environment.UID;
                                    data.name = data.Name = environment.Name;
                                    data.CharacterName = environment.CallingNode.Name;

                                }

                                if (sortedList[i].GetType() == typeof(RouteNode))
                                {
                                    var route = (RouteNode)sortedList[i];
                                    SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i] = ScriptableObject.CreateInstance(typeof(RouteNodeData)) as NodeData;
                                    AssetDatabase.AddObjectToAsset(SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i], SelectedDialoguer.dialogueData);
                                    SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i].hideFlags = HideFlags.HideInHierarchy;
                                    var data = (RouteNodeData)SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i];
                                    // data.DataID = route.Id;
                                    data.UID = route.UID;
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
                                    SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i] = ScriptableObject.CreateInstance(typeof(LinkNodeData)) as NodeData;
                                    AssetDatabase.AddObjectToAsset(SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i], SelectedDialoguer.dialogueData);
                                    SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i].hideFlags = HideFlags.HideInHierarchy;
                                    var data = (LinkNodeData)SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i];
                                    //  data.DataID = link.Id;
                                    data.UID = link.UID;
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
                                    SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i] = ScriptableObject.CreateInstance(typeof(EndNodeData)) as NodeData;
                                    AssetDatabase.AddObjectToAsset(SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i], SelectedDialoguer.dialogueData);
                                    SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i].hideFlags = HideFlags.HideInHierarchy;
                                    var data = (EndNodeData)SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i];
                                    // data.DataID = end.Id;
                                    data.UID = end.UID;
                                    data.Pass = end.Pass;
                                    data.name = data.Name = end.Name;
                                    data.CharacterName = end.CallingNode.CharacterBios[end.CallingNode.BioID].CharacterName;
                                    if (end.Environment)
                                        data.EnvironmentName = end.Environment.Name;

                                }

                                if (sortedList[i].GetType() == typeof(ActionNode))
                                {
                                    var action = (ActionNode)sortedList[i];
                                    SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i] = ScriptableObject.CreateInstance(typeof(ActionNodeData)) as NodeData;
                                    AssetDatabase.AddObjectToAsset(SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i], SelectedDialoguer.dialogueData);
                                    SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i].hideFlags = HideFlags.HideInHierarchy;
                                    var data = (ActionNodeData)SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i];
                                    // data.DataID = action.Id;
                                    data.UID = action.UID;
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
                                    SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i] = ScriptableObject.CreateInstance(typeof(DialogueNodeData)) as NodeData;
                                    AssetDatabase.AddObjectToAsset(SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i], SelectedDialoguer.dialogueData);
                                    SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i].hideFlags = HideFlags.HideInHierarchy;
                                    var data = (DialogueNodeData)SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i];
                                    //  data.DataID = dialogue.Id;
                                    data.UID = dialogue.UID;
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

                                if (SelectedDialoguer.TempReflectedDataSet.Count != 0)
                                {
                                    foreach (var tempData in SelectedDialoguer.TempReflectedDataSet)
                                    {
                                        var data = SelectedDialoguer.ReflectedDataSet[i];
                                        if (sortedList[i].UID == tempData.UID)
                                        {
                                            if (tempData.UID == data.UID)
                                            {
                                                data.DialoguerGameObject = tempData.DialoguerGameObject;
                                                data.dialoguer = tempData.dialoguer;
                                                data.dialoguerComponent = tempData.dialoguerComponent;


                                                for (var c = 0; c < data.Conditions.Count; c++)
                                                {
                                                    var conditionToDelete = data.Conditions[c];
                                                    DestroyImmediate(conditionToDelete.Self);
                                                    data.Conditions.RemoveAt(c);
                                                }

                                                foreach (var condition in tempData.Conditions)
                                                {
                                                    condition.Self.transform.SetParent(data.self.transform);
                                                    data.Conditions.Add(condition);
                                                }


                                            }
                                        }
                                    }


                                }

                            }
                            foreach (var item in SelectedDialoguer.TempReflectedDataSet)
                            {
                                DestroyImmediate(item.self);
                            }
                            SelectedDialoguer.TempReflectedDataSet.RemoveAll(n => n == null);



                            #region we do the loop yet again , this time to pass in specific data to nodes that have properties that take in node data values like character nodes Nodes in its hain, link nodes linked nodes and route nodes routed nodes
                            // the process is quite efficient a node data in the sortedDataList and fullChracterDialogueSet have data with ID values that match.
                            for (var i = 0; i < sortedList.Count; i++)
                            {
                                if (sortedList[i].GetType() == typeof(CharacterNode))
                                {
                                    var character = (CharacterNode)sortedList[i];
                                    var data = (CharacterNodeData)SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i];
                                    SelectedDialoguer.Characters.Add(data);

                                    for (var n = 0; n < sortedList.Count; n++)
                                    {
                                        var node = sortedList[n];
                                        if (node.CallingNode == character && node != character)
                                        {

                                            data.NodeDataInMyChain.Add(SelectedDialoguer.dialogueData.FullCharacterDialogueSet.Find(c => c.UID == node.UID));
                                        }
                                    }
                                    //   data.NodeDataInMyChain = data.NodeDataInMyChain.OrderBy(m => m.StartTime).ToList();
                                    data.NodeDataInMyChain.All(a => a.CallingNodeData = data);


                                }

                                if (sortedList[i].GetType() == typeof(RouteNode))
                                {
                                    var route = (RouteNode)sortedList[i];
                                    var data = (RouteNodeData)SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i];

                                    if (route.LinkedRout != null)
                                    {
                                        var idOfLinkRoute = route.LinkedRout.UID;
                                        data.LinkedRoute = SelectedDialoguer.dialogueData.FullCharacterDialogueSet.Find(id => id.UID == idOfLinkRoute) as RouteNodeData;

                                        data.LinkedRoute.RoutesLinkedToMe.Add(data);
                                    }
                                }

                                if (sortedList[i].GetType() != typeof(LinkNode)) continue;
                                {
                                    var link = (LinkNode)sortedList[i];
                                    var data = (LinkNodeData)SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i];


                                    if (link.LoopRoute == null) continue;
                                    var idOfLinkRoute = link.LoopRoute.UID;
                                    data.loopRoute = SelectedDialoguer.dialogueData.FullCharacterDialogueSet.Find(id => id.UID == idOfLinkRoute) as RouteNodeData;
                                }
                            }
                            #endregion

                            #region now we use out matching IS's again to find out which nodes were connected in the SortedData list and we connect the nodes with matching IDs in the FullCharacterDataList with the same ID's
                            for (var i = 0; i < SelectedDialoguer.dialogueData.FullCharacterDialogueSet.Count; i++)
                            {
                                var data = SelectedDialoguer.dialogueData.FullCharacterDialogueSet[i];
                                var matchingStoryElement = story.Scenes[SelectedDialoguer.SceneID].NodeElements.Find(id => id.UID == data.UID);
                                data.DataIconnectedTo.Resize(matchingStoryElement.NodesIMadeConnectionsTo.Count);
                                data.DataConnectedToMe.Resize(matchingStoryElement.NodesThatMadeAConnectionToMe.Count);

                                // assign all the node data that the fullcharacterdataset element at i connected to
                                for (int d = 0; d < data.DataIconnectedTo.Count; d++)
                                {
                                    //   var iConnectedTo = matchingStoryElement.NodesIMadeConnectionsTo[d];
                                    //   if (iConnectedTo.GetType() == typeof(DialogueNode)|| iConnectedTo.GetType() == typeof(ActionNode) || iConnectedTo.GetType() == typeof(RouteNode)|| iConnectedTo.GetType() == typeof(LinkNode))

                                    data.DataIconnectedTo[d] = SelectedDialoguer.dialogueData.FullCharacterDialogueSet.Find(v => v.UID == matchingStoryElement.NodesIMadeConnectionsTo[d].UID);

                                }
                                // assign all the node data that are connected to the fullcharacterdataset element at i
                                for (var d = 0; d < data.DataConnectedToMe.Count; d++)
                                {
                                    data.DataConnectedToMe[d] = SelectedDialoguer.dialogueData.FullCharacterDialogueSet.Find(v => v.UID == matchingStoryElement.NodesThatMadeAConnectionToMe[d].UID);

                                }
                            }
                            #endregion

                            // lastly we order the list of NodeData by their startStime value, this is  necessary for when we populate the  ActiveCharacterDialogueSet
                            SelectedDialoguer.dialogueData.FullCharacterDialogueSet = SelectedDialoguer.dialogueData.FullCharacterDialogueSet.OrderBy(r => r.StartTime).ToList();

                        }
                        #endregion
                        #endregion
                    }

                    #endregion

                    break;
                case 3:

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

        public Character SelectedCharacterData;
        public Dialoguer SelectedDialoguer;
        internal bool M5_64_F1H = false;
        private int EditID;
        public List<Rect> scenesAreas = new List<Rect>();
        private List<Rect> _componentAreas = new List<Rect>();
        private List<Rect> _characterAreas = new List<Rect>();
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