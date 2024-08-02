using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Events;

namespace Carrot
{
    public class Carrot_ads : MonoBehaviour, IUnityAdsShowListener, IUnityAdsInitializationListener, IUnityAdsLoadListener
    {
        //Obj Carrot Ads
        public GameObject window_ads_prefab;
        private Carrot carrot;
        private string s_data_carrotapp_all_ads = "";
        //Config
        private int count_show_ads = 0;
        private bool is_ads = true;

        //Ads Unity config
        private string id_ads_app_unity;
        private string id_ads_Interstitial_unity;
        private string id_ads_Banner_unity;
        private string id_ads_Rewarded_unity;
        //Event Customer
        public UnityAction onRewardedSuccess;

        public void On_load(Carrot carrot)
        {
            this.carrot = carrot;
            if (PlayerPrefs.GetInt("is_ads", 0) == 0)
                this.is_ads = true;
            else
                this.is_ads = false;

            this.Set_enable_all_emp_btn_removeads(this.is_ads);

            if (this.carrot.type_ads == TypeAds.Admod_Unity_Carrot || this.carrot.type_ads == TypeAds.Unity_Admob_Carrot)
            {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
                //Setup_ads_Admob();
#endif
            }

            if (this.carrot.type_ads == TypeAds.Admod_Unity_Carrot || this.carrot.type_ads == TypeAds.Unity_Admob_Carrot)
            {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
                Setup_ads_unity();
#endif
            }

            if (this.carrot.is_offline() && this.is_ads)
            {
                this.s_data_carrotapp_all_ads = PlayerPrefs.GetString("s_data_carrotapp_all_ads");
            }
        }

        private void Set_enable_all_emp_btn_removeads(bool is_show_ads)
        {
            for (int i = 0; i < this.carrot.emp_btn_remove_ads.Length; i++)
            {
                this.carrot.emp_btn_remove_ads[i].SetActive(is_show_ads);
            }
        }


        public void create_banner_ads()
        {
            if (this.is_ads)
            {
                if (carrot.type_ads == TypeAds.Unity_Admob_Carrot)
                {
                    if (carrot.id_ads_unity_Banner_android!="") this.Unity_ShowBannerAd();
                }
            }
        }

        public void show_ads_Interstitial()
        {
            if (this.is_ads)
            {
                this.count_show_ads++;
                if (this.count_show_ads >= this.carrot.click_show_ads)
                {
                    /*
                    if (this.carrot.type_ads == TypeAds.Admod_Unity_Carrot)
                        this.Admob_Show_InterstitialAd();
                    else
                    */
                    if (this.carrot.type_ads == TypeAds.Unity_Admob_Carrot)
                        this.Unity_ShowVideoAd();
                    else
                        this.show_carrot_ads();
                    this.count_show_ads = 0;
                }
            }
        }

        public void set_act_Rewarded_Success(UnityAction act)
        {
            this.onRewardedSuccess = act;
        }

        public void show_ads_Rewarded()
        {
            //if(carrot.type_ads==TypeAds.Admod_Unity_Carrot) this.Admob_ShowRewardedAd();
            if (carrot.type_ads == TypeAds.Unity_Admob_Carrot) this.Unity_ShowRewardedAd();
        }

        public void remove_ads()
        {
            //this.Destroy_Banner_Ad();
            //this.Destroy_Interstitial_Ad();
            PlayerPrefs.SetInt("is_ads", 1);
            this.is_ads = false;
            this.Set_enable_all_emp_btn_removeads(this.is_ads);
        }

        public bool get_status_ads()
        {
            return this.is_ads;
        }

        public void set_status_ads(bool is_status)
        {
            this.is_ads = is_status;
        }

        public void Show_box_dev_test()
        {
            Carrot_Box box_test = carrot.Create_Box();
            box_test.set_icon(carrot.icon_carrot_ads);
            box_test.set_title("Ads Test");

            Carrot_Box_Item item_unity_video = box_test.create_item();
            item_unity_video.set_title("Unity Video Ads");
            item_unity_video.set_tip("Interstitia ID:" + this.carrot.id_ads_unity_Interstitial_android);
            item_unity_video.set_icon(carrot.game.icon_play_music_game);
            item_unity_video.set_act(() => Unity_ShowVideoAd());

            Carrot_Box_Item item_unity_Rewarded = box_test.create_item();
            item_unity_Rewarded.set_title("Unity Rewarded Ads");
            item_unity_Rewarded.set_tip("Ads ID:" + this.carrot.id_ads_unity_Rewarded_android);
            item_unity_Rewarded.set_icon(carrot.game.icon_play_music_game);
            item_unity_Rewarded.set_act(() => Unity_ShowRewardedAd());

            Carrot_Box_Item item_carrot_ads = box_test.create_item();
            item_carrot_ads.set_title("Carrot Ads");
            item_carrot_ads.set_tip("App ID:" + this.carrot.Carrotstore_AppId);
            item_carrot_ads.set_icon(carrot.game.icon_play_music_game);
            item_carrot_ads.set_act(() => show_carrot_ads());
        }

        #region Carrot Ads
        [ContextMenu("Show Carrot Ads")]
        public void show_carrot_ads()
        {
            if (this.s_data_carrotapp_all_ads != "")
            {
                this.Act_load_ads_data(this.s_data_carrotapp_all_ads);
            }
            else
            {
                this.carrot.show_loading();
                IList list_url_app = carrot.config["list_url_app"] as IList;
                this.carrot.Get_Data(carrot.random(list_url_app), (data) =>
                {
                    Act_show_carrot_ads_done(data);
                },show_carrot_ads);
            }
        }

