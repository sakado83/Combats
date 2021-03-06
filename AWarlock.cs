using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using Anthrax.WoW.Classes.ObjectManager;
using Anthrax.WoW.Internals;
using Anthrax.AI.Controllers;
using Anthrax.WoW;
using Anthrax;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Xml.Serialization;
using System.IO;



//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// nrgdret AOE/Single Target Rotation Used in this. (well done)                                                                                             //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////


namespace Anthrax
{
    class Warlock : Modules.ICombat
    {
        #region private vars
        bool isAOE;
        WowLocalPlayer ME;
        //Stopwatch stopwatch;
        //List<long> averageScanTimes;
        #endregion

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        public override string Name
        {
            get { return "A Warlock by Koha"; }                      //This is the name displayed in SPQR's Class selection DropdownList
        }

        #region enums
        internal enum Spells : int                      //This is a convenient list of all spells used by our combat routine
        {  		//you can have search on wowhead.com for spell name, and get the id in url
		//Demo Spell List
            LifeTap = 1454,
			Corruption = 172,
			ShadowBoltG = 112092,
			HealthFunnel = 755,
			ShadowBolt = 686,
			HealthFunnelG = 114189,
			HandofG = 105174,
			DrainLife = 689,
			SoulFire = 6353,
			SoulFireMeto = 104027,
			Meto = 103958,
			Wrathstorm = 119915,
			TwilightWard = 6229,
			CoE = 1490,
			TouchofChaos = 103964,
			ImmoAura = 104025,
			WrathStorm = 119915,
			Doom = 603,
			VoidRay = 115422,
			Hellfire = 1949,
			DarkSoul = 113861,
			ImpSwarm = 104316,
		//End Demo Spell List	
		//Destro Build
			ChaosBolt = 116858,
			ShadowBurn = 17877,
			Immolate = 348,
			ImmoFandB = 108686,
			Conflag = 17962,
			ConFandB = 108685,
			Incinerate = 29722,
			IncFandB = 114654,
			FandBrim = 108683,
			RainofFire = 104232,
		//Affliction
		    CreateHealthstone = 6201,
			MaleficGrasp = 103103,			
            Haunt = 48181,
            DrainSoul = 1120,
            Agony = 980,
            UnstableAffliction = 30108,
            DSMisery = 77801,			
            UnendingResolve = 104773,
			HealFunnel = 755,	
			FelFlame = 77799,
			Soulburn = 74434,
			SoulSwapInh = 86121,
			SoulSwapSB = 119678,
			SoulSwapExh = 86213,
			
			
			
        }

        internal enum Auras : int                       //This is another convenient list of Auras used in our combat routine
        {				//you can have those in wowhead.com (again) and get the id in url
		//Demo Aura List
            GlyphofDH = 63303,
			GlyphofHF = 56238,
			DemoCheck = 124913,
			ShadowFlame = 47960,
			Corruption = 146739,
			Doom = 603,
			Meto = 103965,
			MoltenCore = 122355,
			ImmoAura = 104025,
			Hellfire = 1949,
			DestroCheck = 108647,
		//End Demo Aura List
		//Destro
		FandBrim = 108683,
		Immolate = 348,
		ImmoFandB = 108686,
		DS = 113858,
		//Affliction
		 Haunt = 48181,
            Agony = 980,				
            UnstableAffliction = 30108,
			FireBreath = 34889,			// CoE equiv
			LightningBreath = 24844,	// CoE equiv
			MasterPoisoner = 93068,		// CoE equiv
			CurseOfElements = 1490,		// CoE
			Soulburn = 74434,
			SoulSwapInh = 86211,
			KC = 137587,
			AffCheck = 108558,
	     	
			
}

#endregion

		
public override void OnPatrol()
    {

    }

        // /!\ WARNING /!\
        // The OnPull function should NOT be blocking function !
        // The bot will handle calling it in loop until the combat is over.
        // Blocking function may lead to slow behavior.
public override void OnPull(WoW.Classes.ObjectManager.WowUnit unit)
{

}
#region singleRotation
//public override void OnCombat(WoW.Classes.ObjectManager.WowUnit unit)
//{}

private int lastShadowFlameTick = 0;
private int lastMetoTick = 0;
private int lastImmolateTick = 0;
private int lastImmoFandBTick = 0;
private int lastConflagTick = 0;
private int lastConFandBTick = 0;
private int lastChaosBoltTick = 0;
private int lastFandBTick = 0;
private int lastIncTick = 0;
private int lastSBTick = 0;
public static int lastUnstableAffliction = 0;
public static int lastHaunt = 0;
public static int lastAgony = 0;
public static int lastCorruption = 0;
public static int lastMaleficGrasp = 0;
public static int lastCoE = 0;
public static int lastSB = 0;
public static int lastSS = 0;
public static int lastDrainSoul = 0;
public static int lastLifeTap = 0;

private void castNextSpellbySinglePriority(WowUnit TARGET)
{

	var DemonicFury = ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.DemonicFury);
	var Embers = ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.BurningEmbers);
	var Shards = ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.SoulShards);
	var IsCasting = ObjectManager.LocalPlayer.IsCasting;
	var Pet = ObjectManager.Pet;
	
