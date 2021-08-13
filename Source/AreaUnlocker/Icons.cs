using UnityEngine;
using Verse;

namespace AreaUnlocker {
    [StaticConstructorOnStartup]
    public static class Icons {
        public static readonly Texture2D Rename, Palette, Invert, Delete, Copy, DragHash;

        static Icons() {
            Rename = ContentFinder<Texture2D>.Get("UI/Buttons/Rename");
            Palette = ContentFinder<Texture2D>.Get("UI/Icons/palette");
            Invert = ContentFinder<Texture2D>.Get("UI/Icons/invert");
            Delete = ContentFinder<Texture2D>.Get("UI/Buttons/Delete");
            Copy = ContentFinder<Texture2D>.Get("UI/Buttons/Copy");
            DragHash = ContentFinder<Texture2D>.Get("UI/Icons/drag-handle");
        }
    }
}
