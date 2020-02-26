// AreaUnlocker/SpecialInjector_AreaUnlocker.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-25 10:55

using System.Reflection;
using Harmony;
using Verse;

namespace AreaUnlocker
{
    public class Bootstrap : Mod
    {
        public Bootstrap( ModContentPack content ) : base( content )
        {
            var harmony = HarmonyInstance.Create( "fluffy.areaunlocker" );
            harmony.PatchAll( Assembly.GetExecutingAssembly() );
        }
    }
}