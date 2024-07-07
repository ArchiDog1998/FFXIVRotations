namespace DefaultRotations.Melee;

[Rotation("Early Enshroud", CombatType.Both, GameVersion = "6.38")]
[BetaRotation]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/rpr/double_communio.png")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/rpr/rpr_6.3_early_enshroud.png")]
[SourceCode(Path = "main/DefaultRotations/Melee/RPR_Default.cs")]
public sealed class RPR_Default : ReaperRotation
{
    [UI("Enshroud Pooling")]
    [RotationConfig(CombatType.PvE)]
    public bool EnshroudPooling { get; set; } = false;

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < HarpePvE.Info.CastTime + CountDownAhead
            && HarpePvE.CanUse(out var act)) return act;

        if (SoulsowPvE.CanUse(out act)) return act;

        return base.CountDownAction(remainTime);
    }

    private bool Reaping(out IAction? act)
    {
        if (GrimReapingPvE.CanUse(out act)) return true;
        if (Player.HasStatus(true, StatusID.EnhancedCrossReaping) || !Player.HasStatus(true, StatusID.EnhancedVoidReaping))
        {
            if (CrossReapingPvE.CanUse(out act)) return true;
        }
        else
        {
            if (VoidReapingPvE.CanUse(out act)) return true;
        }
        return false;
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

        if (SoulsowPvEReplace.CanUse(out act)) return true;

        if (WhorlOfDeathPvE.CanUse(out act)) return true;
        if (ShadowOfDeathPvE.CanUse(out act)) return true;

        if (HasEnshrouded)
        {
            if (ShadowOfDeathPvE.CanUse(out act)) return true;

            if (LemureShroud > 1)
            {
                if (PlentifulHarvestPvE.EnoughLevel && ArcaneCirclePvE.CD.WillHaveOneCharge(9) &&
                   (LemureShroud == 4 && (HostileTarget?.WillStatusEnd(30, true, StatusID.DeathsDesign) ?? false) || LemureShroud == 3 && (HostileTarget?.WillStatusEnd(50, true, StatusID.DeathsDesign) ?? false)))
                {
                    if (ShadowOfDeathPvE.CanUse(out act, skipStatusProvideCheck: true)) return true;
                }

                if (Reaping(out act)) return true;
            }
            if (LemureShroud == 1)
            {
                if (CommunioPvE.EnoughLevel)
                {
                    if (!IsMoving && CommunioPvEReplace.CanUse(out act, skipAoeCheck: true))
                    {
                        return true;
                    }
                    else
                    {
                        if (ShadowOfDeathPvE.CanUse(out act, skipAoeCheck: IsMoving)) return true;
                    }
                }
                else
                {
                    if (Reaping(out act)) return true;
                }
            }
        }

        if (HasSoulReaver)
        {
            if (GuillotinePvE.CanUse(out act)) return true;
            if (Player.HasStatus(true, StatusID.EnhancedGibbet))
            {
                if (GibbetPvEReplace.CanUse(out act)) return true;
            }
            else
            {
                if (GallowsPvEReplace.CanUse(out act)) return true;
            }
        }

        if (!CombatElapsedLessGCD(2) && PlentifulHarvestPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (SoulScythePvE.CanUse(out act, usedUp: true)) return true;
        if (SoulSlicePvE.CanUse(out act, usedUp: true)) return true;

        if (NightmareScythePvE.CanUse(out act)) return true;
        if (SpinningScythePvE.CanUse(out act)) return true;

        if (InfernalSlicePvE.CanUse(out act)) return true;
        if (WaxingSlicePvE.CanUse(out act)) return true;
        if (SlicePvE.CanUse(out act)) return true;

        if (InCombat && HarvestMoonPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (HarpePvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        #region PvP
        if (GrimSwathePvP.CanUse(out act)) return true;
        if (LemuresSlicePvP.CanUse(out act)) return true;
        if (HarvestMoonPvP.CanUse(out act)) return true;
        #endregion

        var IsTargetBoss = HostileTarget?.IsBossFromTTK() ?? false;
        var IsTargetDying = HostileTarget?.IsDying() ?? false;

        if (IsBurst)
        {
            if (UseBurstMedicine(out act))
            {
                if (CombatElapsedLess(10))
                {
                    if (!CombatElapsedLess(5)) return true;
                }
                else
                {
                    if (ArcaneCirclePvE.CD.WillHaveOneCharge(5)) return true;
                }
            }
            if ((HostileTarget?.HasStatus(true, StatusID.DeathsDesign) ?? false)
                && ArcaneCirclePvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (IsTargetBoss && IsTargetDying ||
           !EnshroudPooling && Shroud >= 50 ||
           EnshroudPooling && Shroud >= 50 &&
           (!PlentifulHarvestPvE.EnoughLevel ||
           Player.HasStatus(true, StatusID.ArcaneCircle) ||
           ArcaneCirclePvE.CD.WillHaveOneCharge(8) ||
           !Player.HasStatus(true, StatusID.ArcaneCircle) && ArcaneCirclePvE.CD.WillHaveOneCharge(65) && !ArcaneCirclePvE.CD.WillHaveOneCharge(50) ||
           !Player.HasStatus(true, StatusID.ArcaneCircle) && Shroud >= 90))
        {
            if (EnshroudPvE.CanUse(out act)) return true;
        }

        if (HasEnshrouded && (Player.HasStatus(true, StatusID.ArcaneCircle) || LemureShroud < 3))
        {
            if (LemuresScythePvE.CanUse(out act, usedUp: true)) return true;
            if (LemuresSlicePvE.CanUse(out act, usedUp: true)) return true;
        }

        if (PlentifulHarvestPvE.EnoughLevel && !Player.HasStatus(true, StatusID.ImmortalSacrifice) && !Player.HasStatus(true, StatusID.BloodsownCircle_2972) || !PlentifulHarvestPvE.EnoughLevel)
        {
            if (GluttonyPvEReplace.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (!Player.HasStatus(true, StatusID.BloodsownCircle_2972) && !Player.HasStatus(true, StatusID.ImmortalSacrifice) && (GluttonyPvE.EnoughLevel && !GluttonyPvE.CD.WillHaveOneChargeGCD(4) || !GluttonyPvE.EnoughLevel || Soul == 100))
        {
            if (GrimSwathePvEReplace.CanUse(out act)) return true;
            if (BloodStalkPvEReplace.CanUse(out act)) return true;
        }

        return base.AttackAbility(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        #region PvP
        if (ArcaneCrestPvP.CanUse(out act)) return true;
        #endregion

        return base.EmergencyAbility(nextGCD, out act);
    }
}