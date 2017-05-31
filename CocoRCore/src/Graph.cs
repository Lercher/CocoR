namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //=====================================================================
    // Graph 
    //=====================================================================

    public class Graph
    {
        public Node l;  // left end of graph = head
        public Node r;  // right end of graph = list of nodes to be linked to successor graph

        public Graph(Node left, Node right)
        {
            l = left;
            r = right;
        }

        public Graph(Node p) : this(p, p) { }
        public Graph() : this(null) { }
    }

} // end namespace
