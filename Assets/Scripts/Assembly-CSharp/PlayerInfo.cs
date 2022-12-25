using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerInfo
{
    private PlayerInfo()
    {
        this.Load();
    }

    public bool dirty
    {
        get
        {
            return this._dirty;
        }
    }

    public int amountOfCoins
    {
        get
        {
            return this._amountOfCoins;
        }
        set
        {
            if (this._amountOfCoins != value)
            {
                this._amountOfCoins = value;
                this._dirty = true;
                Action action = this.onCoinsChanged;
                if (action != null)
                {
                    action();
                }
            }
        }
    }

    public int highestScore
    {
        get
        {
            return this._highestScore;
        }
        set
        {
            if (value > this._highestScore)
            {
                this._oldHighestScore = this._highestScore;
                this._highestScore = value;
                this._dirty = true;
                Action action = this.onHighScoreChanged;
                if (action != null)
                {
                    action();
                }
            }
        }
    }

    public int oldHighestScore
    {
        get
        {
            return this._oldHighestScore;
        }
    }

    public void SetOldestHighestScore()
    {
        if (this._oldHighestScore < this._highestScore)
        {
            this._oldHighestScore = this._highestScore;
            this._dirty = true;
        }
    }

    public void BragCompleted()
    {
        if (this._oldHighestScore < this._highestScore)
        {
            this._oldHighestScore = this._highestScore;
            this._dirty = true;
        }
    }

    public int highestMeters
    {
        get
        {
            return this._highestMeters;
        }
        set
        {
            this._highestMeters = value;
            this._dirty = true;
        }
    }

    public int amountOfMysteryBoxesOpened
    {
        get
        {
            return this._amountOfMysteryBoxesOpened;
        }
        set
        {
            this._amountOfMysteryBoxesOpened = value;
        }
    }

    public int mysteryBoxesToUnlock
    {
        get
        {
            return this._mysteryBoxesToUnlock;
        }
        set
        {
            this._mysteryBoxesToUnlock = value;
        }
    }

    public int currentMissionSet
    {
        get
        {
            return this._currentMissionSet;
        }
    }

    public int lastMissionCompleted
    {
        get
        {
            return this._lastMissionCompleted;
        }
    }

    public bool IsCurrentMissionSetInited()
    {
        return false;
    }

    public void InitCurrentMissionSet(int missionSet, int missionCount)
    {
        if (missionSet != this._currentMissionSet)
        {
            this._currentMissionSet = missionSet;
            this._currentMissionProgress = new int[missionCount];
            for (int i = 0; i < missionCount; i++)
            {
                this._currentMissionProgress[i] = 0;
            }
            this._dirty = true;
            Action action = this.onScoreMultiplierChanged;
            if (action != null)
            {
                action();
            }
        }
    }

    public void ReInitCurrentMissionSet(int missionSet, int missionCount)
    {
        this._currentMissionSet = missionSet;
        this._currentMissionProgress = new int[missionCount];
        for (int i = 0; i < missionCount; i++)
        {
            this._currentMissionProgress[i] = 0;
        }
        this._dirty = true;
        Action action = this.onScoreMultiplierChanged;
        if (action != null)
        {
            action();
        }
    }

    public int GetCurrentMissionProgress(int mission)
    {
        if (this._currentMissionProgress == null)
        {
            return 0;
        }
        if (mission < this._currentMissionProgress.Length)
        {
            return this._currentMissionProgress[mission];
        }
        return 0;
    }

    public void SetCurrentMissionProgress(int mission, int progress)
    {
        if (this._currentMissionProgress[mission] != progress)
        {
            this._currentMissionProgress[mission] = progress;
            this._dirty = true;
        }
    }

    public bool IncrementCurrentMissionProgress(int mission, int target)
    {
        if (this._currentMissionProgress[mission] < target)
        {
            this._currentMissionProgress[mission]++;
            this._dirty = true;
            return this._currentMissionProgress[mission] == target;
        }
        return false;
    }

    public int scoreMultiplier
    {
        get
        {
            int num = Mathf.Clamp(this._currentMissionSet + 1, 0, 30);
            if (this.doubleScore)
            {
                num *= 2;
            }
            return num;
        }
    }

    public int rawMultiplier
    {
        get
        {
            return Mathf.Clamp(this._currentMissionSet + 1, 0, 30);
        }
    }

    public int GetCurrentRank()
    {
        return this.currentMissionSet / Missions.Instance.GetNumberOfBasicMission();
    }

    public int currentCharacter
    {
        get
        {
            return this._currentCharacter;
        }
        set
        {
            if (value != this._currentCharacter)
            {
                this._currentCharacter = value;
                this._dirty = true;
                this.SaveIfDirty();
            }
        }
    }

    public void CollectToken(CharacterModels.TokenType tokenType, int amount = 1)
    {
        this._collectedCharacterTokens[(int)tokenType] += amount;
        this._dirty = true;
        Action<CharacterModels.TokenType> onTokenCollected = this.OnTokenCollected;
        if (onTokenCollected != null)
        {
            onTokenCollected(tokenType);
        }
        this.SaveIfDirty();
    }

    public bool[] GetUnlockedTrophies()
    {
        return this._unlockedTrophies;
    }

    public void unlockTrophy(Trophies.Trophy trophy)
    {
        if (this._unlockedTrophies[(int)trophy])
        {
            return;
        }
        this._unlockedTrophies[(int)trophy] = true;
        this._dirty = true;
        Action<Trophies.Trophy> onTrophyUnlocked = this.OnTrophyUnlocked;
        if (onTrophyUnlocked != null)
        {
            onTrophyUnlocked(trophy);
        }
        bool flag = true;
        bool[] unlockedTrophies = this._unlockedTrophies;
        for (int i = 0; i < unlockedTrophies.Length; i++)
        {
            if (!unlockedTrophies[i])
            {
                flag = false;
            }
        }
        if (flag)
        {
            SocialManager.instance.setAchievement(Trophies.trophyAchievementID, 100f);
        }
        this.SaveIfDirty();
    }

    public bool isTrophyUnlocked(Trophies.Trophy trophy)
    {
        return this._unlockedTrophies[(int)trophy];
    }

    public int[] achievementProgress
    {
        get
        {
            return this._achievementProgress;
        }
        set
        {
            this._achievementProgress = value;
        }
    }

    public bool IsCollectionComplete(CharacterModels.ModelType modelType)
    {
        CharacterModels.Model model = CharacterModels.modelData[modelType];
        return model.unlockType == CharacterModels.UnlockType.free || this.GetCollectedTokens(modelType) >= model.Price;
    }

    public int GetCollectedTokens(CharacterModels.ModelType modelType)
    {
        int num = 0;
        foreach (CharacterModels.TokenType tokenType2 in CharacterModels.modelData[modelType].tokenType)
        {
            num += this._collectedCharacterTokens[(int)tokenType2];
        }
        return num;
    }

    public bool IsTokenUseful(CharacterModels.TokenType tokenType)
    {
        foreach (object obj in Enum.GetValues(typeof(CharacterModels.ModelType)))
        {
            CharacterModels.ModelType modelType = (CharacterModels.ModelType)((int)obj);
            CharacterModels.Model model = CharacterModels.modelData[modelType];
            bool flag = false;
            foreach (CharacterModels.TokenType tokenType3 in model.tokenType)
            {
                if (tokenType3 == tokenType)
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                if (PlayerInfo.Instance.GetCollectedTokens(modelType) < model.Price)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool hasPayedOutFacebook
    {
        get
        {
            return this._hasPayedOutFacebook;
        }
        set
        {
            this._hasPayedOutFacebook = value;
            this._dirty = true;
        }
    }

    public bool hasPayedOutGameCenter
    {
        get
        {
            return this._hasPayedOutGameCenter;
        }
        set
        {
            this._hasPayedOutGameCenter = value;
            this._dirty = true;
        }
    }

    public string dailyWord
    {
        get
        {
            return this._dailyWord;
        }
    }

    public IntMask dailyWordUnlockedMask
    {
        get
        {
            return this._dailyWordUnlockedMask;
        }
    }

    public DateTime dailyWordExpireTime
    {
        get
        {
            return this._dailyWordExpireTime;
        }
    }

    public DateTime dailyWordPayedOutTime
    {
        get
        {
            return this._dailyWordPayedOutTime;
        }
    }

    public void InitDailyWord(string word, DateTime expires)
    {
        if (!this._dailyWord.Equals(word) || !this._dailyWordExpireTime.Equals(expires))
        {
            this._dailyWord = word;
            this._dailyWordExpireTime = expires;
            this._dailyWordPayedOutTime = DateTime.UtcNow;
            this._dailyWordUnlockedMask = 0;
            this._dirty = true;
            this.SaveIfDirty();
        }
        DailyLetterPickupManager.Instance.UpdateLetter();
    }

    public void PickedupLetter(char letter)
    {
        for (int i = 0; i < this._dailyWord.Length; i++)
        {
            if (this._dailyWord[i] == letter && !this._dailyWordUnlockedMask[i])
            {
                this._dailyWordUnlockedMask[i] = true;
                Action onPickedUpLetter = this.OnPickedUpLetter;
                if (onPickedUpLetter != null)
                {
                    onPickedUpLetter();
                }
                this._dirty = true;
                this.SaveIfDirty();
                break;
            }
        }
        if (this.isDailyWordComplete() && this._dailyWordPayedOutTime != this._dailyWordExpireTime)
        {
            Missions.Instance.PlayerDidThis(Missions.MissionTarget.DailyQuests, 1);
            this._mysteryBoxesToUnlock++;
            this._dailyWordPayedOutTime = this._dailyWordExpireTime;
            this._dirty = true;
            this.SaveIfDirty();
            UIScreenController.Instance.QueueSlideIn(UIScreenController.SlideInType.LettersComplete, string.Empty);
            FlurryAndroid.logEvent("Daily Challenge completed", new Dictionary<string, string> { { "Id", this._dailyWord } });
        }
    }

    public char GetNewDailyLetter()
    {
        for (int i = 0; i < this._dailyWord.Length; i++)
        {
            if (!this._dailyWordUnlockedMask[i])
            {
                return this._dailyWord[i];
            }
        }
        return '\0';
    }

    public bool isDailyWordComplete()
    {
        return (1 << this._dailyWord.Length) - 1 == this._dailyWordUnlockedMask;
    }

    public bool tutorialCompleted
    {
        get
        {
            return this._tutorialCompleted;
        }
        set
        {
            this._tutorialCompleted = value;
            this._dirty = true;
            this.SaveIfDirty();
        }
    }

    public int inAppPurchaseCount
    {
        get
        {
            return this._inAppPurchaseCount;
        }
        set
        {
            this._inAppPurchaseCount = value;
            this._dirty = true;
        }
    }

    public string earnCurrenyData
    {
        get
        {
            return this._earnCurrenyData;
        }
        set
        {
            this._earnCurrenyData = value;
            this._dirty = true;
        }
    }

    public int GetUpgradeTierSum()
    {
        return this.GetCurrentTier(PowerupType.jetpack) + this.GetCurrentTier(PowerupType.doubleMultiplier) + this.GetCurrentTier(PowerupType.coinmagnet) + this.GetCurrentTier(PowerupType.supersneakers);
    }

    public int GetUpgradeAmount(PowerupType type)
    {
        return this._upgradeAmounts[type];
    }

    public int GetCurrentTier(PowerupType type)
    {
        if (!this._upgradeTiers.ContainsKey(type))
        {
            return 0;
        }
        return this._upgradeTiers[type];
    }

    public float GetPowerupDuration(PowerupType type)
    {
        if (!Upgrades.upgrades.ContainsKey(type))
        {
            Debug.Log("Couldn't find any upgrades of the type: " + type.ToString() + ". Returning 0");
            return 0f;
        }
        return Upgrades.upgrades[type].durations[this.GetCurrentTier(type)];
    }

    public void IncreasePowerupTier(PowerupType type)
    {
        if (this._upgradeTiers.ContainsKey(type))
        {
            this._upgradeTiers[type] = this._upgradeTiers[type] + 1;
            this._dirty = true;
            this.SaveIfDirty();
        }
        else
        {
            Debug.LogError("Trying to increase tier for a non-tiered upgrade");
        }
    }

    public void UseUpgrade(PowerupType type)
    {
        Debug.Log("Used powerup: " + type.ToString());
        if (this._upgradeAmounts.ContainsKey(type))
        {
            Dictionary<PowerupType, int> upgradeAmounts;
            Dictionary<PowerupType, int> dictionary = (upgradeAmounts = this._upgradeAmounts);
            int num = upgradeAmounts[type];
            dictionary[type] = num - 1;
            this._dirty = true;
            Action action = this.onPowerupAmountChanged;
            if (action != null)
            {
                action();
            }
            this.SaveIfDirty();
        }
    }

    public void IncreaseUpgradeAmount(PowerupType type, int amount = 1)
    {
        if (this._upgradeAmounts.ContainsKey(type))
        {
            Dictionary<PowerupType, int> upgradeAmounts;
            Dictionary<PowerupType, int> dictionary = (upgradeAmounts = this._upgradeAmounts);
            int num = upgradeAmounts[type];
            dictionary[type] = num + amount;
            this._dirty = true;
            Action action = this.onPowerupAmountChanged;
            if (action != null)
            {
                action();
            }
            this.SaveIfDirty();
        }
        else
        {
            Debug.LogError("Trying to increase upgrade amount for a non-consumable");
        }
    }

    public int GetNumberOfAffordableUpgrades()
    {
        int num = 0;
        bool flag = Missions.Instance.HasMoreMissions();
        foreach (KeyValuePair<PowerupType, Upgrade> keyValuePair in Upgrades.upgrades)
        {
            Upgrade value = keyValuePair.Value;
            if (value.numberOfTiers > 0)
            {
                int num2 = this.GetCurrentTier(keyValuePair.Key) + 1;
                if (num2 < value.pricesRaw.Length)
                {
                    int price = value.getPrice(num2);
                    if (price <= this.amountOfCoins)
                    {
                        num++;
                    }
                }
            }
            else if (value.pricesRaw != null && value.pricesRaw.Length > 0)
            {
                if (keyValuePair.Key != PowerupType.skipmission1 || (flag && !Missions.Instance.GetMissionInfo(0).complete))
                {
                    if (keyValuePair.Key != PowerupType.skipmission2 || (flag && !Missions.Instance.GetMissionInfo(1).complete))
                    {
                        if (keyValuePair.Key != PowerupType.skipmission3 || (flag && !Missions.Instance.GetMissionInfo(2).complete))
                        {
                            int price2 = value.getPrice(0);
                            if (price2 <= this.amountOfCoins)
                            {
                                num++;
                            }
                        }
                    }
                }
            }
        }
        return num;
    }

    private static string GetSaveDataPath()
    {
        string text = Application.persistentDataPath + "/playerdata";
        Debug.Log("playerdata save data path: \"" + text + "\"");
        return text;
    }

    public void SaveIfDirty()
    {
        if (this._dirty)
        {
            this.Save();
        }
    }

    public void Save()
    {
        try
        {
            MemoryStream memoryStream = new MemoryStream(8192);
            BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(1);
            Dictionary<PlayerInfo.Key, string> dictionary = new Dictionary<PlayerInfo.Key, string>(17);
            dictionary[PlayerInfo.Key.AmountOfCoins] = this._amountOfCoins.ToString();
            dictionary[PlayerInfo.Key.HighestScore] = this._highestScore.ToString();
            dictionary[PlayerInfo.Key.OldHighestScore] = this._oldHighestScore.ToString();
            dictionary[PlayerInfo.Key.DailyWord] = this._dailyWord;
            dictionary[PlayerInfo.Key.DailyWordUnlockMask] = this._dailyWordUnlockedMask.ToString();
            dictionary[PlayerInfo.Key.DailyWordExpireTime] = this._dailyWordExpireTime.ToString("o");
            dictionary[PlayerInfo.Key.DailyWordPayedOutTime] = this._dailyWordPayedOutTime.ToString("o");
            dictionary[PlayerInfo.Key.CurrentCharacter] = this._currentCharacter.ToString();
            dictionary[PlayerInfo.Key.CurrentMissionSet] = this._currentMissionSet.ToString();
            dictionary[PlayerInfo.Key.AmountOfMysteryBoxesOpened] = this._amountOfMysteryBoxesOpened.ToString();
            dictionary[PlayerInfo.Key.TutorialCompleted] = this._tutorialCompleted.ToString();
            dictionary[PlayerInfo.Key.InAppPurchaseCount] = this._inAppPurchaseCount.ToString();
            dictionary[PlayerInfo.Key.EarnCurrencyData] = this._earnCurrenyData;
            dictionary[PlayerInfo.Key.PayBonusFacebook] = this._hasPayedOutFacebook.ToString();
            dictionary[PlayerInfo.Key.PayBonusGameCenter] = this._hasPayedOutGameCenter.ToString();
            if (this._currentMissionSet >= 0)
            {
                dictionary[PlayerInfo.Key.CurrentMissionSetProgress] = string.Join(",", Array.ConvertAll<int, string>(this._currentMissionProgress, (int input) => input.ToString()));
            }
            dictionary[PlayerInfo.Key.CollectedCharacterTokens] = string.Join(",", Array.ConvertAll<int, string>(this._collectedCharacterTokens, (int input) => input.ToString()));
            dictionary[PlayerInfo.Key.UnlockedTrophies] = string.Join(",", Array.ConvertAll<bool, string>(this._unlockedTrophies, (bool input) => input.ToString()));
            dictionary[PlayerInfo.Key.AchievementProgress] = string.Join(",", Array.ConvertAll<int, string>(this._achievementProgress, (int input) => input.ToString()));
            FileUtil.WriteEnumStringDictionary<PlayerInfo.Key>(binaryWriter, dictionary);
            FileUtil.WriteEnumIntDictionary<PowerupType>(binaryWriter, this._upgradeAmounts);
            FileUtil.WriteEnumIntDictionary<PowerupType>(binaryWriter, this._upgradeTiers);
            FileUtil.Save(PlayerInfo.GetSaveDataPath(), "we12rtyuiklhgfdjerKJGHfvghyuhnjiokLJHl145rtyfghjvbn", memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
            memoryStream.Close();
            this._dirty = false;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error saving player info: " + ex.ToString());
        }
    }
    public int TryParseIntMask(string mask)
    {
        if (int.TryParse(mask,out var result))
        {
            return result;
        }
        else
        {
            return 0;
        }
    }
    public void Load()
    {
        try
        {
            byte[] array = FileUtil.Load(PlayerInfo.GetSaveDataPath(), "we12rtyuiklhgfdjerKJGHfvghyuhnjiokLJHl145rtyfghjvbn");
            MemoryStream memoryStream = new MemoryStream(array);
            BinaryReader binaryReader = new BinaryReader(memoryStream);
            int num = binaryReader.ReadInt32();
            Dictionary<PlayerInfo.Key, string> dictionary = FileUtil.ReadEnumStringDictionary<PlayerInfo.Key>(binaryReader);
            this._amountOfCoins = ((!dictionary.ContainsKey(PlayerInfo.Key.AmountOfCoins)) ? 0 : int.Parse(dictionary[PlayerInfo.Key.AmountOfCoins]));
            this._highestScore = ((!dictionary.ContainsKey(PlayerInfo.Key.HighestScore)) ? 0 : int.Parse(dictionary[PlayerInfo.Key.HighestScore]));
            this._oldHighestScore = ((!dictionary.ContainsKey(PlayerInfo.Key.OldHighestScore)) ? 0 : int.Parse(dictionary[PlayerInfo.Key.HighestScore]));
            this._dailyWord = ((!dictionary.ContainsKey(PlayerInfo.Key.DailyWord)) ? string.Empty : dictionary[PlayerInfo.Key.DailyWord]);
            this._dailyWordUnlockedMask = ((!dictionary.ContainsKey(PlayerInfo.Key.DailyWordUnlockMask)) ? 0 : TryParseIntMask(dictionary[PlayerInfo.Key.DailyWordUnlockMask])) ;
            this._dailyWordExpireTime = ((!dictionary.ContainsKey(PlayerInfo.Key.DailyWordExpireTime)) ? DateTime.UtcNow : DateTime.ParseExact(dictionary[PlayerInfo.Key.DailyWordExpireTime],"o",null));
            this._dailyWordPayedOutTime = ((!dictionary.ContainsKey(PlayerInfo.Key.DailyWordPayedOutTime)) ? DateTime.UtcNow : DateTime.ParseExact(dictionary[PlayerInfo.Key.DailyWordPayedOutTime], "o", null));
            this._currentCharacter = ((!dictionary.ContainsKey(PlayerInfo.Key.CurrentCharacter)) ? 0 : int.Parse(dictionary[PlayerInfo.Key.CurrentCharacter]));
            this._currentMissionSet = ((!dictionary.ContainsKey(PlayerInfo.Key.CurrentMissionSet)) ? (-1) : int.Parse(dictionary[PlayerInfo.Key.CurrentMissionSet]));
            this._amountOfMysteryBoxesOpened = ((!dictionary.ContainsKey(PlayerInfo.Key.AmountOfMysteryBoxesOpened)) ? 0 : int.Parse(dictionary[PlayerInfo.Key.AmountOfMysteryBoxesOpened]));
            this._tutorialCompleted = dictionary.ContainsKey(PlayerInfo.Key.TutorialCompleted) && bool.Parse(dictionary[PlayerInfo.Key.TutorialCompleted]);
            this._inAppPurchaseCount = ((!dictionary.ContainsKey(PlayerInfo.Key.InAppPurchaseCount)) ? 0 : int.Parse(dictionary[PlayerInfo.Key.InAppPurchaseCount]));
            this._earnCurrenyData = ((!dictionary.ContainsKey(PlayerInfo.Key.EarnCurrencyData)) ? string.Empty : dictionary[PlayerInfo.Key.EarnCurrencyData]);
            this._hasPayedOutFacebook = dictionary.ContainsKey(PlayerInfo.Key.PayBonusFacebook) && bool.Parse(dictionary[PlayerInfo.Key.PayBonusFacebook]);
            this._hasPayedOutGameCenter = dictionary.ContainsKey(PlayerInfo.Key.PayBonusGameCenter) && bool.Parse(dictionary[PlayerInfo.Key.PayBonusGameCenter]);
            this._currentMissionProgress = null;
            if (dictionary.ContainsKey(PlayerInfo.Key.CurrentMissionSetProgress))
            {
                string text = dictionary[PlayerInfo.Key.CurrentMissionSetProgress];
                if (!string.IsNullOrEmpty(text))
                {
                    this._currentMissionProgress = Array.ConvertAll<string, int>(text.Split(new char[] { ',' }), (string input) => int.Parse(input));
                }
            }
            if (dictionary.ContainsKey(PlayerInfo.Key.CollectedCharacterTokens))
            {
                string text2 = dictionary[PlayerInfo.Key.CollectedCharacterTokens];
                if (!string.IsNullOrEmpty(text2))
                {
                    int[] array2 = Array.ConvertAll<string, int>(text2.Split(new char[] { ',' }), (string input) => int.Parse(input));
                    int i = Mathf.Min(array2.Length, this._collectedCharacterTokens.Length);
                    Array.Copy(array2, this._collectedCharacterTokens, i);
                    while (i < this._collectedCharacterTokens.Length)
                    {
                        this._collectedCharacterTokens[i] = 0;
                        i++;
                    }
                }
            }
            if (dictionary.ContainsKey(PlayerInfo.Key.UnlockedTrophies))
            {
                string text3 = dictionary[PlayerInfo.Key.UnlockedTrophies];
                if (!string.IsNullOrEmpty(text3))
                {
                    bool[] array3 = Array.ConvertAll<string, bool>(text3.Split(new char[] { ',' }), (string input) => bool.Parse(input));
                    int j = Mathf.Min(array3.Length, this._unlockedTrophies.Length);
                    Array.Copy(array3, this._unlockedTrophies, j);
                    while (j < this._unlockedTrophies.Length)
                    {
                        this._unlockedTrophies[j] = false;
                        j++;
                    }
                }
            }
            if (dictionary.ContainsKey(PlayerInfo.Key.AchievementProgress))
            {
                string text4 = dictionary[PlayerInfo.Key.AchievementProgress];
                if (!string.IsNullOrEmpty(text4))
                {
                    int[] array4 = Array.ConvertAll<string, int>(text4.Split(new char[] { ',' }), (string input) => int.Parse(input));
                    int k = Mathf.Min(array4.Length, this._achievementProgress.Length);
                    Array.Copy(array4, this._achievementProgress, k);
                    while (k < this._achievementProgress.Length)
                    {
                        this._achievementProgress[k] = 0;
                        k++;
                    }
                }
            }
            Dictionary<PowerupType, int> dictionary2 = FileUtil.ReadEnumIntDictionary<PowerupType>(binaryReader);
            foreach (KeyValuePair<PowerupType, int> keyValuePair in dictionary2)
            {
                this._upgradeAmounts[keyValuePair.Key] = keyValuePair.Value;
            }
            Dictionary<PowerupType, int> dictionary3 = FileUtil.ReadEnumIntDictionary<PowerupType>(binaryReader);
            foreach (KeyValuePair<PowerupType, int> keyValuePair2 in dictionary3)
            {
                this._upgradeTiers[keyValuePair2.Key] = keyValuePair2.Value;
            }
            memoryStream.Close();
            this._dirty = false;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Error loading player info: " + ex.ToString());
            this.InitNew();
        }
    }

    public void InitNew()
    {
        this._amountOfCoins = 0;
        this._highestScore = 0;
        this._dailyWord = string.Empty;
        this._dailyWordUnlockedMask = 0;
        this._dailyWordExpireTime = DateTime.UtcNow;
        this._dailyWordPayedOutTime = DateTime.UtcNow;
        this._amountOfMysteryBoxesOpened = 0;
        this._currentCharacter = 0;
        this._currentMissionSet = -1;
        this._currentMissionProgress = null;
        this._tutorialCompleted = false;
        this._inAppPurchaseCount = 0;
        this._earnCurrenyData = string.Empty;
        this._hasPayedOutFacebook = false;
        this._hasPayedOutGameCenter = false;
        for (int i = 0; i < this._collectedCharacterTokens.Length; i++)
        {
            this._collectedCharacterTokens[i] = 0;
        }
        for (int j = 0; j < this._achievementProgress.Length; j++)
        {
            this._achievementProgress[j] = 0;
        }
        Dictionary<PowerupType, int> dictionary = new Dictionary<PowerupType, int>(this._upgradeAmounts.Count);
        foreach (PowerupType powerupType in this._upgradeAmounts.Keys)
        {
            if (powerupType == PowerupType.hoverboard)
            {
                dictionary[powerupType] = 3;
            }
            else
            {
                dictionary[powerupType] = 0;
            }
        }
        this._upgradeAmounts = dictionary;
        dictionary = new Dictionary<PowerupType, int>(this._upgradeTiers.Count);
        foreach (PowerupType powerupType2 in this._upgradeTiers.Keys)
        {
            dictionary[powerupType2] = 0;
        }
        this._upgradeTiers = dictionary;
    }

    public float doubleScoreMultiplierDuration
    {
        get
        {
            return this.GetPowerupDuration(PowerupType.doubleMultiplier);
        }
    }

    public bool doubleScore
    {
        get
        {
            return this._doubleScore;
        }
        set
        {
            if (value != this._doubleScore)
            {
                this._doubleScore = value;
                Action action = this.onScoreMultiplierChanged;
                if (action != null)
                {
                    action();
                }
            }
        }
    }

    public float GetHoverBoardCoolDown()
    {
        return 5f;
    }

    public static PlayerInfo Instance
    {
        get
        {
            PlayerInfo playerInfo;
            if ((playerInfo = PlayerInfo._instance) == null)
            {
                playerInfo = (PlayerInfo._instance = new PlayerInfo());
            }
            return playerInfo;
        }
    }

    private const string SECRET = "we12rtyuiklhgfdjerKJGHfvghyuhnjiokLJHl145rtyfghjvbn";

    private const int VERSION = 1;

    private bool _dirty;

    public Action onCoinsChanged;

    private int _amountOfCoins;

    private int _highestScore;

    private int _oldHighestScore;

    public Action onHighScoreChanged;

    private int _highestMeters;

    private int _amountOfMysteryBoxesOpened;

    private int _mysteryBoxesToUnlock;

    private int _lastMissionCompleted = -1;

    private int _currentMissionSet = -1;

    private int[] _currentMissionProgress;

    public Action onScoreMultiplierChanged;

    private int _currentCharacter;

    public Action<CharacterModels.TokenType> OnTokenCollected;

    private int[] _collectedCharacterTokens = new int[CharacterModels.tokenInfo.Count];

    public Action<Trophies.Trophy> OnTrophyUnlocked;

    private bool[] _unlockedTrophies = new bool[Enum.GetValues(typeof(Trophies.Trophy)).Length];

    private int[] _achievementProgress = new int[27];

    private bool _hasPayedOutFacebook;

    private bool _hasPayedOutGameCenter;

    private string _dailyWord = string.Empty;

    private IntMask _dailyWordUnlockedMask;

    private DateTime _dailyWordExpireTime;

    private DateTime _dailyWordPayedOutTime;

    public Action OnPickedUpLetter;

    private bool _tutorialCompleted;

    private int _inAppPurchaseCount;

    private string _earnCurrenyData = string.Empty;

    public Action onPowerupAmountChanged;

    private Dictionary<PowerupType, int> _upgradeAmounts = new Dictionary<PowerupType, int>
    {
        {
            PowerupType.hoverboard,
            0
        },
        {
            PowerupType.headstart500,
            0
        },
        {
            PowerupType.headstart2000,
            0
        },
        {
            PowerupType.mysterybox,
            0
        }
    };

    private Dictionary<PowerupType, int> _upgradeTiers = new Dictionary<PowerupType, int>
    {
        {
            PowerupType.jetpack,
            0
        },
        {
            PowerupType.supersneakers,
            0
        },
        {
            PowerupType.coinmagnet,
            0
        },
        {
            PowerupType.letters,
            0
        },
        {
            PowerupType.doubleMultiplier,
            4
        }
    };

    private bool _doubleScore;

    private static PlayerInfo _instance;

    private enum Key
    {
        AmountOfCoins,
        OldHighestScore,
        HighestScore,
        DailyWord,
        DailyWordUnlockMask,
        DailyWordExpireTime,
        DailyWordPayedOutTime,
        CurrentCharacter,
        CurrentMissionSet,
        CurrentMissionSetProgress,
        CollectedCharacterTokens,
        AmountOfMysteryBoxesOpened,
        TutorialCompleted,
        InAppPurchaseCount,
        EarnCurrencyData,
        PayBonusFacebook,
        PayBonusGameCenter,
        Count,
        UnlockedTrophies,
        AchievementProgress
    }
}