        private void Act_show_carrot_ads_done(string s_data)
        {
            this.carrot.hide_loading();
            this.s_data_carrotapp_all_ads = s_data;
            PlayerPrefs.SetString("s_data_carrotapp_all_ads", s_data);
            this.Act_load_ads_data(s_data);
        }

        private void Act_show_carrot_ads_fail(string s_error)
        {
            this.carrot.hide_loading();
            if (this.s_data_carrotapp_all_ads != "") this.Act_load_ads_data(this.s_data_carrotapp_all_ads);
        }

        private void Act_load_ads_data(string s_data)
        {
            IDictionary data_app = Json.Deserialize(s_data) as IDictionary;
            IList list_app = data_app["all_item"] as IList;
            int index_random = Random.Range(0, list_app.Count);
            IDictionary app_ads = list_app[index_random] as IDictionary;
            this.Act_show_ads(app_ads);
        }

        private void Act_show_ads(IDictionary data_ads)
        {
            GameObject window_ads = this.carrot.create_window(this.window_ads_prefab);
            window_ads.name = "window_ads";
            window_ads.gameObject.SetActive(true);
            Carrot_Window_Ads ads = window_ads.GetComponent<Carrot_Window_Ads>();
            ads.On_load(this.carrot);
            ads.Load_data_ads(data_ads);
        }
        #endregion

        #region Unity Ads

        private void Setup_ads_unity()
        {
#if UNITY_ANDROID
            this.id_ads_app_unity = this.carrot.id_ads_unity_App_android;
            this.id_ads_Interstitial_unity = this.carrot.id_ads_unity_Interstitial_android;
            this.id_ads_Banner_unity = this.carrot.id_ads_unity_Banner_android;
            this.id_ads_Rewarded_unity = this.carrot.id_ads_unity_Rewarded_android;
#elif UNITY_IOS
            this.id_ads_app_unity = this.carrot.id_ads_unity_App_ios;
            this.id_ads_Interstitial_unity = this.carrot.id_ads_unity_Interstitial_ios;
            this.id_ads_Banner_unity = this.carrot.id_ads_unity_Banner_ios;
            this.id_ads_Rewarded_unity = this.carrot.id_ads_unity_Rewarded_ios;
#endif

            Advertisement.Initialize(this.id_ads_app_unity, carrot.ads_uniy_test_mode, this);
        }

        public void Unity_ShowBannerAd()
        {
            if (this.id_ads_Banner_unity != "")
            {
                Advertisement.Banner.SetPosition(BannerPosition.TOP_CENTER);
                Advertisement.Banner.Show(this.id_ads_Banner_unity);
            }
        }

        public void Unity_HideBannerAd()
        {
            Advertisement.Banner.Hide();
        }

        public void Unity_ShowVideoAd()
        {
            if(this.id_ads_Interstitial_unity != "") Advertisement.Show(this.id_ads_Interstitial_unity, this);
        }

        public void Unity_ShowRewardedAd()
        {
            if(this.id_ads_Rewarded_unity != "") Advertisement.Show(this.id_ads_Rewarded_unity, this);
        }

        public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
        {
            Debug.Log("Unity Ads show fail:" + placementId + " ->" + message);
            if (placementId == id_ads_Interstitial_unity)
            {
                /*
                if (carrot.type_ads == TypeAds.Unity_Admob_Carrot)
                    this.Admob_Show_InterstitialAd();
                else*/
                    this.show_carrot_ads();
            }

            if (placementId == id_ads_Rewarded_unity)
            {
                /*
                if (carrot.type_ads == TypeAds.Unity_Admob_Carrot)
                    this.Admob_ShowRewardedAd();
                else*/
                    this.show_carrot_ads();
            }

            if (placementId == id_ads_Banner_unity)
            {
                if (carrot.type_ads == TypeAds.Unity_Admob_Carrot)
                {
                    //this.Admob_CreateBannerView();
                }
            }
        }

        public void OnUnityAdsShowStart(string placementId)
        {
            Debug.Log("Unity Ads Show start:" + placementId);
        }

        public void OnUnityAdsShowClick(string placementId)
        {
            Debug.Log("Unity Ads click:" + placementId);
        }

        public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
        {
            Debug.Log("Unity Ads show complete:" + placementId);
            if (placementId == id_ads_Rewarded_unity)
            {
                if (showCompletionState == UnityAdsShowCompletionState.COMPLETED) onRewardedSuccess?.Invoke();
            }
        }

        public void OnInitializationComplete()
        {
            Advertisement.Load(this.id_ads_Banner_unity, this);
            Advertisement.Load(this.id_ads_Interstitial_unity, this);
            Advertisement.Load(this.id_ads_Rewarded_unity, this);
            Debug.Log("Unity Ads setup success!");
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.Log("Unity Ads setup fail:" + message);
        }

        public void OnUnityAdsAdLoaded(string placementId)
        {
            Debug.Log("Unity Ads load success:" + placementId);
        }

        public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
        {
            Debug.Log("Unity Ads Load fail:" + placementId + " ->" + message);
            if (placementId == this.id_ads_Banner_unity)
            {
                //if (this.carrot.type_ads == TypeAds.Unity_Admob_Carrot) this.Admob_CreateBannerView();
            }
        }
        #endregion
    }
}