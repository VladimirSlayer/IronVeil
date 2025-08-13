using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeAbility : MonoBehaviour
{
    [Header("Настройки способности")]
    public float abilityRadius;               
    private float basicAbilityRaduis = 100f;     
    public float abilityDuration = 5f;         
    public float abilityCooldown = 5f;         
    public float manaCost = 20f;               

    [Header("Настройки подсветки")]
    public LayerMask targetLayer;               
    public Material glowMaterial;               

    private bool isAbilityActive = false;       
    private bool isOnCooldown = false;         

    private Dictionary<GameObject, Material> originalMaterials = new(); 
    private HashSet<GameObject> currentlyHighlightedObjects = new();   

    private PlayerStats playerStats;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        abilityRadius = basicAbilityRaduis;
    }

    void Update()
    {
        abilityRadius = basicAbilityRaduis + SkillModifier.Instance.GetModifier("eyeAbility");

        if (Input.GetKeyDown(KeyCode.R) && !isAbilityActive && !isOnCooldown)
        {
            if (playerStats != null && playerStats.currentMana >= manaCost)
            {
                playerStats.UseMana(manaCost);
                StartCoroutine(ActivateAbility());
            }
        }

        if (isAbilityActive)
        {
            CheckObjectsInRadius();
        }
    }

    IEnumerator ActivateAbility()
    {
        isAbilityActive = true;
        Debug.Log("Способность активирована!");

        CheckObjectsInRadius(); 

        yield return new WaitForSeconds(abilityDuration);

        ClearAllHighlights(); 
        isAbilityActive = false;

        StartCoroutine(Cooldown());
    }


    void CheckObjectsInRadius()
    {
        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, abilityRadius, targetLayer);

        HashSet<GameObject> newHighlightedObjects = new();
        foreach (var obj in objectsInRange)
        {
            GameObject target = obj.gameObject;
            if (!currentlyHighlightedObjects.Contains(target))
            {
                EnableGlow(target);
            }

            newHighlightedObjects.Add(target);
        }

        foreach (var obj in currentlyHighlightedObjects)
        {
            if (!newHighlightedObjects.Contains(obj))
            {
                DisableGlow(obj);
            }
        }

        currentlyHighlightedObjects = newHighlightedObjects;
    }

    void EnableGlow(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            if (!originalMaterials.ContainsKey(rend.gameObject))
            {
                originalMaterials[rend.gameObject] = rend.material;
            }

            rend.material = glowMaterial;
        }
    }


    void DisableGlow(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {
            GameObject rendObj = rend.gameObject;

            if (originalMaterials.ContainsKey(rendObj))
            {
                rend.material = originalMaterials[rendObj];
                originalMaterials.Remove(rendObj);
            }
        }
    }


    void ClearAllHighlights()
    {
        foreach (var obj in currentlyHighlightedObjects)
        {
            DisableGlow(obj);
        }

        currentlyHighlightedObjects.Clear();
        originalMaterials.Clear();
    }

    IEnumerator Cooldown()
    {
        isOnCooldown = true;
        Debug.Log("Способность перезаряжается...");

        yield return new WaitForSeconds(abilityCooldown);

        isOnCooldown = false;
        Debug.Log("Способность готова!");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, abilityRadius);
    }
}
