using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading;
using System.Collections.Generic;

public partial class PUParticles : PUParticlesBase {

	public bool emitting = true;

	public Color systemColor = Color.white;

	ParticleSystemRenderer particleRenderer;
	ParticleSystem particleSystem;

	private ParticleSystem.TextureSheetAnimationModule textureSheetAnimation;
	private int textureSheetAnimationFrames;
	private Vector2 textureSheedAnimationFrameSize;

	private const int lutMax = 1000;
	private Color[] colorLUT = new Color[lutMax];
	private float[] sizeLUT = new float[lutMax];
	private Vector4[] sheetLUT = new Vector4[lutMax];

	private const int posMax = 1000;
	private Vector3[] positionLUT = new Vector3[posMax];

	private int designedMaxParticles;

	private Thread workerThread;

	public override void gaxb_init () {
		base.gaxb_init ();
		gameObject.name = "<Particles/>";
	}

	public override void gaxb_unload() {
		if (workerThread != null) {
			workerThread.Abort ();
			workerThread = null;
		}
	}

	public override void gaxb_complete() {
		if (systemName != null) {
			GameObject particleSystemGO = GameObject.Find (systemName);
			if (particleSystemGO != null) {
				particleSystem = particleSystemGO.GetComponent<ParticleSystem> ();
				particleRenderer = particleSystemGO.GetComponent<ParticleSystemRenderer> ();

				shim.material = new Material(particleRenderer.material);

				particleSystem.simulationSpace = ParticleSystemSimulationSpace.Local;
				particleRenderer.enabled = false;

				designedMaxParticles = particleSystem.maxParticles;

				// create the LUTs for less CPU usages during mesh generation
				for (int i = 0; i < lutMax; i++) {
					colorLUT [i] = particleSystem.startColor;
					sizeLUT [i] = particleSystem.startSize * 0.5f;
				}

				// support changing color over lifetime
				if (particleSystem.colorOverLifetime.enabled) {
					for (int i = 0; i < lutMax; i++) {
						colorLUT [i] = particleSystem.colorOverLifetime.color.Evaluate (1.0f - (float)i / (float)lutMax);
					}
				}

				// support changing size over lifetime
				if (particleSystem.sizeOverLifetime.enabled) {
					for (int i = 0; i < lutMax; i++) {
						sizeLUT [i] = particleSystem.sizeOverLifetime.size.Evaluate (1.0f - (float)i / (float)lutMax) * 0.5f;
					}
				}
					

				// prepare texture sheet animation
				textureSheetAnimation = particleSystem.textureSheetAnimation;
				textureSheetAnimationFrames = 0;
				textureSheedAnimationFrameSize = Vector2.zero;
				if (textureSheetAnimation.enabled) {
					textureSheetAnimationFrames = textureSheetAnimation.numTilesX * textureSheetAnimation.numTilesY;
					textureSheedAnimationFrameSize = new Vector2(1f / textureSheetAnimation.numTilesX, 1f / textureSheetAnimation.numTilesY);

					// support texture sheet animations changing over lifetime
					for (int i = 0; i < lutMax; i++) {
						float frameProgress = (float)i / (float)lutMax;
						frameProgress = Mathf.Repeat ((1.0f - frameProgress) * textureSheetAnimation.cycleCount, 1);
						int frame = 0;

						switch (textureSheetAnimation.animation) {

						case ParticleSystemAnimationType.WholeSheet:
							frame = Mathf.FloorToInt (frameProgress * textureSheetAnimationFrames);
							break;

						case ParticleSystemAnimationType.SingleRow:
							frame = Mathf.FloorToInt (frameProgress * textureSheetAnimation.numTilesX);

							int row = textureSheetAnimation.rowIndex;
							frame += row * textureSheetAnimation.numTilesX;
							break;
						}

						frame %= textureSheetAnimationFrames;

						float x = (frame % textureSheetAnimation.numTilesX) * textureSheedAnimationFrameSize.x;
						float y = Mathf.FloorToInt (frame / textureSheetAnimation.numTilesX) * textureSheedAnimationFrameSize.y;
						float x2 = x + textureSheedAnimationFrameSize.x;
						float y2 = y - textureSheedAnimationFrameSize.y;

						sheetLUT [i] = new Vector4 (x, y, x2, y2);
					}
				}

				// If we have one of our custom UI emitters, then fill the positionLUT to allow quick spawning
				UpdatePositionTable ();

				autoEvent = new AutoResetEvent(false);

				workerThread = new Thread (ParticleThread);
				workerThread.Name = "PUParticles";
				workerThread.Start ();
			}

		}

		ScheduleForUpdate ();


		// If we emit particles ourselves, we need to clear any particles which the system may have already spawned
		if (emitMode == PlanetUnity2.ParticleEmitMode.Edge ||
		    emitMode == PlanetUnity2.ParticleEmitMode.Center ||
		    emitMode == PlanetUnity2.ParticleEmitMode.Fill ||
			emitMode == PlanetUnity2.ParticleEmitMode.Image) {
			particleSystem.enableEmission = false;
			particleSystem.Clear (true);
		}
	}
		
