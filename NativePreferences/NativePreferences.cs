using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MelonLoader;
using NativePreferences.ReModCE.Core;
using Newtonsoft.Json;
using ReMod.Core.Managers;
using ReMod.Core.UI;
using ReMod.Core.VRChat;
using UnityEngine;
using VRC;
using VRC.Core;
using VRC.SDKBase.Validation.Performance.Stats;
using VRCSDK2;
using AvatarList = Il2CppSystem.Collections.Generic.List<VRC.Core.ApiAvatar>;
using Object = UnityEngine.Object;
using NativeLoader;

namespace NativePreferences
{
    internal class NativePreferences : IAvatarListOwner
    {
        private List<ReAvatar> _favoritesList;

        private ReAvatarList _favoriteavatarList;

        private ReUiButton _favoriteButton;

        internal IEnumerator OnUIManager()
        {
            if (File.Exists(NativeLoader.MelonEntry.FileLocation.Value))
            {
                try
                {
                    _favoritesList =
                        JsonConvert.DeserializeObject<List<ReAvatar>>(File.ReadAllText(NativeLoader.MelonEntry.FileLocation.Value));
                }
                catch (Exception)
                {
                    NativeLogger.Error("Failed to load favorites from file, file may be malformed!");
                    NativeLogger.Warn("Stopping NativePreferences from loading to prevent further damage!");
                }
            }
            else
            {
                _favoritesList = new List<ReAvatar>();
            }

            while (GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)") == null) yield return null;

            new NativeLocalPlayerHeaderButton("Add To Local Avatar Favorites",
                ResourceManager.GetSprite("nativepreferences.heart"),
                () => MelonCoroutines.Start(Favoritefromplayer()));

            new NativeLocalPlayerHeaderButton("Log avatar info to console, and copy avatar info to clipboard",
                ResourceManager.GetSprite("nativepreferences.clipboard"), () => CopyAvatarInfo());

            while (GameObject.Find("UserInterface/MenuContent/Screens/Avatar") == null) yield return null;

            _favoriteavatarList = new ReAvatarList("NP Favs", this, false)
            {
                AvatarPedestal =
                {
                    field_Internal_Action_4_String_GameObject_AvatarPerformanceStats_ObjectPublicBoBoBoBoBoBoBoBoBoBoUnique_0 =
                        new Action<string, GameObject, AvatarPerformanceStats, ObjectPublicBoBoBoBoBoBoBoBoBoBoUnique>(OnAvatarInstantiated)
                }
            };

            new ReUiButton("ID", new Vector2(530, 0), new Vector2(0.25f, 1), () => MelonCoroutines.Start(LoadByID()),
                _favoriteavatarList.GameObject.transform.Find("Button").gameObject.transform);

            _favoriteButton = new ReUiButton("Favorite", new Vector2(640, 0), new Vector2(0.5f, 1),
                () => FavoriteAvatar(_favoriteavatarList.AvatarPedestal.field_Internal_ApiAvatar_0),
                _favoriteavatarList.GameObject.transform.Find("Button").gameObject.transform);

            _favoriteavatarList.RefreshAvatars();
            yield return new WaitForSecondsRealtime(0.5f);
            _favoriteavatarList.RefreshAvatars();
        }

        private IEnumerator LoadByID()
        {
            if (!GUIUtility.systemCopyBuffer.ToLower().StartsWith("avtr_"))
            {
                ShowAlert("Could not find valid avatar ID in clipboard!");
                yield break;
            }

            var obj = new GameObject("obj");
            var pedestal = obj.AddComponent<VRC_AvatarPedestal>();
            pedestal.blueprintId = GUIUtility.systemCopyBuffer;
            var timer = 0;

            while (pedestal.Instance.GetComponent<AvatarPedestal>().field_Private_ApiAvatar_0 == null & timer <= 120)
            {
                timer++;
                yield return null;
            }

            if (timer >= 120)
            {
                ShowAlert(
                    "Took to long trying to fetch avatar, stopping fetch process!\nPlease make sure the ID is valid and the avatar is not deleted.");
                Object.DestroyImmediate(obj);
                yield break;
            }

            _favoriteavatarList.AvatarPedestal.Method_Public_Void_ApiAvatar_0(pedestal.Instance
                .GetComponent<AvatarPedestal>()
                .field_Private_ApiAvatar_0);
            Object.DestroyImmediate(obj);
        }

