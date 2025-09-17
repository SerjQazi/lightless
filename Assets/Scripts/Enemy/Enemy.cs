using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
public abstract class Enemy : MonoBehaviour
{
    protected SpriteRenderer sr;
    protected Animator anim;

    public int CurrentHealth => health;


    [SerializeField] protected int maxHealth = 5; // Now protected so child classes can see it
    protected int health; // Also protected for child class access

    // Virtual - child classes can override this if needed
    protected virtual void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (maxHealth <= 0)
        {
            Debug.LogError("Max health must be greater than 0. Setting to default value of 5.");
            maxHealth = 5; 
        }

        health = maxHealth;
    }

    public virtual void TakeDamage(int damageValue, DamageType damageType = DamageType.Default)
    {
        health -= damageValue;
        Debug.Log($"{name} took {damageValue} damage. Remaining health: {health}");

        if (health <= 0)
        {
            anim.SetTrigger("Death");
            Debug.Log($"☠️ {name} died!");

            if (transform.parent != null)
            {
                Destroy(transform.parent.gameObject, 0.5f);
            }
            else
            {
                Destroy(gameObject, 0.5f);
            }
        }
    }
}

public enum DamageType
{
    Default,
    JumpedOn
}
