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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RuiJi.Slice.App
{
    /// <summary>
    /// CircleControl.xaml 的交互逻辑
    /// </summary>
    public partial class CircleControl : UserControl
    {
        public static readonly RoutedEvent ResetClickEvent = EventManager.RegisterRoutedEvent("ResetClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CircleControl));

        public event RoutedEventHandler ResetClick
        {
            add { this.AddHandler(ResetClickEvent, value); }
            remove { this.RemoveHandler(ResetClickEvent, value); }
        }

        public static readonly RoutedEvent PreClickEvent = EventManager.RegisterRoutedEvent("PreClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CircleControl));

        public event RoutedEventHandler PreClick
        {
            add { this.AddHandler(PreClickEvent, value); }
            remove { this.RemoveHandler(PreClickEvent, value); }
        }

        public static readonly RoutedEvent NextClickEvent = EventManager.RegisterRoutedEvent("NextClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CircleControl));

        public event RoutedEventHandler NextClick
        {
            add { this.AddHandler(NextClickEvent, value); }
            remove { this.RemoveHandler(NextClickEvent, value); }
        }

        public static readonly RoutedEvent LeftClickEvent = EventManager.RegisterRoutedEvent("LeftClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CircleControl));

        public event RoutedEventHandler LeftClick
        {
            add { this.AddHandler(LeftClickEvent, value); }
            remove { this.RemoveHandler(LeftClickEvent, value); }
        }

        public static readonly RoutedEvent RightClickEvent = EventManager.RegisterRoutedEvent("RightClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CircleControl));

        public event RoutedEventHandler RightClick
        {
            add { this.AddHandler(RightClickEvent, value); }
            remove { this.RemoveHandler(RightClickEvent, value); }
        }

        public CircleControl()
        {
            InitializeComponent();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var args = new RoutedEventArgs(ResetClickEvent);
            RaiseEvent(args);
        }
        private void Pre_Click(object sender, RoutedEventArgs e)
        {
            var args = new RoutedEventArgs(PreClickEvent);
            RaiseEvent(args);
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            var args = new RoutedEventArgs(NextClickEvent);
            RaiseEvent(args);
        }
        private void Left_Click(object sender, RoutedEventArgs e)
        {
            var args = new RoutedEventArgs(LeftClickEvent);
            RaiseEvent(args);
        }
        private void Right_Click(object sender, RoutedEventArgs e)
        {
            var args = new RoutedEventArgs(RightClickEvent);
            RaiseEvent(args);
        }
    }
}
