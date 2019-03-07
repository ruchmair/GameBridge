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

        public int RouteID = 0;
        public int TempRouteID = 0;
        public RouteNodeData LinkedRoute;
        public int AutoSwitchValue;

        public List<RouteNodeData> RoutesLinkedToMe = new List<RouteNodeData>();
        // a linked route will be in control of this Route

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

            if (TempRouteID != RouteID)
            {
                if (Dialoguer.ActiveEvents == -1)
                    Dialoguer.ActiveEvents = 0;

                Dialoguer.ActiveEvents += 1;

                TempRouteID = RouteID;
                Aggregate();
                


                foreach (var route in RoutesLinkedToMe)
                {
                    route.RouteID = RouteID;
                    route.ProcessData();
                }

                Dialoguer.ActiveEvents -= 1;
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