if (ME.HasAuraById((int)Auras.Meto) && !ME.InCombat && Environment.TickCount - lastMetoTick > 2000)
        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Meto);
			lastShadowFlameTick = Environment.TickCount;
			return;
        }
if (TARGET.Health >= 1 && ME.InCombat)
{ //Combat Check


/////////////////////////////////////////////////////////////Destruction//////////////////////////////////////////////
if (ME.HasAuraById((int)Auras.DestroCheck) && !IsCasting)
{ //Spec Check

                if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.RainofFire)
                         && !IsCasting)
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RainofFire);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }

	if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Mana) <= 70 && AI.Controllers.Spell.CanCast((int)Spells.LifeTap) && ME.HealthPercent >= 50)
		{
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.LifeTap);
			return;
		}
			
			
	if (Pet.HealthPercent <= 80 && AI.Controllers.Spell.CanCast((int)Spells.HealthFunnel) && Pet.IsAlive && TARGET.HasAuraById((int)Auras.Corruption))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealthFunnel);
            return;
        }
		
		
	if (ME.HasAuraById((int)Auras.GlyphofHF) && Pet.HealthPercent <= 80 && AI.Controllers.Spell.CanCast((int)Spells.HealthFunnel) && Pet.IsAlive && TARGET.HasAuraById((int)Auras.Corruption))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealthFunnel);
            return;
        }	
		
		
		//Fireand Brim
	if (ME.HasAuraById((int)Auras.FandBrim) && Environment.TickCount - lastFandBTick > 2000)
	        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FandBrim);
			lastFandBTick = Environment.TickCount;
			return;
        }	

		//ExecutePhase
	if (Embers >= 1 && TARGET.HealthPercent < 20 && Environment.TickCount - lastSBTick > 5000 || TARGET.Health <= 100000 && Embers >= 1 && Environment.TickCount - lastSBTick > 2000)
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ShadowBurn);
			lastSBTick = Environment.TickCount;
            return;
        }
		
			//ChaosBolt
	if (Embers >= 3.2 && AI.Controllers.Spell.CanCast((int)Spells.ChaosBolt) && Environment.TickCount - lastChaosBoltTick > 3000 && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0)
	|| Embers >= 1 && AI.Controllers.Spell.CanCast((int)Spells.ChaosBolt) && Environment.TickCount - lastChaosBoltTick > 3000 && ME.HasAuraById((int)Auras.DS) && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ChaosBolt);
			lastChaosBoltTick = Environment.TickCount;
            return;
        }	
	
	//Immolate
	if (!TARGET.HasAuraById((int)Auras.Immolate) && Environment.TickCount - lastImmolateTick > 2000 && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0) && !TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Immolate) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any()
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.Immolate && x.TimeLeft <= 7000).Any() && Environment.TickCount - lastImmolateTick > 2000 && TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Immolate) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any()
	&& (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0))
        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Immolate);
			lastImmolateTick = Environment.TickCount;
			return;
        }	
		

	
	//conflag on 2 Charges
	if (Environment.TickCount - lastConflagTick > 8000)
        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Conflag);
			lastConflagTick = Environment.TickCount;
			return;
        }
	

	
	//Incinerate
	if (AI.Controllers.Spell.CanCast((int)Spells.Incinerate) && Environment.TickCount - lastIncTick > 2000
	&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Immolate) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any())
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Incinerate);
			lastIncTick = Environment.TickCount;
            return;
        }	



}

