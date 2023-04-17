namespace DefaultRotations.Tank;

[SourceCode("https://github.com/ArchiDog1998/FFXIVRotations/blob/main/DefaultRotations/Tank/GNB_Default.cs")]
public sealed class GNB_Default : GNB_Base
{
    public override string GameVersion => "6.18";

    public override string RotationName => "Default";

    protected override bool CanHealSingleSpell => false;

    protected override bool CanHealAreaSpell => false;

    protected override bool GeneralGCD(out IAction act)
    {
        if (CanUseDoubleDown(out act)) return true;

        if (FatedCircle.CanUse(out act)) return true;

        if (DemonSlaughter.CanUse(out act)) return true;
        if (DemonSlice.CanUse(out act)) return true;

        if (CanUseGnashingFang(out act)) return true;

        if (CanUseSonicBreak(out act)) return true;

        if (WickedTalon.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;
        if (SavageClaw.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;

        if (CanUseBurstStrike(out act)) return true;

        if (SolidBarrel.CanUse(out act)) return true;
        if (BrutalShell.CanUse(out act)) return true;
        if (KeenEdge.CanUse(out act)) return true;

        if (SpecialType == SpecialCommandType.MoveForward && MoveForwardAbility(1, out act)) return true;

        if (LightningShot.CanUse(out act)) return true;

        return false;
    }

    protected override bool AttackAbility(byte abilitiesRemaining, out IAction act)
    {
        if (InBurst && abilitiesRemaining == 1 && CanUseNoMercy(out act)) return true;

        if (DangerZone.CanUse(out act))
        {
            if (!IsFullParty && !DangerZone.IsTargetBoss) return true;

            if (!GnashingFang.EnoughLevel && (Player.HasStatus(true, StatusID.NoMercy) || !NoMercy.WillHaveOneCharge(15))) return true;

            if (Player.HasStatus(true, StatusID.NoMercy) && GnashingFang.IsCoolingDown) return true;

            if (!Player.HasStatus(true, StatusID.NoMercy) && !GnashingFang.WillHaveOneCharge(20)) return true;
        }

        if (CanUseBowShock(out act)) return true;

        if (JugularRip.CanUse(out act)) return true;
        if (AbdomenTear.CanUse(out act)) return true;
        if (EyeGouge.CanUse(out act)) return true;
        if (Hypervelocity.CanUse(out act)) return true;

        if (GnashingFang.IsCoolingDown && BloodFest.CanUse(out act)) return true;

        if (Player.HasStatus(true, StatusID.NoMercy) && RoughDivide.CanUse(out act, CanUseOption.MustUse)) return true;

        act = null;
        return false;
    }

    [RotationDesc(ActionID.HeartOfLight, ActionID.Reprisal)]
    protected override bool DefenseAreaAbility(byte abilitiesRemaining, out IAction act)
    {
        if (HeartOfLight.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;
        if (Reprisal.CanUse(out act, CanUseOption.MustUse)) return true;
        return false;
    }

    [RotationDesc(ActionID.HeartOfStone, ActionID.Nebula, ActionID.Rampart, ActionID.Camouflage, ActionID.Reprisal)]
    protected override bool DefenseSingleAbility(byte abilitiesRemaining, out IAction act)
    {
        if (abilitiesRemaining == 1)
        {
            //10
            if (Camouflage.CanUse(out act)) return true;

            //10
            if (HeartOfStone.CanUse(out act)) return true;
        }
        else
        {
            //30
            if ((!Rampart.IsCoolingDown || Rampart.ElapsedAfter(60)) && Nebula.CanUse(out act)) return true;

            //20
            if (Nebula.IsCoolingDown && Nebula.ElapsedAfter(60) && Rampart.CanUse(out act)) return true;
        }

        if (Reprisal.CanUse(out act)) return true;

        act = null;
        return false;
    }

    [RotationDesc(ActionID.Aurora)]
    protected override bool HealSingleAbility(byte abilitiesRemaining, out IAction act)
    {
        if (Aurora.CanUse(out act, CanUseOption.EmptyOrSkipCombo) && abilitiesRemaining == 1) return true;
        return false;
    }

    private bool CanUseNoMercy(out IAction act)
    {
        if (!NoMercy.CanUse(out act)) return false;

        if (!IsFullParty && !IsTargetBoss && !IsMoving && DemonSlice.CanUse(out _)) return true;

        //�ȼ����ڱ��������ж�
        if (!BurstStrike.EnoughLevel) return true;

        if (BurstStrike.EnoughLevel)
        {
            //4GCD�����ж�
            if (IsLastGCD((ActionID)KeenEdge.ID) && Ammo == 1 && !GnashingFang.IsCoolingDown && !BloodFest.IsCoolingDown) return true;

            //3��������
            else if (Ammo == (Level >= 88 ? 3 : 2)) return true;

            //2��������
            else if (Ammo == 2 && GnashingFang.IsCoolingDown) return true;
        }

        act = null;
        return false;
    }

    private bool CanUseGnashingFang(out IAction act)
    {
        //�����ж�
        if (GnashingFang.CanUse(out act))
        {
            //��4�˱�����ʹ��
            if (DemonSlice.CanUse(out _)) return false;

            //������3������
            if (Ammo == (Level >= 88 ? 3 : 2) && (Player.HasStatus(true, StatusID.NoMercy) || !NoMercy.WillHaveOneCharge(55))) return true;

            //����������
            if (Ammo > 0 && !NoMercy.WillHaveOneCharge(17) && NoMercy.WillHaveOneCharge(35)) return true;

            //3���ҽ�������ӵ������,��ǰ������ǰ������
            if (Ammo == 3 && IsLastGCD((ActionID)BrutalShell.ID) && NoMercy.WillHaveOneCharge(3)) return true;

            //1����Ѫ������ȴ����
            if (Ammo == 1 && !NoMercy.WillHaveOneCharge(55) && BloodFest.WillHaveOneCharge(5)) return true;

            //4GCD���������ж�
            if (Ammo == 1 && !NoMercy.WillHaveOneCharge(55) && (!BloodFest.IsCoolingDown && BloodFest.EnoughLevel || !BloodFest.EnoughLevel)) return true;
        }
        return false;
    }

    /// <summary>
    /// ������
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool CanUseSonicBreak(out IAction act)
    {
        //�����ж�
        if (SonicBreak.CanUse(out act))
        {
            //��4�˱����в�ʹ��
            if (DemonSlice.CanUse(out _)) return false;

            //if (!IsFullParty && !SonicBreak.IsTargetBoss) return false;

            if (!GnashingFang.EnoughLevel && Player.HasStatus(true, StatusID.NoMercy)) return true;

            //����������ʹ��������
            if (GnashingFang.IsCoolingDown && Player.HasStatus(true, StatusID.NoMercy)) return true;

            //�����ж�
            if (!DoubleDown.EnoughLevel && Player.HasStatus(true, StatusID.ReadyToRip)
                && GnashingFang.IsCoolingDown) return true;

        }
        return false;
    }

    /// <summary>
    /// ����
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool CanUseDoubleDown(out IAction act)
    {
        //�����ж�
        if (DoubleDown.CanUse(out act, CanUseOption.MustUse))
        {
            //��4�˱�����
            if (DemonSlice.CanUse(out _) && Player.HasStatus(true, StatusID.NoMercy)) return true;

            //�������ƺ�ʹ�ñ���
            if (SonicBreak.IsCoolingDown && Player.HasStatus(true, StatusID.NoMercy)) return true;

            //2������������ж�,��ǰʹ�ñ���
            if (Player.HasStatus(true, StatusID.NoMercy) && !NoMercy.WillHaveOneCharge(55) && BloodFest.WillHaveOneCharge(5)) return true;

        }
        return false;
    }

    /// <summary>
    /// ������
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool CanUseBurstStrike(out IAction act)
    {
        if (BurstStrike.CanUse(out act))
        {
            //��4�˱�������AOEʱ��ʹ��
            if (DemonSlice.CanUse(out _)) return false;

            //�������ʣ0.5����ȴ��,���ͷű�����,��Ҫ��Ϊ���ٲ�ͬ���ܻ�ʹ�����Ӻ�̫�������ж�һ��
            if (SonicBreak.IsCoolingDown && SonicBreak.WillHaveOneCharge(0.5f) && GnashingFang.EnoughLevel) return false;

            //�����б������ж�
            if (Player.HasStatus(true, StatusID.NoMercy) &&
                AmmoComboStep == 0 &&
                !GnashingFang.WillHaveOneCharge(1)) return true;
            if (Level < 88 && Ammo == 2) return true;
            //�������ֹ���
            if (IsLastGCD((ActionID)BrutalShell.ID) &&
                (Ammo == (Level >= 88 ? 3 : 2) ||
                BloodFest.WillHaveOneCharge(6) && Ammo <= 2 && !NoMercy.WillHaveOneCharge(10) && BloodFest.EnoughLevel)) return true;

        }
        return false;
    }

    private bool CanUseBowShock(out IAction act)
    {
        if (BowShock.CanUse(out act, CanUseOption.MustUse))
        {
            if (DemonSlice.CanUse(out _) && !IsFullParty) return true;

            if (!SonicBreak.EnoughLevel && Player.HasStatus(true, StatusID.NoMercy)) return true;

            //������,������������������ȴ��
            if (Player.HasStatus(true, StatusID.NoMercy) && SonicBreak.IsCoolingDown) return true;
        }
        return false;
    }
}