
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using JetBrains.Annotations;
using System.Numerics;
using UnityEngine.AI;
// using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour,IDataPersistence
{


    [Header("Health and damage")]
    public int MaxHealth = 100;
    public int currentHealth;
    public GameObject healthBar;
    private float timePassed = 0f;

    [Header("camera")]
    [SerializeField] private Camera view;


    [Header("movement")]
    public float speed = 8f;
    [SerializeField] private float jumpingPower = 8f;
    private bool isFacingRight = true;
    [SerializeField] private float MineSpeed = 2f;

    [Header("collisions")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform WaterCheck;
    [SerializeField] private LayerMask fluids;

    private float horizontal;

    public void LoadData(GameData data)
    {
        this.transform.position = data.PlayerPosition;
        Debug.Log($"[LoadData] Setting player position to: {data.PlayerPosition}");
    } 
    public void SaveData(GameData data)
    {     
        data.PlayerPosition = this.transform.position; 
        Debug.Log($"[SaveData] Saved player position: {data.PlayerPosition}"); 
        Debug.Log("saved position");
        
    } 


    // Update is called once per frame

    private void Start()
    {
        Debug.Log($"Player at position: {transform.position} in scene: {gameObject.scene.name}");
        healthBar = GameObject.Find("Healthbar");
        currentHealth = MaxHealth;
        // healthBar.SetMaxHealth(MaxHealth);  
    }
    void Update()
    {
        
        if (IsInWater())
        {   
            speed = 2f;
            jumpingPower =  1f;
            //Debug.Log("in water");
        
            if (Input.GetButton("Jump"))
                rb.velocity = new UnityEngine.Vector2(rb.velocity.x, jumpingPower);
            
            timePassed+= Time.deltaTime;
            if(timePassed>200f)
            {
                TakeDamage(10);
                timePassed = 0f;
                Debug.Log(currentHealth);
            }
        }
        else
        {
            speed = 8f;
            jumpingPower = 10f;
        }

        void TakeDamage(int damage)
        {
            currentHealth -= damage;
            // healthBar.SetHealth(currentHealth);
        }
        

        Flip();




        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && IsGrounded() )
        {
            rb.velocity = new UnityEngine.Vector2(rb.velocity.x, jumpingPower);
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new UnityEngine.Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        UnityEngine.Vector3 PlayerPosition = this.transform.position;
        

        // Vector3 mousePosition = Input.mousePosition;

        // Vector3 mouseWorldPosition = view.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -view.transform.position.z));

        // Vector2 direction = (Vector2)mouseWorldPosition - (Vector2)transform.position;

        // direction.Normalize();

        // Debug.DrawLine(transform.position, transform.position + (Vector3)direction * 10, Color.yellow);
        
    }




    // Mine function
    // Do a 2D raycast from the play towards the mouse point
    // if that hits an object
    // if that object is dirt
    // destroy that dirt piece game object

    private void FixedUpdate()
    {
        rb.velocity = new UnityEngine.Vector2(horizontal * speed, rb.velocity.y);
    }

    private bool IsGrounded() 
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.5f, groundLayer);
    }

    private bool IsInWater()
    {
        return Physics2D.OverlapCircle(WaterCheck.position, 0.5f, fluids);
    }
    private void Flip() 
    { 
        if(isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            UnityEngine.Vector3 LocalScale = transform.localScale;
            LocalScale.x *= -1f;
            transform.localScale = LocalScale;
        }


    }



}


