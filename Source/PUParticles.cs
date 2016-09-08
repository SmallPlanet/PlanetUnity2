using UnityEngine;
using UnityEngine.UI;
using System;

public partial class PUParticles : PUParticlesBase {

	ParticleSystemRenderer particleRenderer;
	ParticleSystem particleSystem;
	ParticleSystem.Particle[] particleArray;

	private ParticleSystem.TextureSheetAnimationModule textureSheetAnimation;
	private int textureSheetAnimationFrames;
	private Vector2 textureSheedAnimationFrameSize;

	private const int lutMax = 100;
	private Color[] colorLUT = new Color[lutMax];
	private float[] sizeLUT = new float[lutMax];
	private Vector4[] sheetLUT = new Vector4[lutMax];

	private int designedMaxParticles;


	public override void gaxb_init () {
		base.gaxb_init ();
		gameObject.name = "<Particles/>";
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
					sizeLUT [i] = particleSystem.startSize;
				}

				// support changing color over lifetime
				if (particleSystem.colorOverLifetime.enabled) {
					for (int i = 0; i < lutMax; i++) {
						colorLUT [i] = particleSystem.colorOverLifetime.color.Evaluate ((float)i / (float)lutMax);
					}
				}

				// support changing size over lifetime
				if (particleSystem.sizeOverLifetime.enabled) {
					for (int i = 0; i < lutMax; i++) {
						sizeLUT [i] = particleSystem.sizeOverLifetime.size.Evaluate ((float)i / (float)lutMax);
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


			}

		}