	private void UpdatePositionTable() {

		Vector3 rectCenter = rectTransform.rect.center;

		Color[] pixelArray = null;
		Texture2D imageMask = null;
		if (imageMaskPath != null) {
			imageMask = PlanetUnityResourceCache.GetTexture (imageMaskPath);
			if (imageMask != null) {
				pixelArray = imageMask.GetPixels ();
			}
		}

		for (int i = 0; i < posMax; i++) {

			if (emitMode == PlanetUnity2.ParticleEmitMode.Edge) {
				float s = 0.5f;
				// add a random position around the edge of the quad
				switch (UnityEngine.Random.Range (0, 4)) {
				case 0:
					positionLUT [i] = new Vector3 (UnityEngine.Random.Range (-s, s), 0.0f, -s);
					break;
				case 1:
					positionLUT [i] = new Vector3 (UnityEngine.Random.Range (-s, s), 0.0f, s);
					break;
				case 2:
					positionLUT [i] = new Vector3 (-s, 0.0f, UnityEngine.Random.Range (-s, s));
					break;
				case 3:
					positionLUT [i] = new Vector3 (s, 0.0f, UnityEngine.Random.Range (-s, s));
					break;
				}
			}

			if (emitMode == PlanetUnity2.ParticleEmitMode.Fill) {
				// add a random position on the interior of the quad
				float s = 0.45f;

				positionLUT [i] = new Vector3 (
					UnityEngine.Random.Range (-s, s), 
					0.0f,
					UnityEngine.Random.Range (-s, s)
					);
			}

			if (emitMode == PlanetUnity2.ParticleEmitMode.Image) {
				// add a random position on the interior of the supplied image

				if (pixelArray == null) {
					
					float s = 0.5f;
					positionLUT [i] = new Vector3 (
						UnityEngine.Random.Range (-s, s), 
						0.0f,
						UnityEngine.Random.Range (-s, s)
					);

				} else {

					int bail = 500;
					while (bail-- > 0) {
						int x = UnityEngine.Random.Range (0, imageMask.width);
						int y = UnityEngine.Random.Range (0, imageMask.height);
						Color c = pixelArray [y * imageMask.width + x];
						if (c.a >= 0.3f) {
							positionLUT [i] = new Vector3 (
								((float)x / (float)imageMask.width) - 0.5f, 
								0.0f,
								((float)y / (float)imageMask.height) - 0.5f
							);
							break;
						}
					}
				}

			}
		}
	}
		


	private int frameCount = 0;
	private float dt = 0.0f;
	private float fps = 0.0f;
	private float updateRate = 2.0f;

	private float emitRate;


