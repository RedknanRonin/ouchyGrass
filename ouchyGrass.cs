using System;
using System.Collections.Generic;
using System.Reflection;
using Modding;
using UnityEngine;

namespace ouchyGrass
{
    public class ouchyGrassMod : Mod, IMenuMod
    {
        private static ouchyGrassMod? _instance;
        new public string GetName() => "OuchyGrass";
        public override string GetVersion() => "0.1";

        public bool damagePlayer => true;  // boolean that determines wether to damage or kill player when touching grass
        private FieldInfo isCutField;


        private bool instakill = false ;
        private bool invincibleGrass = false;  // variable that determines wether grass can be destroyed or not.
        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            return new List<IMenuMod.MenuEntry>
        {
            new IMenuMod.MenuEntry {
                Name = "Damage / instakill",
                Description = "Touching grass will deal damage or kill instantly.",
                Values = new string[] {
                    "Damage",
                    "Instakill"
                },
                // opt will be the index of the option that has been chosen
                Saver = opt => this.instakill = opt switch {
                    0 => false,
                    1 => true,
                    // This should never be called
                    _ => throw new InvalidOperationException()
                },
                Loader = () => this.instakill switch {
                    false => 0,
                    true => 1,
                }
            },
            new IMenuMod.MenuEntry {
                Name = "Invincible grass",
                Description ="Determines wether grass can be cut" ,
                Values = new string[] {
                    "Off",
                    "On"
                },
                Saver = opt => this.invincibleGrass = opt switch {
                    0 => false,
                    1 => true,
                    // This should never be called
                    _ => throw new InvalidOperationException()
                },
                Loader = () => this.invincibleGrass switch {
                    false => 0,
                    true => 1,
                }
            }
        };
        }


        internal static ouchyGrassMod Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException($"An instance of {nameof(ouchyGrassMod)} was never constructed");
                }
                return _instance;
            }
        }

        public bool ToggleButtonInsideMenu => throw new NotImplementedException();

        public override string GetVersion() => GetType().Assembly.GetName().Version.ToString();

        public ouchyGrassMod() : base("ouchyGrass")
        {
            _instance = this;
        }

        public override void Initialize()
        {
            On.GrassBehaviour.OnTriggerEnter2D += GrassReact_OnTriggerEnter2D;
            On.GrassCut.ShouldCut += GrassCut_ShouldCut;
            isCutField = typeof(GrassBehaviour).GetField("isCut", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        public void Unload()
        {
            On.GrassCut.ShouldCut -= GrassCut_ShouldCut;
            On.GrassBehaviour.OnTriggerEnter2D -= GrassReact_OnTriggerEnter2D;
        }

        // hook to determine grass indestructability
        private bool GrassCut_ShouldCut(On.GrassCut.orig_ShouldCut orig, Collider2D collision)
        {
            // Only allow grass cutting if this is true
            if (this.invincibleGrass)
            {
                return false;
            }

            // Otherwise use original logic
            return orig(collision);
        }

        private void GrassReact_OnTriggerEnter2D(On.GrassBehaviour.orig_OnTriggerEnter2D orig, GrassBehaviour self, Collider2D collision)
        {
            // Call original method first
            orig(self, collision);
            bool isCut = (bool)isCutField.GetValue(self);
            

            if (collision.tag == "Player" && !PlayerData.instance.isInvincible &&!isCut)
            {
                if (!this.instakill) 
                {
                    HeroController.instance.TakeDamage(HeroController.instance.gameObject, GlobalEnums.CollisionSide.other, 1, 2);
                }
                if (this.instakill)  // kills player
                {
                    HeroController.instance.TakeDamage(HeroController.instance.gameObject, GlobalEnums.CollisionSide.other, 100, 2);
                }
            }
              

                
            }
        }
    }

