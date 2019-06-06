using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Reflection;
using Object = UnityEngine.Object;

namespace DaiMangou.BridgedData
{
    /// <summary>
    ///  the condition system allows foe any  accessible function to be called once a condition is met
    /// </summary>
    [Serializable]
    public class Condition : MonoBehaviour
    {
        /// <summary>
        /// This is the gameobject which this Condition Component is atached to
        /// </summary>
        public GameObject Self;

        #region Dialoguer Specific
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
        #endregion

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
        /// once this bool is equal to a bool value you decide upon the condition system will be activated
        /// </summary>
        public bool ObjectiveBool;
        /// <summary>
        /// This is the gameobject whose mono scrits we wish to analuze for public methods
        /// </summary>
        public GameObject TargetGameObject;
        /// <summary>
        /// helps to determine the state of the Condition editor (do not edit)
        /// </summary>
        public GameObject cachedTargetObject;
        /// <summary>
        /// this is an array of all the components on a TargetGameObject
        /// </summary>
        public Component[] Components = new Component[0];
        /// <summary>
        /// This is an array of all the public methods of a Component 
        /// </summary>
        public MethodInfo[] cacheMethods = new MethodInfo[0];
        /// <summary>
        /// here we use a speial class which allows us to Serialize methodInfo
        /// </summary>
        public SerializableMethodInfo[] serializedMethods = new SerializableMethodInfo[0];
        /// <summary>
        /// this is the index value of the component in the Components array
        /// </summary>
        public int ComponentIndex = 0;
        /// <summary>
        /// this is the index value of the target Method in the serializedMethods array
        /// </summary>
        public int MethodIndex = 0;
        /// <summary>
        /// delegate that we will use to create a delegate method
        /// </summary>
        /// <returns></returns>
        private delegate bool Del();
        /// <summary>
        /// he delegate method
        /// </summary>
        private  Del theDelegate;
        /// <summary>
        /// flag to check i the target event is invoked 
        /// </summary>
       // [NonSerialized]
        public bool Invoked;
        /// <summary>
        /// a flag to check if the target ent is to be invoked automatically onve a condition is met or if the nodedata is being processed
        /// </summary>
        public bool AutoStart = false;
        // public bool PlaySoundEffect;
        //  public bool PlayVoiceClip;
        /// <summary>
        /// if the node data uses time then the 
        /// </summary>
        public bool UseTime;
        /// <summary>
        /// this unity event only act as a proxy for an unity event in the ReflectedData
        /// </summary>
        public UnityEvent targetEvent = new UnityEvent();
        /// <summary>
        /// 
        /// </summary>
        private bool DelayTimerStarted;// not fully setup

        public void Awake()
        {

        }


        /// <summary>
        /// 
        /// </summary>
        public void GetGameObjectComponents()
        {
            Components = TargetGameObject.GetComponents(typeof(MonoBehaviour));// Component


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void SetComponent(object index)
        {
            ComponentIndex = (int)index;
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetComponentMethods()
        {

            cacheMethods = Type.GetType(Components[ComponentIndex].GetType().Name)
                        .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            serializedMethods = new SerializableMethodInfo[cacheMethods.Length];
            for (var i = 0; i < cacheMethods.Length; i++)
            {
                serializedMethods[i] = new SerializableMethodInfo(cacheMethods[i]);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void SetMethod(object index)
        {
            MethodIndex = (int)index;

        }


        /// <summary>
        /// 
        /// </summary>
        public void ProcessConditionData()
        {
            if (AutoStart)
            {
                if (!Invoked)
                {
                    targetEvent.Invoke();
                    Invoked = true;
                }
            }

            if (UseTime)
            {
                // setup the elapse timeer
                if (!DelayTimerStarted && !Invoked)
                {
                    DelayTimerStarted = true;
                    StartCoroutine(DelayTimer());
                }
            }

            /*  if (PlayVoiceClip)
              {
                  if (!Invoked)
                  {
                      StartPlayingVoiceClip();
                      Invoked = true;
                  }
              }

              if (PlaySoundEffect)
              {
                  if (!Invoked)
                  {
                      StatyPlayingPlaySoundEffect();
                      Invoked = true;
                  }
              }*/




            if (TargetGameObject != null)
            {

                var comp = Components[ComponentIndex];


                if (theDelegate == null)
                    theDelegate = (Del)Delegate.CreateDelegate(typeof(Del), comp, serializedMethods[MethodIndex].methodName);
                // theDelegate = (Del)Delegate.CreateDelegate(typeof(Del),comp, Methods[MethodIndex].Name);

                if (theDelegate() == ObjectiveBool)
                {
                    if (!Invoked)
                        targetEvent.Invoke();
                    Invoked = true;
                }

                /*   var bl = (bool)Methods[MethodIndex].Invoke(comp, null);
                   if (bl == ObjectiveBool)
                   {
                       if (!Invoked)
                           targetEvent.Invoke();
                   }*/


            }

        }
        /// <summary>
        /// 
        /// </summary>
        public void StartPlayingVoiceClip()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        public void StatyPlayingPlaySoundEffect()
        {

        }
        /// <summary>
        /// will be drawn out to show types of images to use and how to transition them, or have then sit in the scene and wait for triger (prefab sprite...)
        /// </summary>
        public void Displayimage()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IEnumerator DelayTimer()
        {
            if (dialoguer)
            {
                yield return new WaitForSeconds(dialoguer.ActiveNodeData.Delay);
                DelayTimerStarted = false;
                targetEvent.Invoke();
                Invoked = true;
            }
            else
            {
                yield return new WaitForSeconds(character.ActiveNodeData.Delay);
                DelayTimerStarted = false;
                targetEvent.Invoke();
                Invoked = true;
            }
        }

    }
}