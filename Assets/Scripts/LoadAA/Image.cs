using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LoadAA
{
    public enum FillOrigin
    {
        Bottom, Right, Top, Left
    }
    public class Image : MonoBehaviour
    {
        public AssetReference spriteRef;
        public UnityEngine.UI.Image.Type imageType;
        public bool useSpriteMesh;
        public bool preserveAspect;
        public bool fillCenter;
        public int pixelsPerUnitMultiplier;
        public UnityEngine.UI.Image.FillMethod fillMethod;
        public FillOrigin fillOrigin;
        public float fillAmount;
        public bool fillClockwise;
        
        protected void Awake()
        {
            spriteRef.LoadAssetAsync<Sprite>().Completed += (obj) =>
            {
                var img = gameObject.GetComponent<UnityEngine.UI.Image>();
                img.sprite = obj.Result;
                img.type = imageType;
                if (imageType == UnityEngine.UI.Image.Type.Simple)
                {
                    img.useSpriteMesh = useSpriteMesh;
                    img.preserveAspect = preserveAspect;
                }
                if (imageType == UnityEngine.UI.Image.Type.Sliced || imageType == UnityEngine.UI.Image.Type.Tiled)
                {
                    img.fillCenter = fillCenter;
                    img.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
                }

                if (imageType == UnityEngine.UI.Image.Type.Filled)
                {
                    img.fillMethod = fillMethod;
                    img.fillOrigin = (int)fillOrigin;
                    img.fillAmount = fillAmount;
                    img.fillClockwise = fillClockwise;
                }
            };
        }
    }
}
