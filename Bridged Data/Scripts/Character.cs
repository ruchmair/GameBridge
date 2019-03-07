﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DaiMangou.BridgedData
{

    #region Personality traits

    #region positive traits

    [Serializable]
    public class PositiveTraits
    {
        public float Accessible;
        public float Active;
        public float Adaptable;
        public float Adventurous;
        public float Alert;
        public float Brilliant;
        public float Calm;
        public float Capable;
        public float Challenging;
        public float Charismatic;
        public float Clever;
        public float Confident;
        public float Cooperative;
        public float Courageous;
        public float Curious;
        public float Daring;
        public float Decisive;
        public float Discreet;
        public float Efficient;
        public float Energetic;
        public float Focused;
        public float Helpful;
        public float Independent;
        public float Individualistic;
        public float Innovative;
        public float Intelligent;
        public float Leaderly;
        public float Logical;
        public float Loyal;
        public float Methodical;
        public float Orderly;
        public float Organized;
        public float Patient;
        public float Patriotic;
        public float Peaceful;
        public float Reliable;
        public float Resourceful;
        public float Responsible;
        public float Responsive;
        public float Sane;
        public float Selfless;
        public float Serious;
        public float Skillful;
        public float Stoic;
        public float Strong;
    }

    #endregion

    #region neutral traits

    [Serializable]
    public class NeutralTraits
    {
        public float Aggressive;
        public float Deceptive;
        public float Dominating;
        public float Enigmatic;
        public float Experimental;
        public float Iconoclastic;
        public float Idiosyncratic;
        public float Impassive;
        public float Obedient;
        public float Progressive;
        public float Questioning;
        public float Reserved;
        public float Stubborn;
    }

    #endregion

    #region negative traits

    [Serializable]
    public class NegativeTraits
    {
        public float Angry;
        public float Arrogantt;
        public float Careless;
        public float Childish;
        public float Cowardly;
        public float Crafty;
        public float Crazy;
        public float Cruel;
        public float Dependent;
        public float Destructive;
        public float Devious;
        public float Disloyal;
        public float Disobedient;
        public float Disorderly;
        public float Disorganized;
        public float Fearful;
        public float Fickle;
        public float Foolish;
        public float Forgetful;
        public float Frightening;
        public float Frivolous;
        public float Graceless;
        public float Gullible;
        public float Hostile;
        public float Impractical;
        public float Irritable;
        public float Lazy;
        public float Meddlesome;
        public float Miserable;
        public float Neglectful;
        public float Opportunistic;
        public float Provocative;
        public float Slow;
        public float Timid;
        public float Uncooperative;
        public float Unreliable;
        public float Weak;
        public float WeakWilled;
    }

    #endregion

    #endregion

    #region Personality
    [Serializable]
    public class Personality
    {
        public NegativeTraits NegativeTraits = new NegativeTraits();

        public NeutralTraits NeutralTraits = new NeutralTraits();

        public PositiveTraits PositiveTraits = new PositiveTraits();
    }
    #endregion

    #region character type
    [Serializable]
    public enum CharacterType
    {
        Main = 0,
        Supporting = 1,
        Background = 2
        // ADD MORE
    }
    #endregion

    #region Voice Sample
    [Serializable]
    public class VoiceSample
    {
        public string SampleName = "";
        public AudioClip Sample;


    }
    #endregion

    #region Sex enum
    public enum Sex
    {
        Male,
        Female,
        Neither,
        Unknown
    }
    #endregion


    public class Character:MonoBehaviour
    {

        public string CharacterName = "No Name";

        public Sex CharacterSex;
        public CharacterType CharacterType;

        public int Age;

        public string Backstory = "No Backstory";
        public string Race = "Human";
        public string Height;
        public string Motivation = " No Motivation";
        public string PhysicalAppearance = "";
        public string CharacterImageFilePath = "";
        public string Birthday = "mm/dd/yy";
        public List<string> Family = new List<string>();
        public List<string> Friends = new List<string>();
        public List<VoiceSample> VoiceSamples = new List<VoiceSample>();
        public Personality Personality = new Personality();
        public Texture2D CharacterImage;
        public List<NodeData> FullCharacterNodeDataSet = new List<NodeData>();
        public int sceneID = -1;
        public List<NodeData> ActiveCharacterNodeDataSet = new List<NodeData>();



        public bool BeginActiveDialogueSetGeneration;

        IEnumerator Start()
        {
            while (true)
            {
                if (BeginActiveDialogueSetGeneration)
                {
                    yield return StartCoroutine(GenerateActiveDialogueSet());
                }


                yield return null;

            }
        }

        IEnumerator GenerateActiveDialogueSet()
        {

            ActiveCharacterNodeDataSet = new List<NodeData>();
            for (int i = 0; i < FullCharacterNodeDataSet.Count; i++)
            {
                var data = FullCharacterNodeDataSet[i];

                if (!data.Pass)
                {
                    ActiveCharacterNodeDataSet.Add(data);
                }
            }

            yield return null;
        }
    }



}
