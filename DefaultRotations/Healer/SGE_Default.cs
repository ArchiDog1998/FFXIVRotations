namespace DefaultRotations.Healer;

[SourceCode("https://github.com/ArchiDog1998/FFXIVRotations/blob/main/DefaultRotations/Healer/SGE_Default.cs")]
public sealed class SGE_Default : SGE_Base
{
    public override string GameVersion => "6.38";

    public override string RotationName => "Default";

    public override string Description => "Please contact Nore#7219 on Discord for questions about this rotation.";

    static bool NoOgcds => Addersgall < 1 && (!Physis.EnoughLevel || Physis.IsCoolingDown || Physis2.IsCoolingDown) && (!Haima.EnoughLevel || Haima.IsCoolingDown) && (!Panhaima.EnoughLevel || Panhaima.IsCoolingDown) && (!Holos.EnoughLevel || Holos.IsCoolingDown) && (!Pneuma.EnoughLevel || Pneuma.IsCoolingDown) && (!Rhizomata.EnoughLevel || Rhizomata.IsCoolingDown) && (!Pneuma.EnoughLevel || Krasis.IsCoolingDown);

    static bool NoOgcdsAOE => Addersgall < 1 && (!Physis.EnoughLevel || Physis.IsCoolingDown || Physis2.IsCoolingDown) && (!Panhaima.EnoughLevel || Panhaima.IsCoolingDown) && (!Holos.EnoughLevel || Holos.IsCoolingDown) && (!Pneuma.EnoughLevel || Pneuma.IsCoolingDown) && (!Rhizomata.EnoughLevel || Rhizomata.IsCoolingDown) && (!Pneuma.EnoughLevel || Krasis.IsCoolingDown);

    private static bool InTwoMinBurst()
    {
        if (RatioOfMembersIn2minsBurst >= 0.5) return true;
        if (RatioOfMembersIn2minsBurst == -1) return true;
        else return false;
    }


    /// <summary>
    /// ���þ�������
    /// </summary>
    private static BaseAction MEukrasianDiagnosis { get; } = new(ActionID.EukrasianDiagnosis, ActionOption.Heal)
    {
        ChoiceTarget = (Targets, mustUse) =>
        {
            var targets = Targets.GetJobCategory(JobRole.Tank);
            if (!targets.Any()) return null;
            return targets.First();
        },
        ActionCheck = (b, m) =>
        {
            if (InCombat || HasHostilesInRange) return false;
            if (b == Player) return false;
            if (b.HasStatus(false, StatusID.EukrasianDiagnosis, StatusID.EukrasianPrognosis, StatusID.Galvanize)) return false;
            return true;
        }
    };

    //private static BaseAction MEukrasianDosis { get; } = new(ActionID.EukrasianDosis)
    //{
    //    ActionCheck = b =>
    //    {
    //        if (!HasHostilesInRange) return false;
    //        if (b.HasStatus(true, StatusID.EukrasianDosis, StatusID.EukrasianDosis2, StatusID.EukrasianDosis3)) return false;
    //        return true;
    //    }
    //};

    protected override bool CanHealSingleSpell => base.CanHealSingleSpell && (Configs.GetBool("GCDHeal") || PartyHealers.Count() < 2);
    protected override bool CanHealAreaSpell => base.CanHealAreaSpell && (Configs.GetBool("GCDHeal") || PartyHealers.Count() < 2);

    protected override IRotationConfigSet CreateConfiguration()
    {
        return base.CreateConfiguration().SetBool("GCDHeal", false, "Use spells with cast times to heal.");
    }

    protected override bool AttackAbility(out IAction act)
    {
        act = null!;
        return false;
    }

