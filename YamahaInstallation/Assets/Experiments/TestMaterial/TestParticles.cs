using System.ComponentModel.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Threading.Tasks;
using System.Threading;
using DG.Tweening;

struct TestParticle
{
	public Vector3 velocity;
	public Vector3 position;
	public Vector3 position2;
	public Vector3 scale;
	public Quaternion rotate;
	public Color color;
	public int life;
	public float mass;
	public float unique;
}

public class TestParticles : MonoBehaviour
{
	[SerializeField] public int instanceCount = 100;
	[SerializeField] public Mesh instanceMesh;
	[SerializeField] public Material instanceMaterial1;
	[SerializeField] public ComputeShader karnelParticles;
	[SerializeField, Range(0f, 1f)] float centerMass = 1f;

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

		_particlesBuffer = new ComputeBuffer(instanceCount, Marshal.SizeOf(typeof(TestParticle)));
		_argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
		_args[0] = (instanceMesh != null) ? (uint)instanceMesh.GetIndexCount(0) : 0;
		_args[1] = (uint)instanceCount;
		_argsBuffer.SetData(_args);

		karnelParticles.SetBuffer(_indexKarnelParticlesInit, "particlesBuffer", _particlesBuffer);
		karnelParticles.Dispatch(_indexKarnelParticlesInit, instanceCount / 8, 1, 1);

		headIndex = 0;
		tailIndex = 0;
		size = 0;
	}

	private void OnEnable()
	{
		InitParticles();

		for(int i =0; i<50; i++)
		{
			Add(new Vector3(0,0,0), 1, Color.blue);
		}
	}

	private void OnDisable()
	{
		InitParticles();
	}

	public void InitParticles()
	{
		centerMass = 1;
		karnelParticles.SetBuffer(_indexKarnelParticlesInit, "particlesBuffer", _particlesBuffer);
		karnelParticles.Dispatch(_indexKarnelParticlesInit, instanceCount / 8, 1, 1);
	}

	void Update()
	{
		instanceMaterial1.SetBuffer("particles", _particlesBuffer);

		Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial1,
			new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), _argsBuffer,
			0, null, UnityEngine.Rendering.ShadowCastingMode.On, true, this.gameObject.layer);
	}

	void FixedUpdate()
	{
		noiseTime = Time.realtimeSinceStartup;
		karnelParticles.SetVector("offsetPos", this.transform.position);
		karnelParticles.SetFloat("deltaTime", Time.deltaTime);
		karnelParticles.SetInt("numParticles", instanceCount);
		karnelParticles.SetFloat("centerMass", centerMass);
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
		if (_particlesBuffer != null)
			_particlesBuffer.Release();
		_particlesBuffer = null;

		if (_argsBuffer != null)
			_argsBuffer.Release();
		_argsBuffer = null;
	}

	public void Add(Vector3 position, float scale, Color color)
	{
		if (size == instanceCount)
		{
			RemoveFirst();
		}

		var velocity = UnityEngine.Random.onUnitSphere * 0.1f;
		var s = new Vector3(scale, scale, scale);
		TestParticle[] particles = new TestParticle[1];
		particles[0] = new TestParticle();
		particles[0].velocity = velocity;
		particles[0].position = position;
		particles[0].position2 = position - velocity;
		particles[0].rotate = Quaternion.identity;
		particles[0].scale = s;
		particles[0].life = 1;
		particles[0].color = color;
		particles[0].mass = 1;
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

		TestParticle[] particles = new TestParticle[instanceCount];
		_particlesBuffer.GetData(particles);
		var particle = particles[headIndex];
		particle.life = 0;

		TestParticle[] write = new TestParticle[1];
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
		await Task.Delay(3000, cancellationToken: token);

		InitParticles();
	}
}