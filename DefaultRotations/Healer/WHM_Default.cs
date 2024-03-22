namespace DefaultRotations.Healer;

[Rotation("Default", CombatType.PvE, GameVersion = "6.28")]
[SourceCode(Path = "main/DefaultRotations/Healer/WHM_Default.cs")]
public sealed class WHM_Default :WhiteMageRotation
{
    [RotationConfig(CombatType.PvE, Name = "Use Lily at max stacks.")]
    public bool UseLilyWhenFull { get; set; } = true;

    [RotationConfig(CombatType.PvE, Name = "Regen on Tank at 5 seconds remaining on Countdown.")]
    public bool UsePreRegen { get; set; } = true;

    public WHM_Default()
    {
        AfflatusRapturePvE.Setting.RotationCheck = () => BloodLily < 3;
        AfflatusSolacePvE.Setting.RotationCheck = () => BloodLily < 3;
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        //if (NotInCombatDelay && RegenDefense.CanUse(out act)) return true;

        if (AfflatusMiseryPvE.CanUse(out act, skipAoeCheck: true)) return true;

        bool liliesNearlyFull = Lily == 2 && LilyTime > 13;
        bool liliesFullNoBlood = Lily == 3;
        if (UseLilyWhenFull && (liliesNearlyFull || liliesFullNoBlood) && AfflatusMiseryPvE.EnoughLevel && BloodLily < 3)
        {
            if (UseLily(out act)) return true;
        }

        if (HolyPvE.CanUse(out act)) return true;

        if (AeroPvE.CanUse(out act)) return true;

        if (StonePvE.CanUse(out act)) return true;

        if (Lily >= 2)
        {
            if (UseLily(out act)) return true;
        }

        if (AeroPvE.CanUse(out act, skipStatusProvideCheck: true)) return true;

        return base.GeneralGCD(out act);
    }

    private bool UseLily(out IAction? act)
    {
        if (AfflatusRapturePvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (AfflatusSolacePvE.CanUse(out act)) return true;
        return false;
    }

    protected override bool AttackAbility(out IAction? act)
    {
        if (InCombat)
        {
            if (PresenceOfMindPvE.CanUse(out act)) return true;
            if (AssizePvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        return base.AttackAbility(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (nextGCD is IBaseAction action && action.Info.MPNeed >= 1000 &&
            ThinAirPvE.CanUse(out act)) return true;

        if (nextGCD.IsTheSameTo(true, AfflatusRapturePvE, MedicaPvE, MedicaIiPvE, CureIiiPvE)
            && (MergedStatus.HasFlag(AutoStatus.HealAreaSpell) || MergedStatus.HasFlag(AutoStatus.HealSingleSpell)))
        {
            if (PlenaryIndulgencePvE.CanUse(out act)) return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.AfflatusSolacePvE, ActionID.RegenPvE, ActionID.CureIiPvE, ActionID.CurePvE)]
    protected override bool HealSingleGCD(out IAction? act)
    {
        if (AfflatusSolacePvE.CanUse(out act)) return true;

        if (RegenPvE.CanUse(out act)
            && (IsMoving || RegenPvE.Target.Target?.GetHealthRatio() > 0.4)) return true;

        if (CureIiPvE.CanUse(out act)) return true;

        if (CurePvE.CanUse(out act)) return true;

        return base.HealSingleGCD(out act);
    }

    [RotationDesc(ActionID.BenedictionPvE, ActionID.AsylumPvE, ActionID.DivineBenisonPvE, ActionID.TetragrammatonPvE)]
    protected override bool HealSingleAbility(out IAction? act)
    {
        if (BenedictionPvE.CanUse(out act) &&
            RegenPvE.Target.Target?.GetHealthRatio() < 0.3) return true;

        if (!IsMoving && AsylumPvE.CanUse(out act)) return true;

        if (DivineBenisonPvE.CanUse(out act)) return true;

        if (TetragrammatonPvE.CanUse(out act)) return true;
        return base.HealSingleAbility(out act);
    }

    [RotationDesc(ActionID.AfflatusRapturePvE, ActionID.MedicaIiPvE, ActionID.CureIiiPvE, ActionID.MedicaPvE)]
    protected override bool HealAreaGCD(out IAction? act)
    {
        if (AfflatusRapturePvE.CanUse(out act)) return true;

        int hasMedica2 = PartyMembers.Count((n) => n.HasStatus(true, StatusID.MedicaIi));

        if (MedicaIiPvE.CanUse(out act) && hasMedica2 < PartyMembers.Count() / 2 && !IsLastAction(true, MedicaIiPvE)) return true;

        if (CureIiiPvE.CanUse(out act)) return true;

        if (MedicaPvE.CanUse(out act)) return true;

        return base.HealAreaGCD(out act);
    }

    [RotationDesc(ActionID.AsylumPvE)]
    protected override bool HealAreaAbility(out IAction? act)
    {
        if (AsylumPvE.CanUse(out act)) return true;
        return base.HealAreaAbility(out act);
    }

    [RotationDesc(ActionID.DivineBenisonPvE, ActionID.AquaveilPvE)]
    protected override bool DefenseSingleAbility(out IAction? act)
    {
        act = null;
        if (DivineBenisonPvE.Cooldown.IsCoolingDown && !DivineBenisonPvE.Cooldown.WillHaveOneCharge(15)
            || AquaveilPvE.Cooldown.IsCoolingDown && !AquaveilPvE.Cooldown.WillHaveOneCharge(52)) return false;

        if (DivineBenisonPvE.CanUse(out act)) return true;

        if (AquaveilPvE.CanUse(out act)) return true;
        return base.DefenseSingleAbility(out act);
    }

    [RotationDesc(ActionID.TemperancePvE, ActionID.LiturgyOfTheBellPvE)]
    protected override bool DefenseAreaAbility(out IAction? act)
    {
        act = null;
        if (TemperancePvE.Cooldown.IsCoolingDown && !TemperancePvE.Cooldown.WillHaveOneCharge(100)
            || LiturgyOfTheBellPvE.Cooldown.IsCoolingDown && !LiturgyOfTheBellPvE.Cooldown.WillHaveOneCharge(160)) return false;

        if (TemperancePvE.CanUse(out act)) return true;

        if (LiturgyOfTheBellPvE.CanUse(out act, skipAoeCheck: true)) return true;
        return base.DefenseAreaAbility(out act);
    }

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < StonePvE.Info.CastTime + CountDownAhead
            && StonePvE.CanUse(out var act)) return act;

        if (UsePreRegen && remainTime <= 5 && remainTime > 3)
        {
            if (RegenPvE.CanUse(out act)) return act;
            if (DivineBenisonPvE.CanUse(out act)) return act;
        }
        return base.CountDownAction(remainTime);
    }

    //public override void DisplayStatus()
    //{
    //    ImGui.Text(LilyTime.ToString());
    //}
}