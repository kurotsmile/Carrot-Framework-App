using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Carrot
{
    public enum Carrot_app_type { all, app, game }
    public class Carrot_list_app
    {
        public string url_data = "https://raw.githubusercontent.com/kurotsmile/Database-Store-Json/refs/heads/main/app.json";
        private const int limit_app_random = 15;
        private const int limit_app_cache_fetch = 1000;
        private const string key_cache_data = "s_data_carrotapp_all";
        private const string key_cache_time = "s_data_carrotapp_all_time";
        private const long cache_seconds = 172800;
        private Carrot carrot;

        private Carrot_Box box_list_app;
        private Carrot_Box_Btn_Item btn_header_all;
        private Carrot_Box_Btn_Item btn_header_game;
        private Carrot_Box_Btn_Item btn_header_app;

        private Carrot_Window_Exit window_exit;

        private List<GameObject> list_btn_gamepad;
        private Carrot_app_type type = Carrot_app_type.all;

        private string s_data_carrotapp_all = "";

        public Carrot_list_app(Carrot carrot)
        {
            this.carrot = carrot;
            this.s_data_carrotapp_all = PlayerPrefs.GetString(key_cache_data);
        }

        [ContextMenu("show_list_carrot_app")]
        public void show_list_carrot_app()
        {
            this.carrot.play_sound_click();
            this.carrot.show_loading();
            this.load_data_app(this.load_list_by_data, get_data_app_exit_fail);
        }

        private void load_list_by_data(string s_data)
        {
            IList all_item = this.get_list_app(s_data);
            if (all_item == null || all_item.Count == 0)
            {
                this.carrot.hide_loading();
                return;
            }

            create_list_box_app();
            IList list_show = Json.Deserialize("[]") as IList;
            for (int i = 0; i < all_item.Count; i++)
            {
                IDictionary item = all_item[i] as IDictionary;
                if (item == null) continue;
                string s_type = this.get_data_value(item, "type");
                if (this.type == Carrot_app_type.all) list_show.Add(item);
                else if (this.type == Carrot_app_type.app && s_type == "app") list_show.Add(item);
                else if (this.type == Carrot_app_type.game && s_type == "game") list_show.Add(item);
            }

            list_show = this.carrot.get_tool().Shuffle_Ilist(list_show);
            for (int i = 0; i < list_show.Count; i++)
            {
                if (i >= limit_app_random) break;
                this.add_item_to_list_box(list_show[i] as IDictionary);
            }

            if (this.carrot.type_control != TypeControl.None)
            {
                this.box_list_app.update_gamepad_cosonle_control();
                this.box_list_app.update_color_table_row();
                this.carrot.game.set_index_button_gamepad_console(3);
                this.carrot.game.set_scrollRect_gamepad_consoles(this.box_list_app.UI.scrollRect);
            }
        }

        private void create_list_box_app()
        {
            this.carrot.hide_loading();
            if (this.box_list_app != null) this.box_list_app.close();
            this.box_list_app = this.carrot.Create_Box(this.carrot.lang.Val("list_app_carrot", "Applications from the developer"), this.carrot.icon_carrot);

            this.btn_header_all = box_list_app.create_btn_menu_header(this.carrot.icon_carrot_all_category);
            this.btn_header_all.set_act(() => this.act_btn_header_box(Carrot_app_type.all));
            if (this.type == Carrot_app_type.all) this.btn_header_all.set_icon_color(this.carrot.color_highlight);

            this.btn_header_app = box_list_app.create_btn_menu_header(this.carrot.icon_carrot_app);
            this.btn_header_app.set_act(() => this.act_btn_header_box(Carrot_app_type.app));
            if (this.type == Carrot_app_type.app) this.btn_header_app.set_icon_color(this.carrot.color_highlight);

            this.btn_header_game = box_list_app.create_btn_menu_header(this.carrot.icon_carrot_game);
            this.btn_header_game.set_act(() => this.act_btn_header_box(Carrot_app_type.game));
            if (this.type == Carrot_app_type.game) this.btn_header_game.set_icon_color(this.carrot.color_highlight);
        }

        private void add_item_to_list_box(IDictionary data_item)
        {
            string s_key_lang = this.carrot.lang.Get_key_lang();
            string s_key_store_public = this.carrot.store_public.ToString().ToLower();

            var s_link = this.get_data_value(data_item, s_key_store_public);
            if (s_link == "") return;
            var s_link_carrot = s_link;

            string s_id_app = this.get_data_value(data_item, "id_import", "id", "app_id");
            if (s_id_app == "") return;
            Carrot_Box_Item item_app = box_list_app.create_item(s_id_app);

            string s_name = this.get_data_value(data_item, "name_" + s_key_lang, "name_en", "id");
            item_app.set_title(s_name);
            item_app.set_tip(this.get_data_value(data_item, "type"));

            Carrot_Box_Btn_Item app_btn_download = item_app.create_item();
            app_btn_download.set_icon(this.carrot.icon_carrot_download);
            app_btn_download.set_color(this.carrot.color_highlight);
            app_btn_download.set_act(() => this.open_link(s_link));

            Carrot_Box_Btn_Item app_btn_share = item_app.create_item();
            app_btn_share.set_icon(this.carrot.sp_icon_share);
            app_btn_share.set_color(this.carrot.color_highlight);
            app_btn_share.set_act(() => this.open_link_share(s_link_carrot));

            string s_url_icon = this.get_data_value(data_item, "icon");
            if (s_url_icon != "")
            {
                Sprite icon_app = this.carrot.get_tool().get_sprite_to_playerPrefs(s_id_app);
                if (icon_app != null)
                {
                    item_app.set_icon_white(icon_app);
                }
                else
                {
                    this.carrot.get_img_and_save_playerPrefs(s_url_icon, item_app.img_icon, s_id_app);
                }
            }
            item_app.set_act(() => this.open_link(s_link));
        }

        public void show_list_app_where_exit()
        {
            this.carrot.play_sound_click();
            this.list_btn_gamepad = new List<GameObject>();
            GameObject window_exit = this.carrot.create_window(this.carrot.window_exit_prefab);
            window_exit.name = "window_exit";
            this.window_exit = window_exit.GetComponent<Carrot_Window_Exit>();
            this.window_exit.txt_exit_msg.text = this.carrot.lang.Val("exit_msg", "Are you sure you want to exit the application?\nPlease press the back button one more time to exit");
            this.window_exit.txt_title_app_other.text = this.carrot.lang.Val("exit_app_other", "Perhaps you will enjoy our other applications");
            this.window_exit.panel_list_app_other.SetActive(false);

            this.load_data_app(this.Act_load_app_where_exit_by_data, get_data_app_exit_fail);

            this.window_exit.UI.set_theme(this.carrot.color_highlight);
        }

        private void get_data_app_exit_fail(string s_error)
        {
            this.carrot.hide_loading();
            if (this.s_data_carrotapp_all != "") this.Act_load_app_where_exit_by_data(this.s_data_carrotapp_all);
        }

        private void Act_load_app_where_exit_by_data(string s_data)
        {
            int count_app_exit = 0;
            this.window_exit.panel_list_app_other.SetActive(true);
            IList list_app = this.get_list_app(s_data);
            if (list_app == null || list_app.Count == 0) return;

            list_app = this.carrot.get_tool().Shuffle_Ilist(list_app);

            for (int i = 0; i < list_app.Count; i++)
            {
                if (count_app_exit < 10) if (Add_item_app_exit(list_app[i] as IDictionary)) count_app_exit++;
            }

            this.list_btn_gamepad.Add(this.window_exit.UI.obj_gamepad[0]);
            this.list_btn_gamepad.Add(this.window_exit.UI.obj_gamepad[1]);
        }

        private bool Add_item_app_exit(IDictionary data_app_exit)
        {
            if (data_app_exit == null) return false;
            string s_id_app = this.get_data_value(data_app_exit, "id_import", "id", "name_en");
            if (s_id_app == "") return false;
            string s_icon = this.get_data_value(data_app_exit, "icon");
            if (s_icon != "")
            {
                var s_store = this.carrot.store_public.ToString().ToLower();
                var s_link = this.get_data_value(data_app_exit, s_store);
                if (s_link == "") return false;
                Carrot_Button_Item item_app_exit = this.window_exit.create_item();
                Sprite icon_app = this.carrot.get_tool().get_sprite_to_playerPrefs(s_id_app);
                if (icon_app != null)
                {
                    item_app_exit.set_icon(icon_app);
                }
                else
                {
                    this.carrot.get_img_and_save_playerPrefs(s_icon, item_app_exit.img_icon, s_id_app);
                }
                item_app_exit.set_act_click(() => this.open_link(s_link));
            }
            return true;
        }

        private void open_link(string s_link)
        {
            Application.OpenURL(s_link);
        }

        public void open_link_share(string s_link)
        {
            this.carrot.show_share(s_link, this.carrot.lang.Val("share_tip", "Choose the platform below to share this great app with your friends or others"));
        }

        private void act_btn_header_box(Carrot_app_type type_show)
        {
            this.type = type_show;
            show_list_carrot_app();
        }

        public void ShowAds()
        {
            this.carrot.show_loading();
            this.load_data_app(this.ShowAdsByData, get_data_app_exit_fail);
        }

        private void ShowAdsByData(string sData)
        {
            this.carrot.hide_loading();
            GameObject objWindowAds = carrot.create_window(carrot.WindowAdsPrefab);
            CarrotAds carrotAds = objWindowAds.GetComponent<CarrotAds>();
            carrotAds.OnLoad(carrot,sData);
        }

        private void load_data_app(UnityEngine.Events.UnityAction<string> act_done, UnityEngine.Events.UnityAction<string> act_fail = null)
        {
            if (this.check_cache_app_valid())
            {
                act_done?.Invoke(this.s_data_carrotapp_all);
                return;
            }

            if (this.carrot.hub == null)
            {
                this.load_data_app_old(act_done, act_fail);
                return;
            }

            Dictionary<string, object> filters = new()
            {
                { "page", 1 },
                { "limit", limit_app_cache_fetch },
                { "status", "public" },
                { "order_key", "created_at" },
                { "order_type", "DESC" }
            };

            this.carrot.hub.ReadTable("app", filters, (s_data) =>
            {
                string s_data_normalized = this.normalize_app_data(s_data);
                if (s_data_normalized != "")
                {
                    this.save_cache_app(s_data_normalized);
                    act_done?.Invoke(s_data_normalized);
                }
                else
                {
                    this.load_data_app_old(act_done, act_fail);
                }
            }, (s_error) =>
            {
                this.load_data_app_old(act_done, act_fail);
            });
        }

        private void load_data_app_old(UnityEngine.Events.UnityAction<string> act_done, UnityEngine.Events.UnityAction<string> act_fail = null)
        {
            this.carrot.Get_Data(this.url_data, (s_data) =>
            {
                string s_data_normalized = this.normalize_app_data(s_data);
                if (s_data_normalized != "")
                {
                    this.save_cache_app(s_data_normalized);
                    act_done?.Invoke(s_data_normalized);
                }
                else if (this.s_data_carrotapp_all != "")
                {
                    act_done?.Invoke(this.s_data_carrotapp_all);
                }
                else
                {
                    act_fail?.Invoke("Data app is empty");
                }
            }, (s_error) =>
            {
                if (this.s_data_carrotapp_all != "")
                    act_done?.Invoke(this.s_data_carrotapp_all);
                else
                    act_fail?.Invoke(s_error);
            });
        }

        private IList get_list_app(string s_data)
        {
            object data = Json.Deserialize(s_data);
            if (data is IList list_data) return list_data;

            IDictionary obj_data = data as IDictionary;
            if (obj_data == null) return null;
            if (obj_data.Contains("all_item")) return obj_data["all_item"] as IList;
            if (obj_data.Contains("items")) return obj_data["items"] as IList;
            if (obj_data.Contains("results")) return obj_data["results"] as IList;
            return null;
        }

        private string get_data_value(IDictionary data, params string[] keys)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                string key = keys[i];
                if (!data.Contains(key) || data[key] == null) continue;
                string value = data[key].ToString().Trim();
                if (value != "") return value;
            }
            return "";
        }

        private string normalize_app_data(string s_data)
        {
            IList list_raw = this.get_list_app(s_data);
            if (list_raw == null || list_raw.Count == 0) return "";

            IList list_normalized = Json.Deserialize("[]") as IList;
            for (int i = 0; i < list_raw.Count; i++)
            {
                IDictionary item_raw = list_raw[i] as IDictionary;
                if (item_raw == null) continue;

                string s_id = this.get_data_value(item_raw, "id_import", "id", "app_id");
                if (s_id == "") continue;

                IDictionary item = Json.Deserialize("{}") as IDictionary;
                item["id_import"] = s_id;
                item["id"] = s_id;
                item["name_en"] = this.get_data_value(item_raw, "name_en", "id", "id_import", "app_id");
                item["describe_en"] = this.get_data_value(item_raw, "describe_en", "decription", "description");
                item["type"] = this.get_data_value(item_raw, "type");
                item["icon"] = this.carrot.GetUrlFile(this.get_data_value(item_raw, "icon"));
                item["google_play"] = this.get_data_value(item_raw, "google_play");
                item["microsoft_store"] = this.get_data_value(item_raw, "microsoft_store");
                item["amazon_app_store"] = this.get_data_value(item_raw, "amazon_app_store");
                item["huawei_store"] = this.get_data_value(item_raw, "huawei_store");
                item["itch"] = this.get_data_value(item_raw, "itch");
                item["uptodown"] = this.get_data_value(item_raw, "uptodown");
                item["simmer"] = this.get_data_value(item_raw, "simmer");
                item["github"] = this.get_data_value(item_raw, "github");
                item["youtube_link"] = this.get_data_value(item_raw, "youtube_link");
                item["category"] = this.get_data_value(item_raw, "category");
                item["priority"] = this.get_data_value(item_raw, "priority");
                item["status"] = this.get_data_value(item_raw, "status");
                item["apk_file"] = this.carrot.GetUrlFile(this.get_data_value(item_raw, "apk_file"));
                item["exe_file"] = this.carrot.GetUrlFile(this.get_data_value(item_raw, "exe_file"));
                item["ipa_file"] = this.carrot.GetUrlFile(this.get_data_value(item_raw, "ipa_file"));
                item["deb_file"] = this.carrot.GetUrlFile(this.get_data_value(item_raw, "deb_file"));
                item["dmg_file"] = this.carrot.GetUrlFile(this.get_data_value(item_raw, "dmg_file"));

                foreach (object key_obj in item_raw.Keys)
                {
                    string key = key_obj.ToString();
                    if (!key.StartsWith("name_") && !key.StartsWith("describe_")) continue;
                    item[key] = item_raw[key_obj];
                }

                list_normalized.Add(item);
            }

            if (list_normalized.Count == 0) return "";

            IDictionary data_normalized = Json.Deserialize("{}") as IDictionary;
            data_normalized["all_item"] = list_normalized;
            return Json.Serialize(data_normalized);
        }

        private bool check_cache_app_valid()
        {
            if (this.s_data_carrotapp_all == "") return false;
            if (this.carrot.is_offline()) return true;

            string s_cache_time = PlayerPrefs.GetString(key_cache_time, "");
            if (s_cache_time == "") return false;
            if (!long.TryParse(s_cache_time, out long cache_time)) return false;

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return now - cache_time <= cache_seconds;
        }

        private void save_cache_app(string s_data)
        {
            this.s_data_carrotapp_all = s_data;
            PlayerPrefs.SetString(key_cache_data, s_data);
            PlayerPrefs.SetString(key_cache_time, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
            PlayerPrefs.Save();
        }
    }
}
