namespace DefaultRotations.Ranged;

[BetaRotation]
[Rotation("369 Opener", CombatType.PvE | CombatType.PvP, GameVersion = "7.05")]
[SourceCode(Path = "main/DefaultRotations/Ranged/BRD_Default.cs")]
[LinkDescription("https://i.imgur.com/ZHSg4M0.png")]
[LinkDescription("https://i.imgur.com/40vYKDd.png")]
public sealed class BRD_Default : BardRotation
{
    [UI(@"Use Raging Strikes on ""Wanderer's Minuet""")]
    [RotationConfig(CombatType.PvE)]
    public bool BindWAND { get; set; } = false;

    private const float WANDRemainTime = 3, MAGERemainTime = 6, ARMYRemainTime = 9;

    public static bool InBurstStatus => Player.WillStatusEndGCD(0, 0, true, StatusID.RagingStrikes);

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

        if (WideVolleyPvEReplace.CanUse(out act)) return true;
        if (StraightShotPvEReplace.CanUse(out act)) return true;

        if (RadiantEncorePvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (ResonantArrowPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (BlastArrowPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (QuickNockPvEReplace.CanUse(out act)) return true;

        if (WindbitePvEReplace.CanUse(out act)) return true;
        if (VenomousBitePvEReplace.CanUse(out act)) return true;

        if (HeavyShotPvEReplace.CanUse(out act)) return true;

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
                if (!Player.HasStatus(true, StatusID.StraightShotReady) && BarragePvEReplace.CanUse(out act)) return true;
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

        if (RainOfDeathPvE.CanUse(out act)) return true;
        if (BloodletterPvEReplace.CanUse(out act)) return true; // Make it into GCD
        if (DoSong(out act)) return true;

        if (IsBurst && Song != Song.NONE)
        {
            if (UseBurstMedicine(out act)) return true;
            if (RadiantFinalePvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (BattleVoicePvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (RagingStrikesPvE.CanUse(out act)) return true;
        }

        if (EmpyrealArrowPvE.CanUse(out act)) return true;
        if (BarragePvE.CanUse(out act)) return true;

        if (SidewinderPvE.CanUse(out act)) return true;

        if (InBurstStatus)
        {
            if (RainOfDeathPvE.CanUse(out act, usedUp: true)) return true;
            if (BloodletterPvEReplace.CanUse(out act, usedUp: true)) return true; // Make it into GCD
        }

        return base.AttackAbility(out act);
    }

    private bool DoSong(out IAction? act)
    {
        switch (Song)
        {
            case Song.WANDERER when SongTimer < WANDRemainTime:
                if (PitchPerfectPvE.CanUse(out act)) return true;
                return MagesBalladPvE.CanUse(out act);

            case Song.MAGE when SongTimer < MAGERemainTime:
                return ArmysPaeonPvE.CanUse(out act);

            default:
            case Song.NONE:
            case Song.ARMY when SongTimer < ARMYRemainTime:
                if (TheWanderersMinuetPvE.CanUse(out act, onLastAbility: true)) return true;
                return MagesBalladPvE.CanUse(out act);
        }
    }

    private bool CanUseApexArrow(out IAction act)
    {
        if (!ApexArrowPvEReplace.CanUse(out act,skipAoeCheck: true)) return false;

        if (QuickNockPvEReplace.CanUse(out _) && SoulVoice == 100) return true;

        if (SoulVoice == 100 && BattleVoicePvE.CD.WillHaveOneCharge(25)) return false;

        if (SoulVoice >= 80 && Player.HasStatus(true, StatusID.RagingStrikes) && Player.WillStatusEnd(10, false, StatusID.RagingStrikes)) return true;

        if (SoulVoice == 100 && Player.HasStatus(true, StatusID.RagingStrikes) && Player.HasStatus(true, StatusID.BattleVoice)) return true;

        if (Song == Song.MAGE && SoulVoice >= 80 && SongTimer < 22 && SongTimer < 18) return true;

        if (!Player.HasStatus(true, StatusID.RagingStrikes) && SoulVoice == 100) return true;

        return false;
    }
}
