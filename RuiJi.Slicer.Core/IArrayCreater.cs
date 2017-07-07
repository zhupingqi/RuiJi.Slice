using System.Numerics;

namespace RuiJi.Slicer.Core
{
    public interface IArrayCreater
    {
        SlicePanelInfo[] CreateArrayPlane(ArrayDefine define);
    }

    public enum ArrayType
    {
        Circle,
        Line
    }
}