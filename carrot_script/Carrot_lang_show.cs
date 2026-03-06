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

        public void load_lang_emp(Carrot_lang lang)
        {
            if (lang == null || this.key == null || this.emp == null) return;

            int count = Mathf.Min(this.key.Length, this.emp.Length);
            for (int i = 0; i < count; i++)
            {
                if (this.emp[i] == null) continue;
                if (string.IsNullOrEmpty(this.key[i])) continue;

                string value = lang.Val(this.key[i]);
                if (value != "") this.emp[i].text = value;
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
