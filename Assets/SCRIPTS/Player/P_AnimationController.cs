using NUnit.Framework;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor.ShaderGraph.Internal;

public class P_AnimationController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer eyeSprite;
    private Animator _animator;
    private List<SpriteRenderer> _spritesRenderer;
    private List<Material> _spritesMaterials = new List<Material>();
    private float _eyeCurrentAngle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _animator = GetComponent<Animator>();
        _spritesRenderer = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
        foreach(var sprite in _spritesRenderer)
        {
            _spritesMaterials.Add(sprite.material);
        }
    }
    void Update()
    {
        RotateEye();
    }
    public void SetGrounded(bool grounded) => _animator.SetBool("IsGrounded", grounded);
    public void WalkAnimation(float movement)
    {
        _animator.SetFloat("movement", movement);
    }
    private void RotateEye()
    {
        if(eyeSprite != null)
        {
            CrosshairController point = GameObject.FindWithTag("Crosshair").GetComponent<CrosshairController>();

            Vector3 direction = (point.transform.position - eyeSprite.transform.position).normalized;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            _eyeCurrentAngle = angle;

            eyeSprite.transform.rotation = Quaternion.Euler(0, 0, angle);
            eyeSprite.flipY = angle >= 90 && angle <= 270;
        }
    }

    public void RotatePlayer(bool facingRight)
    {
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x) * (facingRight ? 1 : -1),
            transform.localScale.y,
            transform.localScale.z
        );
    }
}
