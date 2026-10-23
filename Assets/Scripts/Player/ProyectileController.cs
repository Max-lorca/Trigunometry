using System.Collections.Generic;
using UnityEngine;

public class ProyectileController : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] public float damage = 10f;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float waveAmplitude = 0.5f; // default 0.5
    [SerializeField] private float waveFrequency = 5f; // default 5

    [Header("Configuración Audio")]
    [SerializeField] private List<AudioClip> audiosExplosion;
    private AudioSource _audioSource;

    [SerializeField] private TipoProyectil tipoProyectil;

    [Header("Particulas")] [SerializeField]
    private GameObject explotionParticlePrefab;

    private Vector2 direction;
    private WeaponData weaponData;
    private float time;
    private float wavePhase;
    private Vector3 startPosition;

    private TrailRenderer _trail;
    private bool _liberado; // evita devolver el mismo proyectil al pool dos veces

    // Awake (no Start): ResetProyectile() puede llamarse antes de que corra Start()
    // en un proyectil recién creado, y necesita la referencia a la estela.
    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _trail = GetComponentInChildren<TrailRenderer>(true);
    }

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        time += Time.deltaTime;
        Vector3 basePosition = startPosition + (Vector3)(direction * speed * time);
        float wave = Mathf.Sin(time * waveFrequency + wavePhase) * waveAmplitude;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x).normalized;
        transform.position = basePosition + (Vector3)(perpendicular * wave);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    public void SetWeaponData(WeaponData wd)
    {
        weaponData = wd;
    }
    public void SetAmplitude(float amplitude)
    {
        waveAmplitude = amplitude;
    }
    public void SetFrequency(float frequency)
    {
        waveFrequency = frequency;
    }

    public void ResetProyectile(Vector3 spawnPosition)
    {
        _liberado = false;
        startPosition = spawnPosition;
        transform.position = spawnPosition;
        time = 0f;
        gameObject.SetActive(true);

        // La estela recuerda sus puntos anteriores: sin esto, al reutilizar el proyectil
        // dibuja una línea desde donde murió hasta el arma (efecto de arrastre/teletransporte).
        if (_trail != null) _trail.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_liberado) return;

        if (other.CompareTag("Enemy"))
        {
            EnemyBase melee = other.GetComponent<EnemyBase>();
            if (melee != null)
            {
                melee.TomarDaño(damage, this.transform.position, tipoProyectil);
                CrearParticulas();
                NotificarImpacto();
                ReproducirSonidoImpacto();
            }
            ReleaseProjectile();
        }
        else if (other.CompareTag("Pared") || other.CompareTag("Ground"))
        {
            ReproducirSonidoImpacto();
            CrearParticulas();
            ReleaseProjectile();
        }
    }

    private void NotificarImpacto()
    {
        TimeStopManager ts = FindFirstObjectByType<TimeStopManager>();
        if (ts != null && ts.IsAnalysisActive)
            return;

        WeaponShoot ws = FindFirstObjectByType<WeaponShoot>();
        if (ws != null) ws.OnHitEnemy();
    }

    private void CrearParticulas()
    {
        if (explotionParticlePrefab != null)
            Instantiate(explotionParticlePrefab, transform.position, Quaternion.identity);
    }

    private void ReleaseProjectile()
    {
        _liberado = true;

        if (weaponData != null)
            weaponData.ReleaseProjectile(gameObject);
        else
            Destroy(gameObject);
    }

    // El AudioSource del proyectil NO sirve para el impacto: al devolverlo al pool se
    // desactiva el objeto y Unity corta el sonido al instante. Por eso el sonido se
    // reproduce en un objeto temporal que se destruye cuando termina el clip.
    private void ReproducirSonidoImpacto()
    {
        if (audiosExplosion == null || audiosExplosion.Count == 0) return;

        AudioClip clip = audiosExplosion[Random.Range(0, audiosExplosion.Count)];
        float orignalVolume = _audioSource.volume;
        _audioSource.volume = 1f;
        if (clip == null) return;

        float pitch = Random.Range(0.9f, 1.1f);

        GameObject go = new GameObject("SonidoImpacto");
        go.transform.position = transform.position;

        AudioSource src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.clip = clip;
        src.pitch = pitch;

        // Copia los ajustes del AudioSource del proyectil (si lo tiene)
        if (_audioSource != null)
        {
            src.volume = _audioSource.volume;
            src.spatialBlend = _audioSource.spatialBlend;
            src.outputAudioMixerGroup = _audioSource.outputAudioMixerGroup;
        }

        src.Play();
        Destroy(go, clip.length / Mathf.Max(0.01f, pitch) + 0.1f);
        _audioSource.volume = orignalVolume;
    }

    public void Configure(float newDamage, float newSpeed, float newAmplitude, float newFrequency, float phase = 0f)
    {
        damage = newDamage;
        speed = newSpeed;
        waveAmplitude = newAmplitude;
        waveFrequency = newFrequency;
        wavePhase = phase;
    }
}