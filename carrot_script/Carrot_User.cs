using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Carrot
{

    public class Carrot_User : MonoBehaviour
    {
        private Carrot carrot;
        private Carrot_Box box_list;
        private On_event_change event_after_login_user;

        public Sprite icon_user_login_true;
        public Sprite icon_user_login_false;
        public Sprite icon_user_anonymous;

        [Header("Icon Emp user")]
        public Sprite icon_user_name;
        public Sprite icon_user_status;
        public Sprite icon_user_register;
        public Sprite icon_user_edit;
        public Sprite icon_user_info;
        public Sprite icon_user_logout;
        public Sprite icon_user_done;
        public Sprite icon_user_change_password;

        [Header("Infor user")]
        public Color32 color_edit;
        public Color32 color_logout;
        public Color32 color_change_password;
        public Carrot_Box_Item user_login_item_setting = null;
        private int count_try_login=0;

        public void On_load(Carrot carrot)
        {
            this.carrot = carrot;
            if (!PlayerAccountService.Instance.IsSignedIn) SignInCachedUser();
            AuthenticationService.Instance.SignedIn += () =>
            {
                if (this.carrot.img_btn_login != null)
                {
                    this.carrot.img_btn_login.sprite = this.icon_user_login_true;
                }
            };
        }

        void SignInCachedUser()
        {
            if (!AuthenticationService.Instance.SessionTokenExists) return;
            try
            {
                Debug.Log("Sign in anonymously succeeded!");
                Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
                this.carrot.img_btn_login.sprite = this.icon_user_anonymous;
            }
            catch (AuthenticationException ex)
            {
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                Debug.LogException(ex);
            }
        }

        public async Task Show_loginAsync(UnityAction act_afte_login = null)
        {
            count_try_login=0;
            this.carrot.play_sound_click();
            if (!PlayerAccountService.Instance.IsSignedIn)
            {
                this.Check_login(act_afte_login);
            }
            else
            {
                if (this.box_list != null) this.box_list.close();
                string playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
                this.box_list = this.carrot.Create_Box("box_info");
                this.box_list.set_icon(this.icon_user_info);
                this.box_list.set_title(this.carrot.lang.Val("acc_info", "Account Information"));
                Carrot_Box_Item item_name = this.box_list.create_item("s_name");
                item_name.set_icon(this.icon_user_name);
                item_name.set_title("Player Name");
                item_name.set_tip(playerName);

                Carrot_Box_Btn_Item btn_edit_name=item_name.create_item();
                btn_edit_name.set_icon(this.carrot.icon_carrot_write);
                btn_edit_name.set_color(Color.white);
                btn_edit_name.set_color(this.carrot.color_highlight);
                btn_edit_name.set_act(()=>{
                    this.carrot.game.set_enable_gamepad_console(false);
                    this.carrot.game.set_enable_all_gamepad(false);
                    Carrot_Window_Input inp_box=this.carrot.Show_input("Change name","Enter new name",playerName);
                    inp_box.set_act_done((s_val)=>{
                        AuthenticationService.Instance.UpdatePlayerNameAsync(s_val);
                        this.carrot.Show_msg(this.carrot.lang.Val("register", "Register Account"), this.carrot.lang.Val("acc_edit_success", "Successful account information update!"), Msg_Icon.Success);
                        inp_box.close();
                        this.Act_close_box();
                        this.carrot.game.set_enable_gamepad_console(true);
                        this.carrot.game.set_enable_all_gamepad(true);
                    });
                });

                Carrot_Box_Item item_accountPortal = this.box_list.create_item("accountPortal");
                item_accountPortal.set_icon(this.carrot.icon_carrot_link);
                item_accountPortal.set_title("Account Portal");
                item_accountPortal.set_tip("Open Account Portal");
                item_accountPortal.set_act(() =>{
                    this.carrot.play_sound_click();
                    Application.OpenURL(PlayerAccountService.Instance.AccountPortalUrl);
                });

                Carrot_Box_Btn_Panel panel_btn = this.box_list.create_panel_btn();
                /*
                Carrot_Button_Item btn_edit = panel_btn.create_btn("btn_edit");
                btn_edit.set_icon(this.carrot.icon_carrot_done);
                btn_edit.set_label(this.carrot.lang.Val("edit", "Edit"));
                btn_edit.set_label_color(Color.white);
                btn_edit.set_bk_color(this.carrot.color_highlight);
                */

                Carrot_Button_Item btn_logout = panel_btn.create_btn("btn_logout");
                btn_logout.set_icon(this.icon_user_logout);
                btn_logout.set_label(this.carrot.lang.Val("logout", "Log out"));
                btn_logout.set_label_color(Color.white);
                btn_logout.set_bk_color(this.carrot.color_highlight);
                btn_logout.set_act_click(Act_logout);

                Carrot_Button_Item btn_canel = panel_btn.create_btn("btn_cancel");
                btn_canel.set_icon(this.carrot.icon_carrot_cancel);
                btn_canel.set_label(this.carrot.lang.Val("cancel", "Cancel"));
                btn_canel.set_label_color(Color.white);
                btn_canel.set_bk_color(this.carrot.color_highlight);
                btn_canel.set_act_click(() => this.Act_close_box());

                this.box_list.update_gamepad_cosonle_control();
            }
        }

        private async void Check_login(UnityAction act_after_login=null)
        {
            if (PlayerAccountService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInWithUnityAsync(PlayerAccountService.Instance.AccessToken);
                Debug.Log($"Login success: {AuthenticationService.Instance.AccessToken}");
                PlayerPrefs.SetString("user_AccessToken", AuthenticationService.Instance.AccessToken);
                act_after_login?.Invoke();
            }
            else
            {
                await PlayerAccountService.Instance.StartSignInAsync();
                this.carrot.delay_function(2f, () =>
                {
                    this.count_try_login++;
                    if(this.count_try_login<=1) Check_login(act_after_login);
                });
            }
        }

        private void Act_logout()
        {
            this.check_and_show_item_login_setting();
            this.carrot.play_sound_click();
            AuthenticationService.Instance.SignOut();
            PlayerAccountService.Instance.SignOut();
            this.carrot.img_btn_login.sprite = icon_user_login_false;
            if (box_list != null) this.box_list.close();
        }

        public void check_and_show_item_login_setting()
        {
            if (this.user_login_item_setting != null)
            {
                if (!PlayerAccountService.Instance.IsSignedIn)
                {
                    this.user_login_item_setting.set_icon(this.icon_user_change_password);
                    this.user_login_item_setting.set_title(this.carrot.lang.Val("login", "Login"));
                    this.user_login_item_setting.set_tip(this.carrot.lang.Val("login_tip", "Sign in to your carrot account to manage data, and use many other services"));
                    this.user_login_item_setting.set_lang_data("login", "login_tip");

                    Carrot_Box_Btn_Item item_btn_regiter = this.user_login_item_setting.create_item();
                    item_btn_regiter.set_icon(this.icon_user_register);
                    item_btn_regiter.set_color(this.carrot.color_highlight);
                    item_btn_regiter.set_act(async ()=>{
                       await this.Show_loginAsync(this.carrot.Reload_setting);
                    });
                }
                else
                {
                    this.user_login_item_setting.set_icon(this.icon_user_login_true);
                    this.user_login_item_setting.set_title(this.carrot.lang.Val("acc_info", "Account Information"));
                    this.user_login_item_setting.set_tip(this.carrot.lang.Val("acc_edit_tip", "Click this button to update account information"));
                    this.user_login_item_setting.set_lang_data("acc_info", "acc_edit_tip");

                    Carrot_Box_Btn_Item item_btn_logout = this.user_login_item_setting.create_item();
                    item_btn_logout.set_icon(this.icon_user_logout);
                    item_btn_logout.set_color(this.carrot.color_highlight);
                    item_btn_logout.set_act(()=>{
                        this.carrot.close_all_window();
                        this.Act_logout();
                    });
                }

                this.user_login_item_setting.set_act(async () =>{
                    await this.Show_loginAsync(this.carrot.Reload_setting);
                });
            }
        }
    
        public string get_id_user_login()
        {
            return "";
        }

        private void Act_close_box()
        {
            if (this.box_list != null) this.box_list.close();
        }
    }
}
