namespace DefaultRotations.Ranged;

[Rotation("Default", CombatType.PvE, GameVersion = "6.28")]
[SourceCode(Path = "main/DefaultRotations/Ranged/DNC_Default.cs")]
public sealed class DNC_Default : DancerRotation
{
    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime <= 15)
        {
            if (StandardStepPvE.CanUse(out var act, skipAoeCheck: true)) return act;
            if (ExecuteStepGCD(out act)) return act;
        }
        return base.CountDownAction(remainTime);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (IsDancing)
        {
            return base.EmergencyAbility(nextGCD, out act);
        }

        if (TechnicalStepPvE.Cooldown.ElapsedAfter(115)
            && UseBurstMedicine(out act)) return true;

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        act = null;
        if (IsDancing) return false;

        if (DevilmentPvE.CanUse(out act))
        {
            if (IsBurst && !TechnicalStepPvE.EnoughLevel) return true;

            if (Player.HasStatus(true, StatusID.TechnicalFinish)) return true;
        }

        if (UseClosedPosition(out act)) return true;

        if (FlourishPvE.CanUse(out act)) return true;
        if (FanDanceIiiPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (Player.HasStatus(true, StatusID.Devilment) || Feathers > 3 || !TechnicalStepPvE.EnoughLevel)
        {
            if (FanDanceIiPvE.CanUse(out act)) return true;
            if (FanDancePvE.CanUse(out act)) return true;
        }

        if (FanDanceIvPvE.CanUse(out act, skipAoeCheck: true))
        {
            if (TechnicalStepPvE.EnoughLevel && TechnicalStepPvE.Cooldown.IsCoolingDown && TechnicalStepPvE.Cooldown.WillHaveOneChargeGCD()) return false;
            return true;
        }

        return base.AttackAbility(out act);
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        if (!InCombat && !Player.HasStatus(true, StatusID.ClosedPosition) && ClosedPositionPvE.CanUse(out act)) return true;

        if (DanceFinishGCD(out act)) return true;
        if (ExecuteStepGCD(out act)) return true;

        if (IsBurst && InCombat && TechnicalStepPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (AttackGCD(out act, Player.HasStatus(true, StatusID.Devilment))) return true;

        return base.GeneralGCD(out act);
    }

    private bool AttackGCD(out IAction? act, bool burst)
    {
        act = null;
        if (IsDancing) return false;

        if ((burst || Esprit >= 85) && SaberDancePvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (StarfallDancePvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (TillanaPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (UseStandardStep(out act)) return true;

        if (BloodshowerPvE.CanUse(out act)) return true;
        if (FountainfallPvE.CanUse(out act)) return true;

        if (RisingWindmillPvE.CanUse(out act)) return true;
        if (ReverseCascadePvE.CanUse(out act)) return true;

        if (BladeshowerPvE.CanUse(out act)) return true;
        if (WindmillPvE.CanUse(out act)) return true;

        if (FountainPvE.CanUse(out act)) return true;
        if (CascadePvE.CanUse(out act)) return true;

        return false;
    }

    private bool UseStandardStep(out IAction act)
    {
        if (!StandardStepPvE.CanUse(out act, skipAoeCheck: true)) return false;
        if (Player.WillStatusEndGCD(2, 0, true, StatusID.StandardFinish)) return true;

        if (!HasHostilesInRange) return false;

        if (TechnicalStepPvE.EnoughLevel && (Player.HasStatus(true, StatusID.TechnicalFinish) || TechnicalStepPvE.Cooldown.IsCoolingDown && TechnicalStepPvE.Cooldown.WillHaveOneCharge(5))) return false;

        return true;
    }

    private bool UseClosedPosition(out IAction act)
    {
        if (!ClosedPositionPvE.CanUse(out act)) return false;

        if (InCombat && Player.HasStatus(true, StatusID.ClosedPosition))
        {
            foreach (var friend in PartyMembers)
            {
                if (friend.HasStatus(true, StatusID.ClosedPosition_2026))
                {
                    if (ClosedPositionPvE.Target.Target != friend) return true;
                    break;
                }
            }
        }
        return false;
    }
}
