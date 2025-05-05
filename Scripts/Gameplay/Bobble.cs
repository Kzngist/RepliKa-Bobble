using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public enum BobbleColour
{
    Default,
    Random,
    Ruby,
    Emerald,
    Sapphire,
    Topaz,
    Amethyst,
    Aquamarine,
    Alexandrite,
    Onyx,
    Diamond,
}

[SelectionBase]
public class Bobble : BobbleTrayAttachable
{
    [SerializeField] internal BobbleColour colour;
    
    [Space]
    [Header("Audio")]
    [SerializeField] AudioClipProfile[] shatterSFXs;
    [SerializeField] AudioClipProfile[] breakSFXs;
    
    internal static float poppingSizeMultiplier { get; private set; } = 1.2f;
    internal static float poppingSpeed { get; private set; } = 0.2f;

    internal int scoreValue = 32;
    
    /// <summary>
    ///   <para> Detaches bobble with popping animation and destroys afterward </para>
    /// </summary>
    internal IEnumerator Pop()
    {
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;

        yield return transform.DOScale(poppingSizeMultiplier * Vector3.one, poppingSpeed).SetEase(Ease.OutCirc).WaitForCompletion();

        AudioManager.Instance.PlaySound(breakSFXs, null, transform.position);
        CurrentLevelManager.Instance.PopScore(transform.position, scoreValue);
        
        Destroy(gameObject);
    }
    
    internal IEnumerator Shatter()
    {
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        
        yield return transform.DOScale(poppingSizeMultiplier * Vector3.one, poppingSpeed / 3f).SetEase(Ease.OutCirc).WaitForCompletion();
        
        AudioManager.Instance.PlaySound(shatterSFXs, null, transform.position);
        CurrentLevelManager.Instance.PopScore(transform.position, scoreValue);
        
        Destroy(gameObject);
    }
        
    /// <summary>
    ///   <para> Detaches and destroys attachable with falling animation </para>
    /// </summary>
    internal IEnumerator Detach()
    {
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = false;
        transform.parent = null;

        float timer = 2f;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            
            // check if colliding with baseline, trigger popping if so
            if (Physics.CheckSphere(transform.position, 0.5f, Layer.BaseLineMask, QueryTriggerInteraction.Collide))
            {
                StartCoroutine(Shatter());
                yield break;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    internal override BobbleTrayObjectState GetState()
    {
        return new BobbleTrayObjectState(coordinates, colour);
    }
}
