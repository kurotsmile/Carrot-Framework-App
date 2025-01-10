using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Carrot
{
    public struct Carrot_Rate_user_data
    {
        public string id {get;set;}
        public string name { get; set; }
        public string avatar { get; set; }
        public string lang {get;set;}
    }

    public struct Carrot_Rate_data
    {
        public Carrot_Rate_user_data user {get;set;}
        public string star {get;set;}
        public string comment {get;set;}
        public string date {get;set;}
    }

    public class Carrot_Window_Rate : MonoBehaviour
    {
        private Carrot carrot;
        public Carrot_UI UI;
        public GameObject panel_rate_rating;
        public GameObject panel_rate_feedback;
        public GameObject button_rate_feeedback;
        public Image[] img_star_feedback;
        public InputField inp_review_feedback;
        private int index_star_feedback;

        public void load(Carrot carrot)
        {
            this.carrot = carrot;
            this.btn_sel_rate(-1);
            this.panel_rate_rating.SetActive(true);
            this.panel_rate_feedback.SetActive(false);

            if (this.carrot.Carrotstore_AppId != "" && this.carrot.user.get_id_user_login() != "")
                this.button_rate_feeedback.SetActive(true);
            else
                this.button_rate_feeedback.SetActive(false);

            if (this.carrot.auto_open_rate_store) this.app_rate();
            if (this.carrot.type_control != TypeControl.None) this.carrot.game.set_list_button_gamepad_console(UI.get_list_btn());
            this.GetComponent<Carrot_lang_show>().load_lang_emp(carrot.lang);
            this.UI.set_theme(this.carrot.color_highlight);
        }

        public void app_rate()
        {
            this.act_rate();
        }

        private void act_rate()
        {
            if (this.carrot.type_rate == TypeRate.Link_Share_CarrotApp) Application.OpenURL(this.carrot.mainhost + "?p=app&id" + this.carrot.Carrotstore_AppId);
            if (this.carrot.type_rate == TypeRate.Market_Android) Application.OpenURL("market://details?id=" + Application.identifier);
            if (this.carrot.type_rate == TypeRate.Ms_Windows_Store) Application.OpenURL("ms-windows-store://review/?ProductId=" + this.carrot.WindowUWP_ProductId);
            if (this.carrot.type_rate == TypeRate.Amazon_app_store) Application.OpenURL("amzn://apps/android?p=" + Application.identifier);
        }

        public void btn_show_rate_feedback()
        {
            this.inp_review_feedback.text = "";
            this.panel_rate_rating.SetActive(false);
            this.panel_rate_feedback.SetActive(true);
            if (this.carrot.type_control != TypeControl.None) this.carrot.game.set_list_button_gamepad_console(UI.get_list_btn());
        }

        public void btn_close_rate_feedback()
        {
            this.panel_rate_rating.SetActive(true);
            this.panel_rate_feedback.SetActive(false);
            if (this.carrot.type_control != TypeControl.None) this.carrot.game.set_list_button_gamepad_console(UI.get_list_btn());
        }

        private void Act_submit_rate_feedback_done(string s_data)
        {
            this.carrot.hide_loading();
            this.carrot.Show_msg(this.carrot.lang.Val("send_feedback", "Send Feedback"), this.carrot.lang.Val("rate_thanks", "Send your comments to the successful developer. Thanks for your feedback!"), Msg_Icon.Success);
        }

        private void Act_submit_rate_feedback_fail(string s_error)
        {
            this.carrot.hide_loading();
        }

        public void btn_sel_rate(int index_star)
        {
            this.index_star_feedback = index_star;
            if (this.index_star_feedback < 0) this.index_star_feedback = 0;
            for (int i = 0; i < this.img_star_feedback.Length; i++)
            {
                if (i <= index_star)
                    this.img_star_feedback[i].color = this.carrot.color_highlight;
                else
                    this.img_star_feedback[i].color = Color.white;
            }
        }

        public void close()
        {
            this.UI.close();
        }

        public void set_enable_gamepad_console(bool is_user_console)
        {
            if (this.carrot.type_control != TypeControl.None) this.carrot.game.set_enable_gamepad_console(is_user_console);
        }
    }
}
