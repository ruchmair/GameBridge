﻿using System;
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
        private bool bl;

        public Component[] Components = new Component[0];
        public MethodInfo[] cacheMethods = new MethodInfo[0];
        public SerializableMethodInfo[] serializedMethods = new SerializableMethodInfo[0];

        public int ComponentIndex = 0;
        public int MethodIndex = 0;

        public delegate bool Del();
        private static Del theDelegate;

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



        public void GetGameObjectComponents()
        {
            Components = TargetGameObject.GetComponents(typeof(MonoBehaviour));// Component


        }

        public void SetComponent( object index)
        {
            ComponentIndex = (int)index;
        }


        public void GetComponentMethods()
        {

            cacheMethods = Type.GetType(Components[ComponentIndex].GetType().Name)
                        .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            serializedMethods = new SerializableMethodInfo[cacheMethods.Count()];
            for (int i = 0; i< cacheMethods.Count(); i++)
            {
                serializedMethods[i] = new SerializableMethodInfo(cacheMethods[i]);
            }

        }

        public void SetMethod(object index)
        {
            MethodIndex = (int)index;

        }



        public void ProcessConditionData()
        {

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




            if (TargetGameObject != null)
            {

                var comp = Components[ComponentIndex];

               
                if (theDelegate == null)
                theDelegate = (Del)Delegate.CreateDelegate(typeof(Del),comp, serializedMethods[MethodIndex].methodName);
                // theDelegate = (Del)Delegate.CreateDelegate(typeof(Del),comp, Methods[MethodIndex].Name);

                if (theDelegate() == ObjectiveBool)
                {
                    if (!Invoked)
                        targetEvent.Invoke();
                }

                /*   var bl = (bool)Methods[MethodIndex].Invoke(comp, null);
                   if (bl == ObjectiveBool)
                   {
                       if (!Invoked)
                           targetEvent.Invoke();
                   }*/


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
