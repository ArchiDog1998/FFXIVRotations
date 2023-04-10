namespace RotationSolver.Default.Melee;

[SourceCode("https://github.com/ArchiDog1998/RotationSolver/blob/main/RotationSolver.Default/Melee/DRG_Default.cs")]
public sealed class DRG_Default : DRG_Base
{
    public override string GameVersion => "6.18";

    public override string RotationName => "Default";

    protected override IRotationConfigSet CreateConfiguration()
    {
        return base.CreateConfiguration().SetBool("DRG_ShouldDelay", true, "Delay the dragon?")
            .SetBool("DRG_Opener", false, "Opener in lv.88")
            .SetBool("DRG_SafeMove", true, "Moving save");
    }

    [RotationDesc(ActionID.SpineShatterDive, ActionID.DragonFireDive)]
    protected override bool MoveForwardAbility(byte abilitiesRemaining, out IAction act, CanUseOption option = CanUseOption.None)
    {
        if (abilitiesRemaining > 1)
        {
            
            if (SpineShatterDive.CanUse(out act, CanUseOption.EmptyOrSkipCombo | option)) return true;
            if (DragonFireDive.CanUse(out act, CanUseOption.MustUse | CanUseOption.EmptyOrSkipCombo | option)) return true;
        }

        act = null;
        return false;
    }
    protected override bool EmergencyAbility(byte abilityRemain, IAction nextGCD, out IAction act)
    {
        if (nextGCD.IsTheSameTo(true, FullThrust, CoerthanTorment)
            || Player.HasStatus(true, StatusID.LanceCharge) && nextGCD.IsTheSameTo(false, FangandClaw))
        {
            //����
            if (abilityRemain == 1 && LifeSurge.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;
        }

        return base.EmergencyAbility(abilityRemain, nextGCD, out act);
    }

    protected override bool AttackAbility(byte abilitiesRemaining, out IAction act)
    {
        if (InBurst)
        {
            //��ǹ
            if (LanceCharge.CanUse(out act, CanUseOption.MustUse))
            {
                if (abilitiesRemaining == 1 && !Player.HasStatus(true, StatusID.PowerSurge)) return true;
                if (Player.HasStatus(true, StatusID.PowerSurge)) return true;
            }

            //��������
            if (DragonSight.CanUse(out act, CanUseOption.MustUse)) return true;

            //ս������
            if (BattleLitany.CanUse(out act, CanUseOption.MustUse)) return true;
        }

        //����֮��
        if (Nastrond.CanUse(out act, CanUseOption.MustUse)) return true;

        //׹�ǳ�
        if (StarDiver.CanUse(out act, CanUseOption.MustUse)) return true;

        //����
        if (HighJump.EnoughLevel)
        {
            if (HighJump.CanUse(out act)) return true;
        }
        else
        {
            if (Jump.CanUse(out act)) return true;
        }

        //���Խ������Ѫ
        if (Geirskogul.CanUse(out act, CanUseOption.MustUse)) return true;

        //�����
        if (SpineShatterDive.CanUse(out act, CanUseOption.EmptyOrSkipCombo))
        {
            if (Player.HasStatus(true, StatusID.LanceCharge) && LanceCharge.ElapsedOneChargeAfterGCD(3)) return true;
        }
        if (Player.HasStatus(true, StatusID.PowerSurge) && SpineShatterDive.CurrentCharges != 1 && SpineShatterDive.CanUse(out act)) return true;

        //�����
        if (MirageDive.CanUse(out act)) return true;

        //���׳�
        if (DragonFireDive.CanUse(out act, CanUseOption.MustUse))
        {
            if (Player.HasStatus(true, StatusID.LanceCharge) && LanceCharge.ElapsedOneChargeAfterGCD(3)) return true;
        }

        //�����㾦
        if (WyrmwindThrust.CanUse(out act, CanUseOption.MustUse)) return true;

        return false;
    }

    protected override bool GeneralGCD(out IAction act)
    {
        #region Ⱥ��
        if (CoerthanTorment.CanUse(out act)) return true;
        if (SonicThrust.CanUse(out act)) return true;
        if (DoomSpike.CanUse(out act)) return true;

        #endregion

        #region ����
        if (Configs.GetBool("ShouldDelay"))
        {
            if (WheelingThrust.CanUse(out act)) return true;
            if (FangandClaw.CanUse(out act)) return true;
        }
        else
        {
            if (FangandClaw.CanUse(out act)) return true;
            if (WheelingThrust.CanUse(out act)) return true;
        }

        if (FullThrust.CanUse(out act)) return true;
        if (ChaosThrust.CanUse(out act)) return true;

        //�����Ƿ���Ҫ��Buff
        if (Player.WillStatusEndGCD(5, 0, true, StatusID.PowerSurge))
        {
            if (Disembowel.CanUse(out act)) return true;
        }

        if (VorpalThrust.CanUse(out act)) return true;
        if (TrueThrust.CanUse(out act)) return true;

        if (SpecialType == SpecialCommandType.MoveForward && MoveForwardAbility(1, out act)) return true;
        if (PiercingTalon.CanUse(out act)) return true;

        return false;

        #endregion
    }
}
