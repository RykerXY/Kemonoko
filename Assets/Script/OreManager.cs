using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(AudioSource))] // Added requirement
public class GlowingOreManager : MonoBehaviour
{
    [Header("Ore Setup")]
    public List<GameObject> initialOres = new List<GameObject>();

    [Header("Timing & Effects")]
    public float fadeInDuration = 0.5f;
    public float glowDuration = 1.5f;
    public float fadeOutDuration = 0.5f;
    public float targetGlowIntensity = 1.5f;

    [Header("Audio")]
    public AudioClip collectSound;

    private int currentOreIndex = -1;
    private Coroutine mainCycleCoroutine;
    private Dictionary<Light2D, Coroutine> activeFadeCoroutines = new Dictionary<Light2D, Coroutine>();
    private AudioSource audioSource;

    void Awake()
    {
         audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (initialOres == null || initialOres.Count == 0)
        {
            Debug.LogError("Initial Ores list is not assigned or empty!", this);
            enabled = false;
            return;
        }

        bool foundValidOre = false;
        foreach (GameObject ore in initialOres)
        {
            if (ore == null) continue;

            Light2D light = ore.GetComponent<Light2D>();
            Collider2D collider = ore.GetComponent<Collider2D>();
            ClickableOre clickable = ore.GetComponent<ClickableOre>();

            if (light == null || collider == null || clickable == null)
            {
                Debug.LogError($"Ore '{ore.name}' is missing required components (Light2D, Collider2D, ClickableOre). Skipping.", ore);
                continue;
            }

            light.intensity = 0f;
            light.enabled = true;
            collider.enabled = false;
            foundValidOre = true;
        }

        if (foundValidOre)
        {
            currentOreIndex = 0;
            mainCycleCoroutine = StartCoroutine(GlowCycle());
        }
        else
        {
            Debug.LogWarning("No valid ores found to start the cycle.");
            enabled = false;
        }
    }

    IEnumerator GlowCycle()
    {
        while (enabled && initialOres.Count > 0) // Loop while enabled and ores exist
        {
            if (currentOreIndex < 0 || currentOreIndex >= initialOres.Count)
            {
                currentOreIndex = 0; // Reset index if out of bounds
            }

            GameObject oreToGlow = initialOres[currentOreIndex];

            // Skip if the ore object somehow became null (unlikely but safe)
            if (oreToGlow == null)
            {
                currentOreIndex = (currentOreIndex + 1) % initialOres.Count;
                yield return null;
                continue;
            }

            Light2D light = oreToGlow.GetComponent<Light2D>();
            Collider2D collider = oreToGlow.GetComponent<Collider2D>();

            if (light == null || collider == null) // Re-check components just in case
            {
                 currentOreIndex = (currentOreIndex + 1) % initialOres.Count;
                 yield return null;
                 continue;
            }

            collider.enabled = true;
            yield return StartCoroutine(FadeLight(light, 0f, targetGlowIntensity, fadeInDuration));

            yield return new WaitForSeconds(glowDuration);

            // Check if still active before fade out (might have been collected)
            if (oreToGlow != null && collider != null && collider.enabled)
            {
                collider.enabled = false;
                yield return StartCoroutine(FadeLight(light, light.intensity, 0f, fadeOutDuration));
            }

            // Only advance index if it wasn't collected (collection handles advancement)
            if (oreToGlow != null && collider != null && !collider.enabled) // Check if it finished its cycle naturally
            {
                 currentOreIndex = (currentOreIndex + 1) % initialOres.Count;
            }

            yield return null;
        }
    }

    IEnumerator FadeLight(Light2D light, float startIntensity, float endIntensity, float duration)
    {
        if (light == null) yield break;

        if (activeFadeCoroutines.ContainsKey(light) && activeFadeCoroutines[light] != null)
        {
            StopCoroutine(activeFadeCoroutines[light]);
        }

        Coroutine fadeCoroutine = StartCoroutine(PerformFade(light, startIntensity, endIntensity, duration));
        activeFadeCoroutines[light] = fadeCoroutine;

        yield return fadeCoroutine;

        // Only remove if the coroutine wasn't stopped externally (like during collection)
        if (activeFadeCoroutines.ContainsKey(light) && activeFadeCoroutines[light] == fadeCoroutine)
        {
            activeFadeCoroutines.Remove(light);
        }
    }

    IEnumerator PerformFade(Light2D light, float startIntensity, float endIntensity, float duration)
    {
        if (duration <= 0)
        {
            if(light != null) light.intensity = endIntensity;
            yield break;
        }

        float timer = 0f;
        while (timer < duration)
        {
            if (light == null) yield break;

            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            light.intensity = Mathf.Lerp(startIntensity, endIntensity, progress);
            yield return null;
        }

        if (light != null) light.intensity = endIntensity;
    }


    public void TryCollectOre(GameObject clickedOre)
    {
        // Check if the clicked ore is the currently active one
        if (initialOres.Count == 0 || currentOreIndex < 0 || currentOreIndex >= initialOres.Count || initialOres[currentOreIndex] != clickedOre)
        {
            return; // Clicked the wrong ore or state is invalid
        }

        Debug.Log($"Collecting ore: {clickedOre.name}");

        if (GlobalPoint.instance != null)
        {
            GlobalPoint.instance.orePoints += 4;
        }
        else
        {
            Debug.LogWarning("GlobalPoint instance not found. Cannot add points.");
        }

        if (collectSound != null && audioSource != null) // Play sound
        {
            audioSource.PlayOneShot(collectSound);
        }

        Light2D light = clickedOre.GetComponent<Light2D>();
        Collider2D collider = clickedOre.GetComponent<Collider2D>();

        // Immediately stop effects on the current ore
        if (light != null)
        {
            if (activeFadeCoroutines.ContainsKey(light) && activeFadeCoroutines[light] != null)
            {
                StopCoroutine(activeFadeCoroutines[light]);
                activeFadeCoroutines.Remove(light);
            }
            light.intensity = 0f; // Turn off light immediately
        }
        if (collider != null)
        {
            collider.enabled = false; // Disable click immediately
        }

        // Stop the current cycle coroutine
        if (mainCycleCoroutine != null)
        {
            StopCoroutine(mainCycleCoroutine);
        }

        // Advance to the next ore and restart the cycle
        currentOreIndex = (currentOreIndex + 1) % initialOres.Count;
        mainCycleCoroutine = StartCoroutine(GlowCycle());
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}