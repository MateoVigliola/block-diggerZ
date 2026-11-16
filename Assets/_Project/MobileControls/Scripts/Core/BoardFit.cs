using UnityEngine;

namespace BlockDiggerz.MobileControls
{
    /// <summary>
    /// Fits an orthographic camera so the board fills as much of a screen area as it can without cropping: the
    /// board stays still and the camera zooms and moves instead. Screen rects are in pixels with y up (like
    /// <see cref="Camera.pixelRect"/>).
    /// </summary>
    public static class BoardFit
    {
        private const float Half = 0.5f;

        /// <summary>
        /// Camera size and position that show <paramref name="boardWorld"/> (plus <paramref name="paddingWorld"/>
        /// on every side) inside <paramref name="targetPixels"/> of a <paramref name="viewportPixels"/> sized view.
        /// When the board doesn't fill the area, <paramref name="alignment"/> places it: x 0 left, 1 right; y 0 bottom,
        /// 1 top. Returns an invalid fit when any size is not positive.
        /// </summary>
        public static CameraFit Fit(Vector2 viewportPixels, Rect targetPixels, Rect boardWorld, float paddingWorld, Vector2 alignment)
        {
            Vector2 boardSize = boardWorld.size + Vector2.one * (2f * Mathf.Max(0f, paddingWorld));
            if (viewportPixels.x <= 0f || viewportPixels.y <= 0f || targetPixels.width <= 0f || targetPixels.height <= 0f
                || boardSize.x <= 0f || boardSize.y <= 0f)
            {
                return default;
            }

            float pixelsPerUnit = Mathf.Min(targetPixels.width / boardSize.x, targetPixels.height / boardSize.y);
            Vector2 boardPixels = boardSize * pixelsPerUnit;
            Vector2 slack = targetPixels.size - boardPixels;
            var clampedAlignment = new Vector2(Mathf.Clamp01(alignment.x), Mathf.Clamp01(alignment.y));
            Vector2 boardCenterPixels = targetPixels.min + boardPixels * Half + Vector2.Scale(slack, clampedAlignment);
            Vector2 offsetWorld = (boardCenterPixels - viewportPixels * Half) / pixelsPerUnit;
            float orthographicSize = viewportPixels.y * Half / pixelsPerUnit;
            return new CameraFit(orthographicSize, boardWorld.center - offsetWorld);
        }
    }
}
