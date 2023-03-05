using UnityEngine;

namespace DeepDreams
{
    public class RTCameraOverlay : MonoBehaviour
    {
        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] private CustomRenderTexture renderTexture2;

        // void OnRenderImage(RenderTexture src, RenderTexture dest)
        // {
        //     // To overwrite the entire screen
        //     Graphics.Blit(replacement, null);
        //
        //     // Or to overwrite only what this specific Camera renders
        //     //Graphics.Blit(replacement, dest);
        // }

        private void OnGUI()
        {
            GUI.DrawTexture(new Rect(0, 0, 512, 512), renderTexture, ScaleMode.ScaleToFit, false, 1);
            GUI.DrawTexture(new Rect(0, 512, 512, 512), renderTexture2.GetDoubleBufferRenderTexture(), ScaleMode.ScaleToFit, false, 1);
        }
    }
}