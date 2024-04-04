namespace DefaultRotations.Melee;

[Rotation("Lunar Solar", CombatType.PvE, GameVersion = "6.35")]
[RotationDesc(ActionID.RiddleOfFirePvE)]
[SourceCode(Path = "main/DefaultRotations/Melee/MNK_Default.cs")]
[LinkDescription("https://i.imgur.com/C5lQhpe.png")]
public sealed class MNK_Default : MonkRotation
{
    [RotationConfig(CombatType.PvE, Name = "Use Form Shift")]
    public bool AutoFormShift { get; set; } = true;

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime < 0.2)
        {
            if (ThunderclapPvE.CanUse(out var act)) return act;
        }
        if (remainTime < 15)
        {
            if (Chakra < 5 && MeditationPvE.CanUse(out var act)) return act;
            if (FormShiftPvE.CanUse(out act)) return act;
        }

        return base.CountDownAction(remainTime);
    }

    private bool OpoOpoForm(out IAction? act)
    {
        if (ArmOfTheDestroyerPvE.CanUse(out act)) return true;
        if (DragonKickPvE.CanUse(out act)) return true;
        if (BootshinePvE.CanUse(out act)) return true;
        return false;
    }

    private bool UseLunarPerfectBalance => (HasSolar || Player.HasStatus(false, StatusID.PerfectBalance))
        && (!Player.WillStatusEndGCD(0, 0, false, StatusID.RiddleOfFire) || Player.HasStatus(false, StatusID.RiddleOfFire) || RiddleOfFirePvE.Cooldown.WillHaveOneChargeGCD(2)) && PerfectBalancePvE.Cooldown.WillHaveOneChargeGCD(3);

    private bool RaptorForm(out IAction? act)
    {
        if (FourpointFuryPvE.CanUse(out act)) return true;
        if ((Player.WillStatusEndGCD(3, 0, true, StatusID.DisciplinedFist)
            || Player.WillStatusEndGCD(7, 0, true, StatusID.DisciplinedFist)
            && UseLunarPerfectBalance) && TwinSnakesPvE.CanUse(out act)) return true;
        if (TrueStrikePvE.CanUse(out act)) return true;
        return false;
    }

    private bool CoerlForm(out IAction? act)
    {
        if (RockbreakerPvE.CanUse(out act)) return true;
        if (UseLunarPerfectBalance && DemolishPvE.CanUse(out act, skipStatusProvideCheck: true)
            && (DemolishPvE.Target.Target?.WillStatusEndGCD(7, 0, true, StatusID.Demolish) ?? false)) return true;
        if (DemolishPvE.CanUse(out act)) return true;
        if (SnapPunchPvE.CanUse(out act)) return true;
        return false;
    }

    protected override bool GeneralGCD(out IAction? act)
    {
        if (PerfectBalanceActions(out act)) return true;

        if (Player.HasStatus(true, StatusID.CoeurlForm))
        {
            if (CoerlForm(out act)) return true;
        }

        if (Player.HasStatus(true, StatusID.RiddleOfFire)
            && !RiddleOfFirePvE.Cooldown.ElapsedAfterGCD(2) && (PerfectBalancePvE.Cooldown.ElapsedAfter(60) || !PerfectBalancePvE.Cooldown.IsCoolingDown))
        {
            if (OpoOpoForm(out act)) return true;
        }
        if (Player.HasStatus(true, StatusID.RaptorForm))
        {
            if (RaptorForm(out act)) return true;
        }
        if (OpoOpoForm(out act)) return true;

        if (Chakra < 5 && MeditationPvE.CanUse(out act)) return true;
        if (AutoFormShift && FormShiftPvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    protected override bool MoveForwardGCD(out IAction? act)
    {
        if (MoveForwardAbility(out act)) return true;
        return base.MoveForwardGCD(out act);
    }

    private bool PerfectBalanceActions(out IAction? act)
    {
        if (!BeastChakras.Contains(BeastChakra.NONE))
        {
            if (HasSolar && HasLunar)
            {
                if (PhantomRushPvE.CanUse(out act, skipAoeCheck: true)) return true;
                if (TornadoKickPvE.CanUse(out act, skipAoeCheck: true)) return true;
            }
            if (BeastChakras.Contains(BeastChakra.RAPTOR))
            {
                if (RisingPhoenixPvE.CanUse(out act, skipAoeCheck: true)) return true;
                if (FlintStrikePvE.CanUse(out act, skipAoeCheck: true)) return true;
            }
            else
            {
                if (ElixirFieldPvE.CanUse(out act, skipAoeCheck: true)) return true;
            }
        }
        else if (Player.HasStatus(true, StatusID.PerfectBalance) && ElixirFieldPvE.EnoughLevel)
        {
            //Sometimes, no choice
            if (HasSolar || BeastChakras.Count(c => c == BeastChakra.OPOOPO) > 1)
            {
                if (LunarNadi(out act)) return true;
            }
            else if (BeastChakras.Contains(BeastChakra.COEURL) || BeastChakras.Contains(BeastChakra.RAPTOR))
            {
                if (SolarNadi(out act)) return true;
            }

            //Add status when solar.
            if (Player.WillStatusEndGCD(3, 0, true, StatusID.DisciplinedFist)
                || (HostileTarget?.WillStatusEndGCD(3, 0, true, StatusID.Demolish) ?? false))
            {
                if (SolarNadi(out act)) return true;
            }
            if (LunarNadi(out act)) return true;
        }

        act = null;
        return false;
    }

    bool LunarNadi(out IAction? act)
    {
        if (OpoOpoForm(out act)) return true;
        return false;
    }

    bool SolarNadi(out IAction? act)
    {
        //Emergency usage of status.
        if (!BeastChakras.Contains(BeastChakra.RAPTOR)
            && HasLunar
            && Player.WillStatusEndGCD(1, 0, true, StatusID.DisciplinedFist))
        {
            if (RaptorForm(out act)) return true;
        }
        if (!BeastChakras.Contains(BeastChakra.COEURL)
            && (HostileTarget?.WillStatusEndGCD(1, 0, true, StatusID.Demolish) ?? false))
        {
            if (CoerlForm(out act)) return true;
        }

        if (!BeastChakras.Contains(BeastChakra.OPOOPO))
        {
            if (OpoOpoForm(out act)) return true;
        }
        if (HasLunar && !BeastChakras.Contains(BeastChakra.RAPTOR))
        {
            if (RaptorForm(out act)) return true;
        }
        if (!BeastChakras.Contains(BeastChakra.COEURL))
        {
            if (CoerlForm(out act)) return true;
        }
        if (!BeastChakras.Contains(BeastChakra.RAPTOR))
        {
            if (RaptorForm(out act)) return true;
        }

        return CoerlForm(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (InCombat)
        {
            if (UseBurstMedicine(out act)) return true;
            if (IsBurst && !CombatElapsedLessGCD(2) && RiddleOfFirePvE.CanUse(out act, onLastAbility: true)) return true;
        }

        act = null;
        if (CombatElapsedLessGCD(3)) return false;

        if (BeastChakras.Contains(BeastChakra.NONE) && Player.HasStatus(true, StatusID.RaptorForm)
            && (!RiddleOfFirePvE.EnoughLevel || Player.HasStatus(false, StatusID.RiddleOfFire) && !Player.WillStatusEndGCD(3, 0, false, StatusID.RiddleOfFire)
            || RiddleOfFirePvE.Cooldown.WillHaveOneChargeGCD(1) && (PerfectBalancePvE.Cooldown.ElapsedAfter(60) || !PerfectBalancePvE.Cooldown.IsCoolingDown)))
        {
            if (PerfectBalancePvE.CanUse(out act, usedUp: true)) return true;
        }

        if (BrotherhoodPvE.CanUse(out act, skipAoeCheck: true)) return true;

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        act = null;

        if (CombatElapsedLessGCD(3)) return false;

        if (HowlingFistPvE.CanUse(out act)) return true;
        if (SteelPeakPvE.CanUse(out act)) return true;
        if (HowlingFistPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if (RiddleOfWindPvE.CanUse(out act)) return true;

        return base.AttackAbility(out act);
    }
}
