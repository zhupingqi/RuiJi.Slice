using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RuiJi.Slice.App
{
    /// <summary>
    /// Loading.xaml 的交互逻辑
    /// </summary>
    public partial class Loading : UserControl
    {
        public Loading()
        {
            InitializeComponent();
            this.Loaded += Loading_Loaded;
        }
        private Storyboard sb = null;
        private double value2 = 200;//中间距离
        void Loading_Loaded(object sender, RoutedEventArgs e)
        {
            if (sb == null)
            {
                sb = new Storyboard();
            }
            double value3 = this.root.ActualWidth;//获取容器呈现宽度
            double value1 = (value3 - value2) / 2;//计算第一段移动距离
            sb.Children.Add(EllipseAnimation(value1, value1 + value2, value3, TimeSpan.FromSeconds(0), e1));
            sb.Children.Add(EllipseAnimation(value1, value1 + value2, value3, TimeSpan.FromSeconds(0.2), e2));
            sb.Children.Add(EllipseAnimation(value1, value1 + value2, value3, TimeSpan.FromSeconds(0.4), e3));
            sb.Children.Add(EllipseAnimation(value1, value1 + value2, value3, TimeSpan.FromSeconds(0.6), e4));
            sb.Children.Add(EllipseAnimation(value1, value1 + value2, value3, TimeSpan.FromSeconds(0.8), e5));
            sb.Children.Add(EllipseAnimation(value1, value1 + value2, value3, TimeSpan.FromSeconds(1.0), e6));
            sb.RepeatBehavior = RepeatBehavior.Forever;
            sb.Begin();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value1">第一阶段移动距离</param>
        /// <param name="value2">第二阶段移动距离</param>
        /// <param name="value3">第三阶段移动距离</param>
        /// <param name="startTime">动画开始时间</param>
        /// <param name="element">目标元素</param>
        /// <returns>DoubleAnimationUsingKeyFrames</returns>
        private DoubleAnimationUsingKeyFrames EllipseAnimation(double value1, double value2, double value3, TimeSpan startTime, UIElement element)
        {
            DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
            doubleAnimationUsingKeyFrames.BeginTime = startTime;
            EasingDoubleKeyFrame easingDoubleKeyFrame1 = new EasingDoubleKeyFrame();
            easingDoubleKeyFrame1.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5));
            easingDoubleKeyFrame1.Value = value1;
            EasingDoubleKeyFrame easingDoubleKeyFrame2 = new EasingDoubleKeyFrame();
            easingDoubleKeyFrame2.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2.1));
            easingDoubleKeyFrame2.Value = value2;
            EasingDoubleKeyFrame easingDoubleKeyFrame3 = new EasingDoubleKeyFrame();
            easingDoubleKeyFrame3.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2.6));
            easingDoubleKeyFrame3.Value = value3;
            doubleAnimationUsingKeyFrames.KeyFrames.Add(easingDoubleKeyFrame1);
            doubleAnimationUsingKeyFrames.KeyFrames.Add(easingDoubleKeyFrame2);
            doubleAnimationUsingKeyFrames.KeyFrames.Add(easingDoubleKeyFrame3);
            Storyboard.SetTarget(doubleAnimationUsingKeyFrames, element);
            Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames, new PropertyPath("(Canvas.Left)"));
            return doubleAnimationUsingKeyFrames;
        }
    }
}