		ScheduleForUpdate ();
	}

	private int skipFramesMax;
	private int skipFramesCounter;

	private int frameCount = 0;
	private float dt = 0.0f;
	private float fps = 0.0f;
	private float updateRate = 2.0f;


	public override void Update() {
		if (particleSystem != null) {
			

			if (adjustToFPS) {
				const float fpsCutoff = 50.0f;
				const int maxFrameSkipAllowed = 5;

				// Get a running average of FPS to know if we should adjust things
				frameCount++;
				dt += Time.deltaTime;
				if (dt > 1.0f / updateRate) {
					fps = frameCount / dt;
					frameCount = 0;
					dt -= 1.0f / updateRate;
				
					int particleAdjust = Mathf.CeilToInt (particleSystem.maxParticles * 0.02f);

					if (fps < fpsCutoff) {
						skipFramesMax++;
						particleSystem.maxParticles -= particleAdjust;
					} else {
						skipFramesMax--;
						particleSystem.maxParticles += particleAdjust;
					}

					if (skipFramesMax < 1) {
						skipFramesMax = 1;
					}
					if (skipFramesMax > maxFrameSkipAllowed) {
						skipFramesMax = maxFrameSkipAllowed;
					}

					if (particleSystem.maxParticles > designedMaxParticles) {
						particleSystem.maxParticles = designedMaxParticles;
					}
					if (particleSystem.maxParticles < designedMaxParticles / 4) {
						particleSystem.maxParticles = designedMaxParticles / 4;
					}

					// We skip frames first; if that doesn't work, then we lower
					// the max number of particles in the system
					if (particleSystem.maxParticles < designedMaxParticles) {
						skipFramesMax = maxFrameSkipAllowed;
					}
				}

				skipFramesCounter--;
				if (skipFramesCounter > 0) {
					return;
				}
				skipFramesCounter = skipFramesMax;
			}

			//Debug.Log (string.Format ("***** {0} :: {1} fps :: {2} skips", particleSystem.maxParticles, fps, skipFramesMax));
			SetVerticesDirty ();
		}
	}

	public override void OnPopulateMesh(VertexHelper vh) {

		vh.Clear ();

		if (particleSystem == null) {
			return;
		}

		// do we need to allocate / resize our particle array?
		if (particleArray == null) {
			particleArray = new ParticleSystem.Particle[particleSystem.maxParticles];
		}
		if (particleArray.Length < particleSystem.maxParticles) {
			Array.Resize (ref particleArray, particleSystem.maxParticles);
		}

		float shapeSizeX = 1.0f;
		float shapeSizeY = 1.0f;

		if (scaleToFit) {
			if (particleSystem.shape.shapeType == ParticleSystemShapeType.Box) {
				shapeSizeX = particleSystem.shape.box.x;
				shapeSizeY = particleSystem.shape.box.y;
			} else if (particleSystem.shape.shapeType == ParticleSystemShapeType.Sphere ||
			           particleSystem.shape.shapeType == ParticleSystemShapeType.SphereShell ||
			           particleSystem.shape.shapeType == ParticleSystemShapeType.Hemisphere ||
			           particleSystem.shape.shapeType == ParticleSystemShapeType.HemisphereShell ||
			           particleSystem.shape.shapeType == ParticleSystemShapeType.Cone ||
			           particleSystem.shape.shapeType == ParticleSystemShapeType.ConeShell ||
			           particleSystem.shape.shapeType == ParticleSystemShapeType.ConeVolume ||
			           particleSystem.shape.shapeType == ParticleSystemShapeType.ConeVolumeShell) {
				shapeSizeX = particleSystem.shape.radius * 2.0f;
				shapeSizeY = particleSystem.shape.radius * 2.0f;
			}
		} else if (customScale != null) {
			shapeSizeX = customScale.Value.x;
			shapeSizeY = customScale.Value.y;
		}

		int liveParticleCount = particleSystem.GetParticles (particleArray);
		float scaleX = rectTransform.rect.width / shapeSizeX;
		float scaleY = rectTransform.rect.height / shapeSizeY;

		Vector2 rectCenter = rectTransform.rect.center;

		float avgScale = (scaleX + scaleY) * 0.5f;

		if (shim.material.HasProperty("_ScaleInfo")) {
			
			// special vertex shader which moves some calculations off of the CPU
			shim.material.SetVector ("_ScaleInfo", new Vector4 (scaleX, scaleY, 0, 0));

			UIVertex[] quad = new UIVertex[4] {
				UIVertex.simpleVert,
				UIVertex.simpleVert,
				UIVertex.simpleVert,
				UIVertex.simpleVert
			};

			quad [0].uv0 = new Vector2 (0f, 0f);
			quad [1].uv0 = new Vector2 (0f, 1f);
			quad [2].uv0 = new Vector2 (1f, 1f);
			quad [3].uv0 = new Vector2 (1f, 0f);

			quad [0].normal = new Vector3 (-1.0f, -1.0f, 0.0f);
			quad [1].normal = new Vector3 (-1.0f, 1.0f, 0.0f);
			quad [2].normal = new Vector3 (1.0f, 1.0f, 0.0f);
			quad [3].normal = new Vector3 (1.0f, -1.0f, 0.0f);


			for (int i = 0; i < liveParticleCount; i++) {
				ParticleSystem.Particle p = particleArray [i];

				float frameProgress = (p.lifetime / p.startLifetime);
				int lutIdx = Mathf.FloorToInt (frameProgress * (lutMax - 1));

				if (textureSheetAnimation.enabled) {

					Vector4 uvs = sheetLUT[lutIdx];

					quad [0].uv0.x = uvs.x;
					quad [0].uv0.y = uvs.w;
					quad [1].uv0.x = uvs.x;
					quad [1].uv0.y = uvs.y;
					quad [2].uv0.x = uvs.z;
					quad [2].uv0.y = uvs.y;
					quad [3].uv0.x = uvs.z;
					quad [3].uv0.y = uvs.w;
				}

				quad [0].position = quad [1].position = quad [2].position = quad [3].position = new Vector3 (p.position.x * scaleX + rectCenter.x, p.position.y * scaleY + rectCenter.y, p.position.z);
				quad [0].color = quad [1].color = quad [2].color = quad [3].color = colorLUT [lutIdx];
				quad [0].uv1 = quad [1].uv1 = quad [2].uv1 = quad [3].uv1 = new Vector2 (sizeLUT [lutIdx], p.rotation);

				vh.AddUIVertexQuad (quad);
			}

		} else {

			for (int i = 0; i < liveParticleCount; i++) {
				ParticleSystem.Particle p = particleArray [i];

				Vector3 pos = p.position;
				float sizeX = p.GetCurrentSize (particleSystem) / 2.0f;
				float sizeY = sizeX;
				Color color = p.GetCurrentColor (particleSystem);
				float rotation = p.rotation * Mathf.Deg2Rad;

				pos.x *= scaleX;
				pos.y *= scaleY;

				sizeX *= avgScale;
				sizeY *= avgScale;

				int idx = vh.currentVertCount;


				if (textureSheetAnimation.enabled) {
					float frameProgress = (p.lifetime / p.startLifetime);
					int lutIdx = Mathf.FloorToInt (frameProgress * (lutMax - 1));

					Vector4 uvs = sheetLUT [lutIdx];

					vh.AddVert (pos + new Vector3 (-sizeX, -sizeY).RotateZ (rotation), color, new Vector2 (uvs.x, uvs.y));
					vh.AddVert (pos + new Vector3 (-sizeX, +sizeY).RotateZ (rotation), color, new Vector2 (uvs.x, uvs.w));
					vh.AddVert (pos + new Vector3 (+sizeX, +sizeY).RotateZ (rotation), color, new Vector2 (uvs.z, uvs.w));
					vh.AddVert (pos + new Vector3 (+sizeX, -sizeY).RotateZ (rotation), color, new Vector2 (uvs.z, uvs.y));

				} else {
					
					vh.AddVert (pos + new Vector3 (-sizeX, -sizeY).RotateZ (rotation), color, new Vector2 (0f, 0f));
					vh.AddVert (pos + new Vector3 (-sizeX, +sizeY).RotateZ (rotation), color, new Vector2 (0f, 1f));
					vh.AddVert (pos + new Vector3 (+sizeX, +sizeY).RotateZ (rotation), color, new Vector2 (1f, 1f));
					vh.AddVert (pos + new Vector3 (+sizeX, -sizeY).RotateZ (rotation), color, new Vector2 (1f, 0f));

				}




				vh.AddTriangle (idx + 0, idx + 1, idx + 2);
				vh.AddTriangle (idx + 2, idx + 3, idx + 0);
			}
		}

	}

}
