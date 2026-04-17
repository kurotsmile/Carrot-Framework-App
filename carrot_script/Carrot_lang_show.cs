using UnityEngine;
using UnityEngine.UI;

namespace Carrot
{
    public class Carrot_lang_show : MonoBehaviour
    {
        public string[] key;
        public Text[] emp;
        [Tooltip("Image for country")]
        public Image[] List_img_change_icon_lang;
        private string[] default_emp_text;

        private void Cache_default_text()
        {
            if (this.emp == null) return;
            if (this.default_emp_text != null && this.default_emp_text.Length == this.emp.Length) return;

            this.default_emp_text = new string[this.emp.Length];
            for (int i = 0; i < this.emp.Length; i++)
            {
                this.default_emp_text[i] = this.emp[i] != null ? this.emp[i].text : "";
            }
        }

        public void load_lang_emp(Carrot_lang lang)
        {
            if (lang == null || this.key == null || this.emp == null) return;
            this.Cache_default_text();

            int count = Mathf.Min(this.key.Length, this.emp.Length);
            for (int i = 0; i < count; i++)
            {
                if (this.emp[i] == null) continue;
                if (string.IsNullOrEmpty(this.key[i])) continue;

                string defaultValue = "";
                if (this.default_emp_text != null && i < this.default_emp_text.Length)
                    defaultValue = this.default_emp_text[i];

                this.emp[i].text = lang.Val(this.key[i], defaultValue);
            }
        }

        public void load_lang_emp(Sprite sp_lang_cur, Carrot_lang lang)
        {
            this.load_lang_emp(lang);
            if (this.List_img_change_icon_lang == null) return;

            for (int i = 0; i < this.List_img_change_icon_lang.Length; i++)
            {
                if (this.List_img_change_icon_lang[i] != null)
                    this.List_img_change_icon_lang[i].sprite = sp_lang_cur;
            }
        }
    }


}