	public override void Update() {
		if (particleSystem != null) {

			if (emitting) {
				if (emitMode == PlanetUnity2.ParticleEmitMode.Edge ||
				   emitMode == PlanetUnity2.ParticleEmitMode.Center ||
				   emitMode == PlanetUnity2.ParticleEmitMode.Fill ||
				   emitMode == PlanetUnity2.ParticleEmitMode.Image) {

					// In these modes, we are responsible for emitting the particles so that we can position them exactly
					// first, make sure normal emissions are turned off
					particleSystem.enableEmission = false;
					
					emitRate += particleSystem.emission.rate.Evaluate (0) * Time.deltaTime;

					while (emitRate > 1.0f) {
						emitRate -= 1.0f;

						if (particleSystem.particleCount < particleSystem.maxParticles) {
							ParticleSystem.EmitParams eParams = new ParticleSystem.EmitParams ();
							eParams.position = positionLUT [UnityEngine.Random.Range (0, posMax)];

							if (particleSystem.shape.randomDirection) {
								eParams.velocity = new Vector3 (UnityEngine.Random.Range (-1.0f, 1.0f), UnityEngine.Random.Range (-1.0f, 1.0f), UnityEngine.Random.Range (-1.0f, 1.0f)) * particleSystem.startSpeed;
							}
							particleSystem.Emit (eParams, 1);
						}
					}
				}
			}

			if (adjustToFPS) {
				const float fpsCutoff = 30.0f;

				// Get a running average of FPS to know if we should adjust things
				frameCount++;
				dt += Time.deltaTime;
				if (dt > 1.0f / updateRate) {
					fps = frameCount / dt;
					frameCount = 0;
					dt -= 1.0f / updateRate;
				
					int particleAdjust = Mathf.CeilToInt (particleSystem.maxParticles * 0.02f);

					if (fps < fpsCutoff) {
						particleSystem.maxParticles -= particleAdjust;
					} else {
						particleSystem.maxParticles += particleAdjust;
					}

					if (particleSystem.maxParticles > designedMaxParticles) {
						particleSystem.maxParticles = designedMaxParticles;
					}
					if (particleSystem.maxParticles < designedMaxParticles / 8) {
						particleSystem.maxParticles = designedMaxParticles / 8;
					}
					if (particleSystem.maxParticles < 1) {
						particleSystem.maxParticles = 1;
					}
				}
			}


			// Ok, the idea here is to have one worker thread always working at converting the particles to a data format suitable for submitting to a
			// vertexhelper quickly.  So here we make sure our worker thread is properly fed, and we ensure that the geometry gets updated if it need
			// to be
			if (particleArrayThreadCommState == 0) {
				if (particleArray == null) {
					particleArray = new ParticleSystem.Particle[particleSystem.maxParticles];
				}
				if (particleArray.Length < particleSystem.maxParticles) {
					Array.Resize (ref particleArray, particleSystem.maxParticles);
				}

				liveParticleCount = particleSystem.GetParticles (particleArray);
				maxParticleCount = particleSystem.maxParticles;
				
				usesOptimizedShader = shim.material.shader.name.StartsWith ("PlanetUnity/Mobile/Particles/");

				rectForThread = rectTransform.rect;


				shapeSizeXForThread = rectForThread.width;
				shapeSizeYForThread = rectForThread.height;

				positionScaleXForThread = 1.0f;
				positionScaleYForThread = 1.0f;

				if (emitMode == PlanetUnity2.ParticleEmitMode.SystemScaled || 
					emitMode == PlanetUnity2.ParticleEmitMode.Fill ||
					emitMode == PlanetUnity2.ParticleEmitMode.Edge ||
					emitMode == PlanetUnity2.ParticleEmitMode.Image ||
					emitMode == PlanetUnity2.ParticleEmitMode.Center) {
					if (particleSystem.shape.shapeType == ParticleSystemShapeType.Box) {
						shapeSizeXForThread = particleSystem.shape.scale.x;
						shapeSizeYForThread = particleSystem.shape.scale.y;
					} else if (particleSystem.shape.shapeType == ParticleSystemShapeType.Sphere ||
						particleSystem.shape.shapeType == ParticleSystemShapeType.SphereShell ||
						particleSystem.shape.shapeType == ParticleSystemShapeType.Hemisphere ||
						particleSystem.shape.shapeType == ParticleSystemShapeType.HemisphereShell ||
						particleSystem.shape.shapeType == ParticleSystemShapeType.Cone ||
						particleSystem.shape.shapeType == ParticleSystemShapeType.ConeShell ||
						particleSystem.shape.shapeType == ParticleSystemShapeType.ConeVolume ||
						particleSystem.shape.shapeType == ParticleSystemShapeType.ConeVolumeShell) {
						shapeSizeXForThread = particleSystem.shape.radius * 2.0f;
						shapeSizeYForThread = particleSystem.shape.radius * 2.0f;
					}

					if (emitMode == PlanetUnity2.ParticleEmitMode.Fill ||
						emitMode == PlanetUnity2.ParticleEmitMode.Center ||
						emitMode == PlanetUnity2.ParticleEmitMode.Image ||
						emitMode == PlanetUnity2.ParticleEmitMode.Edge) {
						// for these modes, we want to normalize to the [-1,1] space
						positionScaleXForThread = shapeSizeXForThread;
						positionScaleYForThread = shapeSizeYForThread;
					}

				} else if (emitMode == PlanetUnity2.ParticleEmitMode.SystemNone) {
					shapeSizeXForThread = rectForThread.width;
					shapeSizeYForThread = rectForThread.height;
				}

				if (customScale != null) {
					shapeSizeXForThread = customScale.Value.x;
					shapeSizeYForThread = customScale.Value.y;
				}

				textureSheetAnimationEnabledForThread = textureSheetAnimation.enabled;

				particleArrayThreadCommState = 1;
				autoEvent.Set();
			}

			if (particleArrayThreadCommState == 2) {
				SetVerticesDirty ();
			}


		}
	}


