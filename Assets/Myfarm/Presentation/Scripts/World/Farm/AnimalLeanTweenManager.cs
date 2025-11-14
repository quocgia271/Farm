// File: Myfarm/Presentation/Scripts/World/AnimalLeanTweenManager.cs
using MyFarm.Domain.Enums;
using UnityEngine;


namespace MyFarm.Presentation.World
{
    public class AnimalLeanTweenManager : MonoBehaviour
    {
        // Bạn có thể kéo các đối tượng con vào đây nếu muốn
        [SerializeField] private Transform modelToAnimate; 
        
        private PlotState _currentState = (PlotState)(-1); // Cache
        private int _currentTweenId = -1; // ID của tween đang chạy

        private void Awake()
        {
            // Nếu không gán model, tự lấy của chính nó
            if (modelToAnimate == null)
            {
                modelToAnimate = transform;
            }
        }

        /// <summary>
        /// Hàm này được FarmPlotView gọi
        /// </summary>
        public void SetState(PlotState state)
        {
            // Chỉ chạy nếu state thay đổi
            if (state == _currentState) return;
            
            _currentState = state;

            // Dừng animation cũ (rất quan trọng)
            if (LeanTween.isTweening(_currentTweenId))
            {
                LeanTween.cancel(_currentTweenId);
            }
            
            // Khôi phục trạng thái gốc (đặc biệt là màu)
            ResetToDefault();

            // Chạy animation mới dựa trên state
            switch (state)
            {
                case PlotState.Growing:
                    AnimateGrowing();
                    break;
                case PlotState.Ready:
                    AnimateReady();
                    break;
                case PlotState.Spoiled:
                    AnimateSpoiled();
                    break;
            }
        }

        // Khôi phục trạng thái gốc
        private void ResetToDefault()
        {
            // Khôi phục scale gốc của prefab
            modelToAnimate.localScale = transform.localScale; 
            
            // Khôi phục màu gốc (nếu bạn có dùng Renderer)
            var renderer = modelToAnimate.GetComponent<Renderer>();
            if (renderer != null)
            {
                LeanTween.color(renderer.gameObject, Color.white, 0.1f);
            }
        }

        // --- ĐÂY LÀ 3 STATE ANIMATION CỦA BẠN ---
        // (Bạn có thể tự do thay đổi code bên trong các hàm này)

        private void AnimateGrowing()
        {
            // Ví dụ: Scale "thở" nhẹ
            var tween = LeanTween.scale(modelToAnimate.gameObject, transform.localScale * 1.05f, 1.5f)
                .setEasePunch() // Nhún một cái
                .setLoopPingPong(); // Lặp đi lặp lại
            
            _currentTweenId = tween.id;
        }

        private void AnimateReady()
        {
            // Ví dụ: Bobbing (nhảy lên xuống) nhẹ
            var tween = LeanTween.moveY(modelToAnimate.gameObject, modelToAnimate.position.y + 0.1f, 0.5f)
                .setEaseOutQuad()
                .setLoopPingPong(); // Lên xuống liên tục
            
            _currentTweenId = tween.id;
        }

        private void AnimateSpoiled()
        {
            // Ví dụ: Đổi màu thành xám/nâu
            var renderer = modelToAnimate.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Đổi màu thành xám đậm trong 0.5s
                var tween = LeanTween.color(renderer.gameObject, new Color(0.3f, 0.3f, 0.3f), 0.5f);
                _currentTweenId = tween.id;
            }
            // Bạn cũng có thể thêm 1 cái .setEaseShake() nếu muốn
        }
    }
}