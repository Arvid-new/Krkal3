using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;
using SlimDX;
using Krkal.GEn.Base;
using System.Runtime.InteropServices;

namespace Krkal.GEn
{
	class KrkalScene : IResource
	{
		GenForm _gameForm;
		GraphicsDeviceManager _manager;
		World _world;
		KrkalTextureAtlas _atlas;
		Ramp _ramp;
		KDTree _tree;
		Camera _camera;
		internal Camera Camera {
			get { return _camera; }
		}
		GlobalLight _globalLight;

		int _depthPeelingCount = 2;
		public int DepthPeelingCount {
			get { return _depthPeelingCount; }
			set { _depthPeelingCount = value; }
		}

		Dictionary<char, ElementSource> _elSources = new Dictionary<char, ElementSource>();
		List<Elem> _elems = new List<Elem>();

		Device Device {
			get { return _manager.Direct3D9.Device; }
		}


		Texture _nBuff;
		Texture _zBuff1, _zBuff2;
		Texture _diffuseBuff;
		Texture _lightOutput;
		Surface _nBuffS;
		Surface _zBuff1S, _zBuff2S;
		Surface _diffuseBuffS;
		Surface _lightOutputS;

		Effect _effect;
		EffectParametersMap _handles;

		IndexBuffer _indexBuffer;
		VertexDeclaration _declaration;
		VertexBuffer _geometryVB;
		VertexBuffer _instanceVB;

		Mesh _sphere;
		Mesh _cylinder;
		VertexDeclaration _globalLightDeclaration;

		List<Element> _elementsToDraw = new List<Element>();
		int _maxInstanceCount = 1024;
		List<SphereLight> _sphereLightsToDraw = new List<SphereLight>();
		List<MeshElement> _meshesToDraw = new List<MeshElement>();

		// CONSTRUCTOR
		public KrkalScene(GenForm gameForm) {
			_gameForm = gameForm;
			_manager = _gameForm.GraphicsDeviceManager;
			_ramp = new Ramp(_manager);
			_atlas = new KrkalTextureAtlas(_manager, _ramp);
			gameForm.Resources.Add(this);
			gameForm.Resources.Add(_ramp);
			gameForm.Resources.Add(_atlas);
		}

		public void Initialize(GraphicsDeviceManager graphicsDeviceManager) {
			CreateWorld();
			CreateElSources();
			CreateElements();
			_tree = new KDTree(_world, _elems);
			InitializeCamera();
			_globalLight = new GlobalLight(new Vector3(-0.7f, -0.5f, -0.3f), new Vector3(0.6f, 0.6f, 0.6f), 0.2f, _camera);

			String errors;
			_effect = Effect.FromFile(Device, @"..\Data\Shaders\Krkal.fx", null, null, null, ShaderFlags.OptimizationLevel3, null, out errors);

			_declaration = _manager.Direct3D9.CreateVertexDeclaration(typeof(GeometryVertex), typeof(InstanceVertex));
			_globalLightDeclaration = _manager.Direct3D9.CreateVertexDeclaration(typeof(GeometryVertex));

			_sphere = Mesh.CreateSphere(Device, 1, 10, 10);
			//_sphere = Mesh.CreateTeapot(Device);
			_cylinder = Mesh.CreateCylinder(Device, 1, 0, 1, 12, 1);
		}

