namespace MyFarm.Domain.Enums
{
    public enum PlotState
    {
        Locked,
        Empty,      // Đất trống
        Growing,    // Đang lớn/đang sản xuất
        Ready,      // Đã có sản phẩm, chờ thu hoạch
        Spoiled,    // Đã bị hỏng (nếu quá hạn 1h ở lần thu hoạch cuối)
      
    }
}