/////////////////////////////////////////////////////////////Affliction//////////////////////////////////////////////
if (ME.HasAuraById((int)Auras.AffCheck))
{ //Spec Check

                if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.RainofFire)
                         && !IsCasting)
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RainofFire);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }
				
	if(!ME.HasAuraById((int)Auras.CurseOfElements) &&
					!ME.HasAuraById((int)Auras.FireBreath) && 
					!ME.HasAuraById((int)Auras.LightningBreath) && 
					!ME.HasAuraById((int)Auras.MasterPoisoner) &&
					AI.Controllers.Spell.CanCast((int)Spells.CoE) &&
					ME.Level > 92 &&
					Environment.TickCount - lastCoE > 2000 )
				{
					WoW.Internals.ActionBar.ExecuteSpell((int)Spells.CoE);
					lastCoE = Environment.TickCount;
					return;
				}

	if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Mana) <= 70 && AI.Controllers.Spell.CanCast((int)Spells.LifeTap) && ME.HealthPercent >= 50)
		{
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.LifeTap);
			return;
		}
			
			
	if (Pet.HealthPercent <= 80 && AI.Controllers.Spell.CanCast((int)Spells.HealthFunnel) && Pet.IsAlive && TARGET.HasAuraById((int)Auras.Corruption))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealthFunnel);
            return;
        }
		
		
	if (ME.HasAuraById((int)Auras.GlyphofHF) && Pet.HealthPercent <= 80 && AI.Controllers.Spell.CanCast((int)Spells.HealthFunnel) && Pet.IsAlive && TARGET.HasAuraById((int)Auras.Corruption))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealthFunnel);
            return;
        }	

                 // Drain Life
                    if (ME.HealthPercent < 50 && AI.Controllers.Spell.CanCast((int)Spells.DrainLife) && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0))
                    {
 						if (!ME.HasAuraById((int)Auras.Soulburn) &&
							Shards > 0 )
						{
							WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Soulburn);
						}
						//AI.Controllers.Spell.Cast((int)Spells.DrainLife, unit);
						WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DrainLife);
                        return;
                    }

                
		
		
		//Agony
	if (!TARGET.HasAuraById((int)Auras.Agony) && Environment.TickCount - lastAgony > 1000 
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.Agony 
	&& x.TimeLeft < 4000).Any() && AI.Controllers.Spell.CanCast((int)Spells.Agony) && TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Agony) 
	&& x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any() && Environment.TickCount - lastAgony > 1000)
	        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Agony);
			lastAgony = Environment.TickCount;
			return;
        }	

		//Corrupton
	if (!TARGET.HasAuraById((int)Auras.Corruption) 
	&& Environment.TickCount - lastCorruption > 1000 
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.Corruption && x.TimeLeft < 4000).Any() && AI.Controllers.Spell.CanCast((int)Spells.Corruption) 
	&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Corruption) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any() && Environment.TickCount - lastCorruption > 1000)
	        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Corruption);
			lastCorruption = Environment.TickCount;
			return;
        }	
		
	
	//Unstable Affliction
	if (!TARGET.HasAuraById((int)Auras.UnstableAffliction) && Environment.TickCount - lastUnstableAffliction > 2000 && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0)
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.UnstableAffliction && x.TimeLeft <= 4000).Any()
	&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.UnstableAffliction) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any() && Environment.TickCount - lastUnstableAffliction > 2000 
	&& !(ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0))
	        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.UnstableAffliction);
			lastUnstableAffliction = Environment.TickCount;
			return;
        }		
		
	//Haunt
	if (!TARGET.HasAuraById((int)Auras.Haunt) && Environment.TickCount - lastHaunt > 2000 && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0 && Shards > 0)
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.Haunt && x.TimeLeft <= 4000).Any() 
	&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Haunt) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any() && Environment.TickCount - lastHaunt > 2000 
	&& !(ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0) && Shards > 0)
	        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Haunt);
			lastHaunt = Environment.TickCount;
			return;
        }	
		
		//Drain Soul
	if (ME.HealthPercent >= 20 && TARGET.Health <= 20 && AI.Controllers.Spell.CanCast((int)Spells.DrainSoul) && Environment.TickCount - lastDrainSoul > 300 && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0) && !IsCasting
	|| ME.HealthPercent >= 20 && Shards < 1 && AI.Controllers.Spell.CanCast((int)Spells.DrainSoul) && Environment.TickCount - lastDrainSoul > 300 && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0) && !IsCasting)
	        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DrainSoul);
			lastMaleficGrasp = Environment.TickCount;
			return;
        }	
	
	//Malefic Grasp
	if (ME.HealthPercent >= 20 && AI.Controllers.Spell.CanCast((int)Spells.MaleficGrasp) && Environment.TickCount - lastMaleficGrasp > 300 && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0) && !IsCasting
	|| ME.HasAuraById((int)Auras.KC) && ME.HealthPercent >= 20 && AI.Controllers.Spell.CanCast((int)Spells.MaleficGrasp) && Environment.TickCount - lastMaleficGrasp > 300 && !IsCasting)
	        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.MaleficGrasp);
			lastMaleficGrasp = Environment.TickCount;
			return;
        }	
		



}


											//////////////////////////////////////////////////////////Demonology////////////////////////////////////////////////////////
