// AreaUnlocker/SpecialInjector_AreaUnlocker.cs
// 
// Copyright Karel Kroeze, 2015.
// 
// Created 2015-11-25 10:55

using System;
using System.Reflection;
using RimWorld;
using Verse;

namespace AreaUnlocker
{
    public class Bootstrap : ITab
    {
        public bool NEW_AREA_ALLOWED_OVERRIDE = true;

        /// <summary>
        /// Edits assembly code to always return true when AreaManager.CanMakeNewAllowed() is called.
        /// I don't actually fully understand how this works, so for questions contact RawCode on the Ludeon forums.
        /// 
        /// All credits go to RawCode, who created this code for overriding combat slowdown;
        /// https://ludeon.com/forums/index.php?topic=16774.0
        /// </summary>
        public void OverrideMethod()
        {
            try
            {
                unsafe
                {
                    //getting pointer to method
                    //double check binding flags, in other case game will fail with NPE
                    byte* mpx_1 =
                        (byte*)
                            typeof (AreaManager).GetMethod( "CanMakeNewAllowed",
                                                            BindingFlags.Instance | BindingFlags.Public )
                                                .MethodHandle.GetFunctionPointer()
                                                .ToPointer();

                    //lazy init
                    int ttz = 0;

                    //read reference of our controller variable
                    fixed (bool* key = &NEW_AREA_ALLOWED_OVERRIDE)
                    {
                        //cast reference into int
                        ttz = (int)key;
                    }

                    //just debug
                    Log.Warning( ttz.ToString( "X2" ) );

                    //convert reference into byte array, this required to "reverse" bytes
                    byte[] bytes = BitConverter.GetBytes( ttz );

                    *( mpx_1 + 0 ) = 0xB8; //XOR MOV RAW
                    *( mpx_1 + 1 ) = bytes[0]; //RAW WORD
                    *( mpx_1 + 2 ) = bytes[1];
                    *( mpx_1 + 3 ) = bytes[2];
                    *( mpx_1 + 4 ) = bytes[3];

                    *( mpx_1 + 5 ) = 0x8b; //XOR DEREFERENCE
                    *( mpx_1 + 6 ) = 0; //into itself
                    *( mpx_1 + 7 ) = 0x90; //padding
                    *( mpx_1 + 8 ) = 0x90;

                    *( mpx_1 + 9 ) = 0xC3; //return
                }
            }
            catch
            {
                Log.Warning( "Area unlocker method override failed." );
            }
        }

        public Bootstrap()
        {
            LongEventHandler.ExecuteWhenFinished( delegate { OverrideMethod(); } );
        }

        protected override void FillTab()
        {
            // required implementation.
        }
    }
}