using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Carrot
{
    public class Carrot_lang : MonoBehaviour
    {
        public Sprite icon;
        public Sprite sp_lang_default_en;
        private Carrot carrot;
        private string s_lang_key;
        private string s_key_lang_temp = "";
        private string s_name_lang_temp = "";

        private UnityAction<string> act_after_selecting_lang = null;
        private Carrot_Box box_lang;
        private IDictionary data_lang_value = null;

        private Transform tr_item_lang_systemLanguage = null;
        private bool is_load_emp_after_sel_lang = true;
        private IList DataListCountry = null;
        private IDictionary DataFrameworkUnity = null;
        private IDictionary DataFrameworkUnity_en = null;
        private IDictionary DataLangApp = null;
        private IDictionary DataLangApp_en = null;
        private string NameFileCustomerFrw = "FrameworkLangCustomer";
        private const string CountryCacheDataKey = "carrot_country_cache_data";
        private const string CountryCacheTimeKey = "carrot_country_cache_time";
        private const double CountryCacheDays = 7d;
        private bool is_loading_country_data = false;

        public void On_load(Carrot carrot)
        {
            this.carrot = carrot;
            this.s_lang_key = PlayerPrefs.GetString("lang", "en");

            TextAsset DataFrwTex = Resources.Load<TextAsset>(this.NameFileCustomerFrw);
            if (DataFrwTex != null)
            {
                this.DataFrameworkUnity = Json.Deserialize(DataFrwTex.text) as IDictionary;
            }
            else
            {
                DataFrwTex = Resources.Load<TextAsset>("FrameworkLang");
                this.DataFrameworkUnity = Json.Deserialize(DataFrwTex.text) as IDictionary;
            }

            if (this.carrot.FileNameLangApp != "")
            {
                TextAsset DataLanAppText = Resources.Load<TextAsset>(this.carrot.FileNameLangApp);
                if (DataLanAppText != null)
                {
                    this.DataLangApp = Json.Deserialize(DataLanAppText.text) as IDictionary;
                }
                else
                {
                    this.DataLangApp = Json.Deserialize("{}") as IDictionary;
                    this.DataLangApp["en"] = Json.Deserialize("{}") as IDictionary;
                }
            }

            this.Try_load_country_data_from_cache();
            this.LoadData();
            this.Load_icon_lang();
            this.Refresh_country_data_if_needed(false);
            this.Load_lang_emp();
        }

        private void LoadData()
        {
            this.data_lang_value = DataFrameworkUnity[this.s_lang_key] as IDictionary;

            if (this.DataLangApp != null)
            {
                if (this.DataLangApp[this.s_lang_key] != null)
                {
                    IDictionary data_lang_cur = this.DataLangApp[this.s_lang_key] as IDictionary;
                    foreach (var key in data_lang_cur.Keys) this.data_lang_value[key.ToString()] = data_lang_cur[key.ToString()].ToString();
                }
            }
        }

        private void Load_icon_lang()
        {
            Sprite sp_lang_icon = carrot.get_tool().get_sprite_to_playerPrefs("icon_" + this.s_lang_key) ?? this.sp_lang_default_en;
            if (sp_lang_icon == this.sp_lang_default_en)
            {
                IDictionary data_lang = this.Find_country_by_key(this.s_lang_key);
                Image img_target = this.Get_lang_icon_target();
                if (data_lang != null && img_target != null && data_lang["icon"] != null && data_lang["icon"].ToString() != "")
                {
                    this.carrot.get_img_and_save_playerPrefs(data_lang["icon"].ToString(), img_target, "icon_" + this.s_lang_key, tex =>
                    {
                        this.Load_icon_lang();
                    });
                    sp_lang_icon = img_target.sprite ?? this.sp_lang_default_en;
                }
            }
            if (this.carrot.emp_show_lang != null && this.carrot.emp_show_lang.List_img_change_icon_lang != null)
                for (int i = 0; i < this.carrot.emp_show_lang.List_img_change_icon_lang.Length; i++)
                    this.carrot.emp_show_lang.List_img_change_icon_lang[i].sprite = sp_lang_icon;
        }

        public Sprite Get_sp_lang_cur()
        {
            Sprite sp_lang_icon = this.carrot.get_tool().get_sprite_to_playerPrefs("icon_" + this.s_lang_key);
            if (sp_lang_icon == null)
                return this.sp_lang_default_en;
            else
                return sp_lang_icon;
        }

        public void Show_list_lang()
        {
            this.carrot.show_loading();
            this.carrot.play_sound_click();
            this.act_after_selecting_lang = null;

            if (this.DataListCountry == null || this.DataListCountry.Count == 0)
            {
                this.Try_load_country_data_from_cache();
            }

            if (this.DataListCountry != null && this.DataListCountry.Count > 0)
            {
                this.Load_list_lang_by_data(DataListCountry);
                this.Refresh_country_data_if_needed(this.box_lang != null);
            }
            else
            {
                this.Refresh_country_data_if_needed(true);
                if (this.carrot.is_offline()) this.carrot.Show_msg(this.carrot.lang.Val("sel_lang_app", "Choose your language and country"), this.carrot.lang.Val("list_none", "List is empty, no items found!"));
            }
        }

        private void Load_list_lang_by_data(IList all_item)
        {
            this.carrot.hide_loading();
            if(this.DataLangApp!=null) this.DataLangApp_en = this.DataLangApp["en"] as IDictionary;
            if (this.box_lang != null) this.box_lang.close();
            this.box_lang = this.carrot.Create_Box(this.carrot.lang.Val("sel_lang_app", "Choose your language and country"), this.icon);
            for (int i = 0; i < all_item.Count; i++)
            {
                var index_item = i;
                IDictionary lang = all_item[i] as IDictionary;
                var data_item = lang;
                Carrot_Box_Item item_lang = this.Add_item_to_list_box(lang);
                if (this.carrot.model_app == ModelApp.Develope)
                {
                    string s_key = lang["key"].ToString();
                    Carrot_Box_Btn_Item btn_frw = item_lang.create_item();
                    btn_frw.set_icon(this.carrot.icon_carrot_all_category);
                    btn_frw.set_icon_color(Color.white);

                    if (this.DataFrameworkUnity[s_key] != null)
                    {
                        btn_frw.set_color(this.carrot.color_highlight);
                        btn_frw.set_act(() =>
                        {
                            this.DataFrameworkUnity_en = this.DataFrameworkUnity["en"] as IDictionary;
                            this.BoxEditData(this.DataFrameworkUnity[s_key] as IDictionary, s_key, true);
                        });
                    }
                    else
                    {
                        btn_frw.set_color(Color.black);
                    }

                    if (this.DataLangApp != null)
                    {
                        Carrot_Box_Btn_Item btn_lapp = item_lang.create_item();
                        btn_lapp.set_icon(this.carrot.icon_carrot_app);
                        btn_lapp.set_icon_color(Color.white);

                        if (this.DataLangApp[s_key] != null)
                        {
                            int count_key_lang_en=DataLangApp_en.Count;
                            int count_key_lang_cur = 0;
                            IDictionary data_lang_cur =DataLangApp[s_key] as IDictionary;
                            foreach (var key in DataLangApp_en.Keys)
                            {
                                if (data_lang_cur[key.ToString()] != null)
                                {
                                    if(data_lang_cur[key.ToString()].ToString().Trim()!="") count_key_lang_cur++;
                                }
                            }
                            if(count_key_lang_en == count_key_lang_cur)
                                btn_lapp.set_color(this.carrot.color_highlight);
                            else
                                btn_lapp.set_color(Color.red);
                        }
                        else
                            btn_lapp.set_color(Color.black);

                        btn_lapp.set_act(() =>
                        {
                            this.BoxEditData(this.DataLangApp[s_key] as IDictionary, s_key, false);
                        });
                    }

                    Carrot_Box_Btn_Item btn_edit = item_lang.create_item();
                    btn_edit.set_icon(this.carrot.user.icon_user_edit);
                    btn_edit.set_icon_color(Color.white);
                    btn_edit.set_color(this.carrot.color_highlight);
                    btn_edit.set_act(() =>
                    {
                        data_item["index"] = index_item;
                        this.Box_Edit_Or_Add(data_item);
                    });

                    Carrot_Box_Btn_Item btn_del = item_lang.create_item();
                    btn_del.set_icon(this.carrot.sp_icon_del_data);
                    btn_del.set_icon_color(Color.white);
                    btn_del.set_color(this.carrot.color_highlight);
                    btn_del.set_act(() =>
                    {
                        this.DataListCountry.RemoveAt(index_item);
                        this.UpdateDataListCountry();
                        this.Show_list_lang();
                        this.carrot.Show_msg("Delete Item '" + item_lang.txt_name.text + "' Success!");
                    });
                }
            }
            ;

            if (this.tr_item_lang_systemLanguage != null) this.tr_item_lang_systemLanguage.SetSiblingIndex(0);
            if (this.carrot.type_control != TypeControl.None) this.carrot.game.set_list_button_gamepad_console(this.box_lang.UI.get_list_btn());

            if(carrot.model_app == ModelApp.Develope)
            {
                this.box_lang.create_btn_menu_header(this.carrot.icon_carrot_add).set_act(() =>
                {
                    this.carrot.play_sound_click();
                    this.Box_Edit_Or_Add();
                });
            }
        }

        private void Box_Edit_Or_Add(IDictionary data = null)
        {
            if (this.box_lang != null) this.box_lang.close();
            this.box_lang = this.carrot.Create_Box();
            if (data != null)
            {
                this.box_lang.set_title("Edit Country");
                this.box_lang.set_icon(this.carrot.user.icon_user_edit);
            }
            else
            {
                this.box_lang.set_title("Add Country");
                this.box_lang.set_icon(this.carrot.icon_carrot_add);
            }

            Carrot_Box_Item item_key = this.box_lang.create_item();
            item_key.set_icon(this.carrot.user.icon_user_register);
            item_key.set_type(Box_Item_Type.box_value_input);
            item_key.set_title("Key");
            item_key.set_tip("Key lang country");
            if (data != null) item_key.set_val(data["key"].ToString());

            Carrot_Box_Item item_name = this.box_lang.create_item();
            item_name.set_icon(this.carrot.user.icon_user_name);
            item_name.set_type(Box_Item_Type.box_value_input);
            item_name.set_title("Name");
            item_name.set_tip("Name lang country");
            if (data != null) item_name.set_val(data["name"].ToString());

            Carrot_Box_Item item_icon = this.box_lang.create_item();
            item_icon.set_icon(this.carrot.user.icon_user_info);
            item_icon.set_type(Box_Item_Type.box_value_input);
            item_icon.set_title("Icon");
            item_icon.set_tip("Icon lang country");
            if (data != null) item_icon.set_val(data["icon"].ToString());

            Carrot_Box_Btn_Panel panel_btn = this.box_lang.create_panel_btn();
            Carrot_Button_Item btn_done = panel_btn.create_btn();
            btn_done.set_icon_white(this.carrot.icon_carrot_done);
            btn_done.set_bk_color(this.carrot.color_highlight);
            btn_done.set_label_color(Color.white);
            btn_done.set_label("Done");
            btn_done.set_act_click(() =>
            {
                this.carrot.play_sound_click();
                IDictionary data_new = Json.Deserialize("{}") as IDictionary;
                data_new["key"] = item_key.get_val();
                data_new["name"] = item_name.get_val();
                data_new["icon"] = item_icon.get_val();

                if (data == null)
                {
                    this.DataListCountry.Add(data_new);
                    this.UpdateDataListCountry();
                    this.Show_list_lang();
                }
                else
                {
                    int index_item = int.Parse(data["index"].ToString());
                    this.DataListCountry[index_item] = data_new;
                    this.UpdateDataListCountry();
                    this.Show_list_lang();
                }
            });

            Carrot_Button_Item btn_cancel = panel_btn.create_btn();
            btn_cancel.set_icon_white(this.carrot.icon_carrot_cancel);
            btn_cancel.set_bk_color(this.carrot.color_highlight);
            btn_cancel.set_label_color(Color.white);
            btn_cancel.set_label("Cancel");
            btn_cancel.set_act_click(this.box_lang.close);
        }

        private void UpdateDataListCountry()
        {
            this.Save_country_cache(this.DataListCountry);
        }

        private void UpdateDataLangApp()
        {
            this.carrot.get_tool().save_file("Resources/" + this.carrot.FileNameLangApp + ".json", Json.Serialize(this.DataLangApp));
        }

        private void UpdateDataFrw()
        {
            this.carrot.get_tool().save_file("Resources/" + this.NameFileCustomerFrw + ".json", Json.Serialize(this.DataFrameworkUnity));
        }

        public void Show_list_lang(UnityAction<string> fnc_after_sel_lang)
        {
            this.Show_list_lang();
            this.is_load_emp_after_sel_lang = true;
            this.act_after_selecting_lang = fnc_after_sel_lang;
        }

        public void Show_list_lang(UnityAction<string> fnc_after_sel_lang, bool load_lang)
        {
            this.Show_list_lang();
            this.is_load_emp_after_sel_lang = load_lang;
            this.act_after_selecting_lang = fnc_after_sel_lang;
        }

        private Carrot_Box_Item Add_item_to_list_box(IDictionary data_lang)
        {
            var s_key = data_lang["key"].ToString();
            string s_key_lang = data_lang["key"].ToString();
            Carrot_Box_Item item_lang = this.box_lang.create_item(s_key);
            item_lang.set_icon(this.icon);
            if (data_lang["name"] != null) item_lang.set_title(data_lang["name"].ToString());
            item_lang.set_tip(s_key_lang);

            string s_id_icon_lang = "icon_" + s_key_lang;
            Sprite sp_icon_lang = this.carrot.get_tool().get_sprite_to_playerPrefs(s_id_icon_lang);
            if (sp_icon_lang != null)
            {
                item_lang.set_icon_white(sp_icon_lang);
            }
            else
            {
                if (data_lang["icon"] != null)
                {
                    if (data_lang["icon"].ToString() != "") this.carrot.get_img_and_save_playerPrefs(data_lang["icon"].ToString(), item_lang.img_icon, s_id_icon_lang);
                }
            }

            if (s_key_lang == this.s_lang_key) item_lang.GetComponent<Image>().color = this.carrot.get_color_highlight_blur(50);

            if (Application.systemLanguage.ToString() == data_lang["name"].ToString())
            {
                Carrot_Box_Btn_Item btn_sugger_lang = item_lang.create_item();
                btn_sugger_lang.set_icon(this.carrot.icon_carrot_location);
                btn_sugger_lang.set_color(this.carrot.color_highlight);
                Destroy(btn_sugger_lang.GetComponent<Button>());
                this.tr_item_lang_systemLanguage = item_lang.transform;
            }

            item_lang.set_act(() => {
                this.s_name_lang_temp = data_lang["name"].ToString();
                this.Select_lang(s_key);
            });
            return item_lang;
        }

        private Image Get_lang_icon_target()
        {
            if (this.carrot.emp_show_lang == null || this.carrot.emp_show_lang.List_img_change_icon_lang == null) return null;
            for (int i = 0; i < this.carrot.emp_show_lang.List_img_change_icon_lang.Length; i++)
            {
                if (this.carrot.emp_show_lang.List_img_change_icon_lang[i] != null)
                    return this.carrot.emp_show_lang.List_img_change_icon_lang[i];
            }
            return null;
        }

        private IDictionary Find_country_by_key(string s_key)
        {
            if (this.DataListCountry == null) return null;
            for (int i = 0; i < this.DataListCountry.Count; i++)
            {
                IDictionary data_lang = this.DataListCountry[i] as IDictionary;
                if (data_lang == null || data_lang["key"] == null) continue;
                if (string.Equals(data_lang["key"].ToString(), s_key, StringComparison.OrdinalIgnoreCase))
                    return data_lang;
            }
            return null;
        }

        private bool Is_country_cache_fresh()
        {
            string s_cache_time = PlayerPrefs.GetString(CountryCacheTimeKey, "");
            if (s_cache_time == "") return false;
            if (!DateTime.TryParse(s_cache_time, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime cache_time)) return false;
            return (DateTime.UtcNow - cache_time) <= TimeSpan.FromDays(CountryCacheDays);
        }

        private void Save_country_cache(IList list_country)
        {
            if (list_country == null) return;
            PlayerPrefs.SetString(CountryCacheDataKey, Json.Serialize(list_country));
            PlayerPrefs.SetString(CountryCacheTimeKey, DateTime.UtcNow.ToString("o"));
            PlayerPrefs.Save();
        }

        private bool Try_load_country_data_from_cache()
        {
            string s_cache = PlayerPrefs.GetString(CountryCacheDataKey, "");
            if (s_cache == "") return false;
            IList list_country = this.Normalize_country_list(Json.Deserialize(s_cache));
            if (list_country == null || list_country.Count == 0) return false;
            this.DataListCountry = list_country;
            return true;
        }

        private IList Normalize_country_list(object data_raw)
        {
            IList source_list = data_raw as IList;
            if (source_list == null)
            {
                IDictionary data_obj = data_raw as IDictionary;
                if (data_obj != null)
                {
                    if (data_obj["items"] is IList items) source_list = items;
                    else if (data_obj["data"] is IList data) source_list = data;
                }
            }

            IList list_country = Json.Deserialize("[]") as IList;
            if (source_list == null) return list_country;

            for (int i = 0; i < source_list.Count; i++)
            {
                IDictionary row = source_list[i] as IDictionary;
                if (row == null) continue;

                IDictionary item = Json.Deserialize("{}") as IDictionary;
                foreach (DictionaryEntry entry in row) item[entry.Key.ToString()] = entry.Value;

                string s_key = "";
                if (row["id"] != null) s_key = row["id"].ToString();
                else if (row["key"] != null) s_key = row["key"].ToString();
                else if (row["key_country"] != null) s_key = row["key_country"].ToString();

                item["key"] = s_key;
                if (item["name"] == null) item["name"] = s_key;
                if (item["icon"] == null) item["icon"] = "";
                if (item["icon"].ToString() != "") item["icon"] = this.carrot.GetUrlFile(item["icon"].ToString());
                list_country.Add(item);
            }

            return list_country;
        }

        private void Refresh_country_data_if_needed(bool reload_ui_when_done)
        {
            if (this.carrot == null || this.carrot.hub == null || this.carrot.is_offline()) return;
            if (this.is_loading_country_data) return;
            if (!reload_ui_when_done && this.DataListCountry != null && this.DataListCountry.Count > 0 && this.Is_country_cache_fresh()) return;

            this.is_loading_country_data = true;
            Dictionary<string, object> filters = new()
            {
                { "limit", 300 },
                { "page", 1 },
                { "order_key", "name" },
                { "order_type", "ASC" }
            };

            this.carrot.hub.ReadTable("country", filters, s_data =>
            {
                this.is_loading_country_data = false;
                IList list_country = this.Normalize_country_list(Json.Deserialize(s_data));
                if (list_country == null || list_country.Count == 0)
                {
                    if (reload_ui_when_done) this.carrot.hide_loading();
                    return;
                }

                this.DataListCountry = list_country;
                this.Save_country_cache(list_country);
                this.Load_icon_lang();

                if (reload_ui_when_done || this.box_lang != null)
                    this.Load_list_lang_by_data(this.DataListCountry);
            }, s_error =>
            {
                this.is_loading_country_data = false;
                if (reload_ui_when_done && this.DataListCountry != null && this.DataListCountry.Count > 0)
                    this.Load_list_lang_by_data(this.DataListCountry);
                else if (reload_ui_when_done)
                    this.carrot.hide_loading();
            });
        }

        public void Load_lang_emp()
        {
            if (this.carrot.emp_show_lang != null) this.carrot.emp_show_lang.load_lang_emp(this);
        }

        public string Get_key_lang()
        {
            return this.s_lang_key;
        }

        public string Get_Name_Lang()
        {
            return PlayerPrefs.GetString("lang_name", "English");
        }

        public void Select_lang(string s_key)
        {
            if (this.is_load_emp_after_sel_lang)
            {
                this.carrot.show_loading();
                this.s_key_lang_temp = s_key;
                Act_sel_lang_done();
            }
            else
            {
                this.act_after_selecting_lang?.Invoke(s_key);
                this.box_lang?.close();
            }
        }

        private void Act_sel_lang_done()
        {
            if (this.DataFrameworkUnity[this.s_key_lang_temp] != null)
            {
                this.Change_lang(this.s_key_lang_temp);
            }
            else
            {
                this.Change_lang(this.s_key_lang_temp);
            }
        }

        private void BoxEditData(IDictionary dataEdit, string key_lang_edit, bool is_frw)
        {
            Carrot_Box box_data = this.carrot.Create_Box();
            IDictionary dataField = null;
            if (is_frw)
            {
                box_data.set_title("Edit framework lang data");
                dataField = this.DataFrameworkUnity_en;
            }
            else
            {
                box_data.set_title("Edit app lang data");
                dataField = this.DataLangApp_en;
            }

            box_data.set_icon(this.carrot.user.icon_user_edit);


            foreach (var key in dataField.Keys)
            {
                Carrot_Box_Item item_data = box_data.create_item();
                item_data.gameObject.name = key.ToString();
                item_data.set_icon(this.carrot.icon_carrot_database);
                item_data.set_title(key.ToString());
                item_data.set_type(Box_Item_Type.box_value_input);
                string s_tip = "";
                if (is_frw)
                {
                    if (this.DataFrameworkUnity_en[key] != null)
                    {
                        s_tip = this.DataFrameworkUnity_en[key].ToString();
                        item_data.set_tip(this.DataFrameworkUnity_en[key].ToString());
                    }
                }
                else
                {
                    s_tip = this.DataLangApp_en[key].ToString();
                    if (this.DataLangApp_en[key] != null) item_data.set_tip(this.DataLangApp_en[key].ToString());
                }
                if (dataEdit!=null&&dataEdit[key] != null) item_data.set_val(dataEdit[key].ToString());

                this.AddBtnCopyItemDataBox(item_data);
                if (key_lang_edit == "en")
                {
                    this.AddBtnDelItemDataBox(item_data);
                }
                else
                {
                    if (s_tip != "")
                    {
                        Carrot_Box_Btn_Item btrn_translate = item_data.create_item();
                        btrn_translate.set_icon(this.carrot.lang.icon);
                        btrn_translate.set_icon_color(Color.white);
                        btrn_translate.set_color(this.carrot.color_highlight);
                        btrn_translate.set_act(() =>
                        {
                            this.carrot.play_sound_click();
                            btrn_translate.set_color(Color.black);
                            Application.OpenURL("https://translate.google.com/?hl=vi&sl=en&tl=" + key_lang_edit + "&text=" + UnityWebRequest.EscapeURL(s_tip) + "&op=translate");
                        });
                    }
                }

            }

            Carrot_Box_Btn_Panel panel = box_data.create_panel_btn();
            Carrot_Button_Item btn_done = panel.create_btn();
            btn_done.set_icon_white(this.carrot.icon_carrot_done);
            btn_done.set_bk_color(this.carrot.color_highlight);
            btn_done.set_label("Done");
            btn_done.set_label_color(Color.white);
            btn_done.set_act_click(() =>
            {
                IDictionary data_u = Json.Deserialize("{}") as IDictionary;
                foreach (Transform tr in box_data.area_all_item)
                {
                    if (tr.gameObject.GetComponent<Carrot_Box_Item>()) data_u[tr.gameObject.name] = tr.gameObject.GetComponent<Carrot_Box_Item>().get_val();
                }

                if (is_frw)
                {
                    this.DataFrameworkUnity[key_lang_edit] = data_u;
                    this.UpdateDataFrw();
                }
                else
                {
                    this.DataLangApp[key_lang_edit] = data_u;
                    this.UpdateDataLangApp();
                }
                box_data.close();
            });

            Carrot_Button_Item btn_cancel = panel.create_btn();
            btn_cancel.set_icon_white(this.carrot.icon_carrot_cancel);
            btn_cancel.set_bk_color(this.carrot.color_highlight);
            btn_cancel.set_label("Cancel");
            btn_cancel.set_label_color(Color.white);
            btn_cancel.set_act_click(() =>
            {
                this.carrot.play_sound_click();
                box_data.close();
            });

            if (key_lang_edit == "en")
            {
                Carrot_Box_Btn_Item btn_add_key = box_data.create_btn_menu_header(this.carrot.icon_carrot_add);
                btn_add_key.set_act(() =>
                {
                    Carrot_Window_Input box_inp = this.carrot.Show_input("New Key", "Enter key name");
                    box_inp.set_act_done(s_key =>
                    {
                        Carrot_Box_Item item_data = box_data.create_item_of_top();
                        item_data.gameObject.name = s_key.ToString();
                        item_data.set_icon(this.carrot.icon_carrot_database);
                        item_data.set_title(s_key.ToString());
                        item_data.set_tip("New data key");
                        item_data.set_type(Box_Item_Type.box_value_input);
                        AddBtnDelItemDataBox(item_data);
                        box_inp.close();
                    });
                });
            }
        }

        private void AddBtnDelItemDataBox(Carrot_Box_Item item_data)
        {
            Carrot_Box_Btn_Item btn_del = item_data.create_item();
            btn_del.set_icon(this.carrot.sp_icon_del_data);
            btn_del.set_icon_color(Color.white);
            btn_del.set_color(this.carrot.color_highlight);
            btn_del.set_act(() =>
            {
                this.carrot.play_sound_click();
                Destroy(item_data.gameObject);
            });
        }

        private void AddBtnCopyItemDataBox(Carrot_Box_Item item_data)
        {
            Carrot_Box_Btn_Item btn_copy = item_data.create_item();
            btn_copy.set_icon(this.carrot.icon_carrot_write);
            btn_copy.set_icon_color(Color.white);
            btn_copy.set_color(this.carrot.color_highlight);
            btn_copy.set_act(() =>
            {
                this.carrot.play_sound_click();
                this.carrot.Show_input("Copy", "Copy Key data", item_data.txt_name.text, Window_Input_value_Type.input_field);
            });
        }

        private void Change_lang(string s_key_new)
        {
            this.carrot.hide_loading();
            PlayerPrefs.SetString("lang", s_key_new);
            PlayerPrefs.SetString("lang_name",s_name_lang_temp);
            act_after_selecting_lang?.Invoke(s_key_new);

            if (this.is_load_emp_after_sel_lang)
            {
                this.s_lang_key = s_key_new;
                this.LoadData();
                this.Load_icon_lang();
                this.Load_lang_emp();
            }
            if (this.box_lang != null) this.box_lang.close();

        }

        public string Val(string s_key, string s_default = "")
        {
            if (data_lang_value != null)
            {
                if (s_key == "")
                {
                    return "";
                }
                else if (s_key == null)
                {
                    return "";
                }
                else
                {
                    if (this.data_lang_value[s_key] != null)
                        return this.data_lang_value[s_key].ToString();
                    else
                        return s_default;
                }
            }
            else
            {
                return s_default;
            }
        }
    }
}
