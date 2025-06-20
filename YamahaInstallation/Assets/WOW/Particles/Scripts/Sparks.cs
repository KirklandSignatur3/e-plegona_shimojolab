using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

struct Spark
{	public Vector3 velocity;
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

public class Sparks : MonoBehaviour
{
    [SerializeField] public int instanceCount = 1000;
    [SerializeField] public Mesh instanceMesh;
	[SerializeField] public List<MaterialSet> materialSets = new List<MaterialSet>();
    [SerializeField] public ComputeShader karnelParticles;
    ComputeBuffer _particlesBuffer;
    ComputeBuffer _argsBuffer;

    int _indexKarnelParticlesInit;
    int _indexKarnelParticlesUpdate;

    uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };

    int index = 0;

    private int headIndex;
	private int tailIndex;
	private int size;

    void Awake()
    {
        _indexKarnelParticlesInit = karnelParticles.FindKernel("KernelSparksInit");
        _indexKarnelParticlesUpdate = karnelParticles.FindKernel("KernelSparksUpdate");

        _particlesBuffer = new ComputeBuffer(instanceCount, Marshal.SizeOf(typeof(Spark)));
        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        _args[0] = (instanceMesh != null) ? (uint)instanceMesh.GetIndexCount(0) : 0;
        _args[1] = (uint)instanceCount;
        _argsBuffer.SetData(_args);

        karnelParticles.SetBuffer(_indexKarnelParticlesInit, "particlesBuffer", _particlesBuffer);
        karnelParticles.Dispatch(_indexKarnelParticlesInit, instanceCount / 8, 1, 1);
    }

    private void OnEnable()
    {
        InitParticles();
    }

    private void OnDisable()
    {
        InitParticles();
    }

    public void InitParticles()
    {
        karnelParticles.SetBuffer(_indexKarnelParticlesInit, "particlesBuffer", _particlesBuffer);
        karnelParticles.Dispatch(_indexKarnelParticlesInit, instanceCount / 8, 1, 1);
    }

    void Update()
    {
		foreach(var item in materialSets)
		{
			item.instanceMaterial.SetBuffer("particles", _particlesBuffer);
			Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, item.instanceMaterial,
			new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), _argsBuffer,
			0, null, UnityEngine.Rendering.ShadowCastingMode.On, true, item.materialLayer);
		}
    }

    void FixedUpdate()
    {
        karnelParticles.SetVector("offsetPos", this.transform.position);
        karnelParticles.SetFloat("deltaTime", Time.deltaTime);
        karnelParticles.SetInt("numParticles", instanceCount);
        karnelParticles.SetBuffer(_indexKarnelParticlesUpdate, "particlesBuffer", _particlesBuffer);
        karnelParticles.Dispatch(_indexKarnelParticlesUpdate, instanceCount / 8, 1, 1);
    }

    void OnDestory()
    {
        _particlesBuffer?.Release();
        _particlesBuffer = null;
        _argsBuffer?.Release();
        _argsBuffer = null;
    }

    public void Add(Vector3 position, Color[] colors)
	{
		if (size == instanceCount)
		{
			RemoveFirst();
		}

		var velocity = UnityEngine.Random.onUnitSphere * 0.1f;
		var s = new Vector3(0.33f, 0.33f, 0.33f);
		Particle[] particles = new Particle[1];
		particles[0] = new Particle();
		particles[0].velocity = velocity;
		particles[0].position = position;
		particles[0].position2 = position - velocity;
		particles[0].rotate = Quaternion.identity;
		particles[0].scale = s;
		particles[0].scale2 = s;
		particles[0].life = 60;
		particles[0].death = 0;
		particles[0].color = colors[1];
		particles[0].color2 = colors[0];
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
}