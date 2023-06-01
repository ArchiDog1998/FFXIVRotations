namespace DefaultRotations.Healer;

[SourceCode("https://github.com/ArchiDog1998/FFXIVRotations/blob/main/DefaultRotations/Healer/WHM_Default.cs")]
public sealed class WHM_Default : WHM_Base
{
    public override string GameVersion => "6.28";

    public override string RotationName => "Default";

    protected override IRotationConfigSet CreateConfiguration()
        => base.CreateConfiguration()
            .SetBool("UseLilyWhenFull", true, "Use Lily at max stacks.")
            .SetBool("UsePreRegen", false, "Regen on Tank at 5 seconds remaining on Countdown.");
    public static IBaseAction RegenDefense { get; } = new BaseAction(ActionID.Regen, ActionOption.Hot)
    {
        ChoiceTarget = TargetFilter.FindAttackedTarget,
        ActionCheck = (b, m) => b.IsJobCategory(JobRole.Tank),
        TargetStatus = Regen.TargetStatus,
    };

    protected override bool GeneralGCD(out IAction act)
    {
        if (NotInCombatDelay && RegenDefense.CanUse(out act)) return true;

        if (AfflatusMisery.CanUse(out act, CanUseOption.MustUse)) return true;

        bool liliesNearlyFull = Lily == 2 && LilyAfter(17);
        bool liliesFullNoBlood = Lily == 3 && BloodLily < 3;
        if (Configs.GetBool("UseLilyWhenFull") && (liliesNearlyFull || liliesFullNoBlood) && AfflatusMisery.EnoughLevel)
        {
            if (PartyMembersAverHP < 0.7)
            {
                if (AfflatusRapture.CanUse(out act)) return true;
            }
            if (AfflatusSolace.CanUse(out act)) return true;
        }

        if (Holy.CanUse(out act)) return true;

        if (Aero.CanUse(out act, IsMoving ? CanUseOption.MustUse : CanUseOption.None)) return true;
        if (Stone.CanUse(out act)) return true;

        act = null;
        return false;
    }

    protected override bool AttackAbility(out IAction act)
    {
        if (PresenseOfMind.CanUse(out act)) return true;

        if (NumberOfHostilesIn(Assize.EffectRange) / (float)NumberOfHostilesInMaxRange > 0.8f 
            && Assize.CanUse(out act, CanUseOption.MustUse)) return true;

        return false;
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction act)
    {
        if (nextGCD is IBaseAction action && action.MPNeed >= 1000 &&
            ThinAir.CanUse(out act)) return true;

        if (nextGCD.IsTheSameTo(true, AfflatusRapture, Medica, Medica2, Cure3))
        {
            if (PlenaryIndulgence.CanUse(out act)) return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.AfflatusSolace, ActionID.Regen, ActionID.Cure2, ActionID.Cure)]
    protected override bool HealSingleGCD(out IAction act)
    {
        if (AfflatusSolace.CanUse(out act)) return true;

        if (Regen.CanUse(out act)
            && (IsMoving || Regen.Target.GetHealthRatio() > 0.4)) return true;

        if (Cure2.CanUse(out act)) return true;

        if (Cure.CanUse(out act)) return true;

        return false;
    }

    [RotationDesc(ActionID.Benediction, ActionID.Asylum, ActionID.DivineBenison, ActionID.Tetragrammaton)]
    protected override bool HealSingleAbility(out IAction act)
    {
        if (Benediction.CanUse(out act) &&
            Benediction.Target.GetHealthRatio() < 0.3) return true;

        if (!IsMoving && Asylum.CanUse(out act)) return true;

        if (DivineBenison.CanUse(out act)) return true;

        if (Tetragrammaton.CanUse(out act)) return true;
        return base.HealSingleAbility(out act);
    }

    [RotationDesc(ActionID.AfflatusRapture, ActionID.Medica2, ActionID.Cure3, ActionID.Medica)]
    protected override bool HealAreaGCD(out IAction act)
    {
        if (AfflatusRapture.CanUse(out act)) return true;

        int hasMedica2 = PartyMembers.Count((n) => n.HasStatus(true, StatusID.Medica2));

        if (Medica2.CanUse(out act) && hasMedica2 < PartyMembers.Count() / 2 && !IsLastAction(true, Medica2)) return true;

        if (Cure3.CanUse(out act)) return true;

        if (Medica.CanUse(out act)) return true;

        return false;
    }

    [RotationDesc(ActionID.Asylum)]
    protected override bool HealAreaAbility(out IAction act)
    {
        if (Asylum.CanUse(out act)) return true;
        return false;
    }

    [RotationDesc(ActionID.DivineBenison, ActionID.Aquaveil)]
    protected override bool DefenseSingleAbility(out IAction act)
    {
        if (DivineBenison.CanUse(out act)) return true;

        if (Aquaveil.CanUse(out act)) return true;
        return false;
    }

    [RotationDesc(ActionID.Temperance, ActionID.LiturgyOfTheBell)]
    protected override bool DefenseAreaAbility(out IAction act)
    {
        if (Temperance.CanUse(out act)) return true;

        if (LiturgyOfTheBell.CanUse(out act)) return true;
        return false;
    }

    //[RotationDesc(ActionID.Regen)]
    //protected override bool DefenseSingleGCD(out IAction act)
    //{
    //    if (RegenDefense.CanUse(out act)) return true;
    //    return base.DefenseSingleGCD(out act);
    //}

    //protected override bool DefenseAreaGCD(out IAction act)
    //{
    //    if (Medica2.CanUse(out act) && PartyMembers.Count((n) => n.HasStatus(true, StatusID.Medica2)) < PartyMembers.Count() / 2) return true;
    //    return base.DefenseAreaGCD(out act);
    //}

    protected override IAction CountDownAction(float remainTime)
    {
        if (remainTime < Stone.CastTime + Service.Config.CountDownAhead
            && Stone.CanUse(out var act)) return act;

        if (Configs.GetBool("UsePreRegen") && remainTime <= 5 && remainTime > 3)
        {
            if (RegenDefense.CanUse(out act, CanUseOption.IgnoreClippingCheck)) return act;
            if (DivineBenison.CanUse(out act, CanUseOption.IgnoreClippingCheck)) return act;
        }
        return base.CountDownAction(remainTime);
    }
}