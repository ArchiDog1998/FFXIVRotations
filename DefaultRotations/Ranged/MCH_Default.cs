namespace DefaultRotations.Ranged;

[Rotation("Delayed Tools Opener", CombatType.Both, GameVersion = "6.38")]
[BetaRotation]
[SourceCode(Path = "main/DefaultRotations/Ranged/MCH_Default.cs")]
[LinkDescription("https://cdn.discordapp.com/attachments/277968251789639680/1086348727691780226/mch_rotation.png")]
public sealed class MCH_Default : MachinistRotation
{
    [Range(0, 5, ConfigUnitType.Seconds)]
    [RotationConfig(CombatType.PvE)]
    [UI("Reassemble countdown time")]
    public float ReassembleCountDownTime { get; set; } = 5;

    [UI("Use Reassemble with ChainSaw")]
    [RotationConfig(CombatType.PvE)]
    public bool MCH_Reassemble { get; set; } = true;

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < CountDownAhead)
        {
            if (AirAnchorPvE.CanUse(out var act1)) return act1;
            else if (!AirAnchorPvE.EnoughLevel && HotShotPvESet.CanUse(out act1)) return act1;
        }
        if (remainTime < 2 && UseBurstMedicine(out var act)) return act;
        if (remainTime < ReassembleCountDownTime && ReassemblePvE.CanUse(out act, usedUp: true)) return act;
        return base.CountDownAction(remainTime);
    }


    protected override bool GeneralGCD(out IAction? act)
    {
        #region PvP
        if (!Player.HasStatus(true, StatusID.Overheated_3149) && ScattergunPvP.CanUse(out act, skipAoeCheck: true) && HostileTarget.DistanceToPlayer() <= 10) return true;

        if (Player.HasStatus(true, StatusID.Analysis))
        {
            if (Player.HasStatus(true, StatusID.AirAnchorPrimed) && !Player.HasStatus(true, StatusID.BioblasterPrimed, StatusID.ChainSawPrimed, StatusID.DrillPrimed, StatusID.Overheated_3149) && AirAnchorPvP.CanUse(out act, usedUp: true)) return true;
            if (Player.HasStatus(true, StatusID.BioblasterPrimed) && !Player.HasStatus(true, StatusID.AirAnchorPrimed, StatusID.ChainSawPrimed, StatusID.DrillPrimed, StatusID.Overheated_3149) && BioblasterPvP.CanUse(out act, skipAoeCheck: true, usedUp: true)) return true;
            if (Player.HasStatus(true, StatusID.ChainSawPrimed) && !Player.HasStatus(true, StatusID.BioblasterPrimed, StatusID.BioblasterPrimed, StatusID.DrillPrimed, StatusID.Overheated_3149) && ChainSawPvP.CanUse(out act, skipAoeCheck: true)) return true;
            if (Player.HasStatus(true, StatusID.DrillPrimed) && !Player.HasStatus(true, StatusID.BioblasterPrimed, StatusID.ChainSawPrimed, StatusID.AirAnchorPrimed, StatusID.Overheated_3149) && DrillPvP.CanUse(out act, usedUp: true)) return true;
        }

        if (AirAnchorPvP.CD.CurrentCharges == 2 && Player.HasStatus(true, StatusID.AirAnchorPrimed) && !Player.HasStatus(true, StatusID.BioblasterPrimed, StatusID.ChainSawPrimed, StatusID.DrillPrimed, StatusID.Overheated_3149) && AirAnchorPvP.CanUse(out act)) return true;
        if (BioblasterPvP.CD.CurrentCharges == 2 && Player.HasStatus(true, StatusID.BioblasterPrimed) && !Player.HasStatus(true, StatusID.AirAnchorPrimed, StatusID.ChainSawPrimed, StatusID.DrillPrimed, StatusID.Overheated_3149) && BioblasterPvP.CanUse(out act, skipAoeCheck: true)) return true;
        if (ChainSawPvP.CD.CurrentCharges == 2 && Player.HasStatus(true, StatusID.ChainSawPrimed) && !Player.HasStatus(true, StatusID.BioblasterPrimed, StatusID.BioblasterPrimed, StatusID.DrillPrimed, StatusID.Overheated_3149) && ChainSawPvP.CanUse(out act, skipAoeCheck: true)) return true;
        if (DrillPvP.CD.CurrentCharges == 2 && Player.HasStatus(true, StatusID.DrillPrimed) && !Player.HasStatus(true, StatusID.BioblasterPrimed, StatusID.ChainSawPrimed, StatusID.AirAnchorPrimed, StatusID.Overheated_3149) && DrillPvP.CanUse(out act)) return true;

        if (Player.HasStatus(true, StatusID.Overheated_3149))
        {
            if (WildfirePvP.CanUse(out act)) return true;
            if (WildfirePvP.CD.IsCoolingDown
                && BlastChargePvP.CanUse(out act, skipCastingCheck: true)) return true;
            return false;
        }

        if (BlastChargePvP.CanUse(out act, skipCastingCheck: true)) return true;
        #endregion

        //Overheated
        if (AutoCrossbowPvE.CanUse(out act)) return true;
        if (HeatBlastPvESet.CanUse(out act)) return true;

        //Long Cds
        if (BioblasterPvE.CanUse(out act)) return true;
        if (!SpreadShotPvESet.CanUse(out _))
        {
            if (AirAnchorPvE.CanUse(out act)) return true;
            else if (!AirAnchorPvE.EnoughLevel && HotShotPvESet.CanUse(out act)) return true;

            if (DrillPvE.CanUse(out act)) return true;
        }

        if (!CombatElapsedLessGCD(4) && ChainSawPvE.CanUse(out act, skipAoeCheck: true)) return true;

        //Aoe
        if (ChainSawPvESet.CanUse(out act)) return true;
        if (SpreadShotPvESet.CanUse(out act)) return true;

        //Single
        if (CleanShotPvESet.CanUse(out act)) return true;
        if (SlugShotPvESet.CanUse(out act)) return true;
        if (SplitShotPvESet.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (MCH_Reassemble && ChainSawPvE.EnoughLevel && nextGCD.IsTheSameTo(true, ChainSawPvE))
        {
            if (ReassemblePvE.CanUse(out act, usedUp: true)) return true;
        }
        if (RicochetPvESet.CanUse(out act, skipAoeCheck: true)) return true;
        if (GaussRoundPvESet.CanUse(out act, skipAoeCheck: true)) return true;

        if (!DrillPvE.EnoughLevel && nextGCD.IsTheSameTo(true, CleanShotPvE)
            || nextGCD.IsTheSameTo(false, AirAnchorPvE, ChainSawPvE, DrillPvE))
        {
            if (ReassemblePvE.CanUse(out act, usedUp: true)) return true;
        }
        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        #region PvP
        if (BishopAutoturretPvP.CanUse(out act, skipAoeCheck: true)) return true;

        if (Player.HasStatus(true, StatusID.Overheated_3149) 
            && WildfirePvP.CanUse(out act, skipAoeCheck: true, skipComboCheck: true)) return true;

        if (InCombat && !Player.HasStatus(true, StatusID.Analysis) &&
            (BioblasterPvP.CanUse(out _) && HostileTarget.DistanceToPlayer() <= 12 || AirAnchorPvP.CanUse(out act) || ChainSawPvP.CanUse(out _)) 
            && AnalysisPvP.CanUse(out act, usedUp: true)) return true;
        #endregion

        if (IsBurst)
        {
            if (UseBurstMedicine(out act)) return true;
            if ((IsLastAbility(false, HyperchargePvE) || Heat >= 50) && !CombatElapsedLess(10)
                && WildfirePvESet.CanUse(out act, onLastAbility: true)) return true;
        }

        if (!CombatElapsedLess(12) && CanUseHypercharge(out act)) return true;
        if (CanUseRookAutoturret(out act)) return true;

        if (BarrelStabilizerPvESet.CanUse(out act)) return true;

        if (CombatElapsedLess(8)) return false;

        if (GaussRoundPvE.CD.CurrentCharges <= RicochetPvE.CD.CurrentCharges)
        {
            if (RicochetPvESet.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;
        }
        if (GaussRoundPvESet.CanUse(out act, usedUp: true, skipAoeCheck: true)) return true;

        return base.AttackAbility(out act);
    }

    private bool CanUseRookAutoturret(out IAction? act)
    {
        act = null;
        if (AirAnchorPvE.EnoughLevel)
        {
            if (!AirAnchorPvE.CD.IsCoolingDown || AirAnchorPvE.CD.ElapsedAfter(18)) return false;
        }
        else
        {
            if (!HotShotPvE.CD.IsCoolingDown || HotShotPvE.CD.ElapsedAfter(18)) return false;
        }

        return RookAutoturretPvESet.CanUse(out act);
    }

    const float REST_TIME = 6f;
    private bool CanUseHypercharge(out IAction? act)
    {
        act = null;

        //Check recast.
        if (!SpreadShotPvESet.CanUse(out _))
        {
            if (AirAnchorPvE.EnoughLevel)
            {
                if (AirAnchorPvE.CD.WillHaveOneCharge(REST_TIME)) return false;
            }
            else
            {
                if (HotShotPvE.EnoughLevel && HotShotPvE.CD.WillHaveOneCharge(REST_TIME)) return false;
            }
        }
        if (DrillPvE.EnoughLevel && DrillPvE.CD.WillHaveOneCharge(REST_TIME)) return false;
        if (ChainSawPvE.EnoughLevel && ChainSawPvE.CD.WillHaveOneCharge(REST_TIME)) return false;

        return HyperchargePvE.CanUse(out act);
    }
}
