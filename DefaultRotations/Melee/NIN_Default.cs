namespace DefaultRotations.Melee;

[Rotation("Standard", CombatType.PvE, GameVersion = "6.35")]

[RotationDesc(ActionID.MugPvE)]
[SourceCode(Path = "main/DefaultRotations/Melee/NIN_Default.cs")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/nin/earlymug3.png")]
[LinkDescription("https://www.thebalanceffxiv.com/img/jobs/nin/nininfographicwindows.png")]
[LinkDescription("https://docs.google.com/spreadsheets/u/0/d/1BZZrqWMRrugCeiBICEgjCz2vRNXt_lRTxPnSQr24Em0/htmlview#",
    "Under the “Planner (With sample section)”")]
[YoutubeLink(ID = "Al9KlhA3Zvw")]
public sealed class NIN_Default : NinjaRotation
{
    private IBaseAction? _ninActionAim = null;
    private bool InTrickAttack => TrickAttackPvE.Cooldown.IsCoolingDown && !TrickAttackPvE.Cooldown.ElapsedAfter(17);
    private bool InMug => MugPvE.Cooldown.IsCoolingDown && !MugPvE.Cooldown.ElapsedAfter(19);
    private static bool NoNinjutsu => AdjustId(ActionID.NinjutsuPvE) is ActionID.NinjutsuPvE or ActionID.RabbitMediumPvE;

    [UI("Use Hide")]
    [RotationConfig(CombatType.PvE)]
    public bool UseHide { get; set; } = true;

    [UI("Use Unhide")]
    [RotationConfig(CombatType.PvE)]
    public bool AutoUnhide { get; set; } = true;

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime > 10) ClearNinjutsu();

        var realInHuton = !HutonEndAfterGCD() || IsLastAction(false, HutonPvE);
        if (realInHuton && _ninActionAim == HutonPvE) ClearNinjutsu();

        if (DoNinjutsu(out var act))
        {
            if (act == SuitonPvE && remainTime > CountDownAhead) return null;
            return act;
        }

        else if (remainTime < 5)
        {
            SetNinjutsu(SuitonPvE);
        }
        else if (remainTime < 10)
        {
            if (_ninActionAim == null && TenPvE.Cooldown.IsCoolingDown && HidePvE.CanUse(out act)) return act;
            if (!realInHuton)
            {
                SetNinjutsu(HutonPvE);
            }
        }
        return base.CountDownAction(remainTime);
    }

    #region Ninjutsu
    private void SetNinjutsu(IBaseAction act)
    {
        if (act == null || AdjustId(ActionID.NinjutsuPvE) == ActionID.RabbitMediumPvE) return;

        if (_ninActionAim != null && IsLastAction(false, TenPvE, JinPvE, ChiPvE, FumaShurikenPvE_18873, FumaShurikenPvE_18874, FumaShurikenPvE_18875)) return;

        if (_ninActionAim != act)
        {
            _ninActionAim = act;
        }
    }

    private void ClearNinjutsu()
    {
        if (_ninActionAim != null)
        {
            _ninActionAim = null;
        }
    }

    private bool ChoiceNinjutsu(out IAction? act)
    {
        act = null;
        if (AdjustId(ActionID.NinjutsuPvE) != ActionID.NinjutsuPvE) return false;
        if (TimeSinceLastAction.TotalSeconds > 4.5) ClearNinjutsu();
        if (_ninActionAim != null && WeaponRemain < 0.2) return false;

        //Kassatsu
        if (Player.HasStatus(true, StatusID.Kassatsu))
        {
            if (GokaMekkyakuPvE.CanUse(out _))
            {
                SetNinjutsu(GokaMekkyakuPvE);
                return false;
            }
            if (HyoshoRanryuPvE.CanUse(out _))
            {
                SetNinjutsu(HyoshoRanryuPvE);
                return false;
            }

            if (KatonPvE.CanUse(out _))
            {
                SetNinjutsu(KatonPvE);
                return false;
            }

            if (RaitonPvE.CanUse(out _))
            {
                SetNinjutsu(RaitonPvE);
                return false;
            }
        }
        else
        {
            if (Player.HasStatus(true, StatusID.Suiton)
                && _ninActionAim == SuitonPvE && NoNinjutsu)
            {
                ClearNinjutsu();
            }

            //Buff
            if (HuraijinPvE.CanUse(out act)) return true;
            if (!HutonEndAfterGCD() && _ninActionAim?.ID == HutonPvE.ID)
            {
                ClearNinjutsu();
                return false;
            }
            if (TenPvE.CanUse(out _, usedUp: true)
               && (!InCombat || !HuraijinPvE.EnoughLevel) && HutonPvE.CanUse(out _)
               && !IsLastAction(false, HutonPvE))
            {
                SetNinjutsu(HutonPvE);
                return false;
            }

            //Aoe
            if (KatonPvE.CanUse(out _))
            {
                if (!Player.HasStatus(true, StatusID.Doton) && !IsMoving 
                    && (!TenChiJinPvE.Cooldown.WillHaveOneCharge(6)) || !TenChiJinPvE.Cooldown.IsCoolingDown)
                    SetNinjutsu(DotonPvE);
                else SetNinjutsu(KatonPvE);
                return false;
            }

            //Vulnerable
            if (IsBurst && TrickAttackPvE.Cooldown.WillHaveOneCharge(18) && SuitonPvE.CanUse(out _))
            {
                SetNinjutsu(SuitonPvE);
                return false;
            }

            //Single
            if (TenPvE.CanUse(out _, usedUp: InTrickAttack && !Player.HasStatus(false, StatusID.RaijuReady)))
            {
                if (RaitonPvE.CanUse(out _))
                {
                    SetNinjutsu(RaitonPvE);
                    return false;
                }

                if (!ChiPvE.EnoughLevel && FumaShurikenPvE.CanUse(out _))
                {
                    SetNinjutsu(FumaShurikenPvE);
                    return false;
                }
            }
        }

        if (IsLastAction(false, DotonPvE, SuitonPvE,
            RabbitMediumPvE, FumaShurikenPvE, KatonPvE, RaitonPvE,
            HyotonPvE, HutonPvE, DotonPvE, SuitonPvE, GokaMekkyakuPvE, HyoshoRanryuPvE))
        {
            ClearNinjutsu();
        }
        return false;
    }

    private bool DoNinjutsu(out IAction? act)
    {
        act = null;

        //TenChiJin
        if (Player.HasStatus(true, StatusID.TenChiJin))
        {
            uint tenId = AdjustId(TenPvE.ID);
            uint chiId = AdjustId(ChiPvE.ID);
            uint jinId = AdjustId(JinPvE.ID);

            //First
            if (tenId == FumaShurikenPvE_18873.ID
                && !IsLastAction(false, FumaShurikenPvE_18875, FumaShurikenPvE_18873))
            {
                //AOE
                if (KatonPvE.CanUse(out _))
                {
                    if (FumaShurikenPvE_18875.CanUse(out act)) return true;
                }
                //Single
                if (FumaShurikenPvE_18873.CanUse(out act)) return true;
            }

            //Second
            else if (tenId == KatonPvE_18876.ID && !IsLastAction(false, KatonPvE_18876))
            {
                if (KatonPvE_18876.CanUse(out act, skipAoeCheck: true)) return true;
            }
            //Others
            else if (chiId == RaitonPvE_18877.ID && !IsLastAction(false, RaitonPvE_18877))
            {
                if (RaitonPvE_18877.CanUse(out act, skipAoeCheck: true)) return true;
            }
            else if (chiId == DotonPvE_18880.ID && !IsLastAction(false, DotonPvE_18880))
            {
                if (DotonPvE_18880.CanUse(out act, skipAoeCheck: true)) return true;
            }
            else if (jinId == SuitonPvE_18881.ID && !IsLastAction(false, SuitonPvE_18881))
            {
                if (SuitonPvE_18881.CanUse(out act, skipAoeCheck: true)) return true;
            }
        }

        //Keep Kassatsu in Burst.
        if (!Player.WillStatusEnd(3, false, StatusID.Kassatsu)
            && Player.HasStatus(false, StatusID.Kassatsu) && !InTrickAttack) return false;
        if (_ninActionAim == null) return false;

        var id = AdjustId(ActionID.NinjutsuPvE);

        //Failed
        if ((uint)id == RabbitMediumPvE.ID)
        {
            ClearNinjutsu();
            act = null;
            return false;
        }
        //First
        else if (id == ActionID.NinjutsuPvE)
        {
            //Can't use.
            if (!Player.HasStatus(true, StatusID.Kassatsu, StatusID.TenChiJin)
                && !TenPvE.CanUse(out _, usedUp: true)
                && !IsLastAction(false, _ninActionAim.Setting.Ninjutsu![0]))
            {
                return false;
            }
            act = _ninActionAim.Setting.Ninjutsu![0];
            return true;
        }
        //Finished
        else if ((uint)id == _ninActionAim.ID)
        {
            if (_ninActionAim.CanUse(out act, skipAoeCheck: true)) return true;
            if (_ninActionAim.ID == DotonPvE.ID && !InCombat)
            {
                act = _ninActionAim;
                return true;
            }
        }
        //Second
        else if ((uint)id == FumaShurikenPvE.ID)
        {
            if (_ninActionAim.Setting.Ninjutsu!.Length > 1
                && !IsLastAction(false, _ninActionAim.Setting.Ninjutsu![1]))
            {
                act = _ninActionAim.Setting.Ninjutsu![1];
                return true;
            }
        }
        //Third
        else if ((uint)id == KatonPvE.ID || (uint)id == RaitonPvE.ID || (uint)id == HyotonPvE.ID)
        {
            if (_ninActionAim.Setting.Ninjutsu!.Length > 2
                && !IsLastAction(false, _ninActionAim.Setting.Ninjutsu![2]))
            {
                act = _ninActionAim.Setting.Ninjutsu![2];
                return true;
            }
        }
        return false;
    }
    #endregion

    protected override bool GeneralGCD(out IAction? act)
    {
        var hasRaijuReady = Player.HasStatus(true, StatusID.RaijuReady);

        if ((InTrickAttack || InMug) && NoNinjutsu && !hasRaijuReady
            && !Player.HasStatus(true, StatusID.TenChiJin)
            && PhantomKamaitachiPvE.CanUse(out act)) return true;

        if ((!InCombat || !CombatElapsedLess(7)) && ChoiceNinjutsu(out act)) return true;
        if (DoNinjutsu(out act)) return true;

        //No Ninjutsu
        if (NoNinjutsu)
        {
            if (!CombatElapsedLess(10) && FleetingRaijuPvE.CanUse(out act)) return true;
            if (hasRaijuReady) return false;
        }

        if (HuraijinPvE.CanUse(out act)) return true;

        //AOE
        if (HakkeMujinsatsuPvE.CanUse(out act)) return true;
        if (DeathBlossomPvE.CanUse(out act)) return true;

        //Single
        if (ArmorCrushPvE.CanUse(out act)) return true;
        if (AeolianEdgePvE.CanUse(out act)) return true;
        if (GustSlashPvE.CanUse(out act)) return true;
        if (SpinningEdgePvE.CanUse(out act)) return true;

        //Range
        if (MergedStatus.HasFlag(AutoStatus.MoveForward) && MoveForwardAbility(out act)) return true;
        if (ThrowingDaggerPvE.CanUse(out act)) return true;

        if (AutoUnhide)
        {
            StatusHelper.StatusOff(StatusID.Hidden);
        }
        if (!InCombat && _ninActionAim == null && UseHide
            && TenPvE.Cooldown.IsCoolingDown && HidePvE.CanUse(out act)) return true;

        return base.GeneralGCD(out act);
    }

    [RotationDesc(ActionID.ForkedRaijuPvE)]
    protected override bool MoveForwardGCD(out IAction? act)
    {
        if (ForkedRaijuPvE.CanUse(out act)) return true;
        return base.MoveForwardGCD(out act);
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        if (!NoNinjutsu || !InCombat) return base.EmergencyAbility(nextGCD, out act);

        if (KassatsuPvE.CanUse(out act)) return true;
        if (UseBurstMedicine(out act)) return true;

        if (IsBurst && !CombatElapsedLess(5) && MugPvE.CanUse(out act)) return true;

        //Use Suiton
        if (!CombatElapsedLess(6))
        {
            if (TrickAttackPvE.CanUse(out act)) return true;
            if (TrickAttackPvE.Cooldown.IsCoolingDown && !TrickAttackPvE.Cooldown.WillHaveOneCharge(19)
                && MeisuiPvE.CanUse(out act)) return true;
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        act = null;
        if (!NoNinjutsu || !InCombat) return false;

        if (!IsMoving && InTrickAttack && !TenPvE.Cooldown.ElapsedAfter(30) && TenChiJinPvE.CanUse(out act)) return true;

        if (!CombatElapsedLess(5) && BunshinPvE.CanUse(out act)) return true;

        if (InTrickAttack)
        {
            if (!DreamWithinADreamPvE.EnoughLevel)
            {
                if (AssassinatePvE.CanUse(out act)) return true;
            }
            else
            {
                if (DreamWithinADreamPvE.CanUse(out act)) return true;
            }
        }

        if ((!InMug || InTrickAttack)
            && (!BunshinPvE.Cooldown.WillHaveOneCharge(10) || Player.HasStatus(false, StatusID.PhantomKamaitachiReady) || MugPvE.Cooldown.WillHaveOneCharge(2)))
        {
            if (HellfrogMediumPvE.CanUse(out act)) return true;
            if (BhavacakraPvE.CanUse(out act)) return true;
        }
        return base.AttackAbility(out act);
    }

    public override void DisplayStatus()
    {
        if(_ninActionAim != null)
        {
            ImGui.Text(_ninActionAim.ToString()  + _ninActionAim.AdjustedID.ToString());
        }
        base.DisplayStatus();
    }
}
