namespace DefaultRotations.Melee;

[RotationDesc(ActionID.Mug)]
[SourceCode("https://github.com/ArchiDog1998/FFXIVRotations/blob/main/DefaultRotations/Melee/NIN_Default.cs")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/nin/earlymug3.png")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/nin/nininfographicwindows.png")]
[LinkDescription("https://docs.google.com/spreadsheets/u/0/d/1BZZrqWMRrugCeiBICEgjCz2vRNXt_lRTxPnSQr24Em0/htmlview#",
    "Under the “Planner (With sample section)”")]
public sealed class NIN_Default : NIN_Base
{
    public override string GameVersion => "6.35";

    public override string RotationName => "Standard";

    private static INinAction _ninActionAim = null;
    private static bool InTrickAttack => TrickAttack.IsCoolingDown && !TrickAttack.ElapsedAfter(17);
    private static bool InMug => Mug.IsCoolingDown && !Mug.ElapsedAfter(19);
    private static bool NoNinjutsu => AdjustId(ActionID.Ninjutsu) is ActionID.Ninjutsu or ActionID.RabbitMedium;

    protected override IRotationConfigSet CreateConfiguration()
    {
        return base.CreateConfiguration()
            .SetBool("UseHide", true, "Use Hide")
            .SetBool("AutoUnhide", true, "Use Unhide");
    }

    protected override IAction CountDownAction(float remainTime)
    {
        if (remainTime > 10) ClearNinjutsu();

        var realInHuton = !HutonEndAfterGCD() || IsLastAction(false, Huton);
        if (realInHuton && _ninActionAim == Huton) ClearNinjutsu();

        if (DoNinjutsu(out var act))
        {
            if (act == Suiton && remainTime > Service.Config.CountDownAhead) return null;
            return act;
        }

        else if (remainTime < 5)
        {
            SetNinjutsu(Suiton);
        }
        else if (remainTime < 10)
        {
            if (_ninActionAim == null && Ten.IsCoolingDown && Hide.CanUse(out act)) return act;
            if (!realInHuton)
            {
                SetNinjutsu(Huton);
            }
        }
        return base.CountDownAction(remainTime);
    }

    #region Ninjutsu
    private static void SetNinjutsu(INinAction act)
    {
        if (act == null || AdjustId(ActionID.Ninjutsu) == ActionID.RabbitMedium) return;
        if (_ninActionAim != null && IsLastAction(false, Ten, Jin, Chi, FumaShurikenTen, FumaShurikenJin)) return;
        if (_ninActionAim != act)
        {
            _ninActionAim = act;
        }
    }

    private static void ClearNinjutsu()
    {
        if (_ninActionAim != null)
        {
            _ninActionAim = null;
        }
    }

