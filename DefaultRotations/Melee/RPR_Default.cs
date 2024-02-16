namespace DefaultRotations.Melee;

[Rotation("Early Enshroud", CombatType.PvE, GameVersion = "6.38")]
[BetaRotation]
[RotationDesc(ActionID.ArcaneCirclePvE)]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/rpr/double_communio.png")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/rpr/rpr_6.3_early_enshroud.png")]
[SourceCode(Path = "main/DefaultRotations/Melee/RPR_Default.cs")]
public sealed class RPR_Default : ReaperRotation
{
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
        if (SoulsowPvE.CanUse(out act)) return true;

        if (WhorlOfDeathPvE.CanUse(out act)) return true;
        if (ShadowOfDeathPvE.CanUse(out act)) return true;

        if (HasEnshrouded)
        {
            if (ShadowOfDeathPvE.CanUse(out act)) return true;

            if (LemureShroud > 1)
            {
                if (PlentifulHarvestPvE.EnoughLevel && ArcaneCirclePvE.Cooldown.WillHaveOneCharge(9) &&
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
                    if (!IsMoving && CommunioPvE.CanUse(out act, skipAoeCheck: true))
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
                if (GibbetPvE.CanUse(out act)) return true;
            }
            else
            {
                if (GallowsPvE.CanUse(out act)) return true;
            }
        }

        if (!CombatElapsedLessGCD(2) && PlentifulHarvestPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (SoulScythePvE.CanUse(out act, skipCombo: true)) return true;
        if (SoulSlicePvE.CanUse(out act, skipCombo: true)) return true;

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
                    if (ArcaneCirclePvE.Cooldown.WillHaveOneCharge(5)) return true;
                }
            }
            if ((HostileTarget?.HasStatus(true, StatusID.DeathsDesign) ?? false)
                && ArcaneCirclePvE.CanUse(out act)) return true;
        }

        if (IsTargetBoss && IsTargetDying ||
           !Configs.GetBool("EnshroudPooling") && Shroud >= 50 ||
           Configs.GetBool("EnshroudPooling") && Shroud >= 50 &&
           (!PlentifulHarvestPvE.EnoughLevel ||
           Player.HasStatus(true, StatusID.ArcaneCircle) ||
           ArcaneCirclePvE.Cooldown.WillHaveOneCharge(8) ||
           !Player.HasStatus(true, StatusID.ArcaneCircle) && ArcaneCirclePvE.Cooldown.WillHaveOneCharge(65) && !ArcaneCirclePvE.Cooldown.WillHaveOneCharge(50) ||
           !Player.HasStatus(true, StatusID.ArcaneCircle) && Shroud >= 90))
        {
            if (EnshroudPvE.CanUse(out act)) return true;
        }

        if (HasEnshrouded && (Player.HasStatus(true, StatusID.ArcaneCircle) || LemureShroud < 3))
        {
            if (LemuresScythePvE.CanUse(out act, isEmpty: true)) return true;
            if (LemuresSlicePvE.CanUse(out act, isEmpty: true)) return true;
        }

        if (PlentifulHarvestPvE.EnoughLevel && !Player.HasStatus(true, StatusID.ImmortalSacrifice) && !Player.HasStatus(true, StatusID.BloodsownCircle_2972) || !PlentifulHarvestPvE.EnoughLevel)
        {
            if (GluttonyPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (!Player.HasStatus(true, StatusID.BloodsownCircle_2972) && !Player.HasStatus(true, StatusID.ImmortalSacrifice) && (GluttonyPvE.EnoughLevel && !GluttonyPvE.Cooldown.WillHaveOneChargeGCD(4) || !GluttonyPvE.EnoughLevel || Soul == 100))
        {
            if (GrimSwathePvE.CanUse(out act)) return true;
            if (BloodStalkPvE.CanUse(out act)) return true;
        }

        return base.AttackAbility(out act);
    }
}