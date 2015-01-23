﻿﻿#region
using System;
using System.Collections;
using System.Linq;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using System.Collections.Generic;
using System.Threading;
#endregion

namespace Vayne
{


    internal class program
    {

        private const string Champion = "Vayne"; //Define the name of the champion which this assembly is gonna be used with.

        private static Orbwalking.Orbwalker Orbwalker; //Defining orbwalker (not much to code for this because its already made for you

        private static Spell Q; //Defining that Q is a spell

        private static Spell W; //Defining that W is a spell

        private static Spell E; //Defining that E is a spell

        private static Spell R; //Defining that R is a spell

        private static List<Spell> SpellList = new List<Spell>(); //Defining SpellList as an actual List

        private static Menu Config; //Defining Config as a menu

        private static Items.Item RDO; //Defning RDO as an item

        public static Obj_AI_Hero Player { get { return ObjectManager.Player; } } //Defining Player as Obj_AI_Hero just to save you typing


        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad; //Creates an event (don't know what this does exactly tell me and I'll add it.

        }


        static void Game_OnGameLoad(EventArgs args)
        {

            if (ObjectManager.Player.BaseSkinName != Champion) return; //If your champion name does not equals the champion you've defined at the top of the code
            //Stop the code. If you want to make an Utility assembly, remove this line.

            Q = new Spell(SpellSlot.Q, 300); //Making Q an actuall spell. You need to put the range of the spell here.
            W = new Spell(SpellSlot.W, 0); //Making W an actuall spell. You need to put the range of the spell here.
            E = new Spell(SpellSlot.E, 550); //Making E an actuall spell. You need to put the range of the spell here.
            R = new Spell(SpellSlot.R, 0); //Making R an actuall spell. You need to put the range of the spell here

            //SetSkillshot(Distancef, Width, Speed, Collission, Skillshot Type); This will be used for Spell Prediction.
            Q.SetSkillshot(0f, 0, 0, false, SkillshotType.SkillshotCone);
            W.SetSkillshot(0f, 0, 0, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0f, 0, 0, true, SkillshotType.SkillshotCircle);

            //Adds a spell to theSpellList, nothing much to explain here.
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            //RDO = new Items.Item(ID_OF_ITEM, RANGE_OF_ITEM don't forget the "f" behind the number because this is a float);
            RDO = new Items.Item(3143, 490f);


            //Creating a menu
            Config = new Menu("PGVayne", "Vayne", true);

            //Ts nothing for you to do here
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            //Orbwalk nothing for you to do here
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            //Combo Menu
            Config.AddSubMenu(new Menu("Combo", "Combo")); //Creating a submenu
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use Q")).SetValue(true); //Adding an item to the submenu (toggle)
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E")).SetValue(true);  //Adding an item to the submenu (toggle)
            Config.SubMenu("Combo").AddItem(new MenuItem("KSE", "KS with E")).SetValue(true);  //Adding an item to the submenu (toggle)
            Config.SubMenu("Combo").AddItem(new MenuItem("UseItems", "Use Items")).SetValue(true);  //Adding an item to the submenu (toggle)
            Config.SubMenu("Combo").AddItem(new MenuItem("ActiveCombo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));  //Adding an item to the submenu (on key down (hotkey))

            //Range Drawings same concept as the Combo menu
            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawEnable", "Enable Drawing"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawQ", "Draw Q")).SetValue(false);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawW", "Draw W")).SetValue(false);
            Config.SubMenu("Drawings").AddItem(new MenuItem("DrawE", "Draw E")).SetValue(true);
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleLag", "Lag Free Circles").SetValue(true));
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleQuality", "Circles Quality").SetValue(new Slider(100, 100, 10)));
            Config.SubMenu("Drawings").AddItem(new MenuItem("CircleThickness", "Circles Thickness").SetValue(new Slider(1, 10, 1)));

            Config.AddToMainMenu();

            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;

        }

        private static void OnDraw(EventArgs args)
        {
            throw new NotImplementedException();
        }

        private static void OnGameUpdate(EventArgs args)
        {




            if (Config.Item("ActiveCombo").GetValue<KeyBind>().Active) //If the key attached to ActiveCombo is held down then
            {
                Combo(); //Execute Combo()
            }

            if (Config.Item("KSE").GetValue<bool>()) //If Killsteal is toggled on in menu
            {
                KSE(); //Execute KSW()
            }


        }

        private static void Combo()
        {


            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical); //Getting a target
            if (target == null) return; //If there is no target, return.

            //Combo
            if (E.IsReady() && (Config.Item("UseECombo").GetValue<bool>())) //If W is ready & UseWCombo is held down then
            {
                var prediction = E.GetPrediction(target); //Create prediction based on W values and the targets movement

                //if the chance of hitting is high and if there are less then 2 minions inbetween you and the target then   
                if (prediction.Hitchance >= HitChance.High && prediction.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 2) //(if u want it to not do anything when there is a minion in between you and target make 2 to 0)
                {
                    E.Cast(prediction.CastPosition); //Cast W on the predicted targets on the predicted place

                }
            }
            if (target.IsValidTarget(E.Range) && E.IsReady() && (Config.Item("UseECombo").GetValue<bool>()))
            {
                E.Cast(target, true, true); //another method of prediction.
            }






            if (Config.Item("UseItems").GetValue<bool>()) //If the UseItems is toggled on then
            {
                if (Player.Distance3D(target) <= RDO.Range) //If the distance of the target is lower then the RDO item range
                {
                    RDO.Cast(target); //Cast RDO
                }

            }



        }


        private static void KSE() //This is a Killsteal feature
        {
            var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
            if (target == null) return;

            var prediction = E.GetPrediction(target);

            if (E.IsReady()) //If W is ready
            {

                if (target.Health < GetEDamage(target)) //If target's hp is lower then the damage of spell W. Its calling the function GetWDamge here which can be found below
                {
                    if (prediction.Hitchance >= HitChance.High && prediction.CollisionObjects.Count(h => h.IsEnemy && !h.IsDead && h is Obj_AI_Minion) < 1)
                    {
                        E.Cast(prediction.CastPosition);
                    }


                }
            }
        }


        private static float GetEDamage(Obj_AI_Base enemy) //Function of calculating the damage of W skill
        {
            double damage = 0d; //Defining damage as a double

            if (E.IsReady()) //Only calculate if W is 
                damage += Player.GetSpellDamage(enemy, SpellSlot.E);

            return (float)damage * 2; //return damage of W back to the function of KS W
        }











    }
}