namespace DefaultRotations.Healer;

[Rotation("Default", CombatType.Both, GameVersion = "6.28")]
[SourceCode(Path = "main/DefaultRotations/Healer/WHM_Default.cs")]
public sealed class WHM_Default :WhiteMageRotation
{
    [UI("Use Lily at max stacks.")]
    [RotationConfig(CombatType.PvE)]
    public bool UseLilyWhenFull { get; set; } = true;

    [UI("Regen on Tank at 5 seconds remaining on Countdown.")]
    [RotationConfig(CombatType.PvE)]
    public bool UsePreRegen { get; set; } = true;

    public WHM_Default()
    {
        AfflatusRapturePvE.RotationCheck = () => BloodLily < 3;
        AfflatusSolacePvE.RotationCheck = () => BloodLily < 3;
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        #region PvP
        if (AfflatusMiseryPvP.CanUse(out act, skipAoeCheck: true)) return true;
        if (GlareIiiPvP.CanUse(out act)) return true;
        if (CureIiPvP.CanUse(out act)) return true;
        #endregion

        //if (NotInCombatDelay && RegenDefense.CanUse(out act)) return true;

        if (AfflatusMiseryPvE.CanUse(out act, skipAoeCheck: true)) return true;

        bool liliesNearlyFull = Lily == 2 && LilyTimer > 13;
        bool liliesFullNoBlood = Lily == 3;
        if (UseLilyWhenFull && (liliesNearlyFull || liliesFullNoBlood) && AfflatusMiseryPvE.EnoughLevel && BloodLily < 3)
        {
            if (UseLily(out act)) return true;
        }

        if (HolyPvEReplace.CanUse(out act)) return true;

        if (AeroPvEReplace.CanUse(out act)) return true;

        if (StonePvE.CanUse(out act)) return true;

        if (Lily >= 2)
        {
            if (UseLily(out act)) return true;
        }

        if (AeroPvEReplace.CanUse(out act, skipStatusProvideCheck: true)) return true;

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
        #region PvP
        if (SeraphStrikePvP.CanUse(out act)) return true;
        #endregion

        if (InCombat)
        {
            if (PresenceOfMindPvEReplace.CanUse(out act)) return true;
            if (AssizePvE.CanUse(out act, skipAoeCheck: true)) return true;
        }

        return base.AttackAbility(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        #region PvP
        if (MiracleOfNaturePvP.CanUse(out act)) return true;
        #endregion

        if (nextGCD is IBaseAction action && action.Info.MPNeed >= 1000 &&
            ThinAirPvE.CanUse(out act)) return true;

        if (nextGCD.IsTheSameTo(true, AfflatusRapturePvE, MedicaPvE, MedicaIiPvE, CureIiiPvE)
            && (MergedStatusState.HasFlag(AutoStatus.HealAreaSpell) || MergedStatusState.HasFlag(AutoStatus.HealSingleSpell)))
        {
            if (PlenaryIndulgencePvE.CanUse(out act)) return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool HealSingleGCD(out IAction? act)
    {
        if (AfflatusSolacePvE.CanUse(out act)) return true;

        if (RegenPvE.CanUse(out act)
            && (IsMoving || RegenPvE.Target.Target?.GetHealthRatio() > 0.4)) return true;

        if (CureIiPvE.CanUse(out act)) return true;

        if (CurePvE.CanUse(out act)) return true;

        return base.HealSingleGCD(out act);
    }

    protected override bool HealSingleAbility(out IAction? act)
    {
        if (BenedictionPvE.CanUse(out act) &&
            RegenPvE.Target.Target?.GetHealthRatio() < 0.3) return true;

        if (!IsMoving && AsylumPvE.CanUse(out act)) return true;

        if (DivineBenisonPvE.CanUse(out act)) return true;

        if (TetragrammatonPvE.CanUse(out act)) return true;
        return base.HealSingleAbility(out act);
    }

    protected override bool HealAreaGCD(out IAction? act)
    {
        if (AfflatusRapturePvE.CanUse(out act)) return true;

        int hasMedica2 = PartyMembers.Count((n) => n.HasStatus(true, StatusID.MedicaIi));

        if (MedicaIiPvEReplace.CanUse(out act) && hasMedica2 < PartyMembers.Count() / 2 && !IsLastAction(true, MedicaIiPvE)) return true;

        if (CureIiiPvE.CanUse(out act)) return true;

        if (MedicaPvE.CanUse(out act)) return true;

        return base.HealAreaGCD(out act);
    }

    protected override bool HealAreaAbility(out IAction? act)
    {
        if (AsylumPvE.CanUse(out act)) return true;
        return base.HealAreaAbility(out act);
    }

    protected override bool DefenseSingleAbility(out IAction? act)
    {
        act = null;
        if (DivineBenisonPvE.CD.IsCoolingDown && !DivineBenisonPvE.CD.WillHaveOneCharge(15)
            || AquaveilPvE.CD.IsCoolingDown && !AquaveilPvE.CD.WillHaveOneCharge(52)) return false;

        if (DivineBenisonPvE.CanUse(out act)) return true;

        if (AquaveilPvE.CanUse(out act)) return true;
        return base.DefenseSingleAbility(out act);
    }

    protected override bool DefenseAreaAbility(out IAction? act)
    {
        act = null;
        if (TemperancePvE.CD.IsCoolingDown && !TemperancePvE.CD.WillHaveOneCharge(100)
            || LiturgyOfTheBellPvE.CD.IsCoolingDown && !LiturgyOfTheBellPvE.CD.WillHaveOneCharge(160)) return false;

        if (TemperancePvEReplace.CanUse(out act)) return true;

        if (LiturgyOfTheBellPvEReplace.CanUse(out act, skipAoeCheck: true)) return true;
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

    protected override bool GeneralAbility(out IAction? act)
    {
        #region PvP
        if (AquaveilPvP.CanUse(out act)) return true;
        #endregion
        return base.GeneralAbility(out act);
    }
}