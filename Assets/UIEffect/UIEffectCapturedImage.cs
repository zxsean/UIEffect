using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Coffee.UIExtensions
{
    /// <summary>
    /// UIEffectCapturedImage
    /// </summary>
    public class UIEffectCapturedImage : RawImage
#if UNITY_EDITOR
		, ISerializationCallbackReceiver
#endif
	{

        //################################
        // Constant or Static Members.
        //################################
        public const string shaderName = "UI/Hidden/UI-EffectCapture";

        /// <summary>
        /// Desampling rate.
        /// </summary>
        public enum DesamplingRate
        {
            None = 0,
            x1 = 1,
            x2 = 2,
            x4 = 4,
            x8 = 8,
        }


        //################################
        // Serialize Members.
        //################################
        [SerializeField][Range(0, 1)] private float m_ToneLevel = 1;
        [SerializeField][Range(0, 1)] private float m_Blur = 0;
        [SerializeField] private EffectMode m_ToneMode;
        [SerializeField] private ColorEffectMode m_ColorMode;
        [SerializeField] private BlurEffectMode m_BlurMode;
        [SerializeField] private Color m_EffectColor = Color.white;
        [SerializeField] private DesamplingRate m_DesamplingRate;
        [SerializeField] private DesamplingRate m_ReductionRate;
        [SerializeField] private FilterMode m_FilterMode = FilterMode.Bilinear;
        [SerializeField] private Material m_EffectMaterial;
        [SerializeField][Range(1, 8)] private int m_Iterations = 1;
        [SerializeField] private bool m_KeepCanvasSize = true;
        [SerializeField] private RenderTexture TextureToCapture = null;


        //################################
        // Public Members.
        //################################
        /// <summary>
        /// Tone effect level between 0(no effect) and 1(complete effect).
        /// </summary>
        public float toneLevel { get { return m_ToneLevel; } set { m_ToneLevel = Mathf.Clamp(value, 0, 1); } }

        /// <summary>
        /// How far is the blurring from the graphic.
        /// </summary>
        public float blur { get { return m_Blur; } set { m_Blur = Mathf.Clamp(value, 0, 4); } }

        /// <summary>
        /// Tone effect mode.
        /// </summary>
        public EffectMode toneMode { get { return m_ToneMode; } set { m_ToneMode = value; } }

        /// <summary>
        /// Color effect mode.
        /// </summary>
        public ColorEffectMode colorMode { get { return m_ColorMode; } set { m_ColorMode = value; } }

        /// <summary>
        /// Blur effect mode.
        /// </summary>
        public BlurEffectMode blurMode { get { return m_BlurMode; } set { m_BlurMode = value; } }

        /// <summary>
        /// Color for the color effect.
        /// </summary>
        public Color effectColor { get { return m_EffectColor; } set { m_EffectColor = value; } }

        /// <summary>
        /// Effect material.
        /// </summary>
        public virtual Material effectMaterial { get { return m_EffectMaterial; } }

        /// <summary>
        /// Desampling rate of the generated RenderTexture.
        /// </summary>
        public DesamplingRate desamplingRate { get { return m_DesamplingRate; } set { m_DesamplingRate = value; } }

        /// <summary>
        /// Desampling rate of reduction buffer to apply effect.
        /// </summary>
        public DesamplingRate reductionRate { get { return m_ReductionRate; } set { m_ReductionRate = value; } }

        /// <summary>
        /// FilterMode for capture.
        /// </summary>
        public FilterMode filterMode { get { return m_FilterMode; } set { m_FilterMode = value; } }

        /// <summary>
        /// Captured texture.
        /// </summary>
        public RenderTexture capturedTexture { get { return this.generatedRt ?? TextureToCapture; } }

        /// <summary>
        /// Iterations.
        /// </summary>
        public int iterations { get { return m_Iterations; } set { m_Iterations = value; } }

        /// <summary>
        /// Fits graphic size to the root canvas.
        /// </summary>
        public bool keepCanvasSize { get { return m_KeepCanvasSize; } set { m_KeepCanvasSize = value; } }

        /// <summary>
        /// This function is called when the MonoBehaviour will be destroyed.
        /// </summary>
        protected override void OnDestroy()
        {
            _Release(true);
            base.OnDestroy();
        }

        /// <summary>
        /// Callback function when a UI element needs to generate vertices.
        /// </summary>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            // When not displaying, clear vertex.
            if (texture == null || effectColor.a < 1 / 255f || canvasRenderer.GetAlpha() < 1 / 255f)
                vh.Clear();
            else
                base.OnPopulateMesh(vh);
        }

        #if UNITY_EDITOR
        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            var obj = this;
            EditorApplication.delayCall += () =>
            {
                if (Application.isPlaying || !obj)
                    return;

                var mat = (0 == toneMode) && (0 == colorMode) && (0 == blurMode)
						? null
						: UIEffect.GetOrGenerateMaterialVariant(Shader.Find(shaderName), toneMode, colorMode, blurMode);

                if (m_EffectMaterial == mat)
                    return;

                m_EffectMaterial = mat;
                EditorUtility.SetDirty(this);
                EditorApplication.delayCall += AssetDatabase.SaveAssets;
            };
        }
        #endif

        /// <summary>
        /// Gets the size of the desampling.
        /// </summary>
        public void GetDesamplingSize(DesamplingRate rate, out int w, out int h)
        {
            var cam = canvas.worldCamera ?? Camera.main;
            h = cam.pixelHeight;
            w = cam.pixelWidth;
            if (rate != DesamplingRate.None)
            {
                h = Mathf.ClosestPowerOfTwo(h / (int)rate);
                w = Mathf.ClosestPowerOfTwo(w / (int)rate);
            }
        }

        /// <summary>
        /// Capture rendering result.
        /// </summary>
        public void Capture()
        {
            // Camera for command buffer.
            this.cameraToCapture = canvas.worldCamera ?? Camera.main;

            // Cache id for RT.
            if (CopyRtId == 0)
            {
                CopyRtId = Shader.PropertyToID("_UIEffectCapturedImage_ScreenCopyId");
                EffectRtId1 = Shader.PropertyToID("_UIEffectCapturedImage_EffectId1");
                EffectRtId2 = Shader.PropertyToID("_UIEffectCapturedImage_EffectId2");
            }

            // If size of generated result RT has changed, relese it.
            int w, h;
            this.GetDesamplingSize(this.m_DesamplingRate, out w, out h);
            if (this.generatedRt && (this.generatedRt.width != w || this.generatedRt.height != h))
            {
                this.rtToRelease = this.generatedRt;
                this.generatedRt = null;
            }

            // Generate result RT.
            if (this.capturedTexture == null)
            {
                this.generatedRt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
                this.generatedRt.filterMode = this.m_FilterMode;
                this.generatedRt.useMipMap = false;
                this.generatedRt.wrapMode = TextureWrapMode.Clamp;
                this.generatedRt.hideFlags = HideFlags.HideAndDontSave;
            }

            // Create command buffer.
            if (this.commandBuffer == null)
            {
                var rtId = new RenderTargetIdentifier(this.generatedRt);

                // Material for effect.
                Material mat = this.effectMaterial;

                this.commandBuffer = new CommandBuffer();
                this.commandBuffer.name =
					this.generatedRt.name =
						mat ? mat.name : "noeffect";

                // Copy to temporary RT.
                this.commandBuffer.GetTemporaryRT(CopyRtId, -1, -1, 0, m_FilterMode);
                this.commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, CopyRtId);

                // Set properties.
                this.commandBuffer.SetGlobalVector("_EffectFactor", new Vector4(toneLevel, 0, blur, 0));
                this.commandBuffer.SetGlobalVector("_ColorFactor", new Vector4(effectColor.r, effectColor.g, effectColor.b, effectColor.a));

                if (!mat)
                {
                    // Blit without effect.
                    this.commandBuffer.Blit(CopyRtId, rtId);
                    this.commandBuffer.ReleaseTemporaryRT(CopyRtId);
                }
				else
                {
                    // Blit with effect.
                    this.GetDesamplingSize(this.m_ReductionRate, out w, out h);
                    this.commandBuffer.GetTemporaryRT(EffectRtId1, w, h, 0, this.m_FilterMode);
                    this.commandBuffer.Blit(CopyRtId, EffectRtId1, mat);    // Apply effect (copied screen -> effect1).
                    this.commandBuffer.ReleaseTemporaryRT(CopyRtId);
					
                    // Iterate the operation.
                    if (1 < this.m_Iterations)
                    {
                        this.commandBuffer.GetTemporaryRT(EffectRtId2, w, h, 0, this.m_FilterMode);
                        for (int i = 1; i < this.m_Iterations; i++)
                        {
                            // Apply effect (effect1 -> effect2, or effect2 -> effect1).
                            this.commandBuffer.Blit(i % 2 == 0 ? EffectRtId2 : EffectRtId1, i % 2 == 0 ? EffectRtId1 : EffectRtId2, mat);
                        }
                    }

                    this.commandBuffer.Blit(this.m_Iterations % 2 == 0 ? EffectRtId2 : EffectRtId1, rtId);
                    this.commandBuffer.ReleaseTemporaryRT(EffectRtId1);
                    if (1 < this.m_Iterations)
                    {
                        this.commandBuffer.ReleaseTemporaryRT(EffectRtId2);
                    }
                }
            }

            // Add command buffer to camera.
            this.cameraToCapture.AddCommandBuffer(CameraEventToCapture, this.commandBuffer);

            // StartCoroutine by CanvasScaler.
            var rootCanvas = canvas.rootCanvas;
            var scaler = rootCanvas.GetComponent<CanvasScaler>();
            scaler.StartCoroutine(this._CoUpdateTextureOnNextFrame());
            if (this.m_KeepCanvasSize)
            {
                var size = (rootCanvas.transform as RectTransform).rect.size;
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            }
        }

        /// <summary>
        /// Release captured image.
        /// </summary>
        public void Release()
        {
            this._Release(true);
        }

        private const CameraEvent CameraEventToCapture = CameraEvent.AfterEverything;
        private Camera cameraToCapture;
        private RenderTexture generatedRt;
        private RenderTexture rtToRelease;
        private CommandBuffer commandBuffer;
        private static int CopyRtId;
        private static int EffectRtId1;
        private static int EffectRtId2;

        /// <summary>
        /// Release genarated objects.
        /// </summary>
        /// <param name="releaseRT">If set to <c>true</c> release cached RenderTexture.</param>
        private void _Release(bool releaseRT)
        {
            if (releaseRT)
            {
                this.texture = null;

                if (this.generatedRt != null)
                {
                    this.generatedRt.Release();
                    this.generatedRt = null;
                }
            }

            if (this.commandBuffer != null)
            {
                if (this.cameraToCapture != null)
                    this.cameraToCapture.RemoveCommandBuffer(CameraEventToCapture, this.commandBuffer);
                this.commandBuffer.Release();
                this.commandBuffer = null;
            }

            if (this.rtToRelease)
            {
                this.rtToRelease.Release();
                this.rtToRelease = null;
            }
        }

        /// <summary>
        /// Set texture on next frame.
        /// </summary>
        /// <returns>The update texture on next frame.</returns>
        private IEnumerator _CoUpdateTextureOnNextFrame()
        {
            yield return new WaitForEndOfFrame();

            this._Release(false);
            this.texture = this.generatedRt;
        }
    }
}