    private bool ChoiceNinjutsu(out IAction act)
    {
        act = null;
        if (AdjustId(ActionID.Ninjutsu) != ActionID.Ninjutsu) return false;
        if (TimeSinceLastAction.TotalSeconds > 4.5) ClearNinjutsu();
        if (_ninActionAim != null && WeaponRemain < 0.2) return false;

        //Kassatsu
        if (Player.HasStatus(true, StatusID.Kassatsu))
        {
            if (GokaMekkyaku.CanUse(out _))
            {
                SetNinjutsu(GokaMekkyaku);
                return false;
            }
            if (HyoshoRanryu.CanUse(out _))
            {
                SetNinjutsu(HyoshoRanryu);
                return false;
            }

            if (Katon.CanUse(out _))
            {
                SetNinjutsu(Katon);
                return false;
            }

            if (Raiton.CanUse(out _))
            {
                SetNinjutsu(Raiton);
                return false;
            }
        }
        else
        {
            //Buff
            if (Huraijin.CanUse(out act)) return true;
            if (!HutonEndAfterGCD() && _ninActionAim?.ID == Huton.ID)
            {
                ClearNinjutsu();
                return false;
            }
            if (Ten.CanUse(out _, CanUseOption.EmptyOrSkipCombo)
               && (!InCombat || !Huraijin.EnoughLevel) && Huton.CanUse(out _)
               && !IsLastAction(false, Huton))
            {
                SetNinjutsu(Huton);
                return false;
            }

            //Aoe
            if (Katon.CanUse(out _))
            {
                if (!Player.HasStatus(true, StatusID.Doton) && !IsMoving && !TenChiJin.WillHaveOneCharge(10))
                    SetNinjutsu(Doton);
                else SetNinjutsu(Katon);
                return false;
            }

            //Vulnerable
            if (InBurst && TrickAttack.WillHaveOneCharge(18) && Suiton.CanUse(out _))
            {
                SetNinjutsu(Suiton);
                return false;
            }

            //Single
            if (Ten.CanUse(out _, InTrickAttack && !Player.HasStatus(false, StatusID.RaijuReady) ? CanUseOption.EmptyOrSkipCombo : CanUseOption.None))
            {
                if (Raiton.CanUse(out _))
                {
                    SetNinjutsu(Raiton);
                    return false;
                }

                if (!Chi.EnoughLevel && FumaShuriken.CanUse(out _))
                {
                    SetNinjutsu(FumaShuriken);
                    return false;
                }
            }
        }

        if (IsLastAction(false, DotonChi, SuitonJin,
            RabbitMedium, FumaShuriken, Katon, Raiton,
            Hyoton, Huton, Doton, Suiton, GokaMekkyaku, HyoshoRanryu))
        {
            ClearNinjutsu();
        }
        return false;
    }

    private bool DoNinjutsu(out IAction act)
    {
        act = null;

        //TenChiJin
        if (Player.HasStatus(true, StatusID.TenChiJin))
        {
            uint tenId = AdjustId(Ten.ID);
            uint chiId = AdjustId(Chi.ID);
            uint jinId = AdjustId(Jin.ID);

            //第一个
            if (tenId == FumaShurikenTen.ID)
            {
                //AOE
                if (Katon.CanUse(out _))
                {
                    if (FumaShurikenJin.CanUse(out act)) return true;
                }
                //Single
                if (FumaShurikenTen.CanUse(out act)) return true;
            }

            //第二击杀AOE
            else if (tenId == KatonTen.ID)
            {
                if (KatonTen.CanUse(out act, CanUseOption.MustUse)) return true;
            }
            //其他几击
            else if (chiId == RaitonChi.ID)
            {
                if (RaitonChi.CanUse(out act, CanUseOption.MustUse)) return true;
            }
            else if (chiId == DotonChi.ID)
            {
                if (DotonChi.CanUse(out act, CanUseOption.MustUse)) return true;
            }
            else if (jinId == SuitonJin.ID)
            {
                if (SuitonJin.CanUse(out act, CanUseOption.MustUse)) return true;
            }
        }

        //Keep Kassatsu in Burst.
        if (!Player.WillStatusEnd(3, false, StatusID.Kassatsu) 
            && Player.HasStatus(false, StatusID.Kassatsu) && !InTrickAttack) return false;
        if (_ninActionAim == null) return false;

        var id = AdjustId(ActionID.Ninjutsu);

        //First
        if (id == ActionID.Ninjutsu)
        {
            //Can't use.
            if (!Player.HasStatus(true, StatusID.Kassatsu, StatusID.TenChiJin)
                && !Ten.CanUse(out _, CanUseOption.EmptyOrSkipCombo))
            {
                return false;
            }
            act = _ninActionAim.Ninjutsu[0];
            return true;
        }
        //Failed
        else if ((uint)id == RabbitMedium.ID)
        {
            ClearNinjutsu();
            act = null;
            return false;
        }
        //Finished
        else if ((uint)id == _ninActionAim.ID)
        {
            if (_ninActionAim.CanUse(out act, CanUseOption.MustUse)) return true;
            if (_ninActionAim.ID == Doton.ID && !InCombat)
            {
                act = _ninActionAim;
                return true;
            }
        }
        //Second
        else if ((uint)id == FumaShuriken.ID)
        {
            if (_ninActionAim.Ninjutsu.Length > 1)
            {
                act = _ninActionAim.Ninjutsu[1];
                return true;
            }
        }
        //Third
        else if ((uint)id == Katon.ID || (uint)id == Raiton.ID || (uint)id == Hyoton.ID)
        {
            if (_ninActionAim.Ninjutsu.Length > 2)
            {
                act = _ninActionAim.Ninjutsu[2];
                return true;
            }
        }
        return false;
    }
    #endregion

