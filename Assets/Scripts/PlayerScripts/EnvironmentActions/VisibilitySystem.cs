using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;


public class VisibilitySystem : MonoBehaviour
{
    [Header("Основные настройки")]
    public float maxVisibility = 1f;        
    public float visibility;           

    public float bonusLightSpace = 5f;      
    public float minusPlayerVisibility = 0f;  
    public float minusVisibilityByCrouching = 0.1f; 

    private PlayerMovement playerMovement;

    [Header("Пост-эффекты")]
    public Volume ppv;                
    public float vignetteCoef = 1f;        
    private Vignette vignetteEffect;        

    [Header("UI")]
    public Image visibilityIcon;     

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();

        if (ppv != null && ppv.profile.TryGet(out Vignette vignette))
        {
            vignetteEffect = vignette;
        }
    }

    private void Update()
    {
        visibility = CalculateVisibility(); 

        if (vignetteEffect != null)

            if (visibility <= 0)
            {
                visibility -= minusPlayerVisibility;
            }

        UpdateVisibilityIcon();
    }

    float CalculateVisibility()
    {
        float bestVisibility = 0f;
        float closestDistance = float.MaxValue;

        Light[] lights = FilterLights();

        foreach (Light light in lights)
        {
            if (!light.enabled) continue;

            Vector3 toLight = light.transform.position - transform.position;
            float distance = toLight.magnitude;

            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, toLight.normalized, distance)) continue;

            if (distance > light.range + bonusLightSpace) continue;

            float falloff = 1f - Mathf.Clamp01(distance / (light.range + bonusLightSpace));

            float angleFactor = 1f;
            if (light.type == LightType.Spot)
            {
                Vector3 toPlayer = (transform.position - light.transform.position).normalized;
                float angle = Vector3.Angle(light.transform.forward, toPlayer);
                float halfAngle = light.spotAngle / 2f;

                if (angle > halfAngle) continue;

                angleFactor = 1f - Mathf.Clamp01(angle / halfAngle);
                angleFactor = Mathf.Pow(angleFactor, 2f);
            }

            float weight = (light.type == LightType.Spot) ? 2f : 1f;

            float visibilityValue = falloff * angleFactor * weight;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestVisibility = visibilityValue;
            }
        }

        if (playerMovement != null && playerMovement.CurrentState == "Crouching")
        {
            bestVisibility -= minusVisibilityByCrouching;
        }

        return Mathf.Clamp(bestVisibility, 0f, maxVisibility);
    }

    Light[] FilterLights()
    {
        Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);

        List<Light> filteredLights = new();
        foreach (Light light in allLights)
        {
            if (light.type != LightType.Directional)
            {
                filteredLights.Add(light);
            }
        }

        return filteredLights.ToArray();
    }

    void UpdateVisibilityIcon()
    {
        if (visibilityIcon == null) return;
        Color color = visibilityIcon.color;
        color.a = Mathf.Clamp(visibility, 0.01f, 1f);
        visibilityIcon.color = color;
    }
}
