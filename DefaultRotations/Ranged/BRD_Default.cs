namespace DefaultRotations.Ranged;

[Rotation("Default", CombatType.PvE | CombatType.PvP, GameVersion = "6.28", 
    Description = "Please make sure that the three song times add up to 120 seconds!")]
[SourceCode(Path = "main/DefaultRotations/Ranged/BRD_Default.cs")]
public sealed class BRD_Default : BardRotation
{
    [UI(@"Use Raging Strikes on ""Wanderer's Minuet""")]
    [RotationConfig(CombatType.PvE)]
    public bool BindWAND { get; set; } = false;

    [UI("First Song")]
    [RotationConfig(CombatType.PvE)]
    private Song FirstSong { get; set; } = Song.WANDERER;

    [UI("Wanderer's Minuet Uptime")]
    [Range(0, 45, ConfigUnitType.Seconds, 1)]
    [RotationConfig(CombatType.PvE)]
    public float WANDTime { get; set; } = 43;

    [UI("Mage's Ballad Uptime")]
    [Range(0, 45, ConfigUnitType.Seconds, 1)]
    [RotationConfig(CombatType.PvE)]
    public float MAGETime { get; set; } = 34;

    [UI("Army's Paeon Uptime")]
    [Range(0, 45, ConfigUnitType.Seconds, 1)]
    [RotationConfig(CombatType.PvE)]
    public float ARMYTime { get; set; } = 43;

    private bool BindWANDEnough => BindWAND && this.TheWanderersMinuetPvE.EnoughLevel;
    private float WANDRemainTime => 45 - WANDTime;
    private float MAGERemainTime => 45 - MAGETime;
    private float ARMYRemainTime => 45 - ARMYTime;


