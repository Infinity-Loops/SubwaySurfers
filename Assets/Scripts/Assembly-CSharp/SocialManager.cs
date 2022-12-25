using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class SocialManager : MonoBehaviour
{
    public FacebookProfile facebookProfile
    {
        get
        {
            return this._fbProfile;
        }
    }

    public Texture2D localUserImage
    {
        get
        {
            if (this.facebookProfile != null)
            {
                return this.facebookProfile.image;
            }
            if (Social.localUser != null)
            {
                return Social.localUser.image;
            }
            Debug.LogError("Local user not initialized");
            return null;
        }
    }

    public string localUserName
    {
        get
        {
            if (this.facebookProfile != null)
            {
                return this.facebookProfile.name;
            }
            if (Social.localUser != null)
            {
                return Social.localUser.userName;
            }
            Debug.LogError("Local user not initialized");
            return null;
        }
    }

    public Friend[] FriendsSortedByScore()
    {
        if (this._friends != null)
        {
            Friend[] array = this._friends.ToArray();
            Array.Sort<Friend>(array, (Friend x, Friend y) => y.score - x.score);
            return array;
        }
        return new Friend[0];
    }

    public Friend[] FriendsSortedByCash()
    {
        if (this._friends != null)
        {
            Debug.Log("Friends was NOT null");
            Friend[] array = this._friends.ToArray();
            Array.Sort<Friend>(array, (Friend x, Friend y) => y.gamesToCashIn - x.gamesToCashIn);
            return array;
        }
        Debug.Log("Friends was null");
        return new Friend[0];
    }
    public void setAchievement(string achievementId, float percentCompleted)
    {
        /*
        if (!Social.localUser.authenticated)
        {
            Debug.Log("Not logged into gamecenter and trying to set Achievement. Not sure if unity has implemented a buffer for achievents. Otherwise the acchievement is not saved");
        }
        try
        {
            percentCompleted = Mathf.Clamp(percentCompleted, 0f, 100f);
            IAchievement achievement = Social.CreateAchievement();
            achievement.id = achievementId;
            achievement.percentCompleted = (double)percentCompleted;
            achievement.ReportProgress(delegate (bool result)
            {
                if (result)
                {
                    Debug.Log("Successfully reported progress");
                }
                else
                {
                    Debug.Log("Failed to report progress");
                }
            });
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Error while setting Achievement: " + ex.Message);
        }
        */
    }
    public static SocialManager instance
    {
        get
        {
            if (SocialManager._instance == null)
            {
                GameObject gameObject = new GameObject("Social Manager");
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
                SocialManager._instance = gameObject.AddComponent<SocialManager>();
            }
            return SocialManager._instance;
        }
    }

    private void InitPushNotifications()
    {
    }

    private void InitGameCenter()
    {
        this._gameCenterAuthenticationComplete = false;
        Social.localUser.Authenticate(delegate (bool authenticated)
        {
            if (authenticated)
            {
                Social.localUser.LoadFriends(delegate (bool friendsLoaded)
                {
                    if (friendsLoaded)
                    {
                        IUserProfile[] friends = Social.localUser.friends;
                        this._gcFriends = new Dictionary<string, IUserProfile>(friends.Length);
                        foreach (IUserProfile userProfile in friends)
                        {
                            this._gcFriends[userProfile.id] = userProfile;
                        }
                    }
                    else
                    {
                        this._gcFriends = null;
                    }
                    this._gameCenterFriendListRequestComplete = true;
                    this.Invalidate();
                });
            }
            else if (this._friends != null)
            {
                this._friends.RemoveAll((Friend item) => item.gcProfile != null && item.fbProfile == null);
                this._friends.ForEach(delegate (Friend item)
                {
                    item.gcProfile = null;
                });
            }
            this._gameCenterAuthenticationComplete = true;
        });
    }

    public void FacebookLogin(Action<bool> onComplete)
    {
        base.StartCoroutine(this.FacebookLoginCoroutine(onComplete));
    }

    public void FacebookLogout()
    {
        
    }

    public bool facebookIsLoggedIn
    {
        get
        {
            return false;
        }
    }

    private IEnumerator FacebookLoginCoroutine(Action<bool> onComplete)
    {
        this._fbCurrentRequest = SocialManager.FacebookCurrentRequest.GettingUserInfo;
        Hashtable args = new Hashtable();
        args["fields"] = "id,name,first_name";
        while (this._fbCurrentRequest != SocialManager.FacebookCurrentRequest.None)
        {
            if (this._fbCurrentRequest == SocialManager.FacebookCurrentRequest.Error)
            {
                if (onComplete != null)
                {
                    onComplete(false);
                }
                yield break;
            }
            yield return null;
        }
        base.StartCoroutine(this.DownloadFacebookPicture(this._fbProfile));
        this._fbCurrentRequest = SocialManager.FacebookCurrentRequest.GettingFriends;
        Hashtable args2 = new Hashtable();
        args2["fields"] = "id,name,first_name";
        while (this._fbCurrentRequest != SocialManager.FacebookCurrentRequest.None)
        {
            if (this._fbCurrentRequest == SocialManager.FacebookCurrentRequest.Error)
            {
                if (onComplete != null)
                {
                    onComplete(false);
                }
                yield break;
            }
            yield return null;
        }
        if (SocialManager.debugGUI)
        {
            this._debugFacebookFriends = new Dictionary<string, FacebookProfile>(this._fbFriends.Count);
            foreach (Hashtable entry in this._fbFriends.Values)
            {
                FacebookProfile p = new FacebookProfile();
                p.id = (string)entry["id"];
                p.name = (string)entry["first_name"];
                p.fullName = (string)entry["name"];
                this._debugFacebookFriends[p.id] = p;
            }
            base.StartCoroutine(this.DownloadFacebookPictures(this._debugFacebookFriends));
        }
        this._fbReady = true;
        this.Invalidate();
        if (onComplete != null)
        {
            onComplete(true);
        }
        yield break;
    }

    private void TwitterLogin(Action<bool> onComplete)
    {
        base.StartCoroutine(this.TwitterLoginCoroutine(onComplete));
    }

    public void TwitterLogout()
    {
     
    }

    private IEnumerator TwitterLoginCoroutine(Action<bool> onComplete)
    {
        this._twitterReady = true;
        if (onComplete != null)
        {
            onComplete(true);
        }
        yield break;
    }

    private void Invalidate()
    {
        if (this._gameCenterAuthenticationComplete && (!Social.localUser.authenticated || this._gameCenterFriendListRequestComplete) && (!this.facebookIsLoggedIn || this._fbReady))
        {
            this._doneLoggingIn = true;
            this.RegisterUser(delegate (bool success)
            {
                Debug.Log((!success) ? "Register user failed" : "Register user succeeded");
            });
            this.ConsolidateFriends(delegate (bool success)
            {
                Debug.Log((!success) ? "Consolidate friends failed" : "Consolidate friends succeeded");
                this._consolidatedFriendsCompleted = true;
            });
        }
    }

    public bool doneLoggingIn
    {
        get
        {
            return this._doneLoggingIn;
        }
    }

    public bool consolidatedFriendsCompleted
    {
        get
        {
            return this._consolidatedFriendsCompleted;
        }
    }

    public bool dirty
    {
        get
        {
            return this._dirty;
        }
    }

    public void CollectFriendReward(Friend friend)
    {
        friend.status.gamesCashedIn = friend.games;
        this._dirty = true;
    }

    public int CashIn(Friend friend, int max)
    {
        int num = friend.games - friend.status.gamesCashedIn;
        if (num > 0)
        {
            friend.status.gamesCashedIn = friend.games;
            this._dirty = true;
            return Mathf.Max(num, max);
        }
        return 0;
    }

    public int CashInAll(int maxPerFriend)
    {
        int num = 0;
        foreach (Friend friend in this._friends)
        {
            num += this.CashIn(friend, maxPerFriend);
        }
        return num;
    }

    public void WriteTo(Stream stream)
    {
        BinaryWriter binaryWriter = new BinaryWriter(stream);
        binaryWriter.Write(1);
        if (this._friendStatus != null)
        {
            binaryWriter.Write(this._friendStatus.Count);
            foreach (KeyValuePair<string, Friend.Status> keyValuePair in this._friendStatus)
            {
                binaryWriter.Write(keyValuePair.Key);
                binaryWriter.Write(keyValuePair.Value.gamesCashedIn);
                binaryWriter.Write(keyValuePair.Value.lastPokeTime.ToBinary());
            }
        }
        else
        {
            binaryWriter.Write(0);
        }
    }

    public void ReadFrom(Stream stream)
    {
        BinaryReader binaryReader = new BinaryReader(stream);
        byte b = binaryReader.ReadByte();
        if (b == 1)
        {
            int num = binaryReader.ReadInt32();
            this._friendStatus = new Dictionary<string, Friend.Status>(num);
            for (int i = 0; i < num; i++)
            {
                string text = binaryReader.ReadString();
                if (!string.IsNullOrEmpty(text))
                {
                    Friend.Status status = new Friend.Status();
                    status.gamesCashedIn = binaryReader.ReadInt32();
                    status.lastPokeTime = DateTime.FromBinary(binaryReader.ReadInt64());
                    this._friendStatus[text] = status;
                }
            }
            return;
        }
        throw new IOException("Unsupported playerdata file version");
    }

    private static string GetSaveDataPath()
    {
        return Application.persistentDataPath + "/socialdata";
    }

    private static bool ArraysAreEqual<T>(T[] a, T[] b)
    {
        if (a == null && b == null)
        {
            return true;
        }
        if (a.Length != b.Length)
        {
            return false;
        }
        for (int i = 0; i < a.Length; i++)
        {
            if (!object.Equals(a[i], b[i]))
            {
                return false;
            }
        }
        return true;
    }

    public void Load()
    {
        try
        {
            string saveDataPath = SocialManager.GetSaveDataPath();
            byte[] array = FileUtil.Load(SocialManager.GetSaveDataPath(), "resxrctrv7tgv7gb8h9h9u0909kllfmolkjnhghgjjkhjghg");
            MemoryStream memoryStream = new MemoryStream(array);
            this.ReadFrom(memoryStream);
            memoryStream.Close();
            this._dirty = false;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Could not load data: " + ex.Message);
        }
    }

    public bool Save()
    {
        try
        {
            MemoryStream memoryStream = new MemoryStream(8192);
            this.WriteTo(memoryStream);
            byte[] buffer = memoryStream.GetBuffer();
            FileUtil.Save(SocialManager.GetSaveDataPath(), "resxrctrv7tgv7gb8h9h9u0909kllfmolkjnhghgjjkhjghg", buffer, 0, (int)memoryStream.Length);
            memoryStream.Close();
            this._dirty = false;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(string.Concat(new string[]
            {
                "Error saving social data: ",
                ex.GetType().Name,
                ": ",
                ex.Message,
                "\n",
                ex.StackTrace
            }));
        }
        return false;
    }

    private void Awake()
    {
        this.Load();
        this.InitPushNotifications();
        if (this.facebookIsLoggedIn)
        {
            this.FacebookLogin(null);
        }
        this.InitGameCenter();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            this.Save();
        }
        else
        {
            if (this.facebookIsLoggedIn)
            {
                this.FacebookLogin(null);
            }
            this.InitGameCenter();
        }
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    private void twitterLogin()
    {
        Debug.Log("Successfully logged in to Twitter");
        if (this._twitterCurrentRequest == SocialManager.TwitterCurrentRequest.LoggingIn)
        {
            this._twitterCurrentRequest = SocialManager.TwitterCurrentRequest.None;
        }
        else
        {
            Debug.LogWarning("Received twitter login message, but we are not in that state");
        }
    }

    private void twitterLoginFailed(string error)
    {
        Debug.Log("Twitter login failed: " + error);
        if (this._twitterCurrentRequest == SocialManager.TwitterCurrentRequest.LoggingIn)
        {
            this._twitterCurrentRequest = SocialManager.TwitterCurrentRequest.Error;
        }
        else
        {
            Debug.LogWarning("Received twitter login failed message, but we are not in that state");
        }
    }

    private void twitterPost()
    {
        Debug.Log("Successfully posted to Twitter");
    }

    private void twitterPostFailed(string error)
    {
        Debug.Log("Twitter post failed: " + error);
    }

    private void twitterHomeTimelineFailed(string error)
    {
        Debug.Log("Twitter HomeTimeline failed: " + error);
    }

    private void twitterHomeTimelineReceived(ArrayList result)
    {
        Debug.Log("received home timeline with tweet count: " + result.Count);
    }

    private void twitterRequestDidFailEvent(string error)
    {
        Debug.Log("twitterRequestDidFailEvent: " + error);
    }

    private void twitterRequestDidFinishEvent(object result)
    {
        if (result != null)
        {
            Debug.Log("twitterRequestDidFinishEvent: " + result.GetType().ToString());
        }
        else
        {
            Debug.Log("twitterRequestDidFinishEvent with no data");
        }
    }

    private void facebookLogin()
    {
        Debug.Log("Successfully logged in to Facebook");
        if (this._fbCurrentRequest == SocialManager.FacebookCurrentRequest.LoggingIn)
        {
            this._fbCurrentRequest = SocialManager.FacebookCurrentRequest.None;
        }
        else
        {
            Debug.LogWarning("Received facebook login message, but we are not in that state");
        }
    }

    private void facebookLoginFailed(string error)
    {
        Debug.Log("Facebook login failed: " + error);
        if (this._fbCurrentRequest == SocialManager.FacebookCurrentRequest.LoggingIn)
        {
            this._fbCurrentRequest = SocialManager.FacebookCurrentRequest.Error;
        }
        else
        {
            Debug.LogWarning("Received facebook login failed message, but we are not in that state");
        }
    }

    private void facebookDidLogoutEvent()
    {
        Debug.Log("facebookDidLogoutEvent");
        if (this._friends != null)
        {
            this._friends.RemoveAll((Friend item) => item.gcProfile == null && item.fbProfile != null);
            this._friends.ForEach(delegate (Friend item)
            {
                item.fbProfile = null;
            });
        }
        this._fbFriends = null;
    }

    private void facebookDidExtendTokenEvent(DateTime newExpiry)
    {
        Debug.Log("facebookDidExtendTokenEvent: " + newExpiry);
    }

    private void facebookSessionInvalidatedEvent()
    {
        Debug.Log("facebookSessionInvalidatedEvent");
    }

    private void facebookReceivedUsername(string username)
    {
        Debug.Log("Facebook logged in users name: " + username);
    }

    private void facebookUsernameRequestFailed(string error)
    {
        Debug.Log("Facebook failed to receive username: " + error);
    }

    private void facebookPost()
    {
        Debug.Log("Successfully posted to Facebook");
    }

    private void facebookPostFailed(string error)
    {
        Debug.Log("Facebook post failed: " + error);
    }

    private void facebookReceivedFriends(ArrayList result)
    {
        Debug.Log("received total friends: " + result.Count);
        if (this._fbCurrentRequest == SocialManager.FacebookCurrentRequest.GettingFriends)
        {
            this._fbCurrentRequest = SocialManager.FacebookCurrentRequest.None;
        }
        this._fbFriends = new Dictionary<string, Hashtable>(result.Count);
        foreach (object obj in result)
        {
            Hashtable hashtable = (Hashtable)obj;
            if (hashtable.ContainsKey("id"))
            {
                this._fbFriends[(string)hashtable["id"]] = hashtable;
            }
            else
            {
                Debug.LogError("Unexpected format of FaceBook Friend");
            }
        }
    }

    private void facebookFriendRequestFailed(string error)
    {
        Debug.Log("FacebookFriendRequestFailed: " + error);
        this._fbFriends = null;
        if (this._fbCurrentRequest == SocialManager.FacebookCurrentRequest.GettingFriends)
        {
            this._fbCurrentRequest = SocialManager.FacebookCurrentRequest.Error;
        }
    }

    private void facebokDialogCompleted()
    {
        Debug.Log("facebokDialogCompleted");
    }

    private void facebookDialogCompletedWithUrl(string url)
    {
        Debug.Log("facebookDialogCompletedWithUrl: " + url);
    }

    private void facebookDialogDidntComplete()
    {
        Debug.Log("facebookDialogDidntComplete");
    }

    private void facebookDialogFailed(string error)
    {
        Debug.Log("facebookDialogFailed: " + error);
    }

    private void facebookReceivedCustomRequest(object obj)
    {
        Debug.Log("facebookReceivedCustomRequest");
        if (this._fbCurrentRequest == SocialManager.FacebookCurrentRequest.GettingUserInfo)
        {
            this._fbProfile = new FacebookProfile();
            Hashtable hashtable = (Hashtable)obj;
            this._fbProfile.id = (string)hashtable["id"];
            this._fbProfile.name = (string)hashtable["first_name"];
            this._fbProfile.fullName = (string)hashtable["name"];
            this._fbCurrentRequest = SocialManager.FacebookCurrentRequest.None;
        }
    }
    private Action<FacebookProfile> _facebookPictureDownloadedHandler;

    private Action _friendsConsolidatedHandler;
    public void AddFacebookPictureDownloadedHandler(Action<FacebookProfile> handler)
    {
        this._facebookPictureDownloadedHandler = (Action<FacebookProfile>)Delegate.Combine(this._facebookPictureDownloadedHandler, handler);
    }

    public void RemoveFacebookPictureDownloadedHandler(Action<FacebookProfile> handler)
    {
        this._facebookPictureDownloadedHandler = (Action<FacebookProfile>)Delegate.Remove(this._facebookPictureDownloadedHandler, handler);
    }

    public void AddFriendsConsolidatedHandler(Action handler)
    {
        this._friendsConsolidatedHandler = (Action)Delegate.Combine(this._friendsConsolidatedHandler, handler);
    }

    public void RemoveFriendsConsolidatedHandler(Action handler)
    {
        this._friendsConsolidatedHandler = (Action)Delegate.Remove(this._friendsConsolidatedHandler, handler);
    }
    private void facebookCustomRequestFailed(string error)
    {
        Debug.Log("facebookCustomRequestFailed failed: " + error);
        if (this._fbCurrentRequest == SocialManager.FacebookCurrentRequest.GettingUserInfo)
        {
            this._fbCurrentRequest = SocialManager.FacebookCurrentRequest.Error;
        }
    }

    private static string GetRandomIdentifier()
    {
        string text = ((!Application.isEditor) ? SystemInfo.deviceUniqueIdentifier : "0000000000000000000000000000000000000000");
        return text + UnityEngine.Random.Range(0, int.MaxValue).ToString();
    }

    public static string GetChecksum(string data)
    {
        return SocialManager.GetSHA1Hash(data + "resxrctrv7tgv7gb8h9h9u0909kllfmolkjnhghgjjkhjghg");
    }

    private static string GetChecksum(params string[] data)
    {
        return SocialManager.GetChecksum(string.Join(null, data));
    }

    private static string GetSHA1Hash(string unhashed)
    {
        SHA1 sha = SHA1.Create();
        byte[] array = sha.ComputeHash(Encoding.Default.GetBytes(unhashed));
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < array.Length; i++)
        {
            stringBuilder.Append(array[i].ToString("x2"));
        }
        return stringBuilder.ToString();
    }

    private static IEnumerator WWWRequestCoroutine(SocialManager.WWWComplete onWWWComplete, string relativeUrl, object cookie, params string[] postItems)
    {
        string url = "http://hoodrunner.kiloo.com" + relativeUrl;
        string identifier = SocialManager.GetRandomIdentifier();
        StringBuilder checksumSB = new StringBuilder();
        for (int i = 1; i < postItems.Length; i += 2)
        {
            checksumSB.Append(postItems[i]);
        }
        string checksum = SocialManager.GetChecksum(identifier + checksumSB.ToString());
        WWWForm postData = new WWWForm();
        postData.AddField("identifier", identifier);
        postData.AddField("checksum", checksum);
        StringBuilder sb = new StringBuilder();
        sb.Append("WWWRequest(").Append(url).Append(")\n");
        for (int j = 0; j < postItems.Length; j += 2)
        {
            sb.Append("Adding post data: \"").Append(postItems[j]).Append("\" = \"")
                .Append(postItems[j + 1])
                .Append("\"\n");
            postData.AddField(postItems[j], postItems[j + 1]);
        }
        WWW www = new WWW(url, postData);
        yield return www;
        if (www.text != null)
        {
            sb.Append("Text: \"").Append(www.text).Append("\"\n");
        }
        if (www.error != null)
        {
            sb.Append("Error: \"").Append(www.error).Append("\"\n");
        }
        Debug.Log(sb.ToString());
        if (onWWWComplete != null)
        {
            if (www.error != null)
            {
                onWWWComplete(SocialManager.WWWRequestResult.Error, null, cookie);
            }
            else
            {
                string result = null;
                int resultStart = www.text.IndexOf("<result>", StringComparison.OrdinalIgnoreCase);
                if (resultStart >= 0)
                {
                    resultStart += 8;
                    int resultEnd = www.text.IndexOf("</result>", resultStart, StringComparison.OrdinalIgnoreCase);
                    if (resultEnd > resultStart)
                    {
                        result = www.text.Substring(resultStart, resultEnd - resultStart);
                    }
                    else if (resultEnd == resultStart)
                    {
                        result = string.Empty;
                    }
                }
                onWWWComplete((result != null) ? SocialManager.WWWRequestResult.Success : SocialManager.WWWRequestResult.Error, result, cookie);
            }
        }
        yield break;
    }

    private static string ByteArrayToHex(byte[] barray)
    {
        char[] array = new char[barray.Length * 2];
        for (int i = 0; i < barray.Length; i++)
        {
            byte b = (byte)(barray[i] >> 4);
            array[i * 2] = (char)((b <= 9) ? (b + 48) : (b + 55));
            b = (byte)(barray[i] & 15);
            array[i * 2 + 1] = (char)((b <= 9) ? (b + 48) : (b + 55));
        }
        return new string(array);
    }

    private static string GetBundleVersion()
    {
        return DeviceUtility.GetBundleVersion();
    }

    private void RegisterUser(Action<bool> registerUserCompleted)
    {
    }

    private void WWWRegisterUserCompleted(SocialManager.WWWRequestResult result, string output, object cookie)
    {
        bool flag = false;
        if (result == SocialManager.WWWRequestResult.Success)
        {
            Dictionary<string, string> dictionary = StringUtility.ParseProperties(output);
            if (dictionary.ContainsKey("userid"))
            {
                string text = dictionary["userid"];
                string text2 = dictionary["score"];
                string text3 = dictionary["meters"];
                string text4 = dictionary["games"];
                string text5 = dictionary["checksum"];
                string checksum = SocialManager.GetChecksum(new string[] { text, text2, text3, text4 });
                if (string.Compare(text5, checksum, true) == 0)
                {
                    try
                    {
                        int num = int.Parse(text);
                        int num2 = int.Parse(text2);
                        int num3 = int.Parse(text3);
                        this._userid = num;
                        PlayerInfo.Instance.highestScore = num2;
                        PlayerInfo.Instance.highestMeters = num3;
                        flag = true;
                    }
                    catch (Exception)
                    {
                        Debug.LogError("Error parsing output data from register user");
                    }
                }
                else
                {
                    Debug.LogError("Output data from register user corrupted or tampered with");
                }
            }
        }
        if (cookie != null)
        {
            ((Action<bool>)cookie)(flag);
        }
    }

    private void ConsolidateFriends(Action<bool> consolidateFriendsCompleted)
    {
        string text;
        if (this._fbFriends != null)
        {
            string[] array = new string[this._fbFriends.Count];
            this._fbFriends.Keys.CopyTo(array, 0);
            text = string.Join(";", array);
        }
        else
        {
            text = string.Empty;
        }
        string text2;
        if (this._gcFriends != null)
        {
            string[] array2 = new string[this._gcFriends.Count];
            this._gcFriends.Keys.CopyTo(array2, 0);
            text2 = string.Join(";", array2);
        }
        else
        {
            text2 = string.Empty;
        }
        if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(text2))
        {
            consolidateFriendsCompleted(true);
        }
        else
        {
            base.StartCoroutine(SocialManager.WWWRequestCoroutine(new SocialManager.WWWComplete(this.WWWConsolidateFriendsCompleted), "/friends.php", consolidateFriendsCompleted, new string[] { "fblist", text, "gclist", text2 }));
        }
    }

    private static string[][] ParseSets(string setsString)
    {
        string[] array = new string[] { ");(" };
        string[] array2 = setsString.Split(array, StringSplitOptions.RemoveEmptyEntries);
        if (array2.Length > 0)
        {
            if (array2[0][0] == '(')
            {
                array2[0] = array2[0].Substring(1);
            }
            int num = array2.Length - 1;
            int num2 = array2[num].Length - 1;
            if (array2[num][num2] == ')')
            {
                array2[num] = array2[num].Remove(num2);
            }
            string[][] array3 = new string[array2.Length][];
            for (int i = 0; i < array2.Length; i++)
            {
                array3[i] = array2[i].Split(new char[] { ';' });
            }
            return array3;
        }
        return new string[0][];
    }

    private void WWWConsolidateFriendsCompleted(SocialManager.WWWRequestResult result, string output, object cookie)
    {
        bool flag = false;
        if (result == SocialManager.WWWRequestResult.Success)
        {
            Dictionary<string, string> dictionary = StringUtility.ParseProperties(output);
            Debug.Log("Parse properties");
            if (dictionary.ContainsKey("friendslist"))
            {
                Debug.Log("props contain friendslist");
                string text = dictionary["friendslist"];
                string text2 = dictionary["checksum"];
                string checksum = SocialManager.GetChecksum(text);
                if (string.Compare(text2, checksum, true) == 0)
                {
                    Debug.Log("Checksum fits");
                    if (string.IsNullOrEmpty(text))
                    {
                        this._friends = null;
                        Debug.Log("no friends");
                    }
                    else
                    {
                        Debug.Log("Friends exist");
                        string[][] array = SocialManager.ParseSets(text);
                        this._friends = new List<Friend>(array.Length);
                        string[][] array2 = array;
                        int i = 0;
                        while (i < array2.Length)
                        {
                            string[] array3 = array2[i];
                            Debug.Log("foreach friend");
                            if (array3.Length != 6)
                            {
                                goto IL_391;
                            }
                            if (array3[1].Length <= 0)
                            {
                                if (array3[2].Length <= 0)
                                {
                                    goto IL_391;
                                }
                            }
                            try
                            {
                                Debug.Log("Trying to create friend");
                                Friend friend = new Friend();
                                friend.userid = int.Parse(array3[0]);
                                string text3 = array3[1];
                                if (text3.Length > 0)
                                {
                                    Debug.Log("gcid length > 0: " + text3);
                                    friend.gcProfile = this._gcFriends[text3];
                                }
                                string text4 = array3[2];
                                if (text4.Length > 0)
                                {
                                    Debug.Log("fbid > 0");
                                    if (this._fbProfiles == null)
                                    {
                                        this._fbProfiles = new Dictionary<string, FacebookProfile>();
                                    }
                                    FacebookProfile facebookProfile;
                                    if (this._fbProfiles.ContainsKey(text4))
                                    {
                                        facebookProfile = this._fbProfiles[text4];
                                    }
                                    else
                                    {
                                        Hashtable hashtable = this._fbFriends[text4];
                                        facebookProfile = new FacebookProfile();
                                        facebookProfile.id = text4;
                                        facebookProfile.name = (string)hashtable["first_name"];
                                        facebookProfile.fullName = (string)hashtable["name"];
                                        this._fbProfiles[text4] = facebookProfile;
                                    }
                                    friend.fbProfile = facebookProfile;
                                }
                                friend.score = int.Parse(array3[3]);
                                friend.meters = int.Parse(array3[4]);
                                friend.games = int.Parse(array3[5]);
                                if (this._friendStatus == null)
                                {
                                    this._friendStatus = new Dictionary<string, Friend.Status>();
                                }
                                Friend.Status status;
                                if (friend.fbProfile != null && this._friendStatus.ContainsKey(friend.fbProfile.id))
                                {
                                    Debug.Log("found fb");
                                    status = this._friendStatus[friend.fbProfile.id];
                                }
                                else if (friend.gcProfile != null && this._friendStatus.ContainsKey(friend.gcProfile.id))
                                {
                                    Debug.Log("found gc");
                                    status = this._friendStatus[friend.gcProfile.id];
                                }
                                else
                                {
                                    Debug.Log("creating new");
                                    status = new Friend.Status();
                                    status.gamesCashedIn = friend.games;
                                    string text5 = ((friend.fbProfile == null) ? friend.gcProfile.id : friend.fbProfile.id);
                                    this._friendStatus[text5] = status;
                                    this._dirty = true;
                                }
                                friend.status = status;
                                this._friends.Add(friend);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError("Friend parse error " + ex.ToString());
                            }
                        IL_3B1:
                            i++;
                            continue;
                        IL_391:
                            Debug.LogError("Malformed friend: (" + string.Join(";", array3) + ")");
                            goto IL_3B1;
                        }
                        if (this._fbProfiles != null)
                        {
                            base.StartCoroutine(this.DownloadFacebookPictures(this._fbProfiles));
                        }
                    }
                    Debug.Log("success");
                    flag = true;
                }
                else
                {
                    Debug.LogError("Consolidated friend data corrupted");
                }
            }
        }
        if (cookie != null)
        {
            ((Action<bool>)cookie)(flag);
        }
    }

    public void ReportScore(int newScore, int newMeters)
    {
        if (this._userid > 0)
        {
            base.StartCoroutine(SocialManager.WWWRequestCoroutine(null, "/report.php", null, new string[]
            {
                "userid",
                this._userid.ToString(),
                "score",
                newScore.ToString(),
                "meters",
                newMeters.ToString()
            }));
            if (Social.localUser.authenticated)
            {
                Social.ReportScore((long)newScore, "com.kiloo.subwaysurfers.ScoreLeaderboard", new Action<bool>(this.GameCenterCallBack));
            }
            else
            {
                Debug.Log("Game Center localuser was not authenticated");
            }
        }
    }

    private void GameCenterCallBack(bool success)
    {
        Debug.Log("Game center score report " + ((!success) ? "failure" : "success"));
    }

    public void UpdateFriendScores(Action<bool> updateFriendsScoresCompleted)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (Friend friend in this._friends)
        {
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Append(';');
            }
            stringBuilder.Append(friend.userid);
        }
        string text = stringBuilder.ToString();
        base.StartCoroutine(SocialManager.WWWRequestCoroutine(new SocialManager.WWWComplete(this.WWWUpdateFriendScoresCompleted), "/scores.php", updateFriendsScoresCompleted, new string[] { "idlist", text }));
    }

    private void WWWUpdateFriendScoresCompleted(SocialManager.WWWRequestResult result, string output, object cookie)
    {
        bool flag = false;
        if (result == SocialManager.WWWRequestResult.Success)
        {
            Dictionary<string, string> dictionary = StringUtility.ParseProperties(output);
            if (dictionary.ContainsKey("scores"))
            {
                string text = dictionary["scores"];
                string text2 = dictionary["checksum"];
                string checksum = SocialManager.GetChecksum(text);
                if (string.Compare(text2, checksum, true) == 0)
                {
                    try
                    {
                        string[][] array = SocialManager.ParseSets(text);
                        string[][] array2 = array;
                        for (int i = 0; i < array2.Length; i++)
                        {
                            string[] array3 = array2[i];
                            if (array3.Length != 4)
                            {
                                Debug.LogError("UpdateFriendScores: Malformed score (" + string.Join(";", array3) + ")");
                                throw new Exception();
                            }
                            int userid = int.Parse(array3[0]);
                            Friend friend = this._friends.Find((Friend f) => f.userid == userid);
                            if (friend != null)
                            {
                                int num = int.Parse(array3[1]);
                                int num2 = int.Parse(array3[2]);
                                int num3 = int.Parse(array3[3]);
                                friend.score = num;
                                friend.meters = num2;
                                friend.games = num3;
                            }
                            else
                            {
                                Debug.LogWarning("UpdateFriendScores: Unexpected friend user id");
                            }
                        }
                        flag = true;
                    }
                    catch (Exception)
                    {
                        Debug.LogError("UpdateFriendScores: Error parsing output data");
                    }
                }
                else
                {
                    Debug.LogError("UpdateFriendScores: Output data corrupt");
                }
            }
        }
        if (cookie != null)
        {
            ((Action<bool>)cookie)(flag);
        }
    }

    public void Poke(Friend friend)
    {
        string text = ((friend.fbProfile == null) ? ((!Social.localUser.authenticated) ? string.Empty : Social.localUser.userName) : this._fbProfile.fullName);
        base.StartCoroutine(SocialManager.WWWRequestCoroutine(null, "/poke.php", null, new string[]
        {
            "friend",
            friend.userid.ToString(),
            "name",
            text
        }));
        friend.status.lastPokeTime = DateTime.UtcNow;
        this._dirty = true;
    }

    public void SetPokeFirstTime(Friend friend)
    {
        friend.status.lastPokeTime = DateTime.UtcNow;
        this._dirty = true;
    }

    public void BragNotify(int oldScore, List<Friend> friends)
    {
        if (friends != null)
        {
            int count = friends.Count;
            StringBuilder stringBuilder = new StringBuilder(count * 8);
            StringBuilder stringBuilder2 = new StringBuilder(count * 2);
            foreach (Friend friend in friends)
            {
                int relation = friend.relation;
                int userid = friend.userid;
                if (relation != 0 && userid != 0)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append(';');
                        stringBuilder2.Append(';');
                    }
                    stringBuilder.Append(userid);
                    stringBuilder2.Append(relation);
                }
            }
            if (stringBuilder.Length > 0)
            {
                base.StartCoroutine(SocialManager.WWWRequestCoroutine(null, "/brag.php", null, new string[]
                {
                    "oldscore",
                    oldScore.ToString(),
                    "newscore",
                    PlayerInfo.Instance.highestScore.ToString(),
                    "useridlist",
                    stringBuilder.ToString(),
                    "relationlist",
                    stringBuilder2.ToString(),
                    "fbname",
                    (this._fbProfile == null) ? string.Empty : this._fbProfile.name,
                    "gcname",
                    (!Social.localUser.authenticated) ? string.Empty : Social.localUser.userName
                }));
            }
        }
    }

    private static string GetDeviceTypeString()
    {
        return "iDevice";
    }

    public void RecommendAppFacebook()
    {
        if (this.facebookIsLoggedIn)
        {
        }
        else
        {
            Debug.LogError("Not logged in to facebook");
        }
    }

    public void BragFacebook(List<Friend> friends)
    {
        if (this.facebookIsLoggedIn)
        {
            List<Friend> list = null;
            if (friends != null)
            {
                list = new List<Friend>(friends.Count);
                foreach (Friend friend in friends)
                {
                    if (friend.fbProfile != null && friend.score < PlayerInfo.Instance.highestScore)
                    {
                        list.Add(friend);
                    }
                }
                list.Sort((Friend x, Friend y) => y.score - x.score);
            }
            string text;
            if (list == null || list.Count == 0)
            {
                text = string.Concat(new object[]
                {
                    "I just scored ",
                    PlayerInfo.Instance.highestScore,
                    " points dodging trains in Subway Surfers on my ",
                    SocialManager.GetDeviceTypeString(),
                    ". Check it out!"
                });
            }
            else if (list.Count == 1)
            {
                text = string.Concat(new object[]
                {
                    "I just scored ",
                    PlayerInfo.Instance.highestScore,
                    " points in Subway Surfers on my ",
                    SocialManager.GetDeviceTypeString(),
                    " and beat ",
                    list[0].fbProfile.fullName
                });
            }
            else if (list.Count == 2)
            {
                text = string.Concat(new object[]
                {
                    "I just scored ",
                    PlayerInfo.Instance.highestScore,
                    " points in Subway Surfers on my ",
                    SocialManager.GetDeviceTypeString(),
                    " and beat ",
                    list[0].fbProfile.fullName,
                    " and ",
                    list[1].fbProfile.fullName
                });
            }
            else if (list.Count == 3)
            {
                text = string.Concat(new object[]
                {
                    "I just scored ",
                    PlayerInfo.Instance.highestScore,
                    " points in Subway Surfers on my ",
                    SocialManager.GetDeviceTypeString(),
                    " and beat ",
                    list[0].fbProfile.fullName,
                    ", ",
                    list[1].fbProfile.fullName,
                    " and ",
                    list[2].fbProfile.fullName
                });
            }
            else
            {
                text = string.Concat(new object[]
                {
                    "I just scored ",
                    PlayerInfo.Instance.highestScore,
                    " points in Subway Surfers on my ",
                    SocialManager.GetDeviceTypeString(),
                    " and beat ",
                    list[0].fbProfile.fullName,
                    ", ",
                    list[1].fbProfile.fullName,
                    " and ",
                    list.Count - 2,
                    " others"
                });
            }
        }
        else
        {
            Debug.LogError("Not logged in to facebook");
        }
    }

    private IEnumerator DownloadFacebookPicture(FacebookProfile profile)
    {
        if (profile == null)
        {
            Debug.LogError("facebook profile was null in DownloadFacebookPictures!");
            yield break;
        }
        string url = "http://graph.facebook.com/" + profile.id + "/picture?type=square";
        Debug.Log(string.Concat(new string[] { "www getting facebook image for ", profile.name, " at \"", url, "\"" }));
        WWW www = new WWW(url);
        yield return www;
        if (www.error != null)
        {
            Debug.LogWarning("www failed getting image for " + profile.name + ": " + www.error);
        }
        Texture2D image = www.texture;
        if (image == null || (image.width == 8 && image.height == 8))
        {
            Debug.LogWarning("www done but no image for " + profile.name);
        }
        else
        {
            profile.image = image;
            Debug.Log(string.Concat(new object[]
            {
                "www done, got image for ",
                profile.name,
                " (width=",
                profile.image.width,
                ", height=",
                profile.image.height,
                ")"
            }));
        }
        yield break;
    }

    private IEnumerator DownloadFacebookPictures(Dictionary<string, FacebookProfile> fbProfiles)
    {
        List<FacebookProfile> profiles = new List<FacebookProfile>(fbProfiles.Count);
        foreach (FacebookProfile profile in fbProfiles.Values)
        {
            if (profile.image == null)
            {
                profiles.Add(profile);
            }
        }
        foreach (FacebookProfile profile2 in profiles)
        {
            yield return base.StartCoroutine(this.DownloadFacebookPicture(profile2));
        }
        yield break;
    }

    private const byte VERSION = 1;

    private const string FACEBOOK_APPID = "254616967963463";

    private const string TWITTER_CONSUMER_KEY = "VKV2NMbj7YIEGblD97ZFSw";

    private const string TWITTER_CONSUMER_SECRET = "z1Wy3GXYL4XS9z9a2YbE4KWF3T0ynAFBwwwxZSYDI";

    private const bool DEBUG_SET_DEBUG_POST_FIELD = false;

    private const string BASE_URL = "http://hoodrunner.kiloo.com";

    private const string REGISTER_DEVICE_URL = "/register.php";

    private const string REPORT_SCORE_URL = "/report.php";

    private const string CONSOLIDATE_FRIENDS_URL = "/friends.php";

    private const string UPDATE_FRIEND_SCORES_URL = "/scores.php";

    private const string POKE_URL = "/poke.php";

    private const string BRAG_URL = "/brag.php";

    private const string SECRET = "resxrctrv7tgv7gb8h9h9u0909kllfmolkjnhghgjjkhjghg";

    private static SocialManager _instance;

    private int _userid;

    private FacebookProfile _fbProfile;

    private List<Friend> _friends;

    private Dictionary<string, Hashtable> _fbFriends;

    private Dictionary<string, IUserProfile> _gcFriends;

    private bool _gameCenterAuthenticationComplete;

    private bool _gameCenterFriendListRequestComplete;

    private bool _fbReady;

    private bool _twitterReady;

    private SocialManager.FacebookCurrentRequest _fbCurrentRequest;

    private SocialManager.TwitterCurrentRequest _twitterCurrentRequest;

    private bool _doneLoggingIn;

    private bool _consolidatedFriendsCompleted;

    private Dictionary<string, Friend.Status> _friendStatus;

    private bool _dirty;

    private Dictionary<string, FacebookProfile> _fbProfiles;

    public static bool debugGUI;

    private Dictionary<string, FacebookProfile> _debugFacebookFriends;

    private Vector2 _debugGCScrollPosition = new Vector2(0f, 0f);

    private Vector2 _debugFBScrollPosition = new Vector2(0f, 0f);

    private enum FacebookCurrentRequest
    {
        None,
        Error,
        LoggingIn,
        GettingUserInfo,
        GettingFriends
    }

    private enum TwitterCurrentRequest
    {
        None,
        Error,
        LoggingIn
    }

    private enum WWWRequestResult
    {
        Success,
        Error
    }

    private delegate void WWWComplete(SocialManager.WWWRequestResult result, string output, object cookie);
}
