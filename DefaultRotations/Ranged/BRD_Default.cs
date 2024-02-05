namespace DefaultRotations.Ranged;

[Rotation("Default", CombatType.PvE, GameVersion = "6.28", 
    Description = "Please make sure that the three song times add up to 120 seconds!")]
[SourceCode(Path = "main/DefaultRotations/Ranged/BRD_Default.cs")]
public sealed class BRD_Default : BardRotation
{
    protected override IRotationConfigSet CreateConfiguration() => base.CreateConfiguration()
            .SetBool(CombatType.PvE, "BindWAND", false, @"Use Raging Strikes on ""Wanderer's Minuet""")
            .SetCombo(CombatType.PvE, "FirstSong", 0, "First Song", "Wanderer's Minuet", "Mage's Ballad", "Army's Paeon")
            .SetFloat(ConfigUnitType.Seconds, CombatType.PvE, "WANDTime", 43, "Wanderer's Minuet Uptime", min: 0, max: 45, speed: 1)
            .SetFloat(ConfigUnitType.Seconds, CombatType.PvE, "MAGETime", 34, "Mage's Ballad Uptime", min: 0, max: 45, speed: 1)
            .SetFloat(ConfigUnitType.Seconds, CombatType.PvE, "ARMYTime", 43, "Army's Paeon Uptime", min: 0, max: 45, speed: 1);

    private bool BindWAND => Configs.GetBool("BindWAND") && this.TheWanderersMinuetPvE.EnoughLevel;
    private int FirstSong => Configs.GetCombo("FirstSong");
    private float WANDRemainTime => 45 - Configs.GetFloat("WANDTime");
    private float MAGERemainTime => 45 - Configs.GetFloat("MAGETime");
    private float ARMYRemainTime => 45 - Configs.GetFloat("ARMYTime");

