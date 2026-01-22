using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Aplikuje barevné pozadí pomocí procedurálního skyboxu
/// Přidej tento skript na TrackGenerator nebo kameru
/// </summary>
public class LevelSkyboxController : MonoBehaviour
{
    [Header("Skybox Settings")]
    [Tooltip("Použít barevný gradient skybox místo solid color?")]
    public bool useGradientSkybox = true;

    [Tooltip("Vrchní barva skyboxu")]
    public Color topColor = new Color(0.2f, 0.3f, 0.5f);

    [Tooltip("Spodní barva skyboxu")]
    public Color bottomColor = new Color(0.1f, 0.1f, 0.2f);

    [Tooltip("Exponent pro gradient (vyšší = ostřejší přechod)")]
    [Range(0.1f, 8f)]
    public float gradientExponent = 1.5f;

    private Material skyboxMaterial;
    private Material originalSkybox;

    void Start()
    {
        // Ulož originální skybox
        originalSkybox = RenderSettings.skybox;

        ApplySkybox();
    }

    public void ApplySkybox()
    {
        if (useGradientSkybox)
        {
            CreateGradientSkybox();
        }
        else
        {
            // Použij solid color na kameře
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = topColor;
            }
        }
    }

    void CreateGradientSkybox()
    {
        // Zkus najít náš custom shader
        Shader skyboxShader = Shader.Find("Custom/SimpleGradientSkybox");

        // Fallback na Skybox/Procedural pokud custom shader neexistuje
        if (skyboxShader == null)
        {
            Debug.LogWarning("[LevelSkybox] Custom shader nenalezen, používám Skybox/Procedural");
            skyboxShader = Shader.Find("Skybox/Procedural");
        }

        if (skyboxShader == null)
        {
            Debug.LogError("[LevelSkybox] Žádný skybox shader nenalezen!");
            return;
        }

        skyboxMaterial = new Material(skyboxShader);

        // Nastav barvy - funguje pro oba shadery
        if (skyboxShader.name == "Custom/SimpleGradientSkybox")
        {
            // Náš custom shader
            skyboxMaterial.SetColor("_TopColor", topColor);
            skyboxMaterial.SetColor("_BottomColor", bottomColor);
            skyboxMaterial.SetFloat("_Exponent", gradientExponent);

            Debug.Log($"[LevelSkybox] Custom gradient: Top={topColor}, Bottom={bottomColor}");
        }
        else
        {
            // Procedural shader
            skyboxMaterial.SetColor("_SkyTint", topColor);
            skyboxMaterial.SetColor("_GroundColor", bottomColor);
            skyboxMaterial.SetFloat("_AtmosphereThickness", gradientExponent);
            skyboxMaterial.SetFloat("_Exposure", 1.3f);

            // Slunce vypneme
            skyboxMaterial.SetFloat("_SunSize", 0f);
            skyboxMaterial.SetFloat("_SunSizeConvergence", 0f);

            Debug.Log($"[LevelSkybox] Procedural skybox: Sky={topColor}, Ground={bottomColor}");
        }

        // Aplikuj skybox
        RenderSettings.skybox = skyboxMaterial;

        // Refresh
        DynamicGI.UpdateEnvironment();
    }

    void OnDestroy()
    {
        // Vrať originální skybox
        if (originalSkybox != null)
        {
            RenderSettings.skybox = originalSkybox;
            DynamicGI.UpdateEnvironment();
        }

        // Smaž dočasný materiál
        if (skyboxMaterial != null)
        {
            Destroy(skyboxMaterial);
        }
    }

    /// <summary>
    /// Nastaví barvy skyboxu z konfigurace
    /// </summary>
    public void SetColors(Color background)
    {
        Debug.Log($"[LevelSkybox] SetColors called with: {background}");

        topColor = background;

        // Spodní barva o něco tmavší
        bottomColor = new Color(
            background.r * 0.5f,
            background.g * 0.5f,
            background.b * 0.5f
        );

        Debug.Log($"[LevelSkybox] TopColor: {topColor}, BottomColor: {bottomColor}");

        ApplySkybox();
    }

    /// <summary>
    /// Nastaví gradient skyboxu s vlastními barvami
    /// </summary>
    public void SetGradient(Color top, Color bottom)
    {
        topColor = top;
        bottomColor = bottom;
        ApplySkybox();
    }
}