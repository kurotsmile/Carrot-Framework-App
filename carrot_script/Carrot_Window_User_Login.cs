using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Carrot
{
    public class Carrot_Window_User_Login : MonoBehaviour
    {
        public Carrot_UI UI;
        private Carrot carrot;

        public Text login_username_title;
        public InputField inp_login_username;
        public InputField inp_login_password;
        public Image img_icon_mode_login;

        public UnityAction act_after_login_success;

        private bool is_model_login_email = true;
        private Carrot_Window_Loading windowLoadingCur = null;

        public void On_load(Carrot carrot)
        {
            this.carrot = carrot;
            this.UI.set_theme(this.carrot.color_highlight);
            this.Check_mode_login();
        }

        public void btn_show_list_lang()
        {
            this.carrot.lang.Show_list_lang(this.after_select_lang);
        }

        public void btn_show_register()
        {
            this.carrot.user.show_user_register();
        }

        private void after_select_lang(string s_data)
        {
            this.GetComponent<Carrot_lang_show>().load_lang_emp(this.carrot.lang.Get_sp_lang_cur(),carrot.lang);
        }

        public void btn_show_lost_password()
        {
            this.carrot.user.show_window_lost_password();
        }

        public void btn_user_login()
        {
            if (!Regex.IsMatch(inp_login_username.text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                this.carrot.Show_msg(this.carrot.lang.Val("login", "Login"), this.carrot.lang.Val("error_email", "Invalid email format!"));
                return;
            }

            if (inp_login_password.text.Trim().Length<5)
            {
                this.carrot.Show_msg(this.carrot.lang.Val("login", "Login"), this.carrot.lang.Val("error_passowrd_login", "Password must be greater than 6 characters!"));
                return;
            }

            WWWForm frmLogin = new();
            frmLogin.AddField("email", inp_login_username.text);
            frmLogin.AddField("password", inp_login_password.text);
            windowLoadingCur=carrot.send(carrot.url_worker+"/login",frmLogin,Act_user_login_done, Act_user_login_fail);
        }

        private void Act_user_login_done(string s_data)
        {
            Debug.Log("Login success:" + s_data);
            this.carrot.hide_loading();
            windowLoadingCur.close();
            IDictionary resData = (IDictionary)Json.Deserialize(s_data);
            IDictionary userData = (IDictionary) resData["user"];
            if (userData != null)
            {
                if (this.is_model_login_email)
                    PlayerPrefs.SetString("login_username_mail", this.inp_login_username.text);
                else
                    PlayerPrefs.SetString("login_username_phone", this.inp_login_username.text);
                this.carrot.user.set_data_user_login(userData);
                this.close();
                
                if (this.act_after_login_success != null) 
                    this.act_after_login_success();
                else
                    this.carrot.user.Show_info_user_by_data(userData);
            }
            else
            {
                this.carrot.Show_msg(this.carrot.lang.Val("login", "Login"), this.carrot.lang.Val("acc_no", "This account information is not in the system!"));
            }
        }

        private void Act_user_login_fail(string s_error)
        {
            windowLoadingCur.close();
            this.carrot.Show_msg(this.carrot.lang.Val("login", "Login"), this.carrot.lang.Val("login_fail", "Login failed, please try again!"));
        }

        public void close()
        {
            this.carrot.play_sound_click();
            this.UI.close();
        }

        public void set_enable_gamepad_console(bool is_user_console)
        {
            if (this.carrot.type_control != TypeControl.None) this.carrot.game.set_enable_gamepad_console(is_user_console);
        }

        public void btn_change_mode_login()
        {
            if (this.is_model_login_email)
                this.is_model_login_email = false;
            else
                this.is_model_login_email = true;
            this.Check_mode_login();
        }

        public void Check_mode_login()
        {
            if (this.is_model_login_email)
            {
                this.inp_login_username.text = PlayerPrefs.GetString("login_username_mail");
                this.img_icon_mode_login.sprite = this.carrot.icon_carrot_mail;
                this.login_username_title.text = this.carrot.lang.Val("user_email", "Email");
                this.inp_login_username.contentType = InputField.ContentType.EmailAddress;
            }
            else
            {
                this.inp_login_username.text = PlayerPrefs.GetString("login_username_phone");
                this.img_icon_mode_login.sprite = this.carrot.icon_carrot_phone;
                this.login_username_title.text = this.carrot.lang.Val("user_phone", "Phone");
                this.inp_login_username.contentType = InputField.ContentType.IntegerNumber;
            }
                
        }
    }
}
