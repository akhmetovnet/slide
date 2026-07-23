using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;

namespace UI
{
    public class ScreenshotController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _bestScore;
        [SerializeField] private Camera _camera;
        [SerializeField] private PixelPerfectCamera _pixelPerfectCamera;


        public void ShareScreenshot()
        {
            var record = PlayerPrefs.GetInt("Record", 0);
            gameObject.SetActive(true);

            _bestScore.text = $"BEST SCORE: {record}";
            
            
            var rt = new RenderTexture(_pixelPerfectCamera.refResolutionX, _pixelPerfectCamera.refResolutionY, 24);
            _camera.targetTexture = rt;
            var screenShot = new Texture2D(_pixelPerfectCamera.refResolutionX, _pixelPerfectCamera.refResolutionX, TextureFormat.RGB24, false);
            _camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, _pixelPerfectCamera.refResolutionX/2, _pixelPerfectCamera.refResolutionX, _pixelPerfectCamera.refResolutionX), 0, 0);
            _camera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            File.WriteAllBytes(Path.Combine( Application.temporaryCachePath, "screenshot.png"), screenShot.EncodeToPNG() );
            Debug.Log($"Took screenshot to: {Path.Combine(Application.temporaryCachePath, "screenshot.png")}");
            
            new NativeShare()
                .AddFile(Path.Combine( Application.temporaryCachePath, "screenshot.png") )
                .SetSubject( "Slide" )
                .SetText( "#Slide" )
                .Share();
            
            gameObject.SetActive(false);
        }
    }
}
