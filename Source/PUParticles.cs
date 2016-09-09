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

	private const int posMax = 1000;
	private Vector3[] positionLUT = new Vector3[posMax];

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
					sizeLUT [i] = particleSystem.startSize * 0.5f;
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
						sizeLUT [i] = particleSystem.sizeOverLifetime.size.Evaluate ((float)i / (float)lutMax) * 0.5f;
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

			}

		}

		ScheduleForUpdate ();
	}


	private void UpdatePositionTable() {

		Vector3 rectCenter = rectTransform.rect.center;

		for (int i = 0; i < posMax; i++) {

			if (emitMode == PlanetUnity2.ParticleEmitMode.Edge) {
				float s = 0.5f;
				// add a random position around the edge of the quad
				switch (UnityEngine.Random.Range (0, 4)) {
				case 0:
					positionLUT [i] = new Vector3 (UnityEngine.Random.Range (-s, s), -s, 0.0f);
					break;
				case 1:
					positionLUT [i] = new Vector3 (UnityEngine.Random.Range (-s, s), s, 0.0f);
					break;
				case 2:
					positionLUT [i] = new Vector3 (-s, UnityEngine.Random.Range (-s, s), 0.0f);
					break;
				case 3:
					positionLUT [i] = new Vector3 (s, UnityEngine.Random.Range (-s, s), 0.0f);
					break;
				}
			}

			if (emitMode == PlanetUnity2.ParticleEmitMode.Fill) {
				// add a random position on the interior of the quad
				float s = 0.45f;

				positionLUT [i] = new Vector3 (
					UnityEngine.Random.Range (-s, s), 
					UnityEngine.Random.Range (s, s), 
					0.0f);
			}

			// TODO: particles positions based on an image in the quad
			/*
				if (emitMode == PlanetUnity2.ParticleEmitMode.Image) {
					// add a random position on the interior of the quad
				}*/
		}
	}




	private int skipFramesMax;
	private int skipFramesCounter;

	private int frameCount = 0;
	private float dt = 0.0f;
	private float fps = 0.0f;
	private float updateRate = 2.0f;

	private float emitRate;


	public override void Update() {
		if (particleSystem != null) {

			if (emitMode == PlanetUnity2.ParticleEmitMode.Edge || emitMode == PlanetUnity2.ParticleEmitMode.Fill) {

				// In these modes, we are responsible for emitting the particles so that we can position them exactly
				// first, make sure normal emissions are turned off
				particleSystem.enableEmission = false;
					
				emitRate += particleSystem.emission.rate.Evaluate(0) * Time.deltaTime;

				while (emitRate > 1.0f) {
					emitRate -= 1.0f;
					ParticleSystem.EmitParams eParams = new ParticleSystem.EmitParams ();
					eParams.position = positionLUT [UnityEngine.Random.Range (0, posMax)];
					particleSystem.Emit (eParams, 1);
				}
			}

			if (adjustToFPS) {
				const float fpsCutoff = 50.0f;
				const int maxFrameSkipAllowed = 3;

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
					if (particleSystem.maxParticles < 1) {
						particleSystem.maxParticles = 1;
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

		float shapeSizeX = rectTransform.rect.width;
		float shapeSizeY = rectTransform.rect.height;

		float positionScaleX = 1.0f;
		float positionScaleY = 1.0f;

		if (emitMode == PlanetUnity2.ParticleEmitMode.SystemScaled || 
			emitMode == PlanetUnity2.ParticleEmitMode.Fill ||
			emitMode == PlanetUnity2.ParticleEmitMode.Edge) {
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

			if (emitMode == PlanetUnity2.ParticleEmitMode.Fill ||
			   	emitMode == PlanetUnity2.ParticleEmitMode.Edge) {
				// for these modes, we want to normalize to the [-1,1] space
				positionScaleX = shapeSizeX;
				positionScaleY = shapeSizeY;
			}

		} else if (emitMode == PlanetUnity2.ParticleEmitMode.SystemNone) {
			shapeSizeX = rectTransform.rect.width;
			shapeSizeY = rectTransform.rect.height;
		}

		if (customScale != null) {
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

			Vector2 uvA = new Vector2 (0f, 0f);
			Vector2 uvB = new Vector2 (0f, 1f);
			Vector2 uvC = new Vector2 (1f, 1f);
			Vector2 uvD = new Vector2 (1f, 0f);

			Vector3 normalA = new Vector3 (-1.0f, -1.0f, 0.0f);
			Vector3 normalB = new Vector3 (-1.0f, 1.0f, 0.0f);
			Vector3 normalC = new Vector3 (1.0f, 1.0f, 0.0f);
			Vector3 normalD = new Vector3 (1.0f, -1.0f, 0.0f);

			for (int i = 0; i < liveParticleCount; i++) {
				ParticleSystem.Particle p = particleArray [i];

				float frameProgress = (p.lifetime / p.startLifetime);
				int lutIdx = Mathf.FloorToInt (frameProgress * (lutMax - 1));

				if (textureSheetAnimation.enabled) {

					Vector4 uvs = sheetLUT[lutIdx];

					uvA.x = uvs.x;
					uvA.y = uvs.w;
					uvB.x = uvs.x;
					uvB.y = uvs.y;
					uvC.x = uvs.z;
					uvC.y = uvs.y;
					uvD.x = uvs.z;
					uvD.y = uvs.w;
				}


				Vector3 position = new Vector3 (p.position.x * scaleX * positionScaleX + rectCenter.x, p.position.y * scaleY * positionScaleY + rectCenter.y, p.position.z);
				Color c = colorLUT [lutIdx];
				Vector2 uv1 = new Vector2 (sizeLUT [lutIdx], p.rotation);

				int startIndex = vh.currentVertCount;

				vh.AddVert (position, c, uvA, uv1, normalA, Vector4.zero);
				vh.AddVert (position, c, uvB, uv1, normalB, Vector4.zero);
				vh.AddVert (position, c, uvC, uv1, normalC, Vector4.zero);
				vh.AddVert (position, c, uvD, uv1, normalD, Vector4.zero);

				vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
				vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);

			}

		} else {

			for (int i = 0; i < liveParticleCount; i++) {
				ParticleSystem.Particle p = particleArray [i];

				Vector3 pos = p.position;
				float sizeX = p.GetCurrentSize (particleSystem) / 2.0f;
				float sizeY = sizeX;
				Color color = p.GetCurrentColor (particleSystem);
				float rotation = p.rotation * Mathf.Deg2Rad;

				pos.x *= scaleX * positionScaleX;
				pos.y *= scaleY * positionScaleY;

				pos.x += rectCenter.x;
				pos.y += rectCenter.y;

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