		public void LoadContent() {
			_nBuff = new Texture(Device, _manager.ScreenWidth, _manager.ScreenHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
			_zBuff1 = new Texture(Device, _manager.ScreenWidth, _manager.ScreenHeight, 1, Usage.RenderTarget, Format.R32F, Pool.Default);
			_zBuff2 = new Texture(Device, _manager.ScreenWidth, _manager.ScreenHeight, 1, Usage.RenderTarget, Format.R32F, Pool.Default);
			_diffuseBuff = new Texture(Device, _manager.ScreenWidth, _manager.ScreenHeight, 1, Usage.RenderTarget, Format.A8R8G8B8, Pool.Default);
			_lightOutput = new Texture(Device, _manager.ScreenWidth, _manager.ScreenHeight, 1, Usage.RenderTarget, Format.X8R8G8B8, Pool.Default);
			_nBuffS = _nBuff.GetSurfaceLevel(0);
			_zBuff1S = _zBuff1.GetSurfaceLevel(0);
			_zBuff2S = _zBuff2.GetSurfaceLevel(0);
			_diffuseBuffS = _diffuseBuff.GetSurfaceLevel(0);
			_lightOutputS = _lightOutput.GetSurfaceLevel(0);
			_effect.OnResetDevice();
			_handles = new EffectParametersMap(typeof(MainEffectParams), _effect);
			LoadIndexBuffer();
			LoadVertexBuffers();

			_camera.ViewPortSize = new Vector2(_manager.ScreenWidth, _manager.ScreenHeight);

			PrepareElementsToDraw();
		}


		public void UnloadContent() {
			FreeResources();
			_effect.OnLostDevice();
		}

		public void Dispose() {
			FreeResources();
			if (_effect != null) 
				_effect.Dispose();
			_effect = null;
			if (_declaration != null)
				_declaration.Dispose();
			_declaration = null;
			if (_globalLightDeclaration != null)
				_globalLightDeclaration.Dispose();
			_globalLightDeclaration = null;
			if (_sphere != null)
				_sphere.Dispose();
			_sphere = null;
			if (_cylinder != null)
				_cylinder.Dispose();
			_cylinder = null;
		}

		private void FreeResources() {
			if (_zBuff1 != null) {
				_nBuff.Dispose();
				_nBuff = null;
				_zBuff1.Dispose();
				_zBuff1 = null;
				_zBuff2.Dispose();
				_zBuff2 = null;
				_diffuseBuff.Dispose();
				_diffuseBuff = null;
				_lightOutput.Dispose();
				_lightOutput = null;
				_indexBuffer.Dispose();
				_indexBuffer = null;
				_instanceVB.Dispose();
				_instanceVB = null;
				_geometryVB.Dispose();
				_geometryVB = null;
				_nBuffS.Dispose();
				_nBuffS = null;
				_zBuff1S.Dispose();
				_zBuff1S = null;
				_zBuff2S.Dispose();
				_zBuff2S = null;
				_diffuseBuffS.Dispose();
				_diffuseBuffS = null;
				_lightOutputS.Dispose();
				_lightOutputS = null;
			}
		}


		private void LoadIndexBuffer() {
			_indexBuffer = new IndexBuffer(Device, 6 * sizeof(int), Usage.WriteOnly, Pool.Default, false);

			using (DataStream stream = _indexBuffer.Lock(0, 0, LockFlags.None)) {
				stream.Write(0); // 1st triangle	0,0
				stream.Write(1); //					1,0
				stream.Write(2); //					1,1
				stream.Write(0); // 2nd triangle	0,0
				stream.Write(2); //					1,1
				stream.Write(3); //					0,1

				_indexBuffer.Unlock();
			}
		}


		private void LoadVertexBuffers() {
			_geometryVB = new VertexBuffer(Device, 4 * GeometryVertex.SizeInBytes, Usage.WriteOnly, VertexFormat.None, Pool.Default);
			CreateInstanceVB();			

			using (DataStream stream = _geometryVB.Lock(0, 0, LockFlags.None)) {
				stream.Write(new GeometryVertex(new Vector2(0, 0)));
				stream.Write(new GeometryVertex(new Vector2(1, 0)));
				stream.Write(new GeometryVertex(new Vector2(1, 1)));
				stream.Write(new GeometryVertex(new Vector2(0, 1)));

				_geometryVB.Unlock();
			}
		}

		private void CreateInstanceVB() {
			_instanceVB = new VertexBuffer(Device, _maxInstanceCount * InstanceVertex.SizeInBytes, Usage.Dynamic | Usage.WriteOnly, VertexFormat.None, Pool.Default);
		}



		private void CreateWorld() {
			_world = new World(3);
			_world.SetBoundPlane(0, Vector3.UnitX, 2 + 4);
			_world.SetBoundPlane(1, Vector3.UnitY, 1 + 4);
			_world.SetBoundPlane(2, Vector3.UnitZ, 1 + 2);
		}

		private void CreateElements() {
			int dim1 = TestLevel.level.GetLength(0);
			int dim2 = TestLevel.level.GetLength(1);
			int dim3 = TestLevel.level[0,0].Length;
			for (int level = 0; level < dim1; level++) {
				for (int g = 0; g < dim2; g++) {
					for (int f = 0; f < dim3; f++) {
						char el = TestLevel.level[level, g][f];
						ElementSource elS;
						if (_elSources.TryGetValue(el, out elS)) {
							Vector3 pos = new Vector3((f-dim3/2)*40, (g-dim2/2)*40, (dim1/2 - level)*20);
							_elems.Add(new Element(pos, elS));
						}
					}
				}
			}

			_elementsToDraw.Add(new Element(new Vector3(10, 10, -204), _elSources['H']));
			_elementsToDraw.Add(new Element(new Vector3(-50, -50, -160), _elSources['H']));
			_elementsToDraw.Add(new Element(new Vector3(-30, -20, -250), _elSources['S']) { Scale = 2.8f });
			_elementsToDraw.Add(new Element(new Vector3(50, -150, -60), _elSources['K']));
			_elementsToDraw.Add(new Element(new Vector3(-30, 35, -250), _elSources['Z']));


			_elementsToDraw.Add(new Element(new Vector3(70, -40, 0), _elSources['n']));
			_elementsToDraw.Add(new Element(new Vector3(50, -50, 0), _elSources['d']));
			_elementsToDraw.Add(new Element(new Vector3(40, -30, 0), _elSources['P']));


			_elementsToDraw.Add(new Element(new Vector3(-140, -130, -10), _elSources['P']));
			_elementsToDraw.Add(new Element(new Vector3(-140, -130, -40), _elSources['P']));

			_elementsToDraw.Add(new Element(new Vector3(-100, 0, 0), _elSources['P']) { Scale = 2.8f });

			_elementsToDraw.Add(new Element(new Vector3(-379, 279, 0), _elSources['P']));

			for (int g = -10; g <= 10; g++) {
				for (int f = -15; f <= 15; f++) {
					_elementsToDraw.Add(new Element(new Vector3(f * 40 - 100, g * 40 - 100, -350), _elSources['K']));
					_elementsToDraw.Add(new Element(new Vector3(f * 40 - 100, g * 40 - 100, -250), _elSources['k']));
				}
			}
			//for (int g = -10; g <= 10; g++) {
			//    for (int f = -15; f <= 15; f++) {
			//        _elementsToDraw.Add(new Element(new Vector3(f * 40 - 100, g * 40 - 100, -100), _elSources['S']));
			//    }
			//}

			_sphereLightsToDraw.Add(new SphereLight(new Vector3(-50, 40, -160), new Vector3(1, 1, 0), 100));
			_sphereLightsToDraw.Add(new SphereLight(new Vector3(-150, -260, -250), new Vector3(1.5f, 0, 0), 150));
			_sphereLightsToDraw.Add(new SphereLight(new Vector3(100, 40, -280), new Vector3(0, 1, 1), 100));
			_sphereLightsToDraw.Add(new SphereLight(new Vector3(70, 60, -260), new Vector3(1, 0, 0), 100));
			_sphereLightsToDraw.Add(new SphereLight(new Vector3(-180, 0, -250), new Vector3(1, 1, 0), 150));
			_sphereLightsToDraw.Add(new SphereLight(new Vector3(180, -240, -300), new Vector3(0, 1, 0), 100));
			_sphereLightsToDraw.Add(new SphereLight(new Vector3(123, -300, -200), new Vector3(1f, 1f, 0f), 140));

			int max = 20;
			for (int f = 0; f < max; f++) {
				double a = f * 2 * Math.PI / max;
				_sphereLightsToDraw.Add(new SphereLight(new Vector3((float)Math.Sin(a) * 240 - 200, (float)Math.Cos(a) * 250 - 100, -320), new Vector3(1, 1, 1), 60));
			}

			foreach (Light light in _sphereLightsToDraw) {
				light.SetAttenFunc("RCF PowerAtten 1 1.8", null, _ramp);
				//light.SetAttenFunc("RCF CellShade 0.5 0.2 0.2 0.8 0", null, _ramp);
			}

			MeshElement mEl;
			mEl = new MeshElement(new Vector3(-140,-300,-310), (MeshSource)_atlas.Manager.GetOrLoadData(@"..\Data\TestMesh\Models\treasure_chest_2.x", KrkalDataType.MeshSource), (ManagedTexture)_atlas.Manager.GetOrLoadData(@"..\Data\TestMesh\Textures\treasure_chest_2.png" , KrkalDataType.ManagedTexture));
			mEl.Transform = Matrix.RotationX(2)* Matrix.RotationZ(1) * Matrix.Scaling(150, 150, 150);
			mEl.SetMaterial("DefaultMaterialInfo", _ramp);
			_meshesToDraw.Add(mEl);

			mEl = new MeshElement(new Vector3(150, -280, -250), (MeshSource)_atlas.Manager.GetOrLoadData(@"..\Data\TestMesh\Models\ship1.x", KrkalDataType.MeshSource), (ManagedTexture)_atlas.Manager.GetOrLoadData(@"..\Data\TestMesh\Textures\ship1UV.bmp", KrkalDataType.ManagedTexture));
			mEl.Transform = Matrix.RotationX(2) * Matrix.RotationZ(1) * Matrix.Scaling(7, 7, 7);
			mEl.SetMaterial("DefaultMaterialInfo", _ramp);
			mEl.MeshSource.Mesh.ComputeNormals();
			_meshesToDraw.Add(mEl);

			mEl = new MeshElement(new Vector3(-140, 100, -350), (MeshSource)_atlas.Manager.GetOrLoadData(@"..\Data\TestMesh\Models\treasure_chest_2.x", KrkalDataType.MeshSource), (ManagedTexture)_atlas.Manager.GetOrLoadData(@"..\Data\TestMesh\Textures\treasure_chest_2.png", KrkalDataType.ManagedTexture));
			mEl.Transform = Matrix.RotationX(2.5f) * Matrix.RotationZ(3) * Matrix.RotationY(1) * Matrix.Scaling(200, 200, 200);
			mEl.SetMaterial("DefaultMaterialInfo", _ramp);
			_meshesToDraw.Add(mEl);

			mEl = new MeshElement(new Vector3(-50, 100, -250), (MeshSource)_atlas.Manager.GetOrLoadData(@"..\Data\TestMesh\Models\ship1.x", KrkalDataType.MeshSource), (ManagedTexture)_atlas.Manager.GetOrLoadData(@"..\Data\TestMesh\Textures\ship1UV.bmp", KrkalDataType.ManagedTexture));
			mEl.Transform = Matrix.RotationX((float)Math.PI / 2) * Matrix.Scaling(5,5,5);
			mEl.SetMaterial("DefaultMaterialInfo", _ramp);
			mEl.MeshSource.Mesh.ComputeNormals();
			_meshesToDraw.Add(mEl);
		}



		private void CreateElSources() {
			_elSources.Add('B', new ElementSource(_atlas, @"..\Data\TestData\bomba01", "stena"));
			_elSources.Add('d', new ElementSource(_atlas, @"..\Data\TestData\dira", "podlaha"));
			_elSources.Add('D', new ElementSource(_atlas, @"..\Data\TestData\dracek6", "stena"));
			_elSources.Add('H', new ElementSource(_atlas, @"..\Data\TestData\hlina", "stena"));
			_elSources.Add('P', new ElementSource(_atlas, @"..\Data\TestData\kamenyA", "podlaha"));
			_elSources.Add('k', new ElementSource(_atlas, @"..\Data\TestData\klic_cerveny", "stena"));
			_elSources.Add('K', new ElementSource(_atlas, @"..\Data\TestData\koule", "stena"));
			_elSources.Add('n', new ElementSource(_atlas, @"..\Data\TestData\naslapnapodlaha0", "podlaha"));
			_elSources.Add('S', new ElementSource(_atlas, @"..\Data\TestData\stena0000", "stena"));
			_elSources.Add('Z', new ElementSource(_atlas, @"..\Data\TestData\zamekB", "stena"));
		}


	
		private void InitializeCamera() {
			_camera = new Camera();
			_camera.PerspectiveDirection = new Vector3(-0.35f, -0.35f, -1);
			_camera.Position = new Vector3(0, 0, 0);
		}




		public void Draw() {
					
			_effect.Technique = "KrkalTechnique";

			SetUniformParameters();

			_effect.Begin();

			for (int peelingPhase = 0; peelingPhase < _depthPeelingCount; peelingPhase++) {
				ClearIt(peelingPhase == 0);
				DoPeelingPhase(peelingPhase);
			}

			_effect.End();
		}



		private void ClearIt(bool firstPhase) {
			if (firstPhase) {

				Device.Clear(ClearFlags.Target | ClearFlags.ZBuffer | ClearFlags.Stencil, new Color4(0, 0, 0, 0), 1.0f, 0);

			} else {

				// clear stencil for nontransparent pixels
				_effect.BeginPass(5); 
				Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
				_effect.EndPass();

				Device.Clear(ClearFlags.ZBuffer, new Color4(0, 0, 0, 0), 1.0f, 0);
			}
		}



		private void DoPeelingPhase(int peelingPhase) {
			Texture zBuff = ((peelingPhase & 1) == 0) ? _zBuff1 : _zBuff2;
			Surface zBuffS = ((peelingPhase & 1) == 0) ? _zBuff1S : _zBuff2S;


			Device.SetRenderState(RenderState.StencilWriteMask, (1 << peelingPhase));

			// Phase 0 -> GBUFF
			Device.SetRenderTarget(1, zBuffS);
			Device.SetRenderTarget(0, _nBuffS);
			SetKrkalBuffers(_declaration);
			SetKrkalFrequency();

			_effect.BeginPass(peelingPhase == 0 ? 0 : 3);
			Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
			_effect.EndPass();

			// MESH Phase
			ClearFrequency();
			Device.SetRenderTarget(0, _diffuseBuffS);
			Device.SetRenderTarget(1, null);
			Device.Clear(ClearFlags.Target, new Color4(0, 0, 0, 0), 1, 0);
			Device.SetRenderTarget(1, _nBuffS);
			Device.SetRenderTarget(2, zBuffS);

			DrawMeshElements(peelingPhase == 0 ? 7 : 8);

			Device.SetRenderState(RenderState.StencilMask, (1 << peelingPhase));

			// Phase 1 -> DiffuseColor
			SetKrkalBuffers(_declaration);
			SetKrkalFrequency();
			Device.SetRenderTarget(1, null);
			Device.SetRenderTarget(2, null);
			Device.SetRenderTarget(0, _diffuseBuffS);

			_effect.BeginPass(1);
			Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
			_effect.EndPass();

			ClearFrequency();

			// Phase 2 -> Local Lights
			Device.SetRenderTarget(0, _lightOutputS);
			Device.Clear(ClearFlags.Target, new Color4(0, 0, 0, 0), 1, 0);
			_effect.SetTexture(_handles[MainEffectParams.t_zBuff], zBuff);

			DrawSphereLights();

			// Phase 3 -> Global Light -> Output
			SetKrkalBuffers(_globalLightDeclaration);

			_manager.Direct3D9.ResetRenderTarget();

			_effect.BeginPass(peelingPhase == 0 ? 2 : 4);
			Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
			_effect.EndPass();

		}



		private void ClearFrequency() {
			Device.SetStreamSourceFrequency(0, 1, StreamSource.IndexedData);
			Device.SetStreamSourceFrequency(1, 1, StreamSource.IndexedData);
		}

		private void SetKrkalFrequency() {
			Device.SetStreamSourceFrequency(0, _elementsToDraw.Count, StreamSource.IndexedData);
			Device.SetStreamSourceFrequency(1, 1, StreamSource.InstanceData);
		}



		private void SetKrkalBuffers(VertexDeclaration declaration) {
			Device.SetStreamSource(0, _geometryVB, 0, GeometryVertex.SizeInBytes);
			Device.SetStreamSource(1, _instanceVB, 0, InstanceVertex.SizeInBytes);
			Device.Indices = _indexBuffer;
			Device.VertexDeclaration = declaration;
		}



		private void SetUniformParameters() {
			_effect.SetValue(_handles[MainEffectParams.ProjectionMatrix], _camera.Proj);
			_effect.SetValue(_handles[MainEffectParams.ViewMatrix], _camera.View);
			_effect.SetValue(_handles[MainEffectParams.ProjZScale], _camera.ProjZScale);
			_effect.SetValue(_handles[MainEffectParams.ZViewShift], _camera.ProjToViewShift);
			_effect.SetValue(_handles[MainEffectParams.PixelCorrection], _camera.PixelCorrection);
			_effect.SetValue(_handles[MainEffectParams.PixelCorrection2], _camera.PixelCorrection2);
			_effect.SetValue(_handles[MainEffectParams.AtlasTextureSize], _atlas.InvSize);
			_effect.SetValue(_handles[MainEffectParams.Ambient], _globalLight.Ambient);
			_effect.SetValue(_handles[MainEffectParams.GlobalLightColor], _globalLight.Color);
			Vector3 toLight, half;
			_globalLight.GetLightDirections(out toLight, out half);
			_effect.SetValue(_handles[MainEffectParams.ToGlobalLight], toLight);
			_effect.SetValue(_handles[MainEffectParams.HalfToCameraToLight], half);
			_effect.SetValue(_handles[MainEffectParams.ToCamera], -_camera.PerspectiveDirInVS);

			_effect.SetTexture(_handles[MainEffectParams.t_color], _atlas.Color);
			_effect.SetTexture(_handles[MainEffectParams.t_normal], _atlas.Normal);
			_effect.SetTexture(_handles[MainEffectParams.t_coeffs], _atlas.Coefficients);

			_effect.SetTexture(_handles[MainEffectParams.t_colorB1], _diffuseBuff);
			_effect.SetTexture(_handles[MainEffectParams.t_nBuff], _nBuff);
			_effect.SetTexture(_handles[MainEffectParams.t_colorB2], _lightOutput);
			_effect.SetTexture(_handles[MainEffectParams.t_ramp], _ramp.Texture);
		}



		private void DrawSphereLights() {
			_effect.BeginPass(6);
			foreach (SphereLight light in _sphereLightsToDraw) {
				_effect.SetValue(_handles[MainEffectParams.ObjectMatrix], light.ObjectMatrix);
				_effect.SetValue(_handles[MainEffectParams.LocalLightPos], light.Position);
				_effect.SetValue(_handles[MainEffectParams.LightRadiusFactor], light.LightRadiusFactor);
				_effect.SetValue(_handles[MainEffectParams.LocalLightColor], light.Color);
				_effect.SetValue(_handles[MainEffectParams.AttenFunc], light.AttenFunc);
				_effect.CommitChanges();
				_sphere.DrawSubset(0);
			}
			_effect.EndPass();
		}



		private void DrawMeshElements(int phase) {
			_effect.BeginPass(phase);
			foreach (MeshElement mesh in _meshesToDraw) {
				_effect.SetValue(_handles[MainEffectParams.ObjectMatrix], mesh.ObjectMatrix);
				_effect.SetValue(_handles[MainEffectParams.NormalMatrix], mesh.NormalTransform);
				_effect.SetValue(_handles[MainEffectParams.ObjectMaterial], mesh.Material);
				_effect.SetTexture(_handles[MainEffectParams.t_oColor], mesh.Texture);
				_effect.CommitChanges();
				for (int f = 0; f < mesh.MeshSource.Materials.Count; f++) {
					mesh.MeshSource.Mesh.DrawSubset(f);
				}
			}
			_effect.EndPass();
		}




		private void PrepareElementsToDraw() {
			foreach (Element e in _elementsToDraw) {
				_atlas.Request(e.Texture);
			}
			_atlas.LoadAllRequested();
			
			if (_maxInstanceCount < _elementsToDraw.Count) {
				while (_maxInstanceCount < _elementsToDraw.Count) {
					_maxInstanceCount *= 2;
				}
				_instanceVB.Dispose();
				_instanceVB = null;
				CreateInstanceVB();
			}

			using (DataStream stream = _instanceVB.Lock(0, _elementsToDraw.Count * InstanceVertex.SizeInBytes, LockFlags.Discard)) {
				foreach (Element e in _elementsToDraw) {
					stream.Write(new InstanceVertex(e.TexturePosition, e.Texture.Coords, e.Texture.Size, e.Scale));
				}

				_instanceVB.Unlock();
			}
		}

		public void ChangeLightPos(Vector3 pos) {
			_sphereLightsToDraw[0].Position = pos;
		}



		internal void ChangeRotation(float p) {
			_meshesToDraw[0].Transform = Matrix.RotationX(p*0.8f) * Matrix.RotationZ(p) * Matrix.Scaling(150, 150, 150);
			_meshesToDraw[1].Transform = Matrix.RotationX(p*1.1f) * Matrix.RotationZ(p*0.7f) * Matrix.Scaling(7, 7, 7);
			_meshesToDraw[2].Transform = Matrix.Scaling(200, 200, 200 + 150 * (float)Math.Sin(p)) * Matrix.RotationX(2.5f) * Matrix.RotationZ(3) * Matrix.RotationY(1);
			_meshesToDraw[3].Position = new Vector3(250*(float)Math.Sin(p) - 50, 200*(float)Math.Cos(p)-50, -300);
			_meshesToDraw[3].Transform = Matrix.RotationX((float)Math.PI / 2) * Matrix.RotationY(0.5f) * Matrix.RotationZ(-p - (float)Math.PI / 2) * Matrix.Scaling(5, 5, 5);
		}
	}








