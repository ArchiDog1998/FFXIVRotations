namespace DefaultRotations.Tank;


[RotationDesc(ActionID.BloodWeapon, ActionID.Delirium)]
[SourceCode("https://github.com/ArchiDog1998/FFXIVRotations/blob/main/DefaultRotations/Tank/DRK_Old.cs")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/drk/drk_standard_6.2_v1.png")]
public sealed class DRK_Old : DRK_Base
{
    public override string GameVersion => "6.38";

    public override string RotationName => "Old";

    public override string Description => "Special thanks to Nore for fixing the rotation.";

    protected override bool CanHealSingleAbility => false;

    private static bool InTwoMinBurst => BloodWeapon.IsCoolingDown && Delirium.IsCoolingDown
        && LivingShadow.IsCoolingDown && !LivingShadow.ElapsedAfter(20);

    private static bool CombatLess => CombatElapsedLess(3);

    private bool CheckDarkSide
    {
        get
        {
            if (DarkSideEndAfterGCD(3)) return true;

            if (CombatLess) return false;

            if (InTwoMinBurst && SaltedEarth.IsCoolingDown && ShadowBringer.CurrentCharges == 0 && CarveAndSpit.IsCoolingDown || HasDarkArts) return true;

            if (Configs.GetBool("TheBlackestNight") && CurrentMp < 6000) return false;

            return CurrentMp >= 8500;
        }
    }

    private bool UseBlood
    {
        get
        {
            if (!Delirium.EnoughLevel) return true;

            if (Player.HasStatus(true, StatusID.Delirium) && Player.StatusStack(true, StatusID.BloodWeapon) < 2) return true;

            if (BloodWeapon.WillHaveOneChargeGCD(1) || Blood >= 90 && !Player.HasStatus(true, StatusID.Delirium)) return true;

            return false;
        }
    }

    protected override IRotationConfigSet CreateConfiguration()
        => base.CreateConfiguration()
            .SetBool("TheBlackestNight", true, "Keep 3000 MP");

    protected override IAction CountDownAction(float remainTime)
    {
        //Provoke when has Shield.
        if (remainTime <= Service.Config.CountDownAhead)
        {
            if (HasTankStance)
            {
                if (Provoke.CanUse(out var act1)) return act1;
            }
            else
            {
                if (Unmend.CanUse(out var act1)) return act1;
            }
        }
        if (remainTime <= 2 && UseBurstMedicine(out var act)) return act;
        if (remainTime <= 3 && TheBlackestNight.CanUse(out act, CanUseOption.IgnoreClippingCheck)) return act;
        if (remainTime <= 4 && BloodWeapon.CanUse(out act, CanUseOption.IgnoreClippingCheck)) return act;
        return base.CountDownAction(remainTime);
    }

    [RotationDesc(ActionID.TheBlackestNight)]
    protected override bool HealSingleAbility(out IAction act)
    {
        if (TheBlackestNight.CanUse(out act)) return true;

        return base.HealSingleAbility(out act);
    }

    [RotationDesc(ActionID.DarkMissionary, ActionID.Reprisal)]
    protected override bool DefenseAreaAbility(out IAction act)
    {
        if (DarkMissionary.CanUse(out act)) return true;
        if (Reprisal.CanUse(out act, CanUseOption.MustUse)) return true;

        return false;
    }

    [RotationDesc(ActionID.TheBlackestNight, ActionID.Oblation, ActionID.ShadowWall, ActionID.Rampart, ActionID.DarkMind, ActionID.Reprisal)]
    protected override bool DefenseSingleAbility(out IAction act)
    {
        act = null;

        if (Player.HasStatus(true, StatusID.TheBlackestNight)) return false;

        //10
        if (Oblation.CanUse(out act, CanUseOption.EmptyOrSkipCombo | CanUseOption.OnLastAbility)) return true;

        if (Reprisal.CanUse(out act, CanUseOption.MustUse | CanUseOption.OnLastAbility)) return true;

        if (TheBlackestNight.CanUse(out act, CanUseOption.OnLastAbility)) return true;
        //30
        if ((!Rampart.IsCoolingDown || Rampart.ElapsedAfter(60)) && ShadowWall.CanUse(out act)) return true;

        //20
        if (ShadowWall.IsCoolingDown && ShadowWall.ElapsedAfter(60) && Rampart.CanUse(out act)) return true;
        if (DarkMind.CanUse(out act)) return true;

        return false;
    }

    protected override bool GeneralGCD(out IAction act)
    {
        if (IsMoving && HasHostilesInRange && BloodWeapon.CanUse(out act)) return true;

        //Use Blood
        if (UseBlood)
        {
            if (Quietus.CanUse(out act)) return true;
            if (BloodSpiller.CanUse(out act)) return true;
        }

        //AOE
        if (StalwartSoul.CanUse(out act)) return true;
        if (Unleash.CanUse(out act)) return true;

        //单体
        if (Souleater.CanUse(out act)) return true;
        if (SyphonStrike.CanUse(out act)) return true;
        if (HardSlash.CanUse(out act)) return true;

        if (SpecialType == SpecialCommandType.MoveForward && MoveForwardAbility(out act)) return true;
        if (Unmend.CanUse(out act)) return true;

        return false;
    }

    protected override bool AttackAbility(out IAction act)
    {
        if (CheckDarkSide)
        {
            if (FloodOfDarkness.CanUse(out act)) return true;
            if (EdgeOfDarkness.CanUse(out act)) return true;
        }

        if (InBurst)
        {
            if (UseBurstMedicine(out act)) return true;
            if (BloodWeapon.CanUse(out act)) return true;
            if (Delirium.CanUse(out act)) return true;
            if (LivingShadow.CanUse(out act, CanUseOption.MustUse)) return true;
        }

        if (CombatLess)
        {
            act = null;
            return false;
        }

        if (!IsMoving && SaltedEarth.CanUse(out act, CanUseOption.MustUse)) return true;

        if (InTwoMinBurst)
        {
            if (ShadowBringer.CanUse(out act, CanUseOption.MustUse)) return true;
        }

        if (AbyssalDrain.CanUse(out act)) return true;
        if (CarveAndSpit.CanUse(out act)) return true;

        if (InTwoMinBurst)
        {
            if (ShadowBringer.CanUse(out act, CanUseOption.MustUse | CanUseOption.EmptyOrSkipCombo)) return true;

            if (Plunge.CanUse(out act, CanUseOption.MustUse) && !IsMoving) return true;
        }

        if (SaltandDarkness.CanUse(out act)) return true;

        if (InTwoMinBurst)
        {
            if (Plunge.CanUse(out act, CanUseOption.MustUse | CanUseOption.EmptyOrSkipCombo) && !IsMoving) return true;
        }

        return false;
    }
}