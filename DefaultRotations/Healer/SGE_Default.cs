namespace DefaultRotations.Healer;

[Rotation("Default", CombatType.PvE, GameVersion = "6.28",
    Description = "Please contact Nore#7219 on Discord for questions about this rotation.")]
[SourceCode(Path = "main/DefaultRotations/Healer/SGE_Default.cs")]
public sealed class SGE_Default : SageRotation
{
    [RotationConfig(CombatType.PvE, Name = "Use spells with cast times to heal.")]
    public bool GCDHeal { get; set; } = false;

    public override bool CanHealSingleSpell => base.CanHealSingleSpell && (GCDHeal || PartyMembers.GetJobCategory(JobRole.Healer).Count() < 2);
    public override bool CanHealAreaSpell => base.CanHealAreaSpell && (GCDHeal || PartyMembers.GetJobCategory(JobRole.Healer).Count() < 2);

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime <= 1.5 && DosisPvE.CanUse(out var act)) return act;
        if (remainTime <= 3 && UseBurstMedicine(out act)) return act;
        return base.CountDownAction(remainTime);
    }
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (base.EmergencyAbility(nextGCD, out act)) return true;

        if (nextGCD.IsTheSameTo(false, PneumaPvE, EukrasianDiagnosisPvE,
            EukrasianPrognosisPvE, DiagnosisPvE, PrognosisPvE))
        {
            if (ZoePvE.CanUse(out act)) return true;
        }

        if (nextGCD.IsTheSameTo(false, PneumaPvE))
        {
            if (KrasisPvE.CanUse(out act)) return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.HaimaPvE, ActionID.TaurocholePvE, ActionID.PanhaimaPvE, ActionID.KeracholePvE, ActionID.HolosPvE)]
    protected override bool DefenseSingleAbility(out IAction? act)
    {
        if (Addersgall <= 1)
        {
            if (HaimaPvE.CanUse(out act, onLastAbility: true)) return true;
        }

        if (TaurocholePvE.CanUse(out act, onLastAbility: true) && TaurocholePvE.Target?.Target?.GetHealthRatio() < 0.8) return true;

        if (Addersgall <= 1)
        {
            if ((!HaimaPvE.EnoughLevel || HaimaPvE.Cooldown.ElapsedAfter(20)) && PanhaimaPvE.CanUse(out act, onLastAbility: true)) return true;
        }

        if ((!TaurocholePvE.EnoughLevel || TaurocholePvE.Cooldown.ElapsedAfter(20)) && KeracholePvE.CanUse(out act, onLastAbility: true)) return true;

        if (HolosPvE.CanUse(out act, onLastAbility: true)) return true;

        return base.DefenseSingleAbility(out act);
    }

    [RotationDesc(ActionID.EukrasianDiagnosisPvE)]
    protected override bool DefenseSingleGCD(out IAction? act)
    {
        if (EukrasianDiagnosisPvE.CanUse(out act))
        {
            if (EukrasianDiagnosisPvE.Target?.Target?.HasStatus(false,
                StatusID.EukrasianDiagnosis_2865,
                StatusID.EukrasianPrognosis_2866,
                StatusID.Galvanize
            ) ?? false) return false;

            if (EukrasiaPvE.CanUse(out act)) return true;

            act = EukrasianDiagnosisPvE;
            return true;
        }

        return base.DefenseSingleGCD(out act);
    }

    [RotationDesc(ActionID.PanhaimaPvE, ActionID.KeracholePvE, ActionID.HolosPvE)]
    protected override bool DefenseAreaAbility(out IAction? act)
    {
        if (Addersgall <= 1)
        {
            if (PanhaimaPvE.CanUse(out act, onLastAbility: true)) return true;
        }

        if (KeracholePvE.CanUse(out act, onLastAbility: true)) return true;

        if (HolosPvE.CanUse(out act, onLastAbility: true)) return true;

        return base.DefenseAreaAbility(out act);
    }

    [RotationDesc(ActionID.EukrasianPrognosisPvE)]
    protected override bool DefenseAreaGCD(out IAction? act)
    {
        if (EukrasianPrognosisPvE.CanUse(out act))
        {
            if (EukrasianDiagnosisPvE.Target?.Target?.HasStatus(false,
                StatusID.EukrasianDiagnosis_2865,
                StatusID.EukrasianPrognosis_2866,
                StatusID.Galvanize
            ) ?? false) return false;

            if (EukrasiaPvE.CanUse(out act)) return true;

            act = EukrasianPrognosisPvE;
            return true;
        }

        return base.DefenseAreaGCD(out act);
    }

    protected override bool GeneralAbility(out IAction? act)
    {
        if (KardiaPvE.CanUse(out act)) return true;

        if (Addersgall <= 1 && RhizomataPvE.CanUse(out act)) return true;

        if (SoteriaPvE.CanUse(out act) && PartyMembers.Any(b => b.HasStatus(true, StatusID.Kardion) && b.GetHealthRatio() < HealthSingleAbility)) return true;

        if (PepsisPvE.CanUse(out act)) return true;

        return base.GeneralAbility(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        if (HostileTarget?.IsBossFromTTK() ?? false)
        {
            if (EukrasianDosisPvE.CanUse(out _, skipCastingCheck: true))
            {
                if (EukrasiaPvE.CanUse(out act, skipCastingCheck: true)) return true;
                if (DosisPvE.CanUse(out act))
                {
                    DosisPvE.Target = EukrasianDosisPvE.Target;
                    return true;
                }
            }
        }

        if (PhlegmaIiiPvE.CanUse(out act, usedUp: IsMoving, skipAoeCheck: true)) return true;
        if (!PhlegmaIiiPvE.EnoughLevel && PhlegmaIiPvE.CanUse(out act, usedUp: IsMoving, skipAoeCheck: true)) return true;
        if (!PhlegmaIiPvE.EnoughLevel && PhlegmaPvE.CanUse(out act, usedUp: IsMoving, skipAoeCheck: true)) return true;

        if (PartyMembers.Any(b => b.GetHealthRatio() < 0.20f) || PartyMembers.GetJobCategory(JobRole.Tank).Any(t => t.GetHealthRatio() < 0.6f))
        {
            if (PneumaPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (IsMoving && ToxikonPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (DyskrasiaPvE.CanUse(out act)) return true;

        if (EukrasianDosisPvE.CanUse(out _, skipCastingCheck: true))
        {
            if (EukrasiaPvE.CanUse(out act, skipCastingCheck: true)) return true;
            if (DosisPvE.CanUse(out act))
            {
                DosisPvE.Target = EukrasianDosisPvE.Target;
                return true;
            }
        }

        if (DosisPvE.CanUse(out act)) return true;

        if (EukrasianDiagnosisPvE.CanUse(out _) && (EukrasianDiagnosisPvE.Target?.Target?.IsJobCategory(JobRole.Tank) ?? false))
        {
            if (EukrasiaPvE.CanUse(out act)) return true;
            act = EukrasianDiagnosisPvE;
            return true;
        }

        return base.GeneralGCD(out act);
    }

    [RotationDesc(ActionID.TaurocholePvE, ActionID.KeracholePvE, ActionID.DruocholePvE, ActionID.HolosPvE, ActionID.PhysisPvE, ActionID.PanhaimaPvE)]
    protected override bool HealSingleAbility(out IAction? act)
    {
        if (TaurocholePvE.CanUse(out act)) return true;

        if (KeracholePvE.CanUse(out act) && EnhancedKeracholeTrait.EnoughLevel) return true;

        if ((!TaurocholePvE.EnoughLevel || TaurocholePvE.Cooldown.IsCoolingDown) && DruocholePvE.CanUse(out act)) return true;

        if (SoteriaPvE.CanUse(out act) && PartyMembers.Any(b => b.HasStatus(true, StatusID.Kardion) && b.GetHealthRatio() < 0.85f)) return true;


        var tank = PartyMembers.GetJobCategory(JobRole.Tank);
        if (Addersgall < 1 && (tank.Any(t => t.GetHealthRatio() < 0.65f) || PartyMembers.Any(b => b.GetHealthRatio() < 0.20f)))
        {
            if (HaimaPvE.CanUse(out act, onLastAbility: true)) return true;

            if (PhysisIiPvE.CanUse(out act)) return true;
            if (!PhysisIiPvE.EnoughLevel && PhysisPvE.CanUse(out act)) return true;

            if (HolosPvE.CanUse(out act, onLastAbility: true)) return true;

            if ((!HaimaPvE.EnoughLevel || HaimaPvE.Cooldown.ElapsedAfter(20)) && PanhaimaPvE.CanUse(out act, onLastAbility: true)) return true;
        }

        if (tank.Any(t => t.GetHealthRatio() < 0.60f))
        {
            if (ZoePvE.CanUse(out act)) return true;
        }

        if (tank.Any(t => t.GetHealthRatio() < 0.70f) || PartyMembers.Any(b => b.GetHealthRatio() < 0.30f))
        {
            if (KrasisPvE.CanUse(out act)) return true;
        }

        if (KeracholePvE.CanUse(out act)) return true;

        return base.HealSingleAbility(out act);
    }

    [RotationDesc(ActionID.DiagnosisPvE)]
    protected override bool HealSingleGCD(out IAction? act)
    {
        if (DiagnosisPvE.CanUse(out act)) return true;
        return base.HealSingleGCD(out act);
    }

    [RotationDesc(ActionID.PneumaPvE, ActionID.PrognosisPvE, ActionID.EukrasianPrognosisPvE)]
    protected override bool HealAreaGCD(out IAction? act)
    {
        if (PartyMembersAverHP < 0.65f || DyskrasiaPvE.CanUse(out _) && PartyMembers.GetJobCategory(JobRole.Tank).Any(t => t.GetHealthRatio() < 0.6f))
        {
            if (PneumaPvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        if (Player.HasStatus(false, StatusID.EukrasianDiagnosis, StatusID.EukrasianPrognosis, StatusID.Galvanize))
        {
            if (PrognosisPvE.CanUse(out act)) return true;
        }

        if (EukrasianPrognosisPvE.CanUse(out _))
        {
            if (EukrasiaPvE.CanUse(out act)) return true;
            act = EukrasianPrognosisPvE;
            return true;
        }

        return base.HealAreaGCD(out act);
    }

    [RotationDesc(ActionID.KeracholePvE, ActionID.PhysisPvE, ActionID.HolosPvE, ActionID.IxocholePvE)]
    protected override bool HealAreaAbility(out IAction? act)
    {
        if (PhysisIiPvE.CanUse(out act)) return true;
        if (!PhysisIiPvE.EnoughLevel && PhysisPvE.CanUse(out act)) return true;

        if (KeracholePvE.CanUse(out act, onLastAbility: true) && EnhancedKeracholeTrait.EnoughLevel) return true;

        if (HolosPvE.CanUse(out act, onLastAbility: true) && PartyMembersAverHP < 0.50f) return true;

        if (IxocholePvE.CanUse(out act, onLastAbility: true)) return true;

        if (KeracholePvE.CanUse(out act, onLastAbility: true)) return true;

        return base.HealAreaAbility(out act);
    }
}
