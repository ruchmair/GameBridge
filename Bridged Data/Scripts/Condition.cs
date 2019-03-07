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
    [Serializable]
    public class Condition : MonoBehaviour
    {
        public GameObject Self;
        public GameObject DialoguerGameObject;
        // public string Name = "No Name";
        public bool ObjectiveBool;
        public GameObject TargetGameObject;
        // helps with setting first component data
        public GameObject cachedTargetObject;

        //  public SerializableMethodInfo[] SerializedMethods;

        /* public string path ="";
         public class ComponentAndMethods
         {
             public Component Component;
             public MethodInfo[] Methods;
             //public SerializableMethodInfo[] SerializedMethods;
         }*/
        // public ComponentAndMethods[] ComponentAndMethodsArray;


        //   public string[] StringComponents = new string[1] { "None"};
        //   public string[] StringMethods = new string[1] {"None" };

        //  public string TargetComponentName = "";
        //  public string TargetMethodName = "";

        //  public string[][] ComponentsAndMethodData;

        public Component[] Components = new Component[0];
        public MethodInfo[] Methods = new MethodInfo[0];

        public int ComponentIndex = 0;
        public int MethodIndex = 0;

        /*   [SerializeField]
           ReflectedData parent;
           public ReflectedData parentReflectedDataObject
           {
               get
               {
                   if(parent == null)
                   {
                       parent = transform.parent.GetComponent<ReflectedData>();
                   }

                   return parent;

               }
               set
               {
                   value = parent;
               }

           }*/
        public bool Invoked;
        public bool AutoStart = false;
        public bool PlaySoundEffect;
        public bool PlayVoiceClip;
        public bool UseTime;

        // this unity event only act as a proxy for an unity event in the ReflectedData
        public UnityEvent targetEvent = new UnityEvent();


        public void Awake()
        {

        }

       /* public void GetAllComponentsAndMethodData()
        {
            var components = TargetGameObject.GetComponents(typeof(Component));
            ComponentAndMethodsArray = new ComponentAndMethods[components.Count()];

            for (int i = 0; i < components.Count(); i++)
            {
                ComponentAndMethodsArray[i].Component = components[i];

                var methods = Type.GetType(ComponentAndMethodsArray[i].Component.GetType().ToString())// or components[i].GetType().ToString()
          .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                ComponentAndMethodsArray[i].Methods = new MethodInfo[methods.Count()];

                for(int m = 0; m < ComponentAndMethodsArray[i].Methods.Count(); m++)
                {
                    ComponentAndMethodsArray[i].Methods[m] = methods[m];
                }

            }


        }

        public void SetComponentAndMethodData(object compIndex, object methIndex)
        {
            ComponentIndex = (int)compIndex;
            MethodIndex = (int)methIndex;

            path = ComponentAndMethodsArray[ComponentIndex].GetType().ToString() + "." + ComponentAndMethodsArray[ComponentIndex].Methods[MethodIndex];

        }*/

        public void GetGameObjectComponents()
        {
            Components = TargetGameObject.GetComponents(typeof(Component));


        }

        public void SetComponent( object index)
        {
            ComponentIndex = (int)index;
        }


        public void GetComponentMethods()
        {
            Methods = Type.GetType(Components[ComponentIndex].GetType().Name)
                        .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        }

        public void SetMethod(object index)
        {
            MethodIndex = (int)index;
        }


        /*  public void processMethods()
        {
           Components = TargetGameObject.GetComponents(typeof(Component));
             for (int i = 0; i < Components.Count(); i++)
                 //  Debug.Log(components[i].GetType().ToString());

                 TargetComponentName = Components[1].GetType().ToString();

             Methods = Type.GetType(TargetComponentName)
                            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

             TargetMethodName = Methods[0].Name;


        }*/

        public void ProcessConditionData()
        {
            /*  if (ObjectiveBool)//targetedBool == 
              {
                  if(!Invoked)
                  targetEvent.Invoke();
              }*/

            if (AutoStart)
            {
                if (!Invoked)
                    targetEvent.Invoke();
            }

            if (UseTime)
            {
                // setup the elapse timeer
                targetEvent.Invoke();
            }

            if (PlayVoiceClip)
            {
                if (!Invoked)
                    StartPlayingVoiceClip();
            }

            if (PlaySoundEffect)
            {
                if (!Invoked)
                    StatyPlayingPlaySoundEffect();
            }


            /*  if (TargetMethodName != "")
              {
                  var typ = Type.GetType(TargetComponentName);
                  var comp = TargetGameObject.GetComponent(TargetComponentName);
                  if (Methods != null)
                  {
                     var bl =  (bool)Methods[0].Invoke(TargetGameObject.GetComponent(TargetComponentName), null);
                      if(bl == ObjectiveBool)
                      {
                          if (!Invoked)
                              targetEvent.Invoke();
                      }
                  }

              }*/
            /*  if (MethodIndex != -1)
              {

                  var comp = ComponentAndMethodsArray[ComponentIndex].Component;

                      var bl = (bool)ComponentAndMethodsArray[ComponentIndex].Methods[MethodIndex].Invoke(comp, null);
                      if (bl == ObjectiveBool)
                      {
                          if (!Invoked)
                              targetEvent.Invoke();
                      }


              }*/

            if (TargetGameObject != null)
            {

                var comp = Components[ComponentIndex];

                var bl = (bool)Methods[MethodIndex].Invoke(comp, null);
                if (bl == ObjectiveBool)
                {
                    if (!Invoked)
                        targetEvent.Invoke();
                }


            }

        }

        public void StartPlayingVoiceClip()
        {

        }

        public void StatyPlayingPlaySoundEffect()
        {

        }
        // will be drawn out to show types of images to use and how to transition them, or have then sit in the scene and wait for triger (prefab sprite...)
        public void Displayimage()
        {

        }


    }
}
