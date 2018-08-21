// HarmonyPatch_Dialog_ManagerAreas.cs
// Copyright Karel Kroeze, 2018-2018

using System.Collections.Generic;
using System.Linq;
using ColourPicker;
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
        private static float height = 100f;
        private static Vector2 scrollposition = Vector2.zero;
        
        public static bool Prefix( Rect inRect, Map ___map )
        {
            Rect outRect = new Rect( inRect.xMin, inRect.yMin, inRect.width, inRect.height - 100 );
            Rect viewRect = new Rect( inRect.xMin, inRect.yMin, inRect.width - 16f, height );
            Rect newAreaButtonRect = new Rect( inRect.xMin, inRect.yMax - 80f, inRect.width, 30f );

            List<Area> allAreas = ___map.areaManager.AllAreas;
            var areas = allAreas.Where(a => a.Mutable).ToList();

            Listing_Standard areaList = new Listing_Standard {ColumnWidth = viewRect.width};

            Widgets.BeginScrollView( outRect, ref scrollposition, viewRect );
            areaList.Begin( viewRect );
            foreach ( var area in areas )
            {
                var row = areaList.GetRect( 24f ); 
                DoAreaRow( row, area );
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

        private static void ChangeColour( Area_Allowed area )
        {
            Find.WindowStack.Add( new Dialog_ColourPicker( area.Color, ( colour ) =>
                {
                    Traverse.Create( area ).Field( "colorInt" ).SetValue( colour );

                    // we also need to clear the texture and material caches
                    Traverse.Create( area ).Field( "colorTextureInt" ).SetValue( null );
                    Traverse.Create( area ).Field( "drawer" ).SetValue( null );
                } ) );
        }

        public static void DoAreaRow(Rect rect, Area area)
        {
            // Adapted from Dialog_ManagerAreas.DoAreaRow()
            if (Mouse.IsOver(rect))
            {
                area.MarkForDraw();
                GUI.color = area.Color;
                Widgets.DrawHighlight(rect);
                GUI.color = Color.white;
            }

            GUI.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow( 0f, 0f );

            // note that in vanilla, the only mutable areas (those that can be edited by the player)
            // are all of type Area_Allowed, but this is not necessarily the case for modded areas.
            var area_allowed = area as Area_Allowed;
            var colourable = area != null;

            if ( colourable )
            {
                if ( widgetRow.ButtonIcon( area.ColorTexture, "Fluffy.AreaUnlocker.ChangeColour".Translate(), GenUI.SubtleMouseoverColor ) )
                    ChangeColour( area_allowed );
            }
            else
                widgetRow.Icon( area.ColorTexture );

            widgetRow.Gap( WidgetRow.DefaultGap );

            widgetRow.Label( area.Label, rect.width - ( colourable ? 5 : 4 ) * ( WidgetRow.IconSize + WidgetRow.DefaultGap ) );
            if ( widgetRow.ButtonIcon( Icons.Rename, "Rename".Translate(), GenUI.MouseoverColor) )
                Find.WindowStack.Add(new Dialog_RenameArea(area));
            if ( colourable )
            {
                if ( widgetRow.ButtonIcon( Icons.Palette, "Fluffy.AreaUnlocker.ChangeColour".Translate(), GenUI.MouseoverColor ) )
                    ChangeColour( area_allowed );
            }
            if ( widgetRow.ButtonIcon( Icons.Invert, "InvertArea".Translate(), GenUI.MouseoverColor) )
                area.Invert();
            if ( widgetRow.ButtonIcon( Icons.Delete, "Delete".Translate(), GenUI.MouseoverColor ) )
                area.Delete();

            GUI.EndGroup();
        }
    }
}