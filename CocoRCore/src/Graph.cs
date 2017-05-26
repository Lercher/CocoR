namespace CocoRCore.CSharp // was at.jku.ssw.Coco for .Net V2
{
    //=====================================================================
    // Graph 
    //=====================================================================

    public class Graph
    {
        public Node l;  // left end of graph = head
        public Node r;  // right end of graph = list of nodes to be linked to successor graph

        public Graph()
        {
            l = null; r = null;
        }

        public Graph(Node left, Node right)
        {
            l = left; r = right;
        }

        public Graph(Node p)
        {
            l = p; r = p;
        }
    }

} // end namespace