	[StructLayout(LayoutKind.Sequential)]
	struct GeometryVertex
	{
		[VertexElement(DeclarationType.Float2, DeclarationUsage.Position)]
		public Vector2 CornerPosition;

		public static int SizeInBytes {
			get { return Marshal.SizeOf(typeof(GeometryVertex)); }
		}

		// CONSTRUCTOR
		public GeometryVertex(Vector2 cornerPosition) {
			CornerPosition = cornerPosition;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	struct InstanceVertex
	{
		[VertexElement(DeclarationType.Float3, DeclarationUsage.TextureCoordinate)]
		public Vector3 WorldPosition;
		[VertexElement(DeclarationType.Float2, DeclarationUsage.TextureCoordinate)]
		public Vector2 TextureCoordinates;
		[VertexElement(DeclarationType.Float2, DeclarationUsage.TextureCoordinate)]
		public Vector2 TextureSize;
		[VertexElement(DeclarationType.Float1, DeclarationUsage.TextureCoordinate)]
		public float Scale;

		public static int SizeInBytes {
			get { return Marshal.SizeOf(typeof(InstanceVertex)); }
		}

		// CONSTRUCTOR
		public InstanceVertex(Vector3 worldPosition, Vector2 textureCoordinates, Vector2 textureSize, float scale) {
			WorldPosition = worldPosition;
			TextureCoordinates = textureCoordinates;
			TextureSize = textureSize;
			Scale = scale;
		}
	
	}

	[StructLayout(LayoutKind.Sequential)]
	struct LightPrimitiveVertex
	{
		[VertexElement(DeclarationType.Float3, DeclarationUsage.Position)]
		public Vector3 Position;

		public static int SizeInBytes {
			get { return Marshal.SizeOf(typeof(LightPrimitiveVertex)); }
		}

		// CONSTRUCTOR
		public LightPrimitiveVertex(Vector3 position) {
			Position = position;
		}
	}

}