    protected override bool GeneralGCD(out IAction? act)
    {
        #region PvP
        if (BlastArrowPvP.CanUse(out act, skipAoeCheck: true)) return true;
        if (ApexArrowPvP.CanUse(out act, skipAoeCheck: true)) return true;

        if (PitchPerfectPvP.CanUse(out act)) return true;
        if (PowerfulShotPvP.CanUse(out act)) return true;
        #endregion

        if (IronJawsPvE.CanUse(out act)) return true;
        if (IronJawsPvE.CanUse(out act, skipStatusProvideCheck: true) && (IronJawsPvE.Target.Target?.WillStatusEnd(30, true, IronJawsPvE.Setting.TargetStatusProvide ?? []) ?? false))
        {
            if (Player.HasStatus(true, StatusID.RagingStrikes) && Player.WillStatusEndGCD(1, 0, true, StatusID.RagingStrikes)) return true;
        }

        if (CanUseApexArrow(out act)) return true;

        if (BlastArrowPvE.CanUse(out act, skipAoeCheck : true))
        {
            if (!Player.HasStatus(true, StatusID.RagingStrikes)) return true;
            if (Player.HasStatus(true, StatusID.RagingStrikes) && BarragePvE.CD.IsCoolingDown) return true;
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
            if ((EmpyrealArrowPvE.CD.IsCoolingDown && !EmpyrealArrowPvE.CD.WillHaveOneChargeGCD(1) || !EmpyrealArrowPvE.EnoughLevel) && Repertoire != 3)
            {
                if (!Player.HasStatus(true, StatusID.StraightShotReady) && BarragePvE.CanUse(out act)) return true;
            }
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        #region PvP
        //if (PvP_FinalFantasia.CanUse(out act, CanUseOption.MustUse)) return true;

        if (SilentNocturnePvP.CanUse(out act)) return true;
        if (TheWardensPaeanPvP.CanUse(out act)) return true;

        
        if (EmpyrealArrowPvP.CanUse(out act, usedUp: true)) return true;

        if (RepellingShotPvP.CanUse(out act)) return true;
        #endregion

        if (Song == Song.NONE)
        {
            switch (FirstSong)
            {
                case Song.WANDERER:
                    if (TheWanderersMinuetPvE.CanUse(out act)) return true;
                    break;

                case Song.ARMY:
                    if (ArmysPaeonPvE.CanUse(out act)) return true;
                    break;

                case Song.MAGE:
                    if (MagesBalladPvE.CanUse(out act)) return true;
                    break;
            }
            if (TheWanderersMinuetPvE.CanUse(out act)) return true;
            if (MagesBalladPvE.CanUse(out act)) return true;
            if (ArmysPaeonPvE.CanUse(out act)) return true;
        }

        if (IsBurst && Song != Song.NONE && MagesBalladPvE.EnoughLevel)
        {
            if (RagingStrikesPvE.CanUse(out act))
            {
                if (BindWANDEnough && Song == Song.WANDERER && TheWanderersMinuetPvE.EnoughLevel) return true;
                if (!BindWANDEnough) return true;
            }

            if (RadiantFinalePvE.CanUse(out act, skipAoeCheck: true))
            {
                if (Player.HasStatus(true, StatusID.RagingStrikes) && RagingStrikesPvE.CD.ElapsedOneChargeAfterGCD(1)) return true;
            }

            if (BattleVoicePvE.CanUse(out act, skipAoeCheck: true))
            {
                if (IsLastAction(true, RadiantFinalePvE)) return true;

                if (Player.HasStatus(true, StatusID.RagingStrikes) && RagingStrikesPvE.CD.ElapsedOneChargeAfterGCD(1)) return true;
            }
        }

        if (RadiantFinalePvE.EnoughLevel && RadiantFinalePvE.CD.IsCoolingDown && BattleVoicePvE.EnoughLevel && !BattleVoicePvE.CD.IsCoolingDown) return false;

        if (TheWanderersMinuetPvE.CanUse(out act, onLastAbility: true))
        {
            if (SongTimer < ARMYRemainTime && (Song != Song.NONE || Player.HasStatus(true, StatusID.ArmysEthos))) return true;
        }

        if (Song != Song.NONE && EmpyrealArrowPvE.CanUse(out act)) return true;

        if (PitchPerfectPvE.CanUse(out act))
        {
            if (SongTimer < 3 && Repertoire > 0) return true;

            if (Repertoire == 3) return true;

            if (Repertoire == 2 && EmpyrealArrowPvE.CD.WillHaveOneChargeGCD(1) && NextAbilityToNextGCD < PitchPerfectPvE.AnimationLockTime + Ping) return true;

            if (Repertoire == 2 && EmpyrealArrowPvE.CD.WillHaveOneChargeGCD() && NextAbilityToNextGCD > PitchPerfectPvE.AnimationLockTime + Ping) return true;
        }

        if (MagesBalladPvE.CanUse(out act))
        {
            if (Song == Song.WANDERER && SongTimer < WANDRemainTime && Repertoire == 0) return true;
            if (Song == Song.ARMY && SongTimer < 2 && TheWanderersMinuetPvE.CD.IsCoolingDown) return true;
        }

        if (ArmysPaeonPvE.CanUse(out act))
        {
            if (TheWanderersMinuetPvE.EnoughLevel && SongTimer < MAGERemainTime && Song == Song.MAGE) return true;
            if (TheWanderersMinuetPvE.EnoughLevel && SongTimer < 2 && MagesBalladPvE.CD.IsCoolingDown && Song == Song.WANDERER) return true;
            if (!TheWanderersMinuetPvE.EnoughLevel && SongTimer < 2) return true;
        }

        if (SidewinderPvE.CanUse(out act))
        {
            if (Player.HasStatus(true, StatusID.BattleVoice) && (Player.HasStatus(true, StatusID.RadiantFinale) || !RadiantFinalePvE.EnoughLevel)) return true;

            if (!BattleVoicePvE.CD.WillHaveOneCharge(10) && !RadiantFinalePvE.CD.WillHaveOneCharge(10)) return true;

            if (RagingStrikesPvE.CD.IsCoolingDown && !Player.HasStatus(true, StatusID.RagingStrikes)) return true;
        }

        if (EmpyrealArrowPvE.CD.IsCoolingDown || !EmpyrealArrowPvE.CD.WillHaveOneChargeGCD() || Repertoire != 3 || !EmpyrealArrowPvE.EnoughLevel)
        {
            if (RainOfDeathPvE.CanUse(out act, usedUp: true)) return true;

            if (BloodletterPvE.CanUse(out act, usedUp: true)) return true;
        }

        return base.AttackAbility(out act);
    }

    private bool CanUseApexArrow(out IAction act)
    {
        if (!ApexArrowPvE.CanUse(out act,skipAoeCheck: true)) return false;

        if (QuickNockPvE.CanUse(out _) && SoulVoice == 100) return true;

        if (SoulVoice == 100 && BattleVoicePvE.CD.WillHaveOneCharge(25)) return false;

        if (SoulVoice >= 80 && Player.HasStatus(true, StatusID.RagingStrikes) && Player.WillStatusEnd(10, false, StatusID.RagingStrikes)) return true;

        if (SoulVoice == 100 && Player.HasStatus(true, StatusID.RagingStrikes) && Player.HasStatus(true, StatusID.BattleVoice)) return true;

        if (Song == Song.MAGE && SoulVoice >= 80 && SongTimer < 22 && SongTimer < 18) return true;

        if (!Player.HasStatus(true, StatusID.RagingStrikes) && SoulVoice == 100) return true;

        return false;
    }
}
