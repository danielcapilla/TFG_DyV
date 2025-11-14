using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Localization.Tables;

public class PowerUpsBehaviour : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject powerUp; // Mismo prefab que cambia el hijo activo segun el powerUp
    [SerializeField] private GameObject horizontalLayout;
    [SerializeField] private GameObject movementPanel;

    //public override void OnNetworkSpawn()
    //{
    //    base.OnNetworkSpawn();
    //    PowerUpEvents.OnInvertControls += HandleInvertControls;
    //    PowerUpEvents.OnPlayerSpeedUp += HandleSpeedUp;
    //}
    //public override void OnNetworkDespawn()
    //{
    //    base.OnNetworkDespawn();
    //    PowerUpEvents.OnInvertControls -= HandleInvertControls;
    //    PowerUpEvents.OnPlayerSpeedUp -= HandleSpeedUp;
    //}
    private void OnEnable()
    {
        PowerUpEvents.OnInvertControls += HandleInvertControls;
        PowerUpEvents.OnPlayerSpeedUp += HandleSpeedUp;
    }
    private void OnDisable()
    {
        PowerUpEvents.OnInvertControls -= HandleInvertControls;
        PowerUpEvents.OnPlayerSpeedUp -= HandleSpeedUp;
    }

    private void HandleInvertControls(float duration)
    {
        AddPowerUp(0, duration, "#FFF931", "#A4689B");
        movementPanel.transform.DOShakePosition(0.4f, 25f, 25, 90f);
    }

    private void HandleSpeedUp(float duration)
    {
        AddPowerUp(1, duration, "#31FF40", "#A4689B");
    }

    private void AnimatePowerUpEntry(Transform t)
    {
        t.localScale = Vector3.zero;
        t.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
    }

    private void AnimatePowerUpExit(Transform t, GameObject go)
    {
        t.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => Destroy(go));
    }

    private Tween AnimatePowerUpActive(Transform t, float duration)
    {
        // Palpitar 
        return t.DOScale(1f, 1.2f)
                .From(0.88f)        
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
    }

    public void AddPowerUp(int index, float duration, string col1, string col2)
    {

        // Buscar y destruir si ya existe un powerUp del mismo tipo
        for (int i = 0; i < horizontalLayout.transform.childCount; i++)
        {
            GameObject child = horizontalLayout.transform.GetChild(i).gameObject;
            Transform grandson = child.transform.GetChild(index);

            if (grandson != null && grandson.gameObject.activeSelf)
            {
                Destroy(child);
                break;
            }
        }

        // Instanciar el nuevo powerUp
        GameObject gameObject = Instantiate(powerUp, horizontalLayout.transform);
        gameObject.transform.GetChild(index).gameObject.SetActive(true);

        // Colores del gradiente
        ColorUtility.TryParseHtmlString(col1, out Color color1);
        ColorUtility.TryParseHtmlString(col2, out Color color2);
        UIGradient gradient = gameObject.transform.GetComponent<UIGradient>();
        gradient.m_color1 = color1;
        gradient.m_color2 = color2;

        AnimatePowerUpEntry(gameObject.transform);

        Tween pulse = AnimatePowerUpActive(gameObject.transform, duration);

        StartCoroutine(PowerUpLifetimeCoroutine(gameObject, duration, pulse));
    }

    private IEnumerator PowerUpLifetimeCoroutine(GameObject gameObject, float duration, Tween pulseTween)
    {
        yield return new WaitForSeconds(duration);
        // Animacion de salida

        if (pulseTween != null && pulseTween.IsActive())
            pulseTween.Kill();

        gameObject.transform.localScale = Vector3.one;

        AnimatePowerUpExit(gameObject.transform, gameObject);
    }
}
