using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Carrot
{
    public struct Carrot_rank_data
    {
        public Carrot_Rate_user_data user { get; set; }
        public string type { get; set; }
        public string scores { get; set; }
        public string date { get; set; }
    }

    public class carrot_game_rank_type
    {
        public string s_name;
        public Sprite icon;
    }

    public enum Carrot_game_rank_order
    {
        Descending,
        Ascending
    }

    public class Carrot_game : MonoBehaviour
    {
        private Carrot carrot;
        [Header("Music Background Game")]
        public Sprite icon_list_music_game;
        public Sprite icon_pause_music_game;
        public Sprite icon_play_music_game;
        public Sprite icon_waiting_music_game;
        private AudioSource sound_bk_game = null;
        public AudioSource sound_bk_game_test;
        private Carrot_Box_Item box_setting_item_bkmusic = null;
        private Carrot_Box box_list;
        private List<string> list_link_music_bk;
        private byte[] data_music_temp;
        private Carrot_Box_Item item_bk_music_play;
        private string id_select_play_bk_music;
        private string id_buy_bk_music_temp;
        private int index_buy_music_link_temp;

        [Header("GamePad")]
        public GameObject obj_gamepad_prefab;
        private int index_gampad_item = 0;
        private List<GameObject> list_obj_gamapad_console;
        private ScrollRect scrollRect_gamepad_console;
        private Color32 color_sel_item_btn_gamepad;
        private Color32 color_nomal_item_btn_gamepad;
        private List<Carrot_Gamepad> list_gamepad;
        private bool is_user_gampad_for_console = true;
        private UnityAction<bool> act_handle_detection = null;
        public UnityAction act_click_watch_ads_in_music_bk;

        [Header("Top player")]
        public string[] leaderboardId;
        public Sprite icon_top_player;
        public GameObject item_top_player_prefab;
        public Sprite[] icon_rank_player;
        private LeaderboardScoresPage scoreResponse = null;
        private int rank_type_temp = 0;
        private Carrot_game_rank_order order_top_player = Carrot_game_rank_order.Descending;
        private List<carrot_game_rank_type> list_rank_type;
        private string url_data_audio = "https://raw.githubusercontent.com/kurotsmile/Database-Store-Json/refs/heads/main/audio.json";
        private string s_data_audio = "";
        public void Load_carrot_game()
        {
            this.carrot = this.GetComponent<Carrot>();
            this.sound_bk_game = this.GetComponent<AudioSource>();

            if (this.carrot.type_control != TypeControl.None)
            {
                this.list_gamepad = new List<Carrot_Gamepad>();
                this.check_connect_gamepad();
                EventSystem.current.sendNavigationEvents = false;
            }
            this.color_sel_item_btn_gamepad = this.carrot.get_color_highlight_blur(100);
            this.color_nomal_item_btn_gamepad = Color.white;

            this.list_rank_type = new List<carrot_game_rank_type>();
        }

        #region Background Music Game
        public void show_list_music_game(Carrot_Box_Item item_setting = null)
        {
            this.carrot.show_loading();
            this.box_setting_item_bkmusic = item_setting;
            if (this.s_data_audio == "")
            {
                this.carrot.Get_Data(this.url_data_audio, s_data =>
                {
                    this.s_data_audio = s_data;
                    this.get_list_music_game_done(s_data);
                }, get_list_music_game_fail);
            }
            else
            {
                this.get_list_music_game_done(this.s_data_audio);
            }
        }

        private void get_list_music_game_done(string s_data)
        {
            bool is_ads = PlayerPrefs.GetInt("is_ads", 0) == 0;
            this.carrot.hide_loading();
            IDictionary data = Json.Deserialize(s_data) as IDictionary;
            IList list_audio = data["all_item"] as IList;
            if (list_audio.Count > 0)
            {
                List<IDictionary> list_music = new();
                for (int i = 0; i < list_audio.Count; i++)
                {
                    list_music.Add(list_audio[i] as IDictionary);
                };
                this.carrot.log("show_list_music_game from server..." + list_music.Count);

                if (this.sound_bk_game != null) this.sound_bk_game.Stop();
                if (this.box_list != null) this.box_list.close();
                this.box_list = this.carrot.Create_Box("carrot_list_bk_music");
                box_list.set_title(this.carrot.lang.Val("list_bk_music", "Background music games"));
                box_list.set_icon(this.icon_list_music_game);
                box_list.set_act_before_closing(this.act_close_list_music);

                this.list_link_music_bk = new List<string>();
                this.item_bk_music_play = null;
                this.id_select_play_bk_music = "";
                this.id_buy_bk_music_temp = "";
                this.index_buy_music_link_temp = -1;

                for (int i = 0; i < list_music.Count; i++)
                {
                    IDictionary item_data_music = (IDictionary)list_music[i];

                    Carrot_Box_Item item_music_bk = this.box_list.create_item("item_bk_music_" + i);
                    item_music_bk.set_icon(this.icon_list_music_game);
                    item_music_bk.set_title(item_data_music["name"].ToString());
                    this.list_link_music_bk.Add(item_data_music["mp3"].ToString());

                    var index_link = i;
                    var id_bk_music = item_data_music["id"].ToString();
                    Carrot_Box_Btn_Item btn_play = item_music_bk.create_item();
                    btn_play.set_icon(this.icon_play_music_game);
                    btn_play.set_color(this.carrot.color_highlight);
                    btn_play.set_act(() => this.play_item_music_background_game(item_music_bk, index_link, id_bk_music));

                    bool is_buy = false;

                    if (item_data_music["buy"].ToString() == "0") is_buy = false;
                    else is_buy = true;

                    if (is_buy)
                    {
                        if (PlayerPrefs.GetInt("is_buy_bk_" + item_data_music["id"].ToString(), 0) == 1) is_buy = false;
                    }

                    if (is_buy)
                    {
                        Carrot_Box_Btn_Item btn_buy = item_music_bk.create_item();
                        btn_buy.set_icon(this.carrot.icon_carrot_buy);
                        btn_buy.set_color(this.carrot.color_highlight);
                        btn_buy.set_act(() => this.act_buy_music_bk(id_bk_music, index_link));
                        item_music_bk.set_tip("Please buy to use this track");
                        item_music_bk.set_act(() => this.act_buy_music_bk(id_bk_music, index_link));

                        if (carrot.os_app != OS.Window)
                        {
                            if (is_ads)
                            {
                                Carrot_Box_Btn_Item btn_ads = item_music_bk.create_item();
                                btn_ads.set_icon(this.carrot.icon_carrot_ads);
                                btn_ads.set_color(this.carrot.color_highlight);
                                btn_ads.set_act(() =>
                                {
                                    this.id_buy_bk_music_temp = id_bk_music;
                                    this.index_buy_music_link_temp = index_link;
                                    this.act_click_watch_ads_in_music_bk?.Invoke();
                                });
                            }
                        }
                    }
                    else
                    {
                        item_music_bk.set_tip("Free");
                        item_music_bk.set_act(() => this.act_change_bk_music_game(id_bk_music, index_link));
                    }

                    this.box_list.update_color_table_row();
                }
                if (this.carrot.type_control != TypeControl.None)
                {
                    this.set_list_button_gamepad_console(box_list.UI.get_list_btn());
                    this.set_scrollRect_gamepad_consoles(box_list.UI.scrollRect);
                }
            }
            else
            {
                this.carrot.Show_msg("Background music games", "There are no songs in the list");
            }
        }

        private void get_list_music_game_fail(string s_error)
        {
            this.carrot.hide_loading();
            this.carrot.log(s_error);
        }

        private void act_close_list_music()
        {
            this.id_buy_bk_music_temp = "";
            this.index_buy_music_link_temp = -1;
            this.sound_bk_game_test.Stop();
            if (this.carrot.get_status_sound())
                if (this.sound_bk_game != null) this.sound_bk_game.Play();
                else
                if (this.sound_bk_game != null) this.sound_bk_game.Stop();
        }

        public void play_item_music_background_game(Carrot_Box_Item item_muic_bk, int index_links, string id_music_bk)
        {
            if (this.id_select_play_bk_music == id_music_bk)
            {
                this.sound_bk_game_test.Stop();
                this.item_bk_music_play.set_icon(this.icon_list_music_game);
                this.id_select_play_bk_music = "";
                return;
            }

            if (this.item_bk_music_play != null)
            {
                this.sound_bk_game_test.Stop();
                this.item_bk_music_play.set_icon(this.icon_list_music_game);
            }

            this.item_bk_music_play = item_muic_bk;
            this.id_select_play_bk_music = id_music_bk;

            this.carrot.log("id_select_play_bk_music:" + id_select_play_bk_music);
            this.item_bk_music_play.set_icon(this.icon_waiting_music_game);
            this.carrot.get_mp3(this.list_link_music_bk[index_links], act_play_music, this.act_cancel_play_music);
            Carrot_Window_Loading loading = this.carrot.get_loading_cur();
            loading.set_act_cancel_session(this.act_cancel_play_music);
        }

        private void act_play_music(UnityWebRequest unityWebRequest)
        {
            this.item_bk_music_play.set_icon(this.icon_pause_music_game);
            this.sound_bk_game_test.clip = DownloadHandlerAudioClip.GetContent(unityWebRequest);
            this.data_music_temp = unityWebRequest.downloadHandler.data;
            this.sound_bk_game_test.Play();
        }

        private void act_buy_music_bk(string id_bk_music, int index_link)
        {
            this.id_buy_bk_music_temp = id_bk_music;
            this.index_buy_music_link_temp = index_link;
            this.carrot.buy_product(this.carrot.index_inapp_buy_bk_music);
        }

        private void act_cancel_play_music()
        {
            this.item_bk_music_play.set_icon(this.icon_list_music_game);
        }

        private void act_change_bk_music_game(string id_change_bk_music, int index_link_bk_music)
        {
            if (id_change_bk_music != this.id_select_play_bk_music)
            {
                this.carrot.get_mp3(this.list_link_music_bk[index_link_bk_music], download_and_set_bk_music);
            }
            else
            {
                this.carrot.get_tool().save_file("music_bk", this.data_music_temp);
                this.load_bk_music(this.sound_bk_game);
                if (this.box_setting_item_bkmusic != null) this.box_setting_item_bkmusic.set_change_status(true);
                if (this.box_list != null) this.box_list.close();
            }
        }

        private void download_and_set_bk_music(UnityWebRequest unityWebRequest)
        {
            this.carrot.get_tool().save_file("music_bk", unityWebRequest.downloadHandler.data);
            this.load_bk_music(this.sound_bk_game);
            if (this.box_setting_item_bkmusic != null) this.box_setting_item_bkmusic.set_change_status(true);
            if (this.box_list != null) this.box_list.close();
        }


        public void load_bk_music(AudioSource audio_bk_music)
        {
            this.sound_bk_game = audio_bk_music;
            if (this.carrot.get_tool().check_file_exist("music_bk"))
            {
                if (Application.isEditor)
                    StartCoroutine(this.downloadAudio("file://" + Application.dataPath + "/music_bk"));
                else
                    StartCoroutine(this.downloadAudio("file://" + Application.persistentDataPath + "/music_bk"));
            }
            else
            {
                if (audio_bk_music.clip != null)
                {
                    if (this.carrot.get_status_sound())
                        this.sound_bk_game.Play();
                    else
                        this.sound_bk_game.Stop();
                }
            }
        }

        public void delete_bk_music()
        {
            if (this.carrot.get_tool().check_file_exist("music_bk")) this.carrot.get_tool().delete_file("music_bk");
            if (this.sound_bk_game != null) this.sound_bk_game.Stop();
        }

        IEnumerator downloadAudio(string url_audio)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url_audio, AudioType.MPEG))
            {
                www.SendWebRequest();
                while (!www.isDone)
                {
                    yield return null;
                }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    if (this.sound_bk_game != null)
                    {
                        this.sound_bk_game.clip = DownloadHandlerAudioClip.GetContent(www);
                        this.sound_bk_game.Play();
                    }
                }
            }
        }

        public void check_buy_music_item_bk(string id_product_success)
        {
            if (id_product_success == this.carrot.shop.get_id_by_index(this.carrot.index_inapp_buy_bk_music))
            {
                PlayerPrefs.SetInt("is_buy_bk_" + this.id_buy_bk_music_temp, 1);
                this.act_change_bk_music_game(this.id_buy_bk_music_temp, this.index_buy_music_link_temp);
                this.carrot.Show_msg(this.carrot.lang.Val("shop", "Shop"), this.carrot.lang.Val("buy_bk_success", "Buy background music successfully!"), Msg_Icon.Success);
                this.id_buy_bk_music_temp = "";
            }
        }

        public void OnRewardedSuccess()
        {
            if (this.id_buy_bk_music_temp != "")
            {
                if (this.id_buy_bk_music_temp != null)
                {
                    Debug.Log("buy music:" + this.id_buy_bk_music_temp.ToString());
                    this.act_change_bk_music_game(this.id_buy_bk_music_temp, this.index_buy_music_link_temp);
                    this.carrot.Show_msg(this.carrot.lang.Val("shop", "Shop"), "You have received the background music reward!", Msg_Icon.Success);
                    this.id_buy_bk_music_temp = "";
                }

            }
        }

        public AudioSource get_audio_source_bk()
        {
            return this.sound_bk_game;
        }
        #endregion

        #region GamePad
        private void check_connect_gamepad()
        {
            StartCoroutine(try_check_gamepad_connect());
        }

        private IEnumerator try_check_gamepad_connect()
        {
            yield return new WaitForSeconds(2.5f);
            string[] names_gamepad = Input.GetJoystickNames();
            if (names_gamepad.Length > 0)
            {
                for (int i = this.list_gamepad.Count - 1; i >= 0; i--)
                {
                    if (names_gamepad[i] != null)
                    {
                        this.list_gamepad[i].txt_gamepad_name.text = names_gamepad[i];
                        this.list_gamepad[i].set_joystick_enable_play(true);
                    }
                    else
                    {
                        this.list_gamepad[i].set_joystick_enable_play(false);
                    }
                }
                if (this.act_handle_detection != null) this.act_handle_detection(true);
            }
            else
            {
                if (this.act_handle_detection != null) this.act_handle_detection(false);
            }
            this.check_connect_gamepad();
        }

        private void reset_btn_console()
        {
            if (this.list_obj_gamapad_console == null || this.is_user_gampad_for_console == false) return;
            for (int i = 0; i < this.list_obj_gamapad_console.Count; i++)
            {
                if (this.list_obj_gamapad_console[i].GetComponent<Button>())
                    this.list_obj_gamapad_console[i].GetComponent<Image>().color = this.list_obj_gamapad_console[i].GetComponent<Button>().colors.normalColor;
                else if (this.list_obj_gamapad_console[i].GetComponent<InputField>())
                    this.list_obj_gamapad_console[i].GetComponent<Image>().color = this.list_obj_gamapad_console[i].GetComponent<InputField>().colors.normalColor;
                else
                    this.list_obj_gamapad_console[i].GetComponent<Image>().color = this.color_nomal_item_btn_gamepad;
            }
        }

        public void set_index_button_gamepad_console(int index_sel)
        {
            this.reset_btn_console();
            this.index_gampad_item = index_sel;
            this.list_obj_gamapad_console[index_sel].GetComponent<Image>().color = this.color_sel_item_btn_gamepad;
            EventSystem.current.SetSelectedGameObject(this.list_obj_gamapad_console[index_sel]);
        }

        public void gamepad_keydown_up_console()
        {
            if (this.list_obj_gamapad_console == null || this.is_user_gampad_for_console == false) return;
            this.reset_btn_console();
            this.index_gampad_item--;
            if (this.index_gampad_item < 0) this.index_gampad_item = this.list_obj_gamapad_console.Count - 1;
            this.list_obj_gamapad_console[this.index_gampad_item].GetComponent<Image>().color = this.color_sel_item_btn_gamepad;
            EventSystem.current.SetSelectedGameObject(this.list_obj_gamapad_console[this.index_gampad_item].gameObject);
            if (this.scrollRect_gamepad_console != null) this.update_pos_scrollrect();
        }

        public void gamepad_keydown_down_console()
        {
            if (this.list_obj_gamapad_console == null || this.is_user_gampad_for_console == false) return;
            this.reset_btn_console();
            this.index_gampad_item++;
            if (this.index_gampad_item >= this.list_obj_gamapad_console.Count) this.index_gampad_item = 0;
            this.list_obj_gamapad_console[this.index_gampad_item].GetComponent<Image>().color = this.color_sel_item_btn_gamepad;
            EventSystem.current.SetSelectedGameObject(this.list_obj_gamapad_console[this.index_gampad_item].gameObject);
            if (this.scrollRect_gamepad_console != null) this.update_pos_scrollrect();
        }

        private void update_pos_scrollrect()
        {
            RectTransform objTransform = this.list_obj_gamapad_console[this.index_gampad_item].GetComponent<RectTransform>();
            RectTransform scrollTransform = this.scrollRect_gamepad_console.GetComponent<RectTransform>();
            ContentSizeFitter obj_body_size = this.scrollRect_gamepad_console.gameObject.GetComponentInChildren<ContentSizeFitter>();
            RectTransform obj_body = obj_body_size.GetComponent<RectTransform>();
            float normalizePosition = scrollTransform.anchorMin.y - objTransform.anchoredPosition.y;
            normalizePosition += (float)objTransform.transform.GetSiblingIndex() / (float)this.scrollRect_gamepad_console.content.transform.childCount;
            normalizePosition /= obj_body.sizeDelta.y - objTransform.sizeDelta.y;
            normalizePosition = Mathf.Clamp01(1 - normalizePosition);
            this.scrollRect_gamepad_console.verticalNormalizedPosition = normalizePosition;
        }

        public void gamepad_keydown_enter_console()
        {
            if (this.list_obj_gamapad_console == null || this.is_user_gampad_for_console == false) return;
            StartCoroutine(act_keydown_enter_console());
        }

        private IEnumerator act_keydown_enter_console()
        {
            yield return new WaitForSeconds(0.3f);
            if (this.list_obj_gamapad_console[this.index_gampad_item].GetComponent<Button>())
                this.list_obj_gamapad_console[this.index_gampad_item].GetComponent<Button>().onClick.Invoke();
            else
                this.list_obj_gamapad_console[this.index_gampad_item].GetComponent<InputField>().ForceLabelUpdate();
        }

        private void set_list_button_gamepad_console(List<GameObject> objs)
        {
            if (objs != null)
            {
                List<GameObject> list_obj = new List<GameObject>();
                for (int i = 0; i < objs.Count; i++) if (objs[i].activeInHierarchy) list_obj.Add(objs[i]);
                this.is_user_gampad_for_console = true;
                this.scrollRect_gamepad_console = null;
                this.list_obj_gamapad_console = list_obj;
                this.set_index_button_gamepad_console(0);
            }
        }

        public void set_list_button_gamepad_console(List<GameObject> objs, int fade_color_sel = 100)
        {
            if (objs == null) return;
            this.color_sel_item_btn_gamepad = this.carrot.get_color_highlight_blur(fade_color_sel);
            this.set_list_button_gamepad_console(objs);
        }

        public void set_list_button_gamepad_console(List<GameObject> objs, ScrollRect sr)
        {
            if (objs == null) return;
            this.color_sel_item_btn_gamepad = this.carrot.get_color_highlight_blur(100);
            this.set_list_button_gamepad_console(objs);
            this.set_scrollRect_gamepad_consoles(sr);
        }

        public void set_scrollRect_gamepad_consoles(ScrollRect sr)
        {
            this.scrollRect_gamepad_console = sr;
        }

        public void clear_button_gamepad_console()
        {
            this.list_obj_gamapad_console = null;
            this.scrollRect_gamepad_console = null;
        }

        public void set_enable_gamepad_console(bool is_user_console)
        {
            this.is_user_gampad_for_console = is_user_console;
        }

        public void set_color_emp_gamepad_nomal(Color32 color_set)
        {
            this.color_nomal_item_btn_gamepad = color_set;
        }

        public void set_color_emp_gamepad_select(Color32 color_set)
        {
            this.color_sel_item_btn_gamepad = color_set;
        }

        public bool get_status_gamepad_console()
        {
            return this.is_user_gampad_for_console;
        }

        public Carrot_Gamepad create_gamepad(string s_id_gamepad)
        {
            GameObject obj_gamepad = Instantiate(this.obj_gamepad_prefab);
            obj_gamepad.transform.SetParent(GameObject.Find("Canvas").transform);
            obj_gamepad.transform.localPosition = obj_gamepad.transform.localPosition;
            obj_gamepad.transform.localScale = new Vector3(1f, 1f, 1f);
            RectTransform rectTransform = obj_gamepad.GetComponent<RectTransform>();
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.offsetMin = new Vector2(0f, 0f);
            rectTransform.offsetMax = new Vector2(0f, 0f);
            obj_gamepad.GetComponent<Carrot_Gamepad>().set_carrot(this.carrot);
            obj_gamepad.GetComponent<Carrot_Gamepad>().load_gamepad_data();
            obj_gamepad.GetComponent<Carrot_Gamepad>().set_id_gamepad(s_id_gamepad);
            obj_gamepad.GetComponent<Carrot_Gamepad>().panel_gamepad.SetActive(false);
            this.list_gamepad.Add(obj_gamepad.GetComponent<Carrot_Gamepad>());
            return obj_gamepad.GetComponent<Carrot_Gamepad>();
        }

        public List<Carrot_Gamepad> get_list_gamepad()
        {
            return this.list_gamepad;
        }

        public void set_act_handle_detection(UnityAction<bool> act)
        {
            this.act_handle_detection = act;
        }

        public void Destroy_all_gamepad()
        {
            if (this.list_gamepad != null)
            {
                for (int i = 0; i < this.list_gamepad.Count; i++) Destroy(this.list_gamepad[i].gameObject);
            }
            this.list_gamepad = new();
        }
        #endregion

        #region Top_Player
        public void Add_type_rank(string s_name, Sprite icon = null)
        {
            carrot_game_rank_type type_rank = new()
            {
                s_name = s_name,
                icon = icon
            };
            this.list_rank_type.Add(type_rank);
        }

        public async Task Show_List_Top_player()
        {
            if (this.scoreResponse == null)
            {
                if (!PlayerAccountService.Instance.IsSignedIn)
                {
                    Task task = this.carrot.user.Show_loginAsync(async () =>
                    {
                        await Show_List_Top_player();
                    });
                }
                else
                {
                    this.carrot.show_loading();
                    scoreResponse = await LeaderboardsService.Instance.GetScoresAsync(this.leaderboardId[0]);

                    foreach (var result in scoreResponse.Results)
                    {
                        Debug.Log($"Player: {result.PlayerName}, Score: {result.Score}, Rank: {result.Rank}");
                    }
                    this.Act_get_List_Top_player_done(scoreResponse);
                }
            }
            else
            {
                this.Act_get_List_Top_player_done(scoreResponse);
            }
        }

        private async void Act_show_Top_player_by_type(int type)
        {
            this.rank_type_temp = type;
            await this.Show_List_Top_player();
        }

        private void Act_get_List_Top_player_done(LeaderboardScoresPage scoreResponse)
        {
            this.carrot.hide_loading();
            if (scoreResponse == null || scoreResponse.Results == null || scoreResponse.Results.Count == 0)
            {
                this.carrot.Show_msg(this.carrot.lang.Val("top_player", "Player rankings"), carrot.lang.Val("top_player_none", "No player scores have been ranked yet, log in and play to add points to the rankings!"));
                return;
            }

            if(this.box_list != null) this.box_list.close();

            box_list = this.carrot.Create_Box();
            box_list.set_icon(this.icon_top_player);
            box_list.set_title(this.carrot.lang.Val("top_player", "Player rankings"));

            string id_user_cur = AuthenticationService.Instance.PlayerId;

            /*
            IList<IDictionary> list_rank = new List<IDictionary>();

            if (this.list_rank_type.Count > 0)
            {
                Carrot_Box_Btn_Panel panel_type_rank = box_list.create_panel_btn();
                for (int i = 0; i < this.list_rank_type.Count; i++)
                {
                    var index = i;
                    Carrot_Button_Item btn_rank = panel_type_rank.create_btn("btn_rank_" + i);
                    btn_rank.set_icon_white(this.list_rank_type[i].icon);
                    btn_rank.set_label(this.list_rank_type[i].s_name);
                    btn_rank.set_label_color(Color.white);
                    btn_rank.set_act_click(() => this.Act_show_Top_player_by_type(index));
                    if (this.rank_type_temp == i)
                        btn_rank.set_bk_color(this.carrot.color_highlight);
                    else
                        btn_rank.set_bk_color(Color.black);
                }
            }
            */

            foreach (var result in scoreResponse.Results)
            {
                var user_id = result.PlayerId;
                Carrot_Box_Item item_p=this.box_list.create_item("item_p");
                item_p.set_title(result.PlayerName);
                item_p.set_tip("Score: "+result.Score.ToString());
                int rank=result.Rank;
                if (rank < this.icon_rank_player.Length)
                    item_p.set_icon_white(this.icon_rank_player[rank]);
                else
                    item_p.set_icon(this.carrot.user.icon_user_login_false);

                if (id_user_cur == user_id) item_p.GetComponent<Image>().color = this.carrot.get_color_highlight_blur(100);

                item_p.set_act(() =>
                {
                    Carrot_Box box_info_player = this.carrot.Create_Box("info_player");
                    box_info_player.set_icon(this.carrot.user.icon_user_info);
                    box_info_player.set_title(this.carrot.lang.Val("acc_info", "Account Information"));

                    Carrot_Box_Item info_name = box_info_player.create_item("info_name");
                    info_name.set_icon(this.carrot.user.icon_user_name);
                    info_name.set_title("Player Name");
                    info_name.set_tip(result.PlayerName);

                    Carrot_Box_Item info_date = box_info_player.create_item("info_date");
                    info_date.set_icon(this.carrot.icon_carrot_game);
                    info_date.set_title("Score");
                    info_date.set_tip(result.Score.ToString());

                    Carrot_Box_Item info_rank = box_info_player.create_item("info_rank");
                    info_rank.set_icon(this.carrot.game.icon_top_player);
                    info_rank.set_title("Rank");
                    info_rank.set_tip(result.Rank.ToString());

                    Carrot_Box_Item info_id = box_info_player.create_item("info_id");
                    info_id.set_icon(this.carrot.icon_carrot_advanced);
                    info_id.set_title("ID Player");
                    info_id.set_tip(result.PlayerId);
                });
            }

            if (this.carrot.type_app == TypeApp.Game)
            {
                box_list.update_gamepad_cosonle_control();
            }
        }

        IList<IDictionary> SortListByScoresKey(IList<IDictionary> list)
        {
            if (this.order_top_player == Carrot_game_rank_order.Ascending)
                return list.OrderBy(dict => int.Parse(dict["scores"].ToString())).ToList();
            else
                return list.OrderByDescending(dict => int.Parse(dict["scores"].ToString())).ToList();
        }

        public void Set_Order_By_Top_player(Carrot_game_rank_order order)
        {
            this.order_top_player = order;
        }

        [ContextMenu("Test update_scores_player")]
        public async void Test_update_scores_player()
        {
            await this.update_scores_playerAsync(UnityEngine.Random.Range(0, 20), 0);
        }

        public async Task update_scores_playerAsync(int scores, int type = 0)
        {
            this.rank_type_temp = type;
            try
            {
                await LeaderboardsService.Instance.AddPlayerScoreAsync(this.leaderboardId[type], scores);
                this.scoreResponse=null;
                Debug.Log($"Điểm số {scores} đã được gửi lên bảng xếp hạng {leaderboardId}.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Lỗi khi gửi điểm: {ex.Message}");
                Act_update_scores_fail(ex.Message);
            }
        }

        private void Act_update_scores_fail(string s_error)
        {
            this.carrot.Show_msg(this.carrot.lang.Val("top_player", "Player rankings"), s_error, Msg_Icon.Error);
        }
        #endregion
    }
}