    protected override bool GeneralGCD(out IAction? act)
    {
        if (IronJawsPvE.CanUse(out act)) return true;
        if (IronJawsPvE.CanUse(out act, skipStatusProvideCheck: true) && (IronJawsPvE.Target?.Target?.WillStatusEnd(30, true, IronJawsPvE.Setting.TargetStatusProvide ?? []) ?? false))
        {
            if (Player.HasStatus(true, StatusID.RagingStrikes) && Player.WillStatusEndGCD(1, 0, true, StatusID.RagingStrikes)) return true;
        }

        if (CanUseApexArrow(out act)) return true;

        if (BlastArrowPvE.CanUse(out act, skipAoeCheck : true))
        {
            if (!Player.HasStatus(true, StatusID.RagingStrikes)) return true;
            if (Player.HasStatus(true, StatusID.RagingStrikes) && BarragePvE.Cooldown.IsCoolingDown) return true;
        }

        if (ShadowbitePvE.CanUse(out act)) return true;
        if (QuickNockPvE.CanUse(out act)) return true;

        if (WindbitePvE.CanUse(out act)) return true;
        if (VenomousBitePvE.CanUse(out act)) return true;

        if (StraightShotPvE.CanUse(out act)) return true;
        if (HeavyShotPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (nextGCD.IsTheSameTo(true, StraightShotPvE, VenomousBitePvE, WindbitePvE, IronJawsPvE))
        {
            return base.EmergencyAbility(nextGCD, out act);
        }
        else if ((!RagingStrikesPvE.EnoughLevel || Player.HasStatus(true, StatusID.RagingStrikes)) && (!BattleVoicePvE.EnoughLevel || Player.HasStatus(true, StatusID.BattleVoice)))
        {
            if ((EmpyrealArrowPvE.Cooldown.IsCoolingDown && !EmpyrealArrowPvE.Cooldown.WillHaveOneChargeGCD(1) || !EmpyrealArrowPvE.EnoughLevel) && Repertoire != 3)
            {
                if (!Player.HasStatus(true, StatusID.StraightShotReady) && BarragePvE.CanUse(out act)) return true;
            }
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        act = null;

        if (Song == Song.NONE)
        {
            if (FirstSong == 0 && TheWanderersMinuetPvE.CanUse(out act)) return true;
            if (FirstSong == 1 && MagesBalladPvE.CanUse(out act)) return true;
            if (FirstSong == 2 && ArmysPaeonPvE.CanUse(out act)) return true;
            if (TheWanderersMinuetPvE.CanUse(out act)) return true;
            if (MagesBalladPvE.CanUse(out act)) return true;
            if (ArmysPaeonPvE.CanUse(out act)) return true;
        }

        if (IsBurst && Song != Song.NONE && MagesBalladPvE.EnoughLevel)
        {
            if (RagingStrikesPvE.CanUse(out act))
            {
                if (BindWAND && Song == Song.WANDERER && TheWanderersMinuetPvE.EnoughLevel) return true;
                if (!BindWAND) return true;
            }

            if (RadiantFinalePvE.CanUse(out act, skipAoeCheck: true))
            {
                if (Player.HasStatus(true, StatusID.RagingStrikes) && RagingStrikesPvE.Cooldown.ElapsedOneChargeAfterGCD(1)) return true;
            }

            if (BattleVoicePvE.CanUse(out act, skipAoeCheck: true))
            {
                if (IsLastAction(true, RadiantFinalePvE)) return true;

                if (Player.HasStatus(true, StatusID.RagingStrikes) && RagingStrikesPvE.Cooldown.ElapsedOneChargeAfterGCD(1)) return true;
            }
        }

        if (RadiantFinalePvE.EnoughLevel && RadiantFinalePvE.Cooldown.IsCoolingDown && BattleVoicePvE.EnoughLevel && !BattleVoicePvE.Cooldown.IsCoolingDown) return false;

        if (TheWanderersMinuetPvE.CanUse(out act, onLastAbility: true))
        {
            if (SongEndAfter(ARMYRemainTime) && (Song != Song.NONE || Player.HasStatus(true, StatusID.ArmysEthos))) return true;
        }

        if (Song != Song.NONE && EmpyrealArrowPvE.CanUse(out act)) return true;

        if (PitchPerfectPvE.CanUse(out act))
        {
            if (SongEndAfter(3) && Repertoire > 0) return true;

            if (Repertoire == 3) return true;

            if (Repertoire == 2 && EmpyrealArrowPvE.Cooldown.WillHaveOneChargeGCD(1) && NextAbilityToNextGCD < PitchPerfectPvE.AnimationLockTime + Ping) return true;

            if (Repertoire == 2 && EmpyrealArrowPvE.Cooldown.WillHaveOneChargeGCD() && NextAbilityToNextGCD > PitchPerfectPvE.AnimationLockTime + Ping) return true;
        }

        if (MagesBalladPvE.CanUse(out act))
        {
            if (Song == Song.WANDERER && SongEndAfter(WANDRemainTime) && Repertoire == 0) return true;
            if (Song == Song.ARMY && SongEndAfterGCD(2) && TheWanderersMinuetPvE.Cooldown.IsCoolingDown) return true;
        }

        if (ArmysPaeonPvE.CanUse(out act))
        {
            if (TheWanderersMinuetPvE.EnoughLevel && SongEndAfter(MAGERemainTime) && Song == Song.MAGE) return true;
            if (TheWanderersMinuetPvE.EnoughLevel && SongEndAfter(2) && MagesBalladPvE.Cooldown.IsCoolingDown && Song == Song.WANDERER) return true;
            if (!TheWanderersMinuetPvE.EnoughLevel && SongEndAfter(2)) return true;
        }

        if (SidewinderPvE.CanUse(out act))
        {
            if (Player.HasStatus(true, StatusID.BattleVoice) && (Player.HasStatus(true, StatusID.RadiantFinale) || !RadiantFinalePvE.EnoughLevel)) return true;

            if (!BattleVoicePvE.Cooldown.WillHaveOneCharge(10) && !RadiantFinalePvE.Cooldown.WillHaveOneCharge(10)) return true;

            if (RagingStrikesPvE.Cooldown.IsCoolingDown && !Player.HasStatus(true, StatusID.RagingStrikes)) return true;
        }

        if (EmpyrealArrowPvE.Cooldown.IsCoolingDown || !EmpyrealArrowPvE.Cooldown.WillHaveOneChargeGCD() || Repertoire != 3 || !EmpyrealArrowPvE.EnoughLevel)
        {
            if (RainOfDeathPvE.CanUse(out act, isEmpty:true)) return true;

            if (BloodletterPvE.CanUse(out act, isEmpty: true)) return true;
        }

        return base.AttackAbility(out act);
    }

    private bool CanUseApexArrow(out IAction act)
    {
        if (!ApexArrowPvE.CanUse(out act,skipAoeCheck: true)) return false;

        if (QuickNockPvE.CanUse(out _) && SoulVoice == 100) return true;

        if (SoulVoice == 100 && BattleVoicePvE.Cooldown.WillHaveOneCharge(25)) return false;

        if (SoulVoice >= 80 && Player.HasStatus(true, StatusID.RagingStrikes) && Player.WillStatusEnd(10, false, StatusID.RagingStrikes)) return true;

        if (SoulVoice == 100 && Player.HasStatus(true, StatusID.RagingStrikes) && Player.HasStatus(true, StatusID.BattleVoice)) return true;

        if (Song == Song.MAGE && SoulVoice >= 80 && SongEndAfter(22) && SongEndAfter(18)) return true;

        if (!Player.HasStatus(true, StatusID.RagingStrikes) && SoulVoice == 100) return true;

        return false;
    }
}
