
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Carrot
{
    public class CarrotAds : MonoBehaviour
    {
        [Header("Ui")]
        public Image[] ImgApps;
        public Text TxtNameAppMain;
        public Text TxtDescAppMain;
        public Text TxtTimer;
        public Image imgTypeAppMain;
        public GameObject objBtndownload;
        public GameObject objBtnClsoe;
        public GameObject objPanelTimer;
        public Animator anim;
        private IList apps;
        private int indexSliderShow = 0;
        private string sLinkDownload = "";
        private Carrot carrot;
        public int totalSeconds = 15;
        private float elapsedTime = 0f;
        private bool isRunning = true;
        public void OnLoad(Carrot c, string sData)
        {
            this.carrot = c;
            this.objBtnClsoe.SetActive(false);
            this.objPanelTimer.SetActive(true);
            IDictionary data = Json.Deserialize(sData) as IDictionary;
            apps = data["all_item"] as IList;
            List<IDictionary> listAppShow = GetCircularRange(apps, this.indexSliderShow, 5);
            LoadDataSlider(listAppShow);
            this.anim.Play("CarrotAds_Load");
        }

        public List<IDictionary> GetCircularRange(IList list, int startIndex, int count = 5)
        {
            var result = new List<IDictionary>();
            int n = list.Count;
            if (n == 0 || count <= 0) return result;

            startIndex = ((startIndex % n) + n) % n;

            for (int i = 0; i < count; i++)
            {
                int index = (startIndex + i) % n;
                IDictionary d = list[index] as IDictionary;
                result.Add(d);
            }
            return result;
        }

        void Update()
        {
            if (!isRunning) return;

            elapsedTime += Time.deltaTime;

            if (elapsedTime >= totalSeconds)
            {
                elapsedTime = totalSeconds;
                isRunning = false;
                this.objPanelTimer.SetActive(false);
                this.objBtnClsoe.SetActive(true);
            }

            string current = FormatTime(elapsedTime);
            string total = FormatTime(totalSeconds);
            TxtTimer.text = $"{current} / {total}";
        }

        string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            return $"{minutes:00}:{seconds:00}";
        }

        public void BtnRemoveAds()
        {
            this.carrot.buy_product(carrot.index_inapp_remove_ads);
        }

        public void BtnNextSlider()
        {
            carrot.play_sound_click();
            indexSliderShow++;
            List<IDictionary> listAppShow = GetCircularRange(apps, this.indexSliderShow, 5);
            LoadDataSlider(listAppShow);
        }

        public void BtnNextTwoSlider()
        {
            carrot.play_sound_click();
            indexSliderShow += 2;
            List<IDictionary> listAppShow = GetCircularRange(apps, this.indexSliderShow, 5);
            LoadDataSlider(listAppShow);
        }

        public void BtnPrevSlider()
        {
            carrot.play_sound_click();
            if (indexSliderShow < 0) indexSliderShow = apps.Count - 1;
            indexSliderShow--;
            List<IDictionary> listAppShow = GetCircularRange(apps, this.indexSliderShow, 5);
            LoadDataSlider(listAppShow);
        }

        public void BtnPrevTwoSlider()
        {
            carrot.play_sound_click();
            if (indexSliderShow < 0) indexSliderShow = apps.Count - 1;
            indexSliderShow -= 2;
            List<IDictionary> listAppShow = GetCircularRange(apps, this.indexSliderShow, 5);
            LoadDataSlider(listAppShow);
        }

        public void BtnDownload()
        {
            carrot.play_sound_click();
            Application.OpenURL(sLinkDownload);
        }

        private void LoadDataSlider(List<IDictionary> listAppShow)
        {
            string s_key_lang = this.carrot.lang.Get_key_lang();
            for (int i = 0; i < listAppShow.Count; i++)
            {
                IDictionary data_item = listAppShow[i];
                string s_id_app = data_item["id_import"].ToString();
                if (i == 2)
                {
                    string s_name = "";
                    string s_desc = "";
                    string s_type = "game";

                    if (data_item["name_" + s_key_lang] != null) s_name = data_item["name_" + s_key_lang].ToString();
                    if (s_name == "")
                    {
                        if (data_item["name_en"] != null) s_name = data_item["name_en"].ToString();
                    }
                    TxtNameAppMain.text = s_name;

                    if (data_item["describe_" + s_key_lang] != null) s_desc = data_item["describe_" + s_key_lang].ToString();
                    if (s_desc == "")
                    {
                        if (data_item["describe_en"] != null) s_desc = data_item["describe_en"].ToString();
                    }
                    TxtDescAppMain.text = s_desc;

                    sLinkDownload = "";
                    string s_key_store_public = this.carrot.store_public.ToString().ToLower();
                    if (data_item[s_key_store_public] != null) sLinkDownload = data_item[s_key_store_public].ToString();
                    if (sLinkDownload == "")
                        objBtndownload.SetActive(false);
                    else
                        objBtndownload.SetActive(true);

                    if (data_item["type"] != null) s_type = data_item["type"].ToString();
                    if (s_type == "game")
                        imgTypeAppMain.sprite = carrot.icon_carrot_game;
                    else
                        imgTypeAppMain.sprite = carrot.icon_carrot_app;
                }

                if (data_item["icon"] != null)
                {
                    Sprite icon_app = this.carrot.get_tool().get_sprite_to_playerPrefs(s_id_app);
                    if (icon_app != null)
                    {
                        ImgApps[i].sprite = icon_app;
                    }
                    else
                    {
                        string s_url_icon = data_item["icon"].ToString();
                        if (s_url_icon != "") this.carrot.get_img_and_save_playerPrefs(s_url_icon, ImgApps[i], s_id_app);
                    }
                }
            }
        }

        public void BtnClose()
        {
            this.carrot.play_sound_click();
            this.carrot.CloseLastWindow();
        }

        public void StopAnimLoad()
        {
            this.anim.Play("CarrotAds");
        }
    }
}