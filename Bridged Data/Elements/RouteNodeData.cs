using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DaiMangou.BridgedData
{

    [Serializable]
    public class RouteNodeData : NodeData
    {
        /// <summary>
        /// value representing the path which will b taken by the route
        /// </summary>
        public int RouteID = 0;
        /// <summary>
        /// we check against this value to see if the Route has beem changed
        /// </summary>
        public int TempRouteID = 0;
        /// <summary>
        /// this is the route node data that is linked to this route and controls this RouteID
        /// </summary>
        public RouteNodeData LinkedRoute;
        /// <summary>
        /// this is path the route node will default to
        /// </summary>
        public int AutoSwitchValue;
        /// <summary>
        /// all the route nodes connected to this route node, if any are conected then thrir route ID will be controlled by ths Route
        /// </summary>
        public List<RouteNodeData> RoutesLinkedToMe = new List<RouteNodeData>();
       

            // an alternative list of titles you can use insted of the default texts read from the route path text
        public List<string> AlternativeRouteTitles = new List<string>();
        public bool UseAlternativeRouteTitles;

        public override void OnEnable()
        {
            type = GetType(); 
            TempRouteID = RouteID;
            base.OnEnable();
        }

        public override void ProcessData()
        {            

            base.ProcessData();

            if(LinkedRoute)
            {
                /*// ensure that the Route id is never less than the number of route paths 
                if (DataIconnectedTo.Count < LinkedRoute.RouteID + 1)
                    RouteID = DataIconnectedTo.Count - 1;
                */

                // ensure that we can never select a route at an index greater then the number of rout paths we have
                if (DataIconnectedTo.Count >= LinkedRoute.RouteID + 1)
                    RouteID = LinkedRoute.RouteID;

            }

            // here we do a somple check to see if a route value is changed (a choice is made) so that we can trigger an Aggregation function
            if (TempRouteID != RouteID)
            {
                // set the static activevents value to 0 
                if (BridgeData.ActiveEvents == -1)
                    BridgeData.ActiveEvents = 0;

                // and now add one to the value
                BridgeData.ActiveEvents += 1;

                // now we must set the  TemprouteID to be the same asthe RouteID
                TempRouteID = RouteID;
                // trigger Aggregation
                Aggregate();
                

                /// and now , if there are any routes which are linked to this route then theor routes are also change d to match this route
                foreach (var route in RoutesLinkedToMe)
                {
                    route.RouteID = RouteID;
                    route.ProcessData();
                }

                // now that all the checks and setting are completed we then reset the ActiveEvents valiue to -1
                BridgeData.ActiveEvents -= 1;
            }

            
       

            // var route = (Route)this;
            /*   if (LinkedRoute != null)
               {
                   RouteID = LinkedRoute.RouteID;

               }*/
            // target routes that are connected to me and tell them to run process data


        }
    }
}
