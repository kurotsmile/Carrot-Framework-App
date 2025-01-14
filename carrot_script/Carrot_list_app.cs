using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Carrot
{
    public enum Carrot_app_type { all, app, game }
    public class Carrot_list_app
    {
        public string url_data = "https://raw.githubusercontent.com/kurotsmile/Database-Store-Json/refs/heads/main/app.json";
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
            if (this.carrot.is_offline()) this.s_data_carrotapp_all = PlayerPrefs.GetString("s_data_carrotapp_all");
        }

        [ContextMenu("show_list_carrot_app")]
        public void show_list_carrot_app()
        {
            this.carrot.play_sound_click();
            this.carrot.show_loading();
            if (this.s_data_carrotapp_all == ""){
                this.carrot.Get_Data(this.url_data, (s_data)=>{
                    Debug.Log("Get new Data list app");
                    this.s_data_carrotapp_all=s_data;
                    load_list_by_data(s_data);
                },get_data_app_exit_fail);
            }else{
                Debug.Log("Load list app from cache");
                this.load_list_by_data(this.s_data_carrotapp_all);
            }            
        }

        private void load_list_by_data(string s_data)
        {
            IDictionary data = (IDictionary)Json.Deserialize(s_data);
            create_list_box_app();
            IList all_item = data["all_item"] as IList;
            this.carrot.get_tool().Shuffle_Ilist(all_item);
            for (int i = 0; i < all_item.Count; i++)
            {
                IDictionary item = all_item[i] as IDictionary;
                if (this.type == Carrot_app_type.all) add_item_to_list_box(item);
                else if (this.type == Carrot_app_type.app && item["type"].ToString() == "app") add_item_to_list_box(item);
                else if (this.type == Carrot_app_type.game && item["type"].ToString() == "game") add_item_to_list_box(item);
            }

            if (this.carrot.type_control != TypeControl.None)
            {
                this.carrot.game.set_list_button_gamepad_console(this.list_btn_gamepad);
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
            this.box_list_app = this.carrot.Create_Box();
            box_list_app.set_icon(this.carrot.icon_carrot);
            box_list_app.set_title(this.carrot.lang.Val("list_app_carrot", "Applications from the developer"));

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

            var s_link = "";
            if (data_item[s_key_store_public] != null) s_link = data_item[s_key_store_public].ToString();
            if (s_link == "") return;
            var s_link_carrot = s_link;

            string s_id_app = data_item["id_import"].ToString();
            Carrot_Box_Item item_app = box_list_app.create_item(s_id_app);

            string s_name = "";
            if (data_item["name_" + s_key_lang] != null) s_name = data_item["name_" + s_key_lang].ToString();
            if (s_name == "")
            {
                if (data_item["name_en"] != null) s_name = data_item["name_en"].ToString();
            }
            item_app.set_title(s_name);
            if (data_item["type"] != null) item_app.set_tip(data_item["type"].ToString());

            Carrot_Box_Btn_Item app_btn_download = item_app.create_item();
            app_btn_download.set_icon(this.carrot.icon_carrot_download);
            app_btn_download.set_color(this.carrot.color_highlight);
            app_btn_download.set_act(() => this.open_link(s_link));

            Carrot_Box_Btn_Item app_btn_share = item_app.create_item();
            app_btn_share.set_icon(this.carrot.sp_icon_share);
            app_btn_share.set_color(this.carrot.color_highlight);
            app_btn_share.set_act(() => this.open_link_share(s_link_carrot));

            if (data_item["icon"] != null)
            {
                Sprite icon_app = this.carrot.get_tool().get_sprite_to_playerPrefs(s_id_app);
                if (icon_app != null)
                {
                    item_app.set_icon_white(icon_app);
                }
                else
                {
                    string s_url_icon = data_item["icon"].ToString();
                    if (s_url_icon != "") this.carrot.get_img_and_save_playerPrefs(s_url_icon, item_app.img_icon, s_id_app);
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

            if (this.s_data_carrotapp_all == "")
                this.carrot.Get_Data(this.url_data, get_data_app_exit_done, get_data_app_exit_fail);
            else
                this.Act_load_app_where_exit_by_data(this.s_data_carrotapp_all);

            this.window_exit.UI.set_theme(this.carrot.color_highlight);
        }

        private void get_data_app_exit_done(string s_data)
        {
            this.s_data_carrotapp_all = s_data;
            this.Act_load_app_where_exit_by_data(s_data);
        }

        private void get_data_app_exit_fail(string s_error)
        {
            if (this.s_data_carrotapp_all != "") this.Act_load_app_where_exit_by_data(this.s_data_carrotapp_all);
        }

        private void Act_load_app_where_exit_by_data(string s_data)
        {
            IDictionary data = (IDictionary)Json.Deserialize(s_data);
            int count_app_exit = 0;
            this.window_exit.panel_list_app_other.SetActive(true);

            IList list_app = data["all_item"] as IList;

            for (int i = 0; i < data.Count; i++) list_app.Add(data[i] as IDictionary);

            list_app = this.carrot.get_tool().Shuffle_Ilist(list_app);

            for (int i = 0; i < list_app.Count; i++)
            {
                if (count_app_exit < 10) if(Add_item_app_exit(list_app[i] as IDictionary)) count_app_exit++;
            }

            this.list_btn_gamepad.Add(this.window_exit.UI.obj_gamepad[0]);
            this.list_btn_gamepad.Add(this.window_exit.UI.obj_gamepad[1]);
        }

        private bool Add_item_app_exit(IDictionary data_app_exit)
        {
            if(data_app_exit==null) return false;
            if(data_app_exit["name_en"]==null) return false;
            string s_id_app = data_app_exit["name_en"].ToString();
            if (data_app_exit["icon"] != null)
            {
                var s_store = this.carrot.store_public.ToString().ToLower();
                var s_link = "";
                if (data_app_exit[s_store] != null) s_link = data_app_exit[s_store].ToString();
                if(s_link=="") return false;
                var s_link_carrot = s_link;
                Carrot_Button_Item item_app_exit = this.window_exit.create_item();
                Sprite icon_app = this.carrot.get_tool().get_sprite_to_playerPrefs(s_id_app);
                if (icon_app != null)
                {
                    item_app_exit.set_icon(icon_app);
                }
                else
                {
                    string s_url_icon = data_app_exit["icon"].ToString();
                    if (s_url_icon != "") this.carrot.get_img_and_save_playerPrefs(s_url_icon, item_app_exit.img_icon, s_id_app);
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
    }
}