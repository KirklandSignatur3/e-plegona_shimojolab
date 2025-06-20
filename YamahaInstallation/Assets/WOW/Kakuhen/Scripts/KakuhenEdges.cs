using System.ComponentModel.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Threading.Tasks;
using System.Threading;
using DG.Tweening;

struct KakuhenEdge
{
	public Vector3 velocity;
	public Vector3 position;
	public Vector3 position2;
	public Vector3 scale;
	public Vector3 scale2;
	public Quaternion rotate;
	public Color color;
	public Color color2;
	public float emission;
	public int life;
	public int death;
	public float unique;
}

public class KakuhenEdges : MonoBehaviour
{
	[SerializeField] public int instanceCount = 300;
	[SerializeField] float max = 5;
	[SerializeField] public Mesh instanceMesh;
	[SerializeField] public Material instanceMaterial1;
	[SerializeField] public ComputeShader karnelParticles;
	[SerializeField, Range(0f, 2f)] public float beatReaction = 1f;
	[SerializeField] public float emitRadiusMin = 0f;
	[SerializeField] public float emitRadiusMax = 1f;

	ComputeBuffer _particlesBuffer;
	ComputeBuffer _argsBuffer;

	int _indexKarnelParticlesInit;
	int _indexKarnelParticlesUpdate;
	uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };
	float noiseTime = 0;
	private int headIndex;
	private int tailIndex;
	private int size;

	int c = 0;

	void Awake()
	{
		_indexKarnelParticlesInit = karnelParticles.FindKernel("KernelKakuhenEdgesInit");
		_indexKarnelParticlesUpdate = karnelParticles.FindKernel("KernelKakuhenEdgesUpdate");

		_particlesBuffer = new ComputeBuffer(instanceCount, Marshal.SizeOf(typeof(KakuhenEdge)));
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
		karnelParticles.SetFloat("beatReaction", beatReaction);
		karnelParticles.SetFloat("noiseTime", noiseTime);
		karnelParticles.SetBuffer(_indexKarnelParticlesUpdate, "particlesBuffer", _particlesBuffer);
		karnelParticles.Dispatch(_indexKarnelParticlesUpdate, instanceCount / 8, 1, 1);
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

	public void Add(Color[] colors)
	{
		if (size == instanceCount)
		{
			RemoveFirst();
		}

		var rr = UnityEngine.Random.onUnitSphere;
		rr.z = 0f;
		var norm = Vector3.Normalize(rr);
		var position = norm * emitRadiusMin + rr * UnityEngine.Random.Range(0f,  (emitRadiusMax - emitRadiusMin));
		var velocity = UnityEngine.Random.onUnitSphere * 0.1f;
		var scale = UnityEngine.Random.Range(1f, 3f);
		var s = new Vector3(scale, scale, scale);
		KakuhenEdge[] particles = new KakuhenEdge[1];
		particles[0] = new KakuhenEdge();
		particles[0].velocity = velocity;
		particles[0].position = position;
		particles[0].position2 = position - velocity;
		particles[0].rotate = Quaternion.identity;
		particles[0].scale = Vector3.zero;
		particles[0].scale2 = s;
		particles[0].life = 300;
		particles[0].death = 1;
		particles[0].color = colors[0];
		particles[0].color2 = colors[1];
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

		KakuhenEdge[] particles = new KakuhenEdge[instanceCount];
		_particlesBuffer.GetData(particles);
		var particle = particles[headIndex];
		particle.life = 0;

		KakuhenEdge[] write = new KakuhenEdge[1];
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
		token.Register(() => { InitParticles(); });
		await Task.Delay(3000, cancellationToken: token);
		InitParticles();
	}
}