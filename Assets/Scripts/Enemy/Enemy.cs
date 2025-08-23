using UnityEngine;


//abstract class is a class that cannot be instantiated, but can be inherited from - if you want to create a base class that other classes can inherit from, but you don't want to allow instantiation of the base class, this is the way to go.
[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
public abstract class Enemy : MonoBehaviour
{
    // private - non accessible from other scripts, only accessible from this class - if you want to keep a variable private and not accessible from other scripts, this is the way to go. The accessiblity is limited to this class only and childern of this class will not be able to access it.
    // protected - private for the entire inheritance hierarchy of this class - if you want to keep a variable private but accessible from child classes, this is the way to go. The accessiblity is limited to this class and its children only.
    // public - a variable that is publicly accessible from other scripts - if an object has a instance of this class (via reference from another class or via GetComponent)
    // this can be a problem if you want to change the variable but don't want other scripts to be able to change it. Tracking down bugs can be difficult if you have many
    // scripts that can change the variable.



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
            maxHealth = 5;
        }

        health = maxHealth;
    }

    public virtual void TakeDamage(int damageValue, DamageType damageType = DamageType.Default)
    {
        health -= damageValue;

        if (health <= 0)
        {
            anim.SetTrigger("Death");

            // Destroy the enemy after the death animation is complete
            if (transform.parent != null)
            {
                Destroy(transform.parent.gameObject, 0.5f); // Adjust the delay as needed for the animation
            }
            else
            {
                Destroy(gameObject, 0.5f); // If no parent, destroy this game object directly
            }
        }
    }
}

public enum DamageType
{
    Default,
    JumpedOn
}