if (ME.HasAuraById((int)Auras.DemoCheck) && !IsCasting)
{ //Spec Check


///Pet Controls

			
			
	if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Mana) <= 70 && AI.Controllers.Spell.CanCast((int)Spells.LifeTap) && ME.HealthPercent >= 50)
		{
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.LifeTap);
			return;
		}
			
			
	if (Pet.HealthPercent <= 80 && AI.Controllers.Spell.CanCast((int)Spells.HealthFunnel) && Pet.IsAlive && TARGET.HasAuraById((int)Auras.Corruption))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealthFunnel);
            return;
        }
		
	if (ME.HasAuraById((int)Auras.GlyphofHF) && Pet.HealthPercent <= 80 && AI.Controllers.Spell.CanCast((int)Spells.HealthFunnel) && Pet.IsAlive && TARGET.HasAuraById((int)Auras.Corruption))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealthFunnel);
            return;
        }	
			
	if(!ME.HasAuraById((int)Auras.CurseOfElements) &&
					!ME.HasAuraById((int)Auras.FireBreath) && 
					!ME.HasAuraById((int)Auras.LightningBreath) && 
					!ME.HasAuraById((int)Auras.MasterPoisoner) &&
					AI.Controllers.Spell.CanCast((int)Spells.CoE) &&
					ME.Level > 92 &&
					Environment.TickCount - lastCoE > 2000 )
				{
					WoW.Internals.ActionBar.ExecuteSpell((int)Spells.CoE);
					lastCoE = Environment.TickCount;
					return;
				}
				
	// Dots Corruption
	if (!TARGET.HasAuraById((int)Auras.Corruption) 
	|| TARGET.HasAuraById((int)Auras.Corruption) && TARGET.Auras.Where(x => x.SpellId == (int)Auras.Corruption && x.TimeLeft <= 3000).Any() && !ME.HasAuraById((int)Auras.Meto)
	&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Corruption) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any())
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Corruption);
            return;
        }
	
	 // Turn into Demon
    if (ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.DemonicFury) >= 900 &&
        AI.Controllers.Spell.CanCast((int)Spells.Meto) && Environment.TickCount - lastMetoTick > 3000)
        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Meto);
			lastMetoTick = Environment.TickCount;
			return;
        }
		
	// Leave Demon
    if (ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.DemonicFury) <= 150 && Environment.TickCount - lastMetoTick > 3000  && ME.HasAuraById((int)Auras.Meto))
        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Meto);
			lastMetoTick = Environment.TickCount;
			return;
        }
	
	
	//Demon Rotation
	if (ME.HasAuraById((int)Auras.Meto))
	{
	
	if (AI.Controllers.Spell.CanCast((int)Spells.DarkSoul))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DarkSoul);
            return;
        }
		
		if (AI.Controllers.Spell.CanCast((int)Spells.ImpSwarm))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ImpSwarm);
            return;
        }
	
	
	if (TARGET.Auras.Where(x => x.SpellId == (int)Auras.Corruption && x.TimeLeft <= 10000).Any() && TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Corruption) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any())
        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.TouchofChaos);
			return;
        }	
	
	if (!TARGET.HasAuraById((int)Auras.Doom) || TARGET.Auras.Where(x => x.SpellId == (int)Auras.Doom && x.TimeLeft <= 60000).Any() && TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Doom) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any())
        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Doom);
			return;
        }
		
	if (ME.Auras.Where(x => x.SpellId == (int)Auras.MoltenCore && x.StackCount <= 0).Any() )
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.TouchofChaos);
            return;
        }	
	
	if (ME.Auras.Where(x => x.SpellId == (int)Auras.MoltenCore && x.StackCount >= 1).Any() && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0))
		{
				WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SoulFireMeto);
            return;
        }
	
	}
	//End Demo Rotation
	
	

	//Drain Life
	if (ME.HealthPercent <= 50)
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DrainLife);
            return;
        }
		
	//Out of Meto Rotation	
	if (!ME.HasAuraById((int)Auras.Meto))
	{
	if (AI.Controllers.Spell.CanCast((int)Spells.HandofG) && !TARGET.HasAuraById((int)Auras.ShadowFlame) && Environment.TickCount - lastShadowFlameTick > 2000)
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HandofG);
			lastShadowFlameTick = Environment.TickCount;
            return;
        }	
	

	if (ME.HasAuraById((int)Auras.MoltenCore) && ME.Auras.Where(x => x.SpellId == (int)Auras.MoltenCore && x.StackCount >= 9).Any())
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SoulFire);
            return;
        }
		
	if (AI.Controllers.Spell.CanCast((int)Spells.ShadowBolt))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ShadowBolt);
            return;
        }
	}

		
} //End of Spec Check
} //Combat Check
} //End AoE Code
#endregion

