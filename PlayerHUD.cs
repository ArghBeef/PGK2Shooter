using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    public Slider hpBar;
    public TMP_Text hpText;
    public TMP_Text weaponNameText;
    public TMP_Text ammoText;
    public Slider reloadBar;

    Player player;

    IEnumerator Start()
    {
        while (player == null)
        {
            if (NetworkManager.Singleton.LocalClient?.PlayerObject != null)
                player = NetworkManager.Singleton
                                   .LocalClient.PlayerObject.GetComponent<Player>();
            yield return null;
        }

        var hp = player.GetComponent<Health>();
        hpBar.maxValue = hp.maxHealth;
        hpBar.value = hp.current.Value;
        hpText.text = hp.current.Value.ToString("0");

        hp.current.OnValueChanged += (oldVal, newVal) =>
        {
            hpBar.value = newVal;
            hpText.text = newVal.ToString("0");
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

        weaponNameText.text = w.data.weaponName.ToUpper();
        ammoText.text = $"{w.CurrentAmmo} / {w.data.magazineSize}";

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
