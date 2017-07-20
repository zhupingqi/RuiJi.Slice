/*
This file is part of RuiJi.Slice: A library for slicing 3D model.
RuiJi.Slice is part of RuiJiHG: RuiJiHG is holographic projection.
see http://www.ruijihg.com/ for more infomation.

Copyright (C) 2017 Pingqi(416803633@qq.com)
Copyright (c) 2017, githublixiang(271800249@qq.com)

RuiJi.Slice is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

RuiJi.Slice is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with wiringPi.
If not, see <http://www.gnu.org/licenses/>.
*/

using System.Numerics;

namespace RuiJi.Slicer.Core
{
    public interface IArrayCreater
    {
        SlicePanelInfo[] CreateArrayPlane(ArrayDefine define);
    }

    /// <summary>
    /// 阵列类型
    /// </summary>
    public enum ArrayType
    {
        /// <summary>
        /// 圆周阵列
        /// </summary>
        Circle,
        /// <summary>
        /// 线性阵列
        /// </summary>
        Line
    }
}