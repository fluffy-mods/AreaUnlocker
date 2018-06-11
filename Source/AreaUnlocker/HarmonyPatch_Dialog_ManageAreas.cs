// HarmonyPatch_Dialog_ManagerAreas.cs
// Copyright Karel Kroeze, 2018-2018

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace AreaUnlocker
{
    [HarmonyPatch(typeof(Dialog_ManageAreas))]
    [HarmonyPatch("DoWindowContents")]
    public class HarmonyPatch_Dialog_ManageAreas
    {
        private static MethodInfo DoAreaRow_MI;
        private static float height = 100f;
        private static Vector2 scrollposition = Vector2.zero;

        public static void Prepare()
        {
            DoAreaRow_MI = AccessTools.Method( typeof( Dialog_ManageAreas ), "DoAreaRow" );
        }
        
        public static bool Prefix( Rect inRect, Map ___map )
        {
            Rect outRect = new Rect( inRect.xMin, inRect.yMin, inRect.width, inRect.height - 100 );
            Rect viewRect = new Rect( inRect.xMin, inRect.yMin, inRect.width - 16f, height );
            Rect newAreaButtonRect = new Rect( inRect.xMin, inRect.yMax - 80f, inRect.width, 30f );
            
            Listing_Standard areaList = new Listing_Standard();
            areaList.ColumnWidth = viewRect.width;
            Widgets.BeginScrollView( outRect, ref scrollposition, viewRect );
            areaList.Begin( viewRect );
            List<Area> allAreas = ___map.areaManager.AllAreas;
            var areas = allAreas.Where( a => a.Mutable ).ToList();
            foreach ( var area in areas )
            {
                var row = areaList.GetRect( 24f );
                DoAreaRow_MI.Invoke( null, new object[] { row, area } );
                areaList.Gap(6f);
            }

            height = areas.Count * 30f;
            areaList.End();
            Widgets.EndScrollView();
            
            if (Widgets.ButtonText( newAreaButtonRect, "NewArea".Translate() ) )
            {
                Area_Allowed area_Allowed;
                ___map.areaManager.TryMakeNewAllowed(out area_Allowed);
            }
            
            // we're done here.
            return false;
        }
    }
}