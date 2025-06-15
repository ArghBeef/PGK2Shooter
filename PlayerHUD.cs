using System.Collections;
using Unity.Netcode;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] Slider hpBar;
    [SerializeField] TMP_Text hpText;
    [SerializeField] TMP_Text weaponNameText;
    [SerializeField] TMP_Text ammoText;
    [SerializeField] Slider reloadBar;

    Player player;

    IEnumerator Start()
    {
        while (player == null)
        {
            if (NetworkManager.Singleton.LocalClient?.PlayerObject != null)
                player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();
            yield return null;
        }

        var hp = player.GetComponent<Health>();
        hpBar.maxValue = hp.maxHealth;
        hpBar.value = hp.current.Value;
        hpText.text = hp.current.Value.ToString("0");

        hp.current.OnValueChanged += (_, val) =>
        {
            hpBar.value = val;
            hpText.text = val.ToString("0");
        };

        reloadBar.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        if (player.CurrentWeapon == null)
        {
            weaponNameText.text = "NO WEAPON";
            ammoText.text = "- / -";
            reloadBar.gameObject.SetActive(false);
            return;
        }

        var w = player.CurrentWeapon;
        weaponNameText.text = w.Data.weaponName.ToUpper();
        ammoText.text = $"{w.CurrentAmmo} / {w.Data.magazineSize}";

        if (w.IsReloading)
        {
            if (!reloadBar.gameObject.activeSelf)
                reloadBar.gameObject.SetActive(true);
            reloadBar.value = w.ReloadFraction;
        }
        else if (reloadBar.gameObject.activeSelf)
            reloadBar.gameObject.SetActive(false);
    }
}
