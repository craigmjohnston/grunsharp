namespace GrunCS.Graphs
{
    using System.Windows;
    using GraphSharp.Controls;

    public class TokenGraphLayout : GraphLayout<PayloadVertex, TokenEdge, TokenGraph>
    {
        protected override void OnDragEnter(DragEventArgs e)
        {
        }
    }
}