	private ParticleSystem.Particle[] particleArray;
	private int liveParticleCount = 0;
	private int maxParticleCount = 0;
	private Rect rectForThread;
	private int particleArrayThreadCommState = 0;
	private bool usesOptimizedShader = false;
	private float shapeSizeXForThread;
	private float shapeSizeYForThread;
	private float positionScaleXForThread;
	private float positionScaleYForThread;
	private bool textureSheetAnimationEnabledForThread;

	private List<UIVertex> particlesFromThread = new List<UIVertex> ();
	private List<int> particleIndicesFromThread = new List<int> ();

	private AutoResetEvent autoEvent;

	private void ParticleThread() {

		while (true) {

			// Wait until we have stuff to process
			if (particleArrayThreadCommState != 1) {
				try {
					autoEvent.WaitOne();
				}catch(Exception e){
					return;
				}
				continue;
			}

			int localMaxParticleCount = liveParticleCount;

			// 0) resize our lists to match the new live particle count, filling in base information for
			// the new particles
			if (particlesFromThread.Count > localMaxParticleCount * 4) {
				particlesFromThread.RemoveRange (0, (particlesFromThread.Count - localMaxParticleCount * 4));
			}
			if (particlesFromThread.Count < localMaxParticleCount * 4) {
				UIVertex a = UIVertex.simpleVert;
				a.uv0 = new Vector2 (0f, 0f);
				a.normal = new Vector3 (-1.0f, -1.0f, 0.0f);
				a.tangent = Vector4.zero;

				UIVertex b = UIVertex.simpleVert;
				b.uv0 = new Vector2 (0f, 1f);
				b.normal = new Vector3 (-1.0f, 1.0f, 0.0f);
				b.tangent = Vector4.zero;

				UIVertex c = UIVertex.simpleVert;
				c.uv0 = new Vector2 (1f, 1f);
				c.normal = new Vector3 (1.0f, 1.0f, 0.0f);
				c.tangent = Vector4.zero;

				UIVertex d = UIVertex.simpleVert;
				d.uv0 = new Vector2 (1f, 0f);
				d.normal = new Vector3 (1.0f, -1.0f, 0.0f);
				d.tangent = Vector4.zero;

				while (particlesFromThread.Count < localMaxParticleCount * 4) {
					particlesFromThread.Add (a);
					particlesFromThread.Add (b);
					particlesFromThread.Add (c);
					particlesFromThread.Add (d);
				}
			}

			if (particleIndicesFromThread.Count > localMaxParticleCount * 6) {
				int n = (particleIndicesFromThread.Count - localMaxParticleCount * 6);
				particleIndicesFromThread.RemoveRange (particleIndicesFromThread.Count - n, n);
			}
			if (particleIndicesFromThread.Count < localMaxParticleCount * 6) {
				int n = 0;
				if (particleIndicesFromThread.Count > 0) {
					n = particleIndicesFromThread [particleIndicesFromThread.Count - 1] + 4;
				}

				while (particleIndicesFromThread.Count < localMaxParticleCount * 6) {
					particleIndicesFromThread.Add (n);
					particleIndicesFromThread.Add (n + 1);
					particleIndicesFromThread.Add (n + 2);
					particleIndicesFromThread.Add (n + 2);
					particleIndicesFromThread.Add (n + 3);
					particleIndicesFromThread.Add (n);
					n += 4;
				}
			}



				
			// figure out any modifications to our shape / sizes
			float scaleX = rectForThread.width / shapeSizeXForThread;
			float scaleY = rectForThread.height / shapeSizeYForThread;

			float hw = rectForThread.width;
			float hh = rectForThread.height;
			float particleSize;

			Vector2 rectCenter = rectForThread.center;

			float avgScale = (scaleX + scaleY) * 0.5f;

			if (usesOptimizedShader) {

				// special vertex shader which moves some calculations off of the CPU

				int vIdx = 0;
				int lutIdx = 0;

				for (int i = 0; i < liveParticleCount; i++) {
					ParticleSystem.Particle p = particleArray [i];

					#if UNITY_5_5_OR_NEWER
					lutIdx = (int)((p.remainingLifetime / p.startLifetime) * ((float)lutMax - 1.0f));
					#else
					lutIdx = (int)((p.lifetime / p.startLifetime) * ((float)lutMax - 1.0f));
					#endif

					UIVertex a = particlesFromThread [vIdx + 0];
					UIVertex b = particlesFromThread [vIdx + 1];
					UIVertex c = particlesFromThread [vIdx + 2];
					UIVertex d = particlesFromThread [vIdx + 3];

					if (textureSheetAnimationEnabledForThread) {

						Vector4 uvs = sheetLUT [lutIdx];

						a.uv0.x = uvs.x;
						a.uv0.y = uvs.w;
						b.uv0.x = uvs.x;
						b.uv0.y = uvs.y;
						c.uv0.x = uvs.z;
						c.uv0.y = uvs.y;
						d.uv0.x = uvs.z;
						d.uv0.y = uvs.w;
					}

					particleSize = sizeLUT [lutIdx] * avgScale;

					a.position = b.position = c.position = d.position = new Vector3 (p.position.x * scaleX * positionScaleXForThread + rectCenter.x, p.position.z * scaleY * positionScaleYForThread + rectCenter.y, p.position.y);
					a.color = b.color = c.color = d.color = colorLUT [lutIdx] * systemColor;
					a.uv1 = b.uv1 = c.uv1 = d.uv1 = new Vector2 (sizeLUT [lutIdx], p.rotation);

					if (limitToInside) {
						if ((a.position.x > (hw + particleSize) || a.position.x < (-particleSize)) || (a.position.y > (hh + particleSize) || a.position.y < (-particleSize))) {
							a.tangent.x = 99;
						} else {
							a.tangent.x = 0;
						}
					}

					particlesFromThread [vIdx + 0] = a;
					particlesFromThread [vIdx + 1] = b;
					particlesFromThread [vIdx + 2] = c;
					particlesFromThread [vIdx + 3] = d;

					vIdx += 4;
				}

			} else {

				for (int i = 0; i < liveParticleCount; i++) {
					ParticleSystem.Particle p = particleArray [i];

					int vIdx = i * 4;

					#if UNITY_5_5_OR_NEWER
					float frameProgress = (p.remainingLifetime / p.startLifetime);
					#else
					float frameProgress = (p.lifetime / p.startLifetime);
					#endif

					int lutIdx = (int)(frameProgress * (lutMax - 1));

					Vector3 pos = new Vector3 (p.position.x, p.position.z, p.position.y);
					float sizeX = sizeLUT [lutIdx];
					float sizeY = sizeX;
					Quaternion rotation = Quaternion.Euler (p.rotation3D);

					pos.x *= scaleX * positionScaleXForThread;
					pos.y *= scaleY * positionScaleYForThread;

					pos.x += rectCenter.x;
					pos.y += rectCenter.y;

					sizeX *= avgScale;
					sizeY *= avgScale;

					UIVertex a = particlesFromThread [vIdx + 0];
					UIVertex b = particlesFromThread [vIdx + 1];
					UIVertex c = particlesFromThread [vIdx + 2];
					UIVertex d = particlesFromThread [vIdx + 3];

					if (textureSheetAnimationEnabledForThread) {
						Vector4 uvs = sheetLUT [lutIdx];

						a.uv0.x = uvs.x;
						a.uv0.y = uvs.w;
						b.uv0.x = uvs.x;
						b.uv0.y = uvs.y;
						c.uv0.x = uvs.z;
						c.uv0.y = uvs.y;
						d.uv0.x = uvs.z;
						d.uv0.y = uvs.w;
					}

					a.position = pos + rotation * new Vector3 (-sizeX, -sizeY);
					b.position = pos + rotation * new Vector3 (-sizeX, +sizeY);
					c.position = pos + rotation * new Vector3 (+sizeX, +sizeY);
					d.position = pos + rotation * new Vector3 (+sizeX, -sizeY);

					a.color = b.color = c.color = d.color = colorLUT [lutIdx] * systemColor;

					if (limitToInside) {
						if ((a.position.x > (hw + sizeX * 2.0f) || a.position.x < (-sizeX * 2.0f)) || (a.position.y > (hh + sizeY * 2.0f) || a.position.y < (-sizeY * 2.0f))) {
							a.tangent.x = 99;
						} else {
							a.tangent.x = 0;
						}
					}

					particlesFromThread [vIdx + 0] = a;
					particlesFromThread [vIdx + 1] = b;
					particlesFromThread [vIdx + 2] = c;
					particlesFromThread [vIdx + 3] = d;
				}
			}

			particleArrayThreadCommState = 2;
		}
	}

	public override void OnPopulateMesh(VertexHelper vh) {

		vh.Clear ();

		if (particleSystem == null) {
			return;
		}

		if (usesOptimizedShader) {
			shim.material.SetVector ("_ScaleInfo", new Vector4 (rectForThread.width / shapeSizeXForThread, rectForThread.height / shapeSizeYForThread, 0, 0));
		}

		vh.AddUIVertexStream (particlesFromThread, particleIndicesFromThread);

		if (limitToInside) {
			// run through all of the particlesFromThread, if the first vertex tangent x is not zero, this particle should be removed.
			ParticleSystem.Particle[] removeParticles = new ParticleSystem.Particle[particleSystem.particleCount];
			particleSystem.GetParticles (removeParticles);
			int lastIdx = removeParticles.Length;
			for (int i = removeParticles.Length-1; i >= 0; i--) {
				if (particlesFromThread.Count > (i << 2) && particlesFromThread [i << 2].tangent.x == 99) {
					lastIdx--;
					removeParticles [i] = removeParticles [lastIdx];
				}
			}
			particleSystem.SetParticles (removeParticles, lastIdx);
		}

		particleArrayThreadCommState = 0;
	}

}
