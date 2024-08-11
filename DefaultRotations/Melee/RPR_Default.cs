namespace DefaultRotations.Melee;

[Rotation("2nd GCD AC", CombatType.Both, GameVersion = "7.05")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/rpr/second-gcd-ac.png")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/rpr/standardouble.png")]
[SourceCode(Path = "main/DefaultRotations/Melee/RPR_Default.cs")]
public sealed class RPR_Default : ReaperRotation
{
    public static bool InBurstStatus => Player.HasStatus(true, StatusID.ArcaneCircle);

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < HarpePvE.Info.CastTime + CountDownAhead
            && HarpePvE.CanUse(out var act)) return act;

        if (SoulsowPvE.CanUse(out act)) return act;

        return base.CountDownAction(remainTime);
    }

    protected override bool EmergencyGCD(out IAction? act)
    {
        if (HostileTarget != null && HostileTarget.WillStatusEnd(30, true, StatusID.DeathsDesign)
            && ArcaneCirclePvE.CD.WillHaveOneCharge(5)
            && !CombatElapsedLess(5))
        {
            if (UseDeathActions(out act, true)) return true;
        }

        if (UseEnshroudedGCD(out act)) return true;

        if (PerfectioPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (PlentifulHarvestPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (UseGThings(out act)) return true;
        return base.EmergencyGCD(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        #region PvP
        if (SoulSlicePvP.CanUse(out act, usedUp: true)) return true;

        if (PlentifulHarvestPvP.CanUse(out act)) return true;

        if (InfernalSlicePvP.CanUse(out act)) return true;
        if (WaxingSlicePvP.CanUse(out act)) return true;
        if (SlicePvP.CanUse(out act)) return true;
        #endregion

        if (!ArcaneCirclePvE.CD.IsCoolingDown || !ArcaneCirclePvE.CD.WillHaveOneCharge(8))
        {
            if (UseDeathActions(out act, false)) return true;
        }

        //Soul Getter
        if (SoulSlicePvECdGrp.CanUse(out act, usedUp: CombatElapsedLess(10) || !InBurstStatus)) return true;

        //Basic
        if (NightmareScythePvECombo.CanUse(out act)) return true;
        if (InfernalSlicePvECombo.CanUse(out act)) return true;

        //Range
        if (SoulsowPvEReplace.CanUse(out act, skipAoeCheck: true)) return true;
        if (HarpePvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        #region PvP
        if (ArcaneCrestPvP.CanUse(out act)) return true;
        #endregion

        if (!CombatElapsedLess(2))
        {
            if (ArcaneCirclePvE.CanUse(out act, skipAoeCheck: true, onLastAbility: true)) return true;
        }

        var cd = ArcaneCirclePvE.CD;
        if (!cd.IsCoolingDown || !cd.ElapsedAfter(110))
        {
            if (UseEnshroudedAbility(out act)) return true;
        }
        if (UseBurstMedicine(out act, onLastAbility: true)) return true;

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        #region PvP
        if (GrimSwathePvP.CanUse(out act)) return true;
        if (LemuresSlicePvP.CanUse(out act)) return true;
        if (HarvestMoonPvP.CanUse(out act)) return true;
        #endregion

        if (!Player.HasStatus(true, StatusID.PerfectioParata, StatusID.ImmortalSacrifice,
            StatusID.SoulReaver, StatusID.Enshrouded))
        {
            var cd = ArcaneCirclePvE.CD;
            if (cd.IsCoolingDown && (!cd.ElapsedAfter(15) || cd.ElapsedAfter(60) && !cd.ElapsedAfter(75))
                || cd.WillHaveOneCharge(6))
            {
                if (EnshroudPvE.CanUse(out act)) return true;
            }

            if (GluttonyPvE.CanUse(out act, skipAoeCheck: true)) return true;
            if (UseSoulReaverGetter(out act)) return true;
        }


        return base.AttackAbility(out act);
    }
}