namespace DefaultRotations.Ranged;

[Rotation("369 Opener", CombatType.Both, GameVersion = "7.05", Description = "Work in Progress!")]
[SourceCode(Path = "main/DefaultRotations/Ranged/BRD_Default.cs")]
[LinkDescription("https://i.imgur.com/ZHSg4M0.png")]
[LinkDescription("https://i.imgur.com/40vYKDd.png")]
public sealed class BRD_Default : BardRotation
{
    [Range(1, 5, ConfigUnitType.None)]
    [UI("Iron Jaws Gcd Behind")]
    [RotationConfig(CombatType.PvE)]
    public int IronJawsGcdCount { get; set; } = 3;

    private const float WANDRemainTime = 3, MAGERemainTime = 6, ARMYRemainTime = 9;

    public static bool InBurstStatus => !Player.WillStatusEndGCD(0, 0, true, StatusID.RagingStrikes);

    public BRD_Default()
    {
        RadiantFinalePvE.RotationCheck = () => RagingStrikesPvE.CD.WillHaveOneChargeGCD(0);
        ApexArrowPvE.RotationCheck = () =>
        {
            if (RagingStrikesPvE.CD.WillHaveOneChargeGCD(3)) return false;
            if (SoulVoice == 100) return true;
            if (Song == Song.MAGE && SongTimer < 21 && SoulVoice >= 80) return true;
            return false;
        };
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        #region PvP
        if (BlastArrowPvP.CanUse(out act, skipAoeCheck: true)) return true;
        if (ApexArrowPvP.CanUse(out act, skipAoeCheck: true)) return true;

        if (PitchPerfectPvP.CanUse(out act)) return true;
        if (PowerfulShotPvP.CanUse(out act)) return true;
        #endregion

        if (CombatElapsedLess(5))
        {
            if (WindbitePvEReplace.CanUse(out act)) return true;
            if (VenomousBitePvEReplace.CanUse(out act)) return true;
        }

        if (IronJawsPvE.CanUse(out act)) return true;
        if (IronJawsPvE.CanUse(out act, skipStatusProvideCheck: true)
            && (IronJawsPvE.Target.Target?.WillStatusEnd(30, true, IronJawsPvE.Info.TargetStatusProvide ?? []) ?? false))
        {
            if (Player.HasStatus(true, StatusID.RagingStrikes)
                && Player.WillStatusEndGCD((uint)IronJawsGcdCount, 0, true, StatusID.RagingStrikes)) return true;
        }

        if (ApexArrowPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (WideVolleyPvEReplace.CanUse(out act)) return true;
        if (StraightShotPvEReplace.CanUse(out act)) return true;

        if (InBurstStatus && !CombatElapsedLess(4))
        {
            if (RadiantEncorePvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (ResonantArrowPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }
        if (BlastArrowPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (QuickNockPvEReplace.CanUse(out act)) return true;

        if (WindbitePvEReplace.CanUse(out act)) return true;
        if (VenomousBitePvEReplace.CanUse(out act)) return true;

        if (HeavyShotPvEReplace.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        #region PvP
        if (SilentNocturnePvP.CanUse(out act)) return true;
        if (TheWardensPaeanPvP.CanUse(out act)) return true;

        if (EmpyrealArrowPvP.CanUse(out act, usedUp: true)) return true;

        if (RepellingShotPvP.CanUse(out act)) return true;
        #endregion

        if (BloodletterPvECdGrp.CanUse(out act)) return true;
        if (DoSong(out act)) return true;

        if (IsBurst && Song != Song.NONE)
        {
            if (UseBurstMedicine(out act)) return true;
            if (RadiantFinalePvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (CombatElapsedLess(4)) return false;
            if (BattleVoicePvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (RagingStrikesPvE.CanUse(out act)) return true;
        }

        if (PitchPerfectPvE.CanUse(out act, skipAoeCheck: true) && Repertoire == 3) return true;

        if (EmpyrealArrowPvE.CanUse(out act)) return true;
        if (BarragePvE.CanUse(out act)) return true;

        if (SidewinderPvE.CanUse(out act)) return true;

        if (InBurstStatus)
        {
            if (BloodletterPvECdGrp.CanUse(out act, usedUp: true)) return true;
        }

        return base.AttackAbility(out act);
    }

    private bool DoSong(out IAction? act)
    {
        switch (Song)
        {
            case Song.WANDERER when SongTimer < WANDRemainTime:
                if (PitchPerfectPvE.CanUse(out act, skipAoeCheck: true)) return true;
                return MagesBalladPvE.CanUse(out act);

            case Song.MAGE when SongTimer < MAGERemainTime:
                return ArmysPaeonPvE.CanUse(out act);

            default:
            case Song.NONE:
            case Song.ARMY when SongTimer < ARMYRemainTime:
                return TheWanderersMinuetPvE.EnoughLevel
                    ? TheWanderersMinuetPvE.CanUse(out act, onLastAbility: true)
                    : MagesBalladPvE.CanUse(out act);
        }
    }
}
