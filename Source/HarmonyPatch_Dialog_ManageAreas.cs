// HarmonyPatch_Dialog_ManagerAreas.cs
// Copyright Karel Kroeze, 2018-2018

using System.Collections.Generic;
using System.Linq;
using ColourPicker;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AreaUnlocker {
    [HarmonyPatch(typeof(Dialog_ManageAreas), nameof(Dialog_ManageAreas.DoWindowContents))]
    public class HarmonyPatch_Dialog_ManageAreas {
        private static float height = 100f;
        private static Vector2 scrollposition = Vector2.zero;

        private static int reorderableGroup = -1;

        [System.Obsolete]
        public static bool Prefix(Rect inRect, Map ___map) {
            Rect outRect = new Rect( inRect.xMin, inRect.yMin, inRect.width, inRect.height - 100 );
            Rect viewRect = new Rect( inRect.xMin, inRect.yMin, inRect.width - 16f, height );
            Rect newAreaButtonRect = new Rect( inRect.xMin, inRect.yMax - 80f, inRect.width, 30f );

            List<Area> allAreas = ___map.areaManager.AllAreas.ToList();
            List<Area> areas = allAreas.Where(a => Visible( a ) ).ToList();

            Listing_Standard areaList = new Listing_Standard {ColumnWidth = viewRect.width};

            if (Event.current.type == EventType.Repaint) {
                reorderableGroup = ReorderableWidget.NewGroup( ( from, to ) =>
                {
                    from = Index( from, ___map );
                    to = Index( to, ___map );
                    ___map.areaManager.AllAreas.Insert( to, ___map.areaManager.AllAreas[from] );
                    ___map.areaManager.AllAreas.RemoveAt( from >= to ? from + 1 : from );
                }, ReorderableDirection.Vertical, viewRect );
            }

            Widgets.BeginScrollView(outRect, ref scrollposition, viewRect);
            areaList.Begin(viewRect);
            foreach (Area area in areas) {
                Rect row = areaList.GetRect( 24f );
                DoAreaRow(row, area, reorderableGroup);
                areaList.Gap(6f);
            }

            height = areas.Count * 30f;
            areaList.End();
            Widgets.EndScrollView();

            if (Widgets.ButtonText(newAreaButtonRect, "NewArea".Translate())) {
                ___map.areaManager.TryMakeNewAllowed(out Area_Allowed area_Allowed);
            }

            // we're done here.
            return false;
        }



        private static void ChangeColour(Area_Allowed area) {
            Find.WindowStack.Add(new Dialog_ColourPicker(area.Color, (colour) => {
                Traverse.Create(area).Field("colorInt").SetValue(colour);

                // we also need to clear the texture and material caches
                Traverse.Create(area).Field("colorTextureInt").SetValue(null);
                Traverse.Create(area).Field("drawer").SetValue(null);
            }));
        }

        private static bool Visible(Area area) {
            return area.Mutable || area is Area_Home;
        }

        private static int Index(int raw, Map map) {
            // loop over areas until we've seen rawIndex amount of visible areas, then return the current id.
            List<Area> areas = map.areaManager.AllAreas;
            int index = 0, visible = 0;
            while (visible < raw && index < areas.Count) {
                if (Visible(areas[index++])) {
                    visible++;
                }
            }

            return index;
        }

        private static void Copy(Area area) {
            // get a unique label
            TaggedString label = "Fluffy.AreaUnlocker.CopiedAreaLabel".Translate( area.Label );
            int i = 1;
            while (area.areaManager.GetLabeled(label) != null) {
                label = "Fluffy.AreaUnlocker.CopiedAreaDuplicateLabel".Translate(area.Label, i++);
            }

            // create the copy
            area.areaManager.TryMakeNewAllowed(out Area_Allowed copy);
            Traverse.Create(copy).Field("labelInt").SetValue(label.Resolve());
            foreach (IntVec3 cell in area.ActiveCells) {
                copy[cell] = true;
            }
        }

        public static void DoAreaRow(Rect rect, Area area, int reorderableWidgetId) {
            // Adapted from Dialog_ManagerAreas.DoAreaRow()
            if (Mouse.IsOver(rect)) {
                area.MarkForDraw();
                GUI.color = area.Color;
                Widgets.DrawHighlight(rect);
                GUI.color = Color.white;
            }

            Rect reorderableHandleRect = new Rect( rect.xMin, rect.yMin, 16, 16 ).CenteredOnYIn( rect );
            GUI.DrawTexture(reorderableHandleRect, Icons.DragHash);
            ReorderableWidget.Reorderable(reorderableWidgetId, rect);

            rect.xMin += 20;
            GUI.BeginGroup(rect);
            WidgetRow widgetRow = new WidgetRow( 0f, 0f );

            // note that in vanilla, the only mutable areas (those that can be edited by the player)
            // are all of type Area_Allowed, but this is not necessarily the case for modded areas.
            Area_Allowed area_allowed = area as Area_Allowed;
            bool mutable = area_allowed != null;

            if (mutable) {
                if (widgetRow.ButtonIcon(area_allowed.ColorTexture, "Fluffy.AreaUnlocker.ChangeColour".Translate(), GenUI.SubtleMouseoverColor)) {
                    ChangeColour(area_allowed);
                }
            } else {
                widgetRow.Icon(area.ColorTexture);
            }

            widgetRow.Gap(WidgetRow.DefaultGap);

            widgetRow.Label(area.Label, rect.width - ((mutable ? 6 : 3) * (WidgetRow.IconSize + WidgetRow.DefaultGap)));
            if (mutable && widgetRow.ButtonIcon(Icons.Rename, "Rename".Translate(), GenUI.MouseoverColor)) {
                Find.WindowStack.Add(new Dialog_RenameArea(area));
            }

            if (mutable && widgetRow.ButtonIcon(Icons.Palette, "Fluffy.AreaUnlocker.ChangeColour".Translate(), GenUI.MouseoverColor)) {
                ChangeColour(area_allowed);
            }

            if (widgetRow.ButtonIcon(Icons.Invert, "InvertArea".Translate(), GenUI.MouseoverColor)) {
                area.Invert();
            }

            if (widgetRow.ButtonIcon(Icons.Copy, "Fluffy.AreaUnlocker.CopyArea".Translate(), GenUI.MouseoverColor)) {
                Copy(area);
            }

            if (mutable && widgetRow.ButtonIcon(Icons.Delete, "Delete".Translate(), GenUI.MouseoverColor)) {
                area.Delete();
            }

            GUI.EndGroup();
        }
    }
}