    protected override bool GeneralGCD(out IAction act)
    {
        var hasRaijuReady = Player.HasStatus(true, StatusID.RaijuReady);

        if ((InTrickAttack || InMug) && NoNinjutsu && !hasRaijuReady
            && PhantomKamaitachi.CanUse(out act)) return true;

        if (ChoiceNinjutsu(out act)) return true;
        if ((!InCombat || !CombatElapsedLess(9)) && DoNinjutsu(out act)) return true;

        //No Ninjutsu
        if (NoNinjutsu)
        {
            if (!CombatElapsedLess(10) && FleetingRaiju.CanUse(out act)) return true;
            if (hasRaijuReady) return false;

            if (Huraijin.CanUse(out act)) return true;

            //AOE
            if (HakkeMujinsatsu.CanUse(out act)) return true;
            if (DeathBlossom.CanUse(out act)) return true;

            //Single
            if (ArmorCrush.CanUse(out act)) return true;
            if (AeolianEdge.CanUse(out act)) return true;
            if (GustSlash.CanUse(out act)) return true;
            if (SpinningEdge.CanUse(out act)) return true;

            //Range
            if (SpecialType == SpecialCommandType.MoveForward && MoveForwardAbility(out act)) return true;
            if (ThrowingDagger.CanUse(out act)) return true;
        }

        if (Configs.GetBool("AutoUnhide"))
        {
            StatusHelper.StatusOff(StatusID.Hidden);
        }
        if (!InCombat && _ninActionAim == null && Configs.GetBool("UseHide")
            && Ten.IsCoolingDown && Hide.CanUse(out act)) return true;

        act = null;
        return false;
    }

    [RotationDesc(ActionID.ForkedRaiju)]
    protected override bool MoveForwardGCD(out IAction act)
    {
        if (ForkedRaiju.CanUse(out act)) return true;
        return base.MoveForwardGCD(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction act)
    {
        if (!NoNinjutsu || !InCombat) return base.EmergencyAbility(nextGCD, out act);

        if (Kassatsu.CanUse(out act)) return true;
        if (UseBurstMedicine(out act)) return true;

        if (InBurst && !CombatElapsedLess(5) && Mug.CanUse(out act)) return true;

        //Use Suiton
        if (!CombatElapsedLess(6))
        {
            if (TrickAttack.CanUse(out act)) return true;
            if (TrickAttack.IsCoolingDown && !TrickAttack.WillHaveOneCharge(19)
                && Meisui.CanUse(out act)) return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction act)
    {
        act = null;
        if (!InCombat || AdjustId(2260) != 2260) return false;

        if (!IsMoving && InTrickAttack && !Ten.ElapsedAfter(30) && TenChiJin.CanUse(out act)) return true;

        if (!CombatElapsedLess(5) && Bunshin.CanUse(out act)) return true;

        if (InTrickAttack)
        {
            if (!DreamWithinADream.EnoughLevel)
            {
                if (Assassinate.CanUse(out act)) return true;
            }
            else
            {
                if (DreamWithinADream.CanUse(out act)) return true;
            }
        }

        if ((!InMug || InTrickAttack)
            && (!Bunshin.WillHaveOneCharge(10) || Player.HasStatus(false, StatusID.PhantomKamaitachiReady) || Mug.WillHaveOneCharge(2)))
        {
            if (HellfrogMedium.CanUse(out act)) return true;
            if (Bhavacakra.CanUse(out act)) return true;
        }
        return false;
    }
}
