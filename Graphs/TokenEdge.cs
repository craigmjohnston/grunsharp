namespace GrunCS.Graphs
{
    using QuickGraph;

    public class TokenEdge : Edge<PayloadVertex>
    {
        public TokenEdge(PayloadVertex source, PayloadVertex target) : base(source, target)
        {
        }
    }
}