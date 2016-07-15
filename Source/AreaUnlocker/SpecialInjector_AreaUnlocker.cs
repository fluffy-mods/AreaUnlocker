// AreaUnlocker/SpecialInjector_AreaUnlocker.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-25 10:55

using System;
using System.Reflection;
using CommunityCoreLibrary;
using RimWorld;
using Verse;

namespace AreaUnlocker
{
    public class DetourInjector : SpecialInjector
    {
        public override bool Inject()
        {

            MethodInfo source = typeof( AreaManager ).GetMethod( "CanMakeNewAllowed", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo destination = typeof( DetourInjector ).GetMethod( "CanMakeNewAllowed", BindingFlags.Instance | BindingFlags.Public );

            return Detours.TryDetourFromTo( source, destination );
        }

        public bool CanMakeNewAllowed( AreaManager _this, AllowedAreaMode mode ) { return true; }

    }
}