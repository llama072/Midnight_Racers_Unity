using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// NEON FLICKER EFFECT — "— SELECT YOUR DESTINY —" stílusú villogás
///
/// BEÁLLÍTÁS:
/// 1. A szöveg GameObject-en legyen TextMeshProUGUI komponens
/// 2. Csatold ezt a scriptet ugyanarra a GameObject-re
/// 3. A TextMeshPro beállításaiban:
///    - Engedélyezd az "Underlay" vagy "Outline" effektet a TMP Inspector-ban
///    - VAGY használd az alábbi Face Color + Glow beállítást
/// 4. Inspectorban állítsd be a színeket és időzítést
///
/// TIPP: A legjobb eredményért a TextMeshPro Material-ban:
///    - Lighting > Baked SDF > Glow Color: narancssárga vagy kék
///    - Glow Power: 0.3-0.6
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class NeonFlickerEffect : MonoBehaviour
{
    [Header("Szöveg Szín")]
    [SerializeField] private Color normalColor = new Color(1f, 0.42f, 0f, 1f);      // Narancssárga
    [SerializeField] private Color dimColor = new Color(1f, 0.42f, 0f, 0.15f);   // Majdnem láthatatlan
    [SerializeField] private Color brightColor = new Color(1f, 0.75f, 0.3f, 1f);    // Fényes fehéres-narancs

    [Header("Flicker Minta")]
    [SerializeField] private float baseGlowSpeed = 1.4f;   // Alap pulzálás sebessége
    [SerializeField] private float flickerChance = 0.04f;  // Valószínűsége egy véletlenszerű villanásnak (0-1)
    [SerializeField] private float flickerMinGap = 2f;     // Minimum idő két flicker között
    [SerializeField] private float flickerMaxGap = 6f;     // Maximum idő két flicker között

    [Header("Opcionális: Glow Image")]
    [SerializeField] private UnityEngine.UI.Image glowImage; // Opcionális: háttér glow sprite a szöveg mögé

    private TextMeshProUGUI tmp;
    private float flickerTimer;
    private float nextFlickerTime;
    private bool isFlickering = false;

    private void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        tmp.color = normalColor;
        ScheduleNextFlicker();
    }

    private void Update()
    {
        if (isFlickering) return;

        // 1. Alap szinusz pulzálás (lassú, folyamatos)
        float pulse = (Mathf.Sin(Time.time * baseGlowSpeed) + 1f) / 2f; // 0-1 között
        tmp.color = Color.Lerp(
            new Color(normalColor.r, normalColor.g, normalColor.b, 0.75f),
            new Color(brightColor.r, brightColor.g, brightColor.b, 1f),
            pulse * 0.6f
        );

        // Opcionális glow kép pulzálása
        if (glowImage != null)
        {
            Color gc = glowImage.color;
            gc.a = Mathf.Lerp(0.1f, 0.4f, pulse);
            glowImage.color = gc;
        }

        // 2. Véletlenszerű flicker ütemezés
        flickerTimer += Time.deltaTime;
        if (flickerTimer >= nextFlickerTime)
        {
            flickerTimer = 0f;
            ScheduleNextFlicker();
            StartCoroutine(DoFlicker());
        }
    }

    /// <summary>
    /// Neon flicker szekvencia — mint egy régi neonlámpa
    /// </summary>
    private IEnumerator DoFlicker()
    {
        isFlickering = true;

        // Véletlenszerű flicker minta választása
        int pattern = Random.Range(0, 3);

        if (pattern == 0)
        {
            // Rövid dupla villanás
            yield return SetAlpha(dimColor, 0.05f);
            yield return SetAlpha(brightColor, 0.04f);
            yield return SetAlpha(dimColor, 0.06f);
            yield return SetAlpha(brightColor, 0.05f);
            yield return SetAlpha(normalColor, 0.08f);
        }
        else if (pattern == 1)
        {
            // Hosszabb kiesés majd visszajövés
            yield return SetAlpha(dimColor, 0.12f);
            yield return SetAlpha(new Color(normalColor.r, normalColor.g, normalColor.b, 0.5f), 0.04f);
            yield return SetAlpha(dimColor, 0.05f);
            yield return SetAlpha(brightColor, 0.06f);
            yield return SetAlpha(normalColor, 0.1f);
        }
        else
        {
            // Gyors tripla villanás
            for (int i = 0; i < 3; i++)
            {
                yield return SetAlpha(dimColor, 0.03f);
                yield return SetAlpha(brightColor, 0.03f);
            }
            yield return SetAlpha(normalColor, 0.1f);
        }

        isFlickering = false;
    }

    private IEnumerator SetAlpha(Color color, float duration)
    {
        tmp.color = color;
        if (glowImage != null)
        {
            Color gc = glowImage.color;
            gc.a = color.a * 0.4f;
            glowImage.color = gc;
        }
        yield return new WaitForSeconds(duration);
    }

    private void ScheduleNextFlicker()
    {
        nextFlickerTime = Random.Range(flickerMinGap, flickerMaxGap);
    }
}