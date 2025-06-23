using System.Collections;
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
        private string NameFileCustomerListCountry = "ListCountryCustomer";
        private string NameFileCustomerFrw = "FrameworkLangCustomer";

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
                if (DataLanAppText != null) this.DataLangApp = Json.Deserialize(DataLanAppText.text) as IDictionary;
            }

            this.LoadData();
            this.Load_icon_lang();
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
            if (this.carrot.emp_show_lang != null) for (int i = 0; i < this.carrot.emp_show_lang.List_img_change_icon_lang.Length; i++) this.carrot.emp_show_lang.List_img_change_icon_lang[i].sprite = sp_lang_icon;
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

            if (this.DataListCountry == null)
            {
                TextAsset data_list_lang = Resources.Load<TextAsset>(this.NameFileCustomerListCountry);
                if (data_list_lang != null)
                {
                    DataListCountry = Json.Deserialize(data_list_lang.text) as IList;
                    this.Load_list_lang_by_data(DataListCountry);
                }
                else
                {
                    data_list_lang = Resources.Load<TextAsset>("ListCountry");
                    DataListCountry = Json.Deserialize(data_list_lang.text) as IList;
                    this.Load_list_lang_by_data(DataListCountry);
                }
            }
            else
                this.Load_list_lang_by_data(DataListCountry);
        }


        private void Load_list_lang_by_data(IList all_item)
        {
            this.carrot.hide_loading();
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
                            btn_lapp.set_color(this.carrot.color_highlight);
                            btn_lapp.set_act(() =>
                            {
                                this.DataLangApp_en = this.DataLangApp["en"] as IDictionary;
                                this.BoxEditData(this.DataLangApp[s_key] as IDictionary, s_key, false);
                            });
                        }
                        else
                        {
                            btn_lapp.set_color(Color.black);
                        }
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

            this.box_lang.create_btn_menu_header(this.carrot.icon_carrot_add).set_act(() =>
            {
                this.carrot.play_sound_click();
                this.Box_Edit_Or_Add();
            });
        }

        private void Box_Edit_Or_Add(IDictionary data = null)
        {
            if (this.box_lang != null) this.box_lang.close();
            this.box_lang = this.carrot.Create_Box("");
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
            this.carrot.get_tool().save_file("Resources/" + this.NameFileCustomerListCountry + ".json", Json.Serialize(this.DataListCountry));
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

            item_lang.set_act(() => this.Select_lang(s_key));
            return item_lang;
        }

        public void Load_lang_emp()
        {
            if (this.carrot.emp_show_lang != null)
            {
                for (int i = 0; i < this.carrot.emp_show_lang.key.Length; i++) if (this.Val(this.carrot.emp_show_lang.key[i]) != "") this.carrot.emp_show_lang.emp[i].text = this.Val(this.carrot.emp_show_lang.key[i]);
            }
        }

        public string Get_key_lang()
        {
            return this.s_lang_key;
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
            Carrot_Box box_data = this.carrot.Create_Box("");
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
                if (dataEdit[key] != null) item_data.set_val(dataEdit[key].ToString());

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
                            Application.OpenURL("https://translate.google.com/?hl=vi&sl=en&tl="+key_lang_edit+"&text="+UnityWebRequest.EscapeURL(s_tip)+"&op=translate");
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