#region AOE>4 rotation
 private void castNextSpellbyAOEPriority(WowUnit TARGET)
{
	var DemonicFury = ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.DemonicFury);
	var Embers = ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.BurningEmbers);
	var Shards = ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.SoulShards);
	var IsCasting = ObjectManager.LocalPlayer.IsCasting;
	var Pet = ObjectManager.Pet;
		
	if (TARGET.Health >= 1 && ME.InCombat)
{ //Combat Check



/////////////////////////////////////////////////////////////Affliction//////////////////////////////////////////////
if (ME.HasAuraById((int)Auras.AffCheck))
{ //Spec Check

                if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.RainofFire)
                         && !IsCasting)
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RainofFire);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
						
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }
				
	if(!ME.HasAuraById((int)Auras.CurseOfElements) &&
					!ME.HasAuraById((int)Auras.FireBreath) && 
					!ME.HasAuraById((int)Auras.LightningBreath) && 
					!ME.HasAuraById((int)Auras.MasterPoisoner) &&
					AI.Controllers.Spell.CanCast((int)Spells.CoE) &&
					ME.Level > 92 &&
					Environment.TickCount - lastCoE > 2000 )
				{
					WoW.Internals.ActionBar.ExecuteSpell((int)Spells.CoE);
					lastCoE = Environment.TickCount;
					return;
				}

	if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Mana) <= 70 && AI.Controllers.Spell.CanCast((int)Spells.LifeTap) && ME.HealthPercent >= 50)
		{
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.LifeTap);
			return;
		}
			
			
	if (Pet.HealthPercent <= 80 && AI.Controllers.Spell.CanCast((int)Spells.HealthFunnel) && Pet.IsAlive && TARGET.HasAuraById((int)Auras.Corruption))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealthFunnel);
            return;
        }
		
		
	if (ME.HasAuraById((int)Auras.GlyphofHF) && Pet.HealthPercent <= 80 && AI.Controllers.Spell.CanCast((int)Spells.HealthFunnel) && Pet.IsAlive && TARGET.HasAuraById((int)Auras.Corruption))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealthFunnel);
            return;
        }	

                 // Drain Life
                    if (ME.HealthPercent < 50 && AI.Controllers.Spell.CanCast((int)Spells.DrainLife) && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0))
                    {
 						if (!ME.HasAuraById((int)Auras.Soulburn) &&
							Shards > 0 )
						{
							WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Soulburn);
						}
						//AI.Controllers.Spell.Cast((int)Spells.DrainLife, unit);
						WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DrainLife);
                        return;
                    }

                
		
		
		//Agony
	if (!TARGET.HasAuraById((int)Auras.Agony) && Environment.TickCount - lastAgony > 1000 
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.Agony 
	&& x.TimeLeft < 4000).Any() && AI.Controllers.Spell.CanCast((int)Spells.Agony) && TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Agony) 
	&& x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any() && Environment.TickCount - lastAgony > 1000)
	        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Agony);
			lastAgony = Environment.TickCount;
			return;
        }	

		//Corrupton
	if (!TARGET.HasAuraById((int)Auras.Corruption) 
	&& Environment.TickCount - lastCorruption > 1000 
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.Corruption && x.TimeLeft < 4000).Any() && AI.Controllers.Spell.CanCast((int)Spells.Corruption) 
	&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Corruption) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any() && Environment.TickCount - lastCorruption > 1000)
	        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Corruption);
			lastCorruption = Environment.TickCount;
			return;
        }	
		
	
	//Unstable Affliction
	if (!TARGET.HasAuraById((int)Auras.UnstableAffliction) && Environment.TickCount - lastUnstableAffliction > 2000 && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0)
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.UnstableAffliction && x.TimeLeft <= 4000).Any()
	&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.UnstableAffliction) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any() && Environment.TickCount - lastUnstableAffliction > 2000 
	&& !(ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0))
	        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.UnstableAffliction);
			lastUnstableAffliction = Environment.TickCount;
			return;
        }		
		
	//Haunt
	if (!TARGET.HasAuraById((int)Auras.Haunt) && Environment.TickCount - lastHaunt > 2000 && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0 && Shards > 0)
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.Haunt && x.TimeLeft <= 4000).Any() 
	&& TARGET.Auras.Where(x => x.SpellId == ((int)Auras.Haunt) && x.CasterGUID == ObjectManager.LocalPlayer.GUID).Any() && Environment.TickCount - lastHaunt > 2000 
	&& !(ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0) && Shards > 0)
	        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Haunt);
			lastHaunt = Environment.TickCount;
			return;
        }	
		
		//Drain Soul
	if (ME.HealthPercent >= 20 && TARGET.Health <= 20 && AI.Controllers.Spell.CanCast((int)Spells.DrainSoul) && Environment.TickCount - lastDrainSoul > 300 && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0) && !IsCasting
	|| ME.HealthPercent >= 20 && Shards < 1 && AI.Controllers.Spell.CanCast((int)Spells.DrainSoul) && Environment.TickCount - lastDrainSoul > 300 && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0) && !IsCasting)
	        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DrainSoul);
			lastMaleficGrasp = Environment.TickCount;
			return;
        }	
	
	//Malefic Grasp
	if (ME.HealthPercent >= 20 && AI.Controllers.Spell.CanCast((int)Spells.MaleficGrasp) && Environment.TickCount - lastMaleficGrasp > 300 && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0) && !IsCasting
	|| ME.HasAuraById((int)Auras.KC) && ME.HealthPercent >= 20 && AI.Controllers.Spell.CanCast((int)Spells.MaleficGrasp) && Environment.TickCount - lastMaleficGrasp > 300 && !IsCasting)
	        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.MaleficGrasp);
			lastMaleficGrasp = Environment.TickCount;
			return;
        }	
		



}

