using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
public abstract class Enemy : MonoBehaviour
{
    protected SpriteRenderer sr;
    protected Animator anim;
    protected int health;

    [SerializeField] private int maxHealth = 5;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Virtual - a method that can be overridden in child classes - if you want to allow child classes to change the behavior of a method, this is the way to go.
    protected virtual void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (maxHealth <= 0)
        {
            Debug.LogError("Max health must be greater than 0. Setting to default value of 5.");
            maxHealth = 6;
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