    protected override IAction CountDownAction(float remainTime)
    {
        if (remainTime <= 1.5 && Dosis.CanUse(out var act)) return act;
        if (remainTime <= 3 && UseBurstMedicine(out act)) return act;
        return base.CountDownAction(remainTime);
    }
    protected override bool EmergencyAbility(IAction nextGCD, out IAction act)
    {
        if (base.EmergencyAbility(nextGCD, out act)) return true;

        if (nextGCD.IsTheSameTo(false, Pneuma, EukrasianDiagnosis,
            EukrasianPrognosis, Diagnosis, Prognosis))
        {
            if (Zoe.CanUse(out act)) return true;
        }

        if (nextGCD.IsTheSameTo(false, Pneuma))
        {
            if (Krasis.CanUse(out act)) return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    [RotationDesc(ActionID.Haima, ActionID.Taurochole, ActionID.Panhaima, ActionID.Kerachole, ActionID.Holos)]
    protected override bool DefenseSingleAbility(out IAction act)
    {
        if (Addersgall <= 1)
        {
            if (Haima.CanUse(out act, CanUseOption.OnLastAbility)) return true;
        }

        //��ţ��֭
        if (Taurochole.CanUse(out act, CanUseOption.OnLastAbility) && Taurochole.Target.GetHealthRatio() < 0.8) return true;

        if (Addersgall <= 1)
        {
            if ((!Haima.EnoughLevel || Haima.ElapsedAfter(20)) && Panhaima.CanUse(out act, CanUseOption.OnLastAbility)) return true;
        }

        //������֭
        if ((!Taurochole.EnoughLevel || Taurochole.ElapsedAfter(20)) && Kerachole.CanUse(out act, CanUseOption.OnLastAbility)) return true;

        //������
        if (Holos.CanUse(out act, CanUseOption.OnLastAbility)) return true;

        return base.DefenseSingleAbility(out act);
    }

    [RotationDesc(ActionID.EukrasianDiagnosis)]
    protected override bool DefenseSingleGCD(out IAction act)
    {
        if (EukrasianDiagnosis.CanUse(out act))
        {
            if (EukrasianDiagnosis.Target.HasStatus(false,
                StatusID.EukrasianDiagnosis,
                StatusID.EukrasianPrognosis,
                StatusID.Galvanize
            )) return false;

            if (Eukrasia.CanUse(out act)) return true;

            act = EukrasianDiagnosis;
            return true;
        }

        return base.DefenseSingleGCD(out act);
    }

    [RotationDesc(ActionID.Panhaima, ActionID.Kerachole, ActionID.Holos)]
    protected override bool DefenseAreaAbility(out IAction act)
    {
        if (Addersgall <= 1)
        {
            if (Panhaima.CanUse(out act, CanUseOption.OnLastAbility)) return true;
        }

        if (Kerachole.CanUse(out act, CanUseOption.OnLastAbility)) return true;

        if (Holos.CanUse(out act, CanUseOption.OnLastAbility)) return true;

        return base.DefenseAreaAbility(out act);
    }

    [RotationDesc(ActionID.EukrasianPrognosis)]
    protected override bool DefenseAreaGCD(out IAction act)
    {
        if (EukrasianPrognosis.CanUse(out act))
        {
            if (EukrasianDiagnosis.Target.HasStatus(false,
                StatusID.EukrasianDiagnosis,
                StatusID.EukrasianPrognosis,
                StatusID.Galvanize
            )) return false;

            if (Eukrasia.CanUse(out act)) return true;

            act = EukrasianPrognosis;
            return true;
        }

        return base.DefenseAreaGCD(out act);
    }

    protected override bool GeneralAbility(out IAction act)
    {
        if (Kardia.CanUse(out act)) return true;

        if (Addersgall <= 1 && Rhizomata.CanUse(out act)) return true;

        if (Soteria.CanUse(out act) && PartyMembers.Any(b => b.HasStatus(true, StatusID.Kardion) && b.GetHealthRatio() < Service.Config.HealthSingleAbility)) return true;

        if (Pepsis.CanUse(out act)) return true;

        act = null!;
        return false;
    }

    protected override bool GeneralGCD(out IAction act)
    {
        //if (Target.IsBoss())
        //{
        //    if ((Player.Level >= 82 && !Target.HasStatus(true, StatusID.EukrasianDosis3)
        //    || (Player.Level >= 72 && Player.Level < 82 && !Target.HasStatus(true, StatusID.EukrasianDosis2))
        //    || (Player.Level > 30 && Player.Level < 72 && !Target.HasStatus(true, StatusID.EukrasianDosis))))
        //    {
        //        if (Eukrasia.CanUse(out act)) return true;
        //        if (Dosis.CanUse(out act)) return true;
        //    }
        //}
        if (Target.IsBoss())
        {
            if (EukrasianDosis.CanUse(out _, CanUseOption.IgnoreCastCheck))
            {
                if (Eukrasia.CanUse(out act, CanUseOption.IgnoreCastCheck)) return true;
                act = EukrasianDosis;
                return true;
            }
        }

        var option = CanUseOption.MustUse;
        if (IsMoving || Dyskrasia.CanUse(out _) || InTwoMinBurst()) option |= CanUseOption.EmptyOrSkipCombo;
        if (Phlegma3.CanUse(out act, option)) return true;
        if (!Phlegma3.EnoughLevel && Phlegma2.CanUse(out act, option)) return true;
        if (!Phlegma2.EnoughLevel && Phlegma.CanUse(out act, option)) return true;

        if (PartyMembers.Any(b => b.GetHealthRatio() < 0.20f) || PartyTanks.Any(t => t.GetHealthRatio() < 0.6f))
        {
            if (Pneuma.CanUse(out act, CanUseOption.MustUse)) return true;
        }
        
        if (IsMoving && Toxikon.CanUse(out act, CanUseOption.MustUse)) return true;

        if (Dyskrasia.CanUse(out act)) return true;

        if (EukrasianDosis.CanUse(out _, CanUseOption.IgnoreCastCheck))
        {
            if (Eukrasia.CanUse(out act, CanUseOption.IgnoreCastCheck)) return true;
            act = EukrasianDosis;
            return true;
        }

        //if ((Player.Level >= 82 && !Target.HasStatus(true, StatusID.EukrasianDosis3)
        //    || (Player.Level >= 72 && Player.Level < 82 && !Target.HasStatus(true, StatusID.EukrasianDosis2))
        //    || (Player.Level > 30 && Player.Level < 72 && !Target.HasStatus(true, StatusID.EukrasianDosis))))
        //{
        //    if (Eukrasia.CanUse(out act)) return true;
        //    if (Dosis.CanUse(out act)) return true;
        //}

        //if ((IsMoving || !IsMoving) && !Target.HasStatus(true, StatusID.EukrasianDosis, StatusID.EukrasianDosis2, StatusID.EukrasianDosis3) && MEukrasianDosis.CanUse(out _))
        //{
        //    if (Eukrasia.CanUse(out act, CanUseOption.IgnoreCastCheck)) return true;
            
        //    act = MEukrasianDosis;
        //    return true;
        //}

        if (Dosis.CanUse(out act)) return true;

        if (MEukrasianDiagnosis.CanUse(out _))
        {
            if (Eukrasia.CanUse(out act)) return true;

            act = MEukrasianDiagnosis;
            return true;
        }

        //if (Eukrasia.CanUse(out act)) return true;

        return false;
    }

    [RotationDesc(ActionID.Taurochole, ActionID.Kerachole, ActionID.Druochole, ActionID.Holos, ActionID.Physis, ActionID.Panhaima)]
    protected override bool HealSingleAbility(out IAction act)
    {
        //��ţ��֭
        if (Taurochole.CanUse(out act)) return true;

        if (Kerachole.CanUse(out act) && EnhancedKerachole.EnoughLevel) return true;

        //������֭
        if ((!Taurochole.EnoughLevel || Taurochole.IsCoolingDown) && Druochole.CanUse(out act)) return true;

        //����Դ����ʱ���뷶Χ���ƻ���ѹ��
        //var tank = PartyTanks;
        //var isBoss = Dosis.Target.IsBoss();
        //if (Addersgall == 0 && tank.Count() == 1 && tank.Any(t => t.GetHealthRatio() < 0.6f) && !isBoss)
        //{
        //    //������
        //    if (Holos.CanUse(out act)) return true;

        //    //����
        //    if (Physis.CanUse(out act)) return true;

        //    if (Haima.CanUse(out act)) return true;

        //    //����Ѫ
        //    if (Panhaima.CanUse(out act)) return true;
        //}

        //����
        if (Soteria.CanUse(out act) && PartyMembers.Any(b => b.HasStatus(true, StatusID.Kardion) && b.GetHealthRatio() < 0.85f)) return true;


        var tank = PartyTanks;
        if (Addersgall < 1 && (tank.Any(t => t.GetHealthRatio() < 0.65f) || PartyMembers.Any(b => b.GetHealthRatio() < 0.20f)))
        {
            if (Haima.CanUse(out act, CanUseOption.OnLastAbility)) return true;

            //����
            if (Physis2.CanUse(out act)) return true;
            if (!Physis2.EnoughLevel && Physis.CanUse(out act)) return true;
          

            //������
            if (Holos.CanUse(out act, CanUseOption.OnLastAbility)) return true;

            //����Ѫ
            if ((!Haima.EnoughLevel || Haima.ElapsedAfter(20)) && Panhaima.CanUse(out act, CanUseOption.OnLastAbility)) return true;
        }

        //�¸�������
        if (PartyTanks.Any(t => t.GetHealthRatio() < 0.60f))
        {
            //�
            if (Zoe.CanUse(out act)) return true;
        }

        if (PartyTanks.Any(t => t.GetHealthRatio() < 0.70f) || PartyMembers.Any(b => b.GetHealthRatio() < 0.30f))
        {
            //����
            if (Krasis.CanUse(out act)) return true;
        }

        if (Kerachole.CanUse(out act)) return true;

        return base.HealSingleAbility(out act);
    }

    [RotationDesc(ActionID.Diagnosis)]
    protected override bool HealSingleGCD(out IAction act)
    {
        if (Diagnosis.CanUse(out act)) return true;
        return false;
    }

    [RotationDesc(ActionID.Pneuma, ActionID.Prognosis, ActionID.EukrasianPrognosis)]
    protected override bool HealAreaGCD(out IAction act)
    {
        if (PartyMembersAverHP < 0.65f || Dyskrasia.CanUse(out _) && PartyTanks.Any(t => t.GetHealthRatio() < 0.6f))
        {
            if (Pneuma.CanUse(out act, CanUseOption.MustUse)) return true;
        }

        if (EukrasianPrognosis.Target.HasStatus(false, StatusID.EukrasianDiagnosis, StatusID.EukrasianPrognosis, StatusID.Galvanize))
        {
            if (Prognosis.CanUse(out act)) return true;
        }

        if (EukrasianPrognosis.CanUse(out _))
        {
            if (Eukrasia.CanUse(out act)) return true;

            act = EukrasianPrognosis;
            return true;
        }

        act = null;
        return false;
    }

    [RotationDesc(ActionID.Kerachole, ActionID.Physis, ActionID.Holos, ActionID.Ixochole)]
    protected override bool HealAreaAbility(out IAction act)
    {
        if (Physis2.CanUse(out act)) return true;
        if (!Physis2.EnoughLevel && Physis.CanUse(out act)) return true;

        if (Kerachole.CanUse(out act, CanUseOption.OnLastAbility) && EnhancedKerachole.EnoughLevel) return true;

        if (Holos.CanUse(out act, CanUseOption.OnLastAbility) && PartyMembersAverHP < 0.50f) return true;

        if (Ixochole.CanUse(out act, CanUseOption.OnLastAbility)) return true;

        if (Kerachole.CanUse(out act, CanUseOption.OnLastAbility)) return true;

        return false;
    }
}
