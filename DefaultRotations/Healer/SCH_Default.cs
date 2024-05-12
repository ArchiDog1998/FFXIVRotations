namespace DefaultRotations.Healer;

[Rotation("Default", CombatType.PvE, GameVersion = "6.28")]
[SourceCode(Path = "main/DefaultRotations/Healer/SCH_Default.cs")]
public sealed class SCH_Default : ScholarRotation
{
    [UI("Gcd Heal", Description = "Use spells with cast times to heal.")]
    [RotationConfig(CombatType.PvE)]
    public bool GCDHeal { get; set; } = false;

    [UI("Recitation at 15 seconds remaining on Countdown.")]
    [RotationConfig(CombatType.PvE)]
    public bool PrevDUN { get; set; } = false;

    [UI("Give Recitation to Tank", Parent = nameof(PrevDUN))]
    [RotationConfig(CombatType.PvE)]
    public bool GiveT { get; set; } = false;

    public override bool CanHealSingleSpell => base.CanHealSingleSpell && (GCDHeal || PartyMembers.GetJobCategory(JobRole.Healer).Count() < 2);
    public override bool CanHealAreaSpell => base.CanHealAreaSpell && (GCDHeal || PartyMembers.GetJobCategory(JobRole.Healer).Count() < 2);

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (nextGCD.IsTheSameTo(true, SuccorPvE, AdloquiumPvE))
        {
            if (RecitationPvE.CanUse(out act)) return true;
        }

        //Remove Aetherpact
        foreach (var item in PartyMembers)
        {
            if (item.GetHealthRatio() < 0.9) continue;
            if (item.HasStatus(true, StatusID.FeyUnion_1223))
            {
                act = AetherpactPvE;
                return true;
            }
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        if (SummonEosPvE.CanUse(out act)) return true;
        if (BioPvE.CanUse(out act)) return true;

        //AOE
        if (ArtOfWarPvE.CanUse(out act)) return true;

        //Single
        if (RuinPvE.CanUse(out act)) return true;
        if (RuinIiPvE.CanUse(out act)) return true;

        //Add dot.
        if (BioPvE.CanUse(out act, skipStatusProvideCheck: true)) return true;

        return base.GeneralGCD(out act);
    }

    protected override bool HealSingleGCD(out IAction? act)
    {
        if (AdloquiumPvE.CanUse(out act)) return true;
        if (PhysickPvE.CanUse(out act)) return true;

        return base.HealSingleGCD(out act);
    }

    protected override bool HealSingleAbility(out IAction? act)
    {
        var haveLink = PartyMembers.Any(p => p.HasStatus(true, StatusID.FeyUnion_1223));

        if (AetherpactPvE.CanUse(out act) && FairyGauge >= 70 && !haveLink) return true;
        if (ProtractionPvE.CanUse(out act)) return true;
        if (SacredSoilPvE.CanUse(out act)) return true;
        if (ExcogitationPvE.CanUse(out act)) return true;
        if (LustratePvE.CanUse(out act)) return true;
        if (AetherpactPvE.CanUse(out act) && !haveLink) return true;

        return base.HealSingleAbility(out act);
    }

    protected override bool DefenseSingleAbility(out IAction? act)
    {
        if (ExcogitationPvE.CanUse(out act)) return true;
        return base.DefenseSingleAbility(out act);
    }

    protected override bool HealAreaGCD(out IAction? act)
    {
        if (SuccorPvE.CanUse(out act)) return true;

        return base.HealAreaGCD(out act);
    }

    protected override bool HealAreaAbility(out IAction? act)
    {
        //慰藉
        if (WhisperingDawnPvE.CD.ElapsedOneChargeAfterGCD(1) || FeyIlluminationPvE.CD.ElapsedOneChargeAfterGCD(1) || FeyBlessingPvE.CD.ElapsedOneChargeAfterGCD(1))
        {
            if (SummonSeraphPvE.CanUse(out act)) return true;
        }
        if (ConsolationPvE.CanUse(out act, usedUp: true)) return true;
        if (FeyBlessingPvE.CanUse(out act)) return true;

        if (WhisperingDawnPvE.CanUse(out act)) return true;
        if (SacredSoilPvE.CanUse(out act)) return true;
        if (IndomitabilityPvE.CanUse(out act)) return true;

        return base.HealAreaAbility(out act);
    }

    protected override bool DefenseAreaGCD(out IAction? act)
    {
        if (SuccorPvE.CanUse(out act)) return true;
        return base.DefenseAreaGCD(out act);
    }

    protected override bool DefenseAreaAbility(out IAction? act)
    {
        if (FeyIlluminationPvE.CanUse(out act)) return true;
        if (ExpedientPvE.CanUse(out act)) return true;

        if (WhisperingDawnPvE.CD.ElapsedOneChargeAfterGCD(1) || FeyIlluminationPvE.CD.ElapsedOneChargeAfterGCD(1) || FeyBlessingPvE.CD.ElapsedOneChargeAfterGCD(1))
        {
            if (SummonSeraphPvE.CanUse(out act)) return true;
        }
        if (ConsolationPvE.CanUse(out act, usedUp: true)) return true;
        if (SacredSoilPvE.CanUse(out act)) return true;

        return base.DefenseAreaAbility(out act);
    }


    protected override bool AttackAbility(out IAction? act)
    {
        if (IsBurst)
        {
            if (ChainStratagemPvE.CanUse(out act)) return true;
        }

        if (DissipationPvE.EnoughLevel && DissipationPvE.CD.WillHaveOneChargeGCD(3) && DissipationPvE.IsEnabled || AetherflowPvE.CD.WillHaveOneChargeGCD(3))
        {
            if (EnergyDrainPvE.CanUse(out act, usedUp: true)) return true;
        }

        if (DissipationPvE.CanUse(out act)) return true;
        if (AetherflowPvE.CanUse(out act)) return true;

        return base.AttackAbility(out act);
    }

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < RuinPvE.Info.CastTime + CountDownAhead
            && RuinPvE.CanUse(out var act)) return act;

        if (PrevDUN && remainTime <= 15 && !DeploymentTacticsPvE.CD.IsCoolingDown && PartyMembers.Count() > 1)
        {

            if (!RecitationPvE.CD.IsCoolingDown) return RecitationPvE;
            if (!PartyMembers.Any((n) => n.HasStatus(true, StatusID.Galvanize)))
            {
                if (GiveT)
                {
                    return AdloquiumPvE;
                }
            }
            else
            {
                return DeploymentTacticsPvE;
            }
        }
        return base.CountDownAction(remainTime);
    }
}