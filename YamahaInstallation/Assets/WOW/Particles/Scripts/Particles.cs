using System.ComponentModel.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Threading.Tasks;
using System.Threading;
using DG.Tweening;



struct Particle
{
    public Vector3 velocity;
    public Vector3 position;
    public Vector3 position2;
    public Vector3 scale;
    public Vector3 scale2;
    public Quaternion rotate;
    public Color color;
    public Color color2;
    public Color color3;
    public float emission;
    public int life;
    public int death;
    public float unique;
}

public class Particles : MonoBehaviour
{
    [SerializeField] public int instanceCount = 100;
    [SerializeField] public Mesh instanceMesh;
    [SerializeField] public List<MaterialSet> materialSets = new List<MaterialSet>();
    [SerializeField] public ComputeShader karnelParticles;
    [SerializeField, Range(0f, 1f)] float centerMass = 1f;
    [SerializeField, Range(0f, 1f)] public float beatReaction = 1f;
    [SerializeField, Range(0f, 1f)] public float kakuhen = 0f;
    [SerializeField, Range(0f, 1f)] public float scale = 1f;
    [SerializeField, Range(0, 200)] public int max = 200;

    ComputeBuffer _particlesBuffer;
    ComputeBuffer _argsBuffer;

    int _indexKarnelParticlesInit;
    int _indexKarnelParticlesUpdate;
    int _indexKarnelParticlesGravityUpdate;
    int _indexKarnelParticlesCollisionUpdate;
    uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
    float noiseTime = 0;
    private int headIndex;
    private int tailIndex;
    private int size;
    private List<float> bufferY = new List<float>();

    void Awake()
    {
        _indexKarnelParticlesInit = karnelParticles.FindKernel("KernelParticlesInit");
        _indexKarnelParticlesUpdate = karnelParticles.FindKernel("KernelParticlesUpdate");

        _indexKarnelParticlesGravityUpdate = karnelParticles.FindKernel("KernelParticlesGravityUpdate");
        _indexKarnelParticlesCollisionUpdate = karnelParticles.FindKernel("KernelParticlesCollisionUpdate");

        _particlesBuffer = new ComputeBuffer(instanceCount, Marshal.SizeOf(typeof(Particle)));
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        _args[0] = (instanceMesh != null) ? (uint)instanceMesh.GetIndexCount(0) : 0;
        _args[1] = (uint)instanceCount;
        _argsBuffer.SetData(_args);

        karnelParticles.SetBuffer(_indexKarnelParticlesInit, "particlesBuffer", _particlesBuffer);
        karnelParticles.Dispatch(_indexKarnelParticlesInit, instanceCount / 8, 1, 1);

        ClickPlayer.Instance.onClickEvent.AddListener(Click);

        InitParticles();
    }

    private void OnEnable()
    {
        InitParticles();
    }

    private void OnDisable()
    {
        InitParticles();
    }

    Sequence sequence;

    public void Click()
    {
        sequence = DOTween.Sequence()
        .Append(DOTween.To(() => beatReaction, (x) => beatReaction = x, 1f, 0.001f))
        .Append(DOTween.To(() => beatReaction, (x) => beatReaction = x, 0f, 0.75f));
    }

    public void InitParticles()
    {
        headIndex = 0;
        tailIndex = 0;
        size = 0;
        max = 0;
        centerMass = 1;
        karnelParticles.SetBuffer(_indexKarnelParticlesInit, "particlesBuffer", _particlesBuffer);
        karnelParticles.Dispatch(_indexKarnelParticlesInit, instanceCount / 8, 1, 1);
    }

    void Update()
    {
        foreach (var item in materialSets)
        {
            item.instanceMaterial.SetBuffer("particles", _particlesBuffer);
            Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, item.instanceMaterial,
            new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), _argsBuffer,
            0, null, UnityEngine.Rendering.ShadowCastingMode.On, true, item.materialLayer);
        }

        //SetBalance();
        SetScore();
    }

    void FixedUpdate()
    {
        noiseTime = Time.realtimeSinceStartup;
        karnelParticles.SetVector("offsetPos", this.transform.position);
        karnelParticles.SetFloat("deltaTime", Time.deltaTime);
        karnelParticles.SetInt("numParticles", instanceCount);
        karnelParticles.SetFloat("centerMass", centerMass);
        karnelParticles.SetFloat("beatReaction", beatReaction);
        karnelParticles.SetFloat("kakuhen", kakuhen);
        karnelParticles.SetFloat("scale", scale);
        karnelParticles.SetFloat("noiseTime", noiseTime);

        karnelParticles.SetBuffer(_indexKarnelParticlesUpdate, "particlesBuffer", _particlesBuffer);
        karnelParticles.Dispatch(_indexKarnelParticlesUpdate, instanceCount / 8, 1, 1);
        karnelParticles.SetBuffer(_indexKarnelParticlesGravityUpdate, "particlesBuffer", _particlesBuffer);
        karnelParticles.Dispatch(_indexKarnelParticlesGravityUpdate, instanceCount / 8, 1, 1);
        karnelParticles.SetBuffer(_indexKarnelParticlesCollisionUpdate, "particlesBuffer", _particlesBuffer);
        karnelParticles.Dispatch(_indexKarnelParticlesCollisionUpdate, instanceCount / 8, 1, 1);
    }

    void OnDestory()
    {
        _particlesBuffer?.Release();
        _particlesBuffer = null;
        _argsBuffer?.Release();
        _argsBuffer = null;
    }

    public void Add(Vector3 position, float scale, Color[] colors)
    {
        if (size == instanceCount)
        {
            RemoveFirst();
        }

        var velocity = UnityEngine.Random.onUnitSphere * 0.1f;
        var s = new Vector3(scale, scale, scale);
        Particle[] particles = new Particle[1];
        particles[0] = new Particle();
        particles[0].velocity = velocity;
        particles[0].position = position;
        particles[0].position2 = position - velocity;
        particles[0].rotate = Quaternion.identity;
        particles[0].scale = s;
        particles[0].scale2 = s;
        particles[0].life = 1;
        particles[0].death = 1;
        particles[0].color = colors[0];
        particles[0].color2 = colors[1];
        particles[0].color3 = colors[2];
        particles[0].emission = 0f;
        particles[0].unique = UnityEngine.Random.Range(0.01f, 1f);
        _particlesBuffer.SetData(particles, 0, tailIndex, 1);
        tailIndex = (tailIndex + 1) % instanceCount;
        size++;
    }

    public void RemoveFirst()
    {
        if (size == 0)
        {
            throw new InvalidOperationException("The ring buffer is empty.");
        }

        Particle[] particles = new Particle[instanceCount];
        _particlesBuffer.GetData(particles);
        var particle = particles[headIndex];
        particle.life = 0;

        Particle[] write = new Particle[1];
        write[0] = particle;
        _particlesBuffer.SetData(write, 0, headIndex, 1);
        headIndex = (headIndex + 1) % instanceCount;
        size--;
    }

    public int Count
    {
        get { return size; }
    }

    [ContextMenu("RefreshAll")]
    public async Task RefreshAll(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        centerMass = 0;
        token.Register(() => { InitParticles(); });
        await Task.Delay(2000, cancellationToken: token);
        InitParticles();
    }

    public void SetScore()
    {
        var config = SharedConfig.Instance.config;
        var context = SharedContext.Instance.context;
        var s = (float)Math.Min(context.score, config.maxScore) / (float)config.maxScore;
        s *= 0.8f;
        scale = s;
        max = (int)(s * (float)config.maxCenterBall) + config.minCenterBall;
        while (Count >= max) RemoveFirst();
    }
}