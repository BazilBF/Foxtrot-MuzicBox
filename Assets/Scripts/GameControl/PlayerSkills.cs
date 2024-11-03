using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSkill
{
    protected float _cooldown = 120.0F;
    protected float _duration = 15.0F;
    protected int _skillId = -1;
    protected string _skillName;
    protected string _skillDescription;
    protected int[] _blockingSkills;


    private float _currentTime = 100.0F;
    private bool _active = false;
    private bool _avaible = false;
    private float _energyPrice = 10.0F;

    protected Player _activePlayer;

    protected UnityEngine.Color[] _netColors = null;

    public enum SkillState
    {
        cooldown,
        active,
        ready,
        noEnergy,
        blocked
    }

    public PlayerSkill(Player _inActivePlayer, bool inIsAvaible)
    {
        this._activePlayer = _inActivePlayer;
        this._avaible = inIsAvaible;
    }

    public UnityEngine.Color[] GetNetColors()
    {
        return this._netColors;
    }

    public static PlayerSkill GetSkill(Player inActivePlayer, int inIndex, bool inIsAvaible)
    {
        PlayerSkill skill = null;
        switch (inIndex)
        {
            case 0: skill = new Lullaby(inActivePlayer, inIsAvaible); break;
            case 1: skill = new NightCore(inActivePlayer, inIsAvaible); break;
        }
        return skill;
    }

    protected virtual void ApplySkill()
    {

    }

    protected virtual void RevokeSkill()
    {

    }

    public PlayerSkill.SkillState CheckState()
    {
        PlayerSkill.SkillState result = SkillState.ready;
        if (this._active)
        {
            result = SkillState.active;
        }
        else if (!this._active && this._currentTime <= this._cooldown)
        {
            result = SkillState.cooldown;
        }
        else if (this._activePlayer.GetEnergyValue() < this._energyPrice)
        {
            result = SkillState.noEnergy;
        }
        else if (this._activePlayer.CheckSkillIsBlocked(this._skillId))
        {
            result = SkillState.blocked;
        }
        return result;
    }

    public bool ActivateSkill()
    {

        bool returnValue = false;



        if (this._avaible && !this._active && this._activePlayer.CheckActiveSkill(this._skillId) && this.CheckState() == SkillState.ready)
        {
            Debug.Log($"Activating skill {this.GetSkillName()}");
            this._active = true;
            this._currentTime = 0;
            this.ApplySkill();
            this._activePlayer.SpendEnergy(this._energyPrice);
            returnValue = true;
        }
        else
        {
            Debug.Log($"Can't activate skill {this.GetSkillName()} {this._currentTime > this._cooldown}");
        }
        return returnValue;
    }

    public bool DeactivateSkill()
    {
        bool returnValue = false;
        if (this._active)
        {
            Debug.Log($"Deactivating skill {this.GetSkillName()}");
            this._currentTime = 0;
            this._active = false;
            this.RevokeSkill();
            returnValue = true;
        }
        return returnValue;
    }

    public bool[] ToggleSkill()
    {
        bool returnValue;
        if (this._active)
        {
            returnValue = this.DeactivateSkill();
        }
        else
        {
            returnValue = this.ActivateSkill();
        }
        return new bool[] { returnValue, this._active };
    }

    public float GetCountDown()
    {
        float duratuion = (this._active ? this._duration : this._cooldown);
        return (duratuion - this._currentTime) / duratuion;
    }



    public void SkillControl(float inDeltaTime)
    {
        this._currentTime += inDeltaTime;
        if (this._active && this._currentTime > this._duration)
        {
            this.DeactivateSkill();
        }

    }

    public bool CheckAvailble()
    {
        return this._avaible;
    }

    public bool CheckActive()
    {
        return this._active;
    }

    public void SetAvaible(bool inIsAvaible)
    {
        this._avaible = inIsAvaible;
    }

    public int[] GetBlockingSkills()
    {
        return this._blockingSkills;
    }

    public string GetSkillName()
    {
        return this._skillName;
    }
}
