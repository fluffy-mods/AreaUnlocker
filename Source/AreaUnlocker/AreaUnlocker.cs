// AreaUnlocker/SpecialInjector_AreaUnlocker.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-25 10:55

using System;
using System.Reflection;
using Harmony;
using RimWorld;
using Verse;

namespace AreaUnlocker
{
    [HarmonyPatch(typeof(AreaManager))]
    [HarmonyPatch("CanMakeNewAllowed")]
    public static class AreaUnlocker
    {
        static bool Prefix( ref bool __result )
        {
            __result = true;
            return false;
        }
    }

    public class Bootstrap : Mod
    {
        public Bootstrap( ModContentPack content ) : base( content )
        {
            var harmony = HarmonyInstance.Create( "fluffy.areaunlocker" );
            harmony.PatchAll( Assembly.GetExecutingAssembly() );
        }
    }
}