if (ME.HasAuraById((int)Auras.DestroCheck) && !IsCasting)
{ //Spec Check


                if (DetectKeyPress.GetKeyState(DetectKeyPress.Alt) < 0)
                {
                    if (AI.Controllers.Spell.CanCast((int)Spells.RainofFire)
                         && !IsCasting)
                    {
                        WoW.Internals.MouseController.RightClick();
                        WoW.Internals.ActionBar.ExecuteSpell((int)Spells.RainofFire);
                        WoW.Internals.MouseController.LockCursor();
                        WoW.Internals.MouseController.MoveMouse(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);
                        WoW.Internals.MouseController.LeftClick();
                        WoW.Internals.MouseController.UnlockCursor();
                    }

                    return;

                }
	if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Mana) <= 70 && AI.Controllers.Spell.CanCast((int)Spells.LifeTap) && ME.HealthPercent >= 50)
		{
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.LifeTap);
			return;
		}
			
			
	if (Pet.HealthPercent <= 80 && AI.Controllers.Spell.CanCast((int)Spells.HealthFunnel) && Pet.IsAlive && TARGET.HasAuraById((int)Auras.Corruption))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealthFunnel);
            return;
        }
		
		
	if (ME.HasAuraById((int)Auras.GlyphofHF) && Pet.HealthPercent <= 80 && AI.Controllers.Spell.CanCast((int)Spells.HealthFunnel) && Pet.IsAlive && TARGET.HasAuraById((int)Auras.Corruption))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealthFunnel);
            return;
        }	

		
	//Fireand Brim
	if (!ME.HasAuraById((int)Auras.FandBrim) && Environment.TickCount - lastFandBTick > 2000 && Embers >= 1)
	        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.FandBrim);
			lastFandBTick = Environment.TickCount;
			return;
        }	
		
		//Immolate
	if (!TARGET.HasAuraById((int)Auras.ImmoFandB) && Environment.TickCount - lastImmoFandBTick > 4000 && ME.HasAuraById((int)Auras.FandBrim)
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.ImmoFandB && x.TimeLeft <= 4000).Any() && Environment.TickCount - lastImmolateTick > 2000 && ME.HasAuraById((int)Auras.FandBrim))
        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ImmoFandB);
			lastImmolateTick = Environment.TickCount;
			return;
        }	

	//conflag on 2 Charges
	if (Environment.TickCount - lastConFandBTick > 8000 && ME.HasAuraById((int)Auras.FandBrim) && Embers >= 1)
        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ConFandB);
			lastConFandBTick = Environment.TickCount;
			return;
        }	
		
	//Immolate
	if (!TARGET.HasAuraById((int)Auras.ImmoFandB) && Environment.TickCount - lastImmoFandBTick > 4000 && ME.HasAuraById((int)Auras.FandBrim) && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0)
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.ImmoFandB && x.TimeLeft <= 4000).Any() && Environment.TickCount - lastImmolateTick > 2000 && ME.HasAuraById((int)Auras.FandBrim) && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0))
        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ImmoFandB);
			lastImmolateTick = Environment.TickCount;
			return;
        }	
	
	//Incinerate
	if (AI.Controllers.Spell.CanCast((int)Spells.IncFandB) && ME.HasAuraById((int)Auras.FandBrim) && Embers >= 1)
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.IncFandB);
            return;
        }	

		//ExecutePhase
	if (Embers > 3.5 && TARGET.HealthPercent < 20)
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ShadowBurn);
            return;
        }

	//Immolate
	if (!TARGET.HasAuraById((int)Auras.Immolate) && Environment.TickCount - lastImmolateTick > 4000 && !TARGET.HasAuraById((int)Auras.ImmoFandB) && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0)
	|| TARGET.Auras.Where(x => x.SpellId == (int)Auras.Immolate && x.TimeLeft <= 4000).Any() && Environment.TickCount - lastImmolateTick > 2000 && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0))
        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Immolate);
			lastImmolateTick = Environment.TickCount;
			return;
        }	
	
	//conflag on 2 Charges
	if (Environment.TickCount - lastConflagTick > 8000)
        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Conflag);
			lastConflagTick = Environment.TickCount;
			return;
        }
	
	//ChaosBolt
	if (Embers > 35 && AI.Controllers.Spell.CanCast((int)Spells.ChaosBolt) && Environment.TickCount - lastChaosBoltTick > 4000 && (ObjectManager.LocalPlayer.MovementField.CurrentSpeed == 0))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ChaosBolt);
			lastChaosBoltTick = Environment.TickCount;
            return;
        }	
	
	//Incinerate
	if (AI.Controllers.Spell.CanCast((int)Spells.Incinerate))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Incinerate);
            return;
        }	


		
		
		
		
		
}
													///////////////////////////DEMO////////////////////////