        private IEnumerator Favoritefromplayer()
        {
            foreach (var player in PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0)
            {
                if (player.prop_APIUser_0.id == QuickMenuEx.SelectedUserLocal.field_Private_IUser_0.prop_String_0)
                {
                    if (player.prop_ApiAvatar_0.releaseStatus == "private")
                    {
                        QuickMenuEx.Instance.ShowAlertDialog("NativePreferences",
                            $"Cannot Favorite {player.prop_ApiAvatar_0.name}\nthis avatar is private!");
                        yield break;
                    }

                    if (!HasAvatarFavorited(player.prop_ApiAvatar_0.id))
                    {
                        _favoritesList.Insert(0, new ReAvatar(player.prop_ApiAvatar_0));
                        QuickMenuEx.Instance.ShowAlertDialog("NativePreferences",
                            $"Locally Favorited {player.prop_ApiAvatar_0.name}");
                    }
                    else
                    {
                        QuickMenuEx.Instance.ShowConfirmDialog("NativePreferences",
                            $"Are you sure you want to unfavorite {player.prop_ApiAvatar_0.name}",
                            () => { _favoritesList.RemoveAll(a => a.Id == player.prop_ApiAvatar_0.id); });
                    }

                    SaveAvatarsToDisk();

                    while (GameObject.Find("UserInterface/MenuContent/Screens/Avatar").activeSelf == false)
                    {
                        yield return null;
                    }

                    yield return new WaitForSecondsRealtime(0.5f);
                    _favoriteavatarList.RefreshAvatars();
                    yield break;
                }

                yield return null;
            }
        }

        private static void CopyAvatarInfo()
        {
            foreach (var player in PlayerManager.field_Private_Static_PlayerManager_0.field_Private_List_1_Player_0)
            {
                if (player.prop_APIUser_0.id == QuickMenuEx.SelectedUserLocal.field_Private_IUser_0.prop_String_0)
                {
                    var apiavatar = player.prop_ApiAvatar_0;
                    var tags = "";
                    foreach (var tag in apiavatar.tags)
                    {
                        tags = string.Join(", ", tag);
                    }

                    var msg = $"Avatar Name: {apiavatar.name}" + $"\nAuthor Name: {apiavatar.authorName}" +
                              $"\nAvatar ID: {apiavatar.id}" + $"\nAuthor ID {apiavatar.authorId}" +
                              $"\nDescription: {apiavatar.description}" + $"\nTags: {tags}" +
                              $"\nVersion: {apiavatar.version}" + $"\nUnity Version: {apiavatar.unityVersion}" +
                              $"\nRelease Status: {apiavatar.releaseStatus}" + $"\nAsset URL: {apiavatar.assetUrl}" +
                              $"\nImage URL: {apiavatar.thumbnailImageUrl}";

                    NativeLogger.Msg(msg);
                    GUIUtility.systemCopyBuffer = msg;
                    return;
                }
            }
        }

        private static void ShowAlert(string message, float seconds = 15f)
        {
            if (VRCUiPopupManager.prop_VRCUiPopupManager_0 == null) return;

            VRCUiPopupManager.prop_VRCUiPopupManager_0.Method_Public_Void_String_String_Single_0("NativePreferences", message, seconds);
        }

        private void OnAvatarInstantiated(string url, GameObject avatar, AvatarPerformanceStats avatarPerformanceStats, ObjectPublicBoBoBoBoBoBoBoBoBoBoUnique unk)
        {
            _favoriteButton.Text = HasAvatarFavorited(_favoriteavatarList.AvatarPedestal.field_Internal_ApiAvatar_0.id)
                ? "Unfavorite"
                : "Favorite";
        }

        private bool HasAvatarFavorited(string id)
        {
            return _favoritesList.FirstOrDefault(a => a.Id == id) != null;
        }

        private void FavoriteAvatar(ApiAvatar apiAvatar)
        {
            if (!HasAvatarFavorited(apiAvatar.id))
            {
                _favoritesList.Insert(0, new ReAvatar(apiAvatar));
                _favoriteButton.Text = "Unfavorite";
            }
            else
            {
                _favoritesList.RemoveAll(a => a.Id == apiAvatar.id);
                _favoriteButton.Text = "Favorite";
            }

            SaveAvatarsToDisk();
            _favoriteavatarList.RefreshAvatars();
        }

        private void SaveAvatarsToDisk()
        {
            File.WriteAllText(NativeLoader.MelonEntry.FileLocation.Value, JsonConvert.SerializeObject(_favoritesList));
        }

        public AvatarList GetAvatars(ReAvatarList avatarList)
        {
            var list = new AvatarList();
            foreach (var avi in _favoritesList.Select(x => x.AsApiAvatar()).ToList())
            {
                list.Add(avi);
            }

            return list;
        }

        public void Clear(ReAvatarList avatarList)
        {
            avatarList.RefreshAvatars();
        }
    }
}