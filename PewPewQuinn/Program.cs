using System;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;
using SharpDX;
using Color = System.Drawing.Color;

namespace PewPewQuinn
{
    internal class Program
    {
        public const string ChampName = "Quinn";
        public static HpBarIndicator Hpi = new HpBarIndicator();
        public static Menu Config;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q;
        public static Spell E;
        public static Spell W;
        public static Spell R;
        public static int SpellRangeTick;
        private static SpellSlot Ignite;
        private static readonly Obj_AI_Hero player = ObjectManager.Player;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnLoad;
        
        }
        private static void OnLoad(EventArgs args)
        {
            if (player.ChampionName != ChampName)
                return;
            Notifications.AddNotification("PewPewQuinn Loaded!", 5000);

            Q = new Spell(SpellSlot.Q, 1010);
            E = new Spell(SpellSlot.E, 660);
            W = new Spell(SpellSlot.W, 1500);
            R = new Spell(SpellSlot.R, 550);

            Q.SetSkillshot(0.25f, 80f, 1150, true, SkillshotType.SkillshotLine);
            E.SetTargetted(0.25f, 2000f);


            Config = new Menu("PewPewQuinn", "Quinn", true);
            Orbwalker = new Orbwalking.Orbwalker(Config.AddSubMenu(new Menu("[PewPew]: Orbwalker", "Orbwalker")));
            TargetSelector.AddToMenu(Config.AddSubMenu(new Menu("[PewPew]: Target Selector", "Target Selector")));

            //COMBOMENU

            var combo = Config.AddSubMenu(new Menu("[PewPew]: Combo Settings", "Combo Settings"));
            var harass = Config.AddSubMenu(new Menu("[PewPew]: Harass Settings", "Harass Settings"));
            var drawing = Config.AddSubMenu(new Menu("[PewPew]: Draw Settings", "Draw"));

            combo.SubMenu("[SBTW] ManaManager").AddItem(new MenuItem("qmana", "[Q] Mana %").SetValue(new Slider(20, 100, 0)));
            combo.SubMenu("[SBTW] ManaManager").AddItem(new MenuItem("emana", "[E] Mana %").SetValue(new Slider(10, 100, 0)));
            combo.SubMenu("[SBTW] ManaManager").AddItem(new MenuItem("rmana", "[R] Mana %").SetValue(new Slider(35, 100, 0)));
            combo.AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            combo.AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            combo.SubMenu("[R] Settings").AddItem(new MenuItem("UseRD", "Use Dynamic R Combo").SetValue(true));
            combo.SubMenu("[R] Settings").AddItem(new MenuItem("enear", "Enemy Count").SetValue(new Slider(2, 5, 1)));
            combo.SubMenu("[R] Settings").AddItem(new MenuItem("rturret", "Don't RE into Turret Range").SetValue(true));
            combo.SubMenu("[W] Settings").AddItem(new MenuItem("autoW", "Use W to reveal enemies").SetValue(true));
            combo.SubMenu("[E] Settings").AddItem(new MenuItem("emgapcloser", "Use E minion gapclose").SetValue(false));
            combo.SubMenu("[E] Settings").AddItem(new MenuItem("donteinturret", "Don't E minion gapclose into turret range").SetValue(true));
            combo.SubMenu("[E] Settings").AddItem(new MenuItem("UseEC", "Only use E if target gets too close").SetValue(false));
            combo.SubMenu("[E] Settings").AddItem(new MenuItem("UseECs", "Target Distance").SetValue(new Slider(150, 650, 50)));



            combo.SubMenu("Item Settings").AddItem(new MenuItem("useGhostblade", "Use Youmuu's Ghostblade").SetValue(true));
            combo.SubMenu("Item Settings").AddItem(new MenuItem("UseBOTRK", "Use Blade of the Ruined King").SetValue(true));
            combo.SubMenu("Item Settings")
                .AddItem(new MenuItem("eL", "  Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));
            combo.SubMenu("Item Settings")
                .AddItem(new MenuItem("oL", "  Own HP Percentage").SetValue(new Slider(65, 100, 0)));
            combo.SubMenu("Item Settings").AddItem(new MenuItem("UseBilge", "Use Bilgewater Cutlass").SetValue(true));
            combo.SubMenu("Item Settings")
                .AddItem(new MenuItem("HLe", "  Enemy HP Percentage").SetValue(new Slider(80, 100, 0)));
            combo.SubMenu("Summoner Settings").AddItem(new MenuItem("UseIgnite", "Use Ignite").SetValue(true));

            //LANECLEARMENU
            Config.SubMenu("[PewPew]: Laneclear Settings")
                .AddItem(new MenuItem("laneQ", "Use Q").SetValue(true));
            Config.SubMenu("[PewPew]: Laneclear Settings").AddItem(new MenuItem("playerlevel", "Don't use abilities till level").SetValue(new Slider(12, 18, 0)));



            Config.SubMenu("[PewPew]: Laneclear Settings")
                .AddItem(new MenuItem("laneclearmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));

            //JUNGLEFARMMENU
            Config.SubMenu("[PewPew]: Jungle Settings")
                .AddItem(new MenuItem("jungleQ", "Use Q").SetValue(true));
            Config.SubMenu("[PewPew]: Laneclear Settings").SubMenu("[Q] Settings").AddItem(new MenuItem("laneQhit", "Q Hitcount").SetValue(new Slider(3, 10, 0)));
            Config.SubMenu("[PewPew]: Jungle Settings")
                .AddItem(new MenuItem("jungleclearmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));

            drawing.AddItem(new MenuItem("Draw_Disabled", "Disable All Spell Drawings").SetValue(false));
            drawing.AddItem(new MenuItem("minionespots", "Draw Minion E Gapclose Positions").SetValue(false));
            drawing.AddItem(new MenuItem("Qdraw", "Draw Q Range").SetValue(new Circle(true, Color.Orange)));
            drawing.AddItem(new MenuItem("Edraw", "Draw E Range").SetValue(new Circle(true, Color.AntiqueWhite)));
            drawing.AddItem(new MenuItem("Rdraw", "Draw R Range").SetValue(new Circle(true, Color.LawnGreen)));
            drawing.AddItem(new MenuItem("ECdraw", "Only use E if target is Close Range").SetValue(new Circle(true, Color.DarkRed)));
            drawing.AddItem(new MenuItem("CircleThickness", "Circle Thickness").SetValue(new Slider(7, 30, 0)));

            harass.AddItem(new MenuItem("AutoHarass", "AutoHarass (TOGGLE)").SetValue(new KeyBind('L', KeyBindType.Toggle)));
            harass.AddItem(new MenuItem("harassQ", "Use Q").SetValue(true));
            harass.AddItem(new MenuItem("harassE", "Use E").SetValue(true));
            harass.AddItem(new MenuItem("harassmana", "Mana Percentage").SetValue(new Slider(30, 100, 0)));
            Config.SubMenu("[PewPew]: Misc Settings").AddItem(new MenuItem("DrawD", "Damage Indicator").SetValue(true));
            Config.SubMenu("[PewPew]: Misc Settings").AddItem(new MenuItem("interrupt", "Interrupt Spells").SetValue(true));
            Config.SubMenu("[PewPew]: Misc Settings").AddItem(new MenuItem("antigap", "AntiGapCloser").SetValue(true));
            Config.SubMenu("[PewPew]: Misc Settings").AddItem(new MenuItem("AntiRengar", "Anti-Rengar Leap").SetValue(true));
            Config.SubMenu("[PewPew]: Misc Settings").AddItem(new MenuItem("AntiKhazix", "Anti-Khazix Leap").SetValue(true));

            Config.AddItem(new MenuItem("PewPew", "            PewPew Prediction Settings"));

            Config.AddItem(new MenuItem("HitchanceQ", "[Q] Hitchance").SetValue(
                    new StringList(
                        new[] { HitChance.Low.ToString(), HitChance.Medium.ToString(), HitChance.High.ToString(), HitChance.VeryHigh.ToString() }, 3)));

            Config.AddToMainMenu();

            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Drawing.OnDraw += Drawings;
            Drawing.OnEndScene += OnEndScene;
            GameObject.OnCreate += GameObject_OnCreate;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapCloser_OnEnemyGapcloser;
            QuinnRanges();
        }
        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {

            var rengar = HeroManager.Enemies.Find(h => h.ChampionName.Equals("Rengar")); //<---- Credits to Asuna (Couldn't figure out how to cast R to Sender so I looked at his vayne ^^
            if (rengar != null)

                if (sender.Name == ("Rengar_LeapSound.troy") && Config.Item("AntiRengar").GetValue<bool>() &&
                    sender.Position.Distance(player.Position) < E.Range)
                    E.Cast(rengar);

            var khazix = HeroManager.Enemies.Find(h => h.ChampionName.Equals("Khazix"));
            if (khazix != null)

                if (sender.Name == ("Khazix_Base_E_Tar.troy") && Config.Item("AntiKhazix").GetValue<bool>() &&
                   sender.Position.Distance(player.Position) < E.Range)
                    E.Cast(khazix);

        }
        private static void Drawings(EventArgs args)
        {
            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;

            var orbwalktarget = Orbwalker.GetTarget();
            if (orbwalktarget.IsValidTarget())
                Render.Circle.DrawCircle(orbwalktarget.Position, 80, System.Drawing.Color.Orange);

            if (Config.Item("Draw_Disabled").GetValue<bool>())
                return;

            if (Config.Item("Qdraw").GetValue<Circle>().Active)
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Config.Item("Qdraw").GetValue<Circle>().Color : Color.Red,
                                                        Config.Item("CircleThickness").GetValue<Slider>().Value);

            if (Config.Item("Edraw").GetValue<Circle>().Active)
                if (E.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range - 1,
                        E.IsReady() ? Config.Item("Edraw").GetValue<Circle>().Color : Color.Red,
                                                        Config.Item("CircleThickness").GetValue<Slider>().Value);

            if (Config.Item("Rdraw").GetValue<Circle>().Active)
                if (R.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, 1600,
                        R.IsReady() ? Config.Item("Rdraw").GetValue<Circle>().Color : Color.Red,
                                                        Config.Item("CircleThickness").GetValue<Slider>().Value);


            if (Config.Item("Rdraw").GetValue<Circle>().Active)
                if (E.Level > 0)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Config.Item("UseECs").GetValue<Slider>().Value,
                        R.IsReady() ? Config.Item("ECdraw").GetValue<Circle>().Color : Color.Red,
                                                        Config.Item("CircleThickness").GetValue<Slider>().Value);


            var pos = Drawing.WorldToScreen(ObjectManager.Player.Position);
            if (Config.Item("AutoHarass").GetValue<KeyBind>().Active)
                Drawing.DrawText(pos.X - 50, pos.Y + 30, System.Drawing.Color.Plum, "AutoHarass Enabled");

            foreach (var minion in
                ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget() && minion.IsEnemy &&
                                                                   minion.Distance(player.ServerPosition) <=
                                                                   E.Range))
            {
                var ecastpos = player.ServerPosition.Extend(minion.Position,
                    player.ServerPosition.Distance(minion.Position) -
                    (Orbwalking.GetRealAutoAttackRange(player) + 35 - player.Distance(minion.Position)));

                if (Config.Item("minionespots").GetValue<bool>())
                Render.Circle.DrawCircle(ecastpos, 15, System.Drawing.Color.Green, 20);
            }
        }

        private static void QuinnRanges()
        {
            
            {
                if (player.HasBuff("quinnrtimeout") || player.HasBuff("QuinnRForm"))
                    Q.Range = 200;
                    E.Range = 650;
            }
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {

            if (E.IsReady() && sender.IsValidTarget(E.Range) && Config.Item("interrupt").GetValue<bool>())
                E.CastOnUnit(sender);
        }

        private static void AntiGapCloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (E.IsReady() && gapcloser.Sender.IsValidTarget(E.Range) && Config.Item("antigap").GetValue<bool>())
                E.CastOnUnit(gapcloser.Sender);
        }

        private static void OnEndScene(EventArgs args)
        {
            {
                //Damage Indicator
                if (Config.SubMenu("[PewPew]: Misc Settings").Item("DrawD").GetValue<bool>())
                {
                    foreach (var enemy in
                        ObjectManager.Get<Obj_AI_Hero>().Where(ene => !ene.IsDead && ene.IsEnemy && ene.IsVisible))
                    {
                        Hpi.unit = enemy;
                        Hpi.drawDmg(CalcDamage(enemy), Color.Green);
                    }
                }
            }
        }


        private static void Game_OnGameUpdate(EventArgs args)
        {

            switch (Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    combo1();
                    elogic();
                    items();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    Laneclear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    Lasthit();
                    break;
            }

            if (Config.Item("AutoHarass").GetValue<KeyBind>().Active)
                harass();
            if (Config.Item("autoW").GetValue<bool>())
                Wlogic();

        }
        private static HitChance PewPewPredQ(string name)
        {
            var qpred = Config.Item(name).GetValue<StringList>();
            switch (qpred.SList[qpred.SelectedIndex])
            {
                case "Low":
                    return HitChance.Low;
                case "Medium":
                    return HitChance.Medium;
                case "High":
                    return HitChance.High;
                case "Very High":
                    return HitChance.VeryHigh;
            }
            return HitChance.VeryHigh;
        }

        private static void Lasthit()
        {
            var AA = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(player));
            foreach (var minion in AA)
                if (minion.HasBuff("QuinnW") && minion.Health < player.CalcDamage(minion, Damage.DamageType.Physical,
                    15 + (player.Level * 10) + (player.FlatPhysicalDamageMod * 0.5) + player.GetAutoAttackDamage(minion)))
                {
                    Orbwalker.ForceTarget(minion);
                    player.IssueOrder(GameObjectOrder.AutoAttack, minion);
                }
        }
        private static void Laneclear()
        {
            var AA = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(player));
            var AAj = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(player), MinionTypes.All, MinionTeam.Neutral);

            var lanemana = Config.Item("laneclearmana").GetValue<Slider>().Value;
            var junglemana = Config.Item("jungleclearmana").GetValue<Slider>().Value;
            var jungleQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width,
                MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var laneQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width);
         
            var Qjunglepos = Q.GetLineFarmLocation(jungleQ, Q.Width + 30);

            var Qfarmpos = Q.GetCircularFarmLocation(laneQ, Q.Width + 30);


            foreach (var minion in jungleQ)
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Qjunglepos.MinionsHit >= 1 &&
                Config.Item("jungleQ").GetValue<bool>()
                && player.ManaPercent>= junglemana)
            
                Q.Cast(minion);

            foreach (var minion in AAj)

                if (minion.HasBuff("QuinnW"))
                {
                    Orbwalker.ForceTarget(minion);
                    player.IssueOrder(GameObjectOrder.AutoAttack, minion);
                }

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear && Qfarmpos.MinionsHit >= Config.Item("laneQhit").GetValue<Slider>().Value &&
            Config.Item("laneQ").GetValue<bool>()
            && player.ManaPercent >= lanemana)

                Q.Cast(Qfarmpos.Position);

            foreach (var minion in AA)
                if (minion.HasBuff("QuinnW") && minion.Health < player.CalcDamage(minion, Damage.DamageType.Physical,
                    15 + (player.Level*10) + (player.FlatPhysicalDamageMod*0.5) + player.GetAutoAttackDamage(minion)))
                {
                    Orbwalker.ForceTarget(minion);
                    player.IssueOrder(GameObjectOrder.AutoAttack, minion);
                }
        }
        private static void harass()
        {
            var harassmana = Config.Item("harassmana").GetValue<Slider>().Value;
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValidTarget())
                return;
            var qpred = Q.GetPrediction(target, true);

            if (Q.IsReady() && qpred.Hitchance >= PewPewPredQ("HitchanceQ") && Config.Item("harassQ").GetValue<bool>() &&
                target.IsValidTarget(Q.Range) &&
                player.ManaPercent >= harassmana)
                Q.Cast(target);

            if (E.IsReady() && target.HasBuff("QuinnW"))
                player.IssueOrder(GameObjectOrder.AutoAttack, target);

            if (E.IsReady() && target.HasBuff("QuinnW"))
                return;

            if (E.IsReady() && Config.Item("harassE").GetValue<bool>() && target.IsValidTarget(E.Range) &&
                player.ManaPercent >= harassmana)
            {
                E.CastOnUnit(target);
                player.IssueOrder(GameObjectOrder.AutoAttack, target);
            }
        }
        private static float IgniteDamage(Obj_AI_Hero target)
        {
            if (Ignite == SpellSlot.Unknown || player.Spellbook.CanUseSpell(Ignite) != SpellState.Ready)
            return 0f;
            return (float)player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
        }

        private static void items()
        {

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValidTarget())
                return;

            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            var Ghost = ItemData.Youmuus_Ghostblade.GetItem();
            var cutlass = ItemData.Bilgewater_Cutlass.GetItem();

            if (botrk.IsReady() && botrk.IsOwned(player) && botrk.IsInRange(target)
            && target.HealthPercent <= Config.Item("eL").GetValue<Slider>().Value
            && Config.Item("UseBOTRK").GetValue<bool>())

                botrk.Cast(target);

            if (botrk.IsReady() && botrk.IsOwned(player) && botrk.IsInRange(target)
                && player.HealthPercent <= Config.Item("oL").GetValue<Slider>().Value
                && Config.Item("UseBOTRK").GetValue<bool>())

                botrk.Cast(target);

            if (cutlass.IsReady() && cutlass.IsOwned(player) && cutlass.IsInRange(target) &&
                target.HealthPercent <= Config.Item("HLe").GetValue<Slider>().Value
                && Config.Item("UseBilge").GetValue<bool>())

                cutlass.Cast(target);

            if (Ghost.IsReady() && Ghost.IsOwned(player) && target.IsValidTarget(E.Range)            
                && Config.Item("useGhostblade").GetValue<bool>())

                Ghost.Cast();

            if (player.Distance(target) <= 600 && IgniteDamage(target) >= target.Health &&
                Config.Item("UseIgnite").GetValue<bool>())
                player.Spellbook.CastSpell(Ignite, target);
        }
        private static void combo1()
        {
            var qmana = Config.Item("qmana").GetValue<Slider>().Value;

            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValidTarget())
                return;
            
            var qpred = Q.GetPrediction(target, true);
            Ignite = player.GetSpellSlot("summonerdot");

            if (R.IsReady() && target.IsValidTarget(1200) && Config.Item("UseRD").GetValue<bool>() && CalcDamage(target) >= target.Health - 20*player.Level)
                rlogic();

            if (player.HasBuff("quinnrtimeout") || player.HasBuff("QuinnRForm"))
                ASMode();

            if (player.HasBuff("quinnrtimeout") || player.HasBuff("QuinnRForm"))
                return;

            if (Q.IsReady() && target.IsValidTarget(Q.Range) && qpred.Hitchance >= PewPewPredQ("HitchanceQ") && Config.Item("UseQ").GetValue<bool>() &&
                player.ManaPercent >= qmana)
                Q.Cast(target);


        }

        private static void elogic()
        {
            var emana = Config.Item("emana").GetValue<Slider>().Value;
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValidTarget())
                return;

            if (target.HasBuff("QuinnW"))
                player.IssueOrder(GameObjectOrder.AutoAttack, target);

            if (target.HasBuff("QuinnW"))
                return;

            if (Config.Item("UseEC").GetValue<bool>() && E.IsReady() && target.IsValidTarget(Config.Item("UseECs").GetValue<Slider>().Value) &&
                player.ManaPercent >= emana)
                E.CastOnUnit(target);

            if (Config.Item("UseEC").GetValue<bool>())
                return;

            if (player.IsWindingUp)
                return;

            if (Config.Item("UseE").GetValue<bool>() && E.IsReady() &&
               player.ManaPercent >= emana)
                E.CastOnUnit(target);

            //ACROBAT QUINN MUTHAFUKKA ayylmao kek.
            foreach (var minion in
                ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget() && minion.IsEnemy &&
                                                                   minion.Distance(player.ServerPosition) <=
                                                                   E.Range))
            {
                if (player.HasBuff("quinnrtimeout") || player.HasBuff("QuinnRForm"))
                    return;
                var ecastpos = player.ServerPosition.Extend(minion.Position,
                    player.ServerPosition.Distance(minion.Position) - (Orbwalking.GetRealAutoAttackRange(player) - player.Distance(minion.Position)));

                if (Config.Item("donteinturret").GetValue<bool>() && ecastpos.UnderTurret(true))
                    return;

                if (ecastpos.Distance(target.Position) < Orbwalking.GetRealAutoAttackRange(player)
                    && target.Distance(player.Position) > Orbwalking.GetRealAutoAttackRange(player) && target.Health < CalcDamage(target) && Config.Item("emgapcloser").GetValue<bool>() && !player.IsFacing(minion))
                    E.Cast(minion);
            }



        }

        private static void Wlogic()
        {
            if (W.IsReady() && Config.Item("AutoW").GetValue<bool>() && AnyEnemyInBush())
                W.Cast();
        }

         
         private static bool AnyEnemyInBush()
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsValid && hero.IsEnemy && hero.IsVisible && !hero.IsDead)
                {
                    if (NavMesh.IsWallOfGrass(hero.ServerPosition,10) &&
                        ObjectManager.Player.ServerPosition.Distance(hero.ServerPosition) < 650)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        

        private static void rlogic()
        {
            var rmana = Config.Item("rmana").GetValue<Slider>().Value;
            var target = TargetSelector.GetTarget(1600, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValidTarget())
                return;
            if (target.Position.UnderTurret(true) && Config.Item("rturret").GetValue<bool>())
                return;

            if (R.IsReady() && (Config.Item("UseRD").GetValue<bool>()))
                ASMode();

            if (player.HasBuff("quinnrtimeout") || player.HasBuff("QuinnRForm"))
                ASMode();

            if (player.HasBuff("quinnrtimeout") || player.HasBuff("QuinnRForm"))
                return;

            if (E.IsReady() && R.IsReady() && player.Distance(target) < 1600
                && target.CountEnemiesInRange(600) <= Config.Item("enear").GetValue<Slider>().Value && target.Health < CalcDamage(target) &&
                player.ManaPercent >= rmana)
                R.Cast();
        }

        private static void ASMode()
        {
            var botrk = ItemData.Blade_of_the_Ruined_King.GetItem();
            var Ghost = ItemData.Youmuus_Ghostblade.GetItem();
            var cutlass = ItemData.Bilgewater_Cutlass.GetItem();
            Ignite = player.GetSpellSlot("summonerdot");


            var target = TargetSelector.GetTarget(1200, TargetSelector.DamageType.Physical);
            if (target == null || !target.IsValidTarget())
                return;
            var ultfinisher = player.CalcDamage(target, Damage.DamageType.Physical,
                (75 + (R.Level*55) + (player.FlatPhysicalDamageMod*0.5))*(2 - (target.Health/target.MaxHealth)));
            var ultignite = IgniteDamage(target) +player.CalcDamage(target, Damage.DamageType.Physical,
                (75 + (R.Level * 55) + (player.FlatPhysicalDamageMod * 0.5)) * (2 - (target.Health / target.MaxHealth)));

            if (botrk.IsReady() && target.IsValidTarget(botrk.Range))
                botrk.Cast(target);
            if (cutlass.IsReady() && target.IsValidTarget(cutlass.Range))
                botrk.Cast(target);
            if (Ghost.IsReady() && target.IsValidTarget(900))
                Ghost.Cast();

            if (E.IsReady())
                E.CastOnUnit(target);

            if (Q.IsReady() && target.IsValidTarget(225) && player.Position.CountEnemiesInRange(250) > 0)
                Q.Cast(target);

            if (R.IsReady() && Ignite.IsReady() && ultignite > target.Health &&
                player.Position.CountEnemiesInRange(500) > 0)
                R.Cast(player);

            if (R.IsReady() && ultfinisher > target.Health && player.Position.CountEnemiesInRange(500) > 0)
                R.Cast(player);
        }

        private static int CalcDamage(Obj_AI_Base target)
        {
                Ignite = player.GetSpellSlot("summonerdot");
                var aa = player.GetAutoAttackDamage(target, true) * (1 + player.Crit);
                var damage = aa;

            if (Ignite.IsReady())
                damage += player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);

            if (Items.HasItem(3153) && Items.CanUseItem(3153))
                damage += player.GetItemDamage(target, Damage.DamageItems.Botrk); //ITEM BOTRK

            if (E.IsReady() && Config.Item("UseE").GetValue<bool>()) // edamage
            {
                damage += player.CalcDamage(target, Damage.DamageType.Physical,
                    10 + (E.Level*30) + (player.FlatPhysicalDamageMod*0.3) + aa);
            }

            if (Q.IsReady() && Config.Item("UseQ").GetValue<bool>()) // qdamage
            {
                damage += Q.GetDamage(target);
            }

            if (target.HasBuff("QuinnW") && !E.IsReady())
                damage += player.CalcDamage(target, Damage.DamageType.Physical,
                    15 + (player.Level*10) + (player.FlatPhysicalDamageMod*0.5)); // passive
            

            if (Config.Item("UseRD").GetValue<bool>() && R.IsReady()) // rdamage              
                        damage += R.Level * 125 + aa;
                    
                        return (int) damage;
                }
            
        private static
                void OnDraw(EventArgs args)
        {
            //Draw Skill Cooldown on Champ
            var pos = Drawing.WorldToScreen(ObjectManager.Player.Position);
            {

            }
        }
    }
}

                
            
            
        


            
        
    
        
    


            
        
    


  