if (ME.HasAuraById((int)Auras.DemoCheck))
{ //Spec Check

///Pet Controls

			
			
	if (ME.GetPowerPercent(WoW.Classes.ObjectManager.WowUnit.WowPowerType.Mana) <= 70 && AI.Controllers.Spell.CanCast((int)Spells.LifeTap) && ME.HealthPercent >= 50)
		{
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.LifeTap);
			return;
		}
			
			
	if (Pet.HealthPercent < 80 && ME.HasAuraById((int)Auras.GlyphofHF) && AI.Controllers.Spell.CanCast((int)Spells.HealthFunnel) && Pet.IsAlive)
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HealthFunnel);
            return;
        }
			
	if (!TARGET.HasAuraById((int)Auras.CurseOfElements) && AI.Controllers.Spell.CanCast((int)Spells.CoE))		
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.CoE);
            return;
        }
		
		if (AI.Controllers.Spell.CanCast((int)Spells.Wrathstorm))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Wrathstorm);
            return;
        }
		
	
	// Leave Demon
    if (ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.DemonicFury) <= 150 &&
        AI.Controllers.Spell.CanCast((int)Spells.Meto) && Environment.TickCount - lastMetoTick > 2000 && ME.HasAuraById((int)Auras.Meto))
        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Meto);
			lastShadowFlameTick = Environment.TickCount;
			return;
        }
	
	
	//Demon Rotation
	
	if (ME.HasAuraById((int)Auras.Meto))
	{
	
	if (TARGET.Auras.Where(x => x.SpellId == (int)Auras.Corruption && x.TimeLeft <= 3000).Any())
        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.TouchofChaos);
			return;
        }	
	
	if (!TARGET.HasAuraById((int)Auras.Doom) || TARGET.Auras.Where(x => x.SpellId == (int)Auras.Doom && x.TimeLeft <= 60000).Any()	&& AI.Controllers.Spell.CanCast((int)Spells.Doom))
        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Doom);
			return;
        }
	
	if (!ME.HasAuraById((int)Auras.ImmoAura) && AI.Controllers.Spell.CanCast((int)Spells.ImmoAura) && TARGET.Position.Distance3DFromPlayer <= 10)
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.ImmoAura);
            return;
        }
	
	if (AI.Controllers.Spell.CanCast((int)Spells.VoidRay) && TARGET.Position.Distance3DFromPlayer < 20)
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.VoidRay);
            return;
        }			
	}
	
	if (!TARGET.HasAuraById((int)Auras.Corruption) || TARGET.HasAuraById((int)Auras.Corruption) && TARGET.Auras.Where(x => x.SpellId == (int)Auras.Corruption && x.TimeLeft <= 3000).Any() && !ME.HasAuraById((int)Auras.Meto))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Corruption);
            return;
        }
		
		
		
	 // Turn into Demon
    if (ObjectManager.LocalPlayer.GetPower(WoW.Classes.ObjectManager.WowUnit.WowPowerType.DemonicFury) >= 900 &&
        AI.Controllers.Spell.CanCast((int)Spells.Meto))
        {
			WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Meto);
			return;
        }

	if (ME.HealthPercent <= 50)
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.DrainLife);
            return;
        }
	if (!ME.HasAuraById((int)Auras.Meto))
	{
	if (!ME.HasAuraById((int)Auras.Meto) && AI.Controllers.Spell.CanCast((int)Spells.HandofG) && !TARGET.HasAuraById((int)Auras.ShadowFlame) && Environment.TickCount - lastShadowFlameTick > 2000)
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.HandofG);
			lastShadowFlameTick = Environment.TickCount;
            return;
        }	
	

	if (ME.HasAuraById((int)Auras.MoltenCore) && ME.Auras.Where(x => x.SpellId == (int)Auras.MoltenCore && x.StackCount >= 9).Any())
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.SoulFire);
            return;
        }
		
	if (AI.Controllers.Spell.CanCast((int)Spells.Hellfire) && !ME.HasAuraById((int)Auras.Hellfire))
		{
            WoW.Internals.ActionBar.ExecuteSpell((int)Spells.Hellfire);
            return;
        }
	}
	
} //End of Spec Check
	
	
	
	
	
	
} //Combat Check
} //End AoE Code
#endregion

        #region auxFunctions
        public void changeRotation()
        {
            if (isAOE)
            {
                Console.Beep(5000, 100);
                isAOE = false;
                Logger.WriteLine("Rotation Single!!");
            }
            else
            {
                Console.Beep(5000, 100);
                Console.Beep(5000, 100);
                Console.Beep(5000, 100);
                isAOE = true;
                Logger.WriteLine("Rotation AOE!!");
            }
        }
        #endregion
	#region DetectKeyPress
    public class DetectKeyPress
    {
        public static int Shift = 0x10;
        public static int Ctrl = 0x11;
        public static int Alt = 0x12;

        public static int Z = 0x5A;
        public static int X = 0x58;
        public static int C = 0x43;
 
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern short GetKeyState(int virtualKeyCode);

    }
    #endregion
        public override void OnCombat(WowUnit TARGET)
        {
            /* Performance tests
            stopwatch.Stop();
            averageScanTimes.Add(stopwatch.ElapsedMilliseconds);
            SPQR.Logger.WriteLine("Elapsed:  " + stopwatch.ElapsedMilliseconds.ToString() + " miliseconds, average:" + (averageScanTimes.Sum() / averageScanTimes.Count()).ToString() + ",Max:" + averageScanTimes.Max());
            stopwatch.Restart();
             */
            if (!Cooldown.IsGlobalCooldownActive && TARGET.IsValid)
            {
                if (isAOE) { castNextSpellbyAOEPriority(TARGET); } else { castNextSpellbySinglePriority(TARGET); }
            }
            if ((GetAsyncKeyState(90) == -32767))
            {
                changeRotation();
            }
        }

        public override void OnLoad()   //This is called when the Customclass is loaded in SPQR
        {
            Logger.WriteLine("CustomClass " + Name + " Loaded");
        }

        public override void OnUnload() //This is called when the Customclass is unloaded in SPQR
        {
 
            Logger.WriteLine("CustomClass " + Name + " Unloaded, Goodbye !");
        }

        public override void OnBotStart() //This is called once, when you hit CTRL+X to start SPQR combat routine
        {
            ME = ObjectManager.LocalPlayer;
            Logger.WriteLine("Launching " + Name + " routine... enjoy! Press z to switch between single/aoe");
            /* Performance tests
            stopwatch=new Stopwatch();
            averageScanTimes = new List<long>();
             */
        }

        public override void OnBotStop() //This is called once, when you hit CTRL+X to stop SPQR combat routine
        {
            Logger.WriteLine("Stopping " + Name + " routine... gl smashing keys.